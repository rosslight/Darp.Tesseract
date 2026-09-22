using Darp.Geometry;
using Shouldly;
using System.Runtime.InteropServices;
using Xunit;
using TesseractEnvironment = Darp.Tesseract.Native.Environment;

namespace Darp.Tesseract.Native.IntegrationTests;

public sealed class KinematicsTests
{
#if PACKAGE_CONSUMER
    [Fact]
    public void PackagedTestDoesNotInheritTheNativeBuildEnvironment()
    {
        System.Environment.GetEnvironmentVariable("CONDA_PREFIX").ShouldBeNullOrEmpty();
        var path = System.Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        path.Contains(".pixi", StringComparison.OrdinalIgnoreCase).ShouldBeFalse();

        var wrapperName = OperatingSystem.IsWindows()
            ? "tesseract_csharp.dll"
            : OperatingSystem.IsMacOS()
                ? "libtesseract_csharp.dylib"
                : "libtesseract_csharp.so";
        var nativeFiles = Directory
            .EnumerateFiles(AppContext.BaseDirectory, "*", SearchOption.AllDirectories)
            .ToArray();
        nativeFiles.ShouldContain(
            path => string.Equals(Path.GetFileName(path), wrapperName, StringComparison.OrdinalIgnoreCase),
            $"missing packaged native asset '{wrapperName}' for {RuntimeInformation.RuntimeIdentifier}"
        );

        string[] wrapperNames =
        [
            "tesseract_csharp.dll",
            "libtesseract_csharp.so",
            "libtesseract_csharp.dylib",
        ];
        nativeFiles
            .Where(path =>
            {
                var fileName = Path.GetFileName(path);
                return (fileName.StartsWith("tesseract_", StringComparison.OrdinalIgnoreCase)
                        || fileName.StartsWith("libtesseract_", StringComparison.OrdinalIgnoreCase))
                    && !wrapperNames.Contains(fileName, StringComparer.OrdinalIgnoreCase);
            })
            .ShouldBeEmpty("Tesseract components and embedded factories should be linked into the wrapper");
        nativeFiles
            .Where(path =>
                Path.GetFileName(path).StartsWith("vtk", StringComparison.OrdinalIgnoreCase)
                || Path.GetFileName(path).StartsWith("pcl_", StringComparison.OrdinalIgnoreCase)
            )
            .ShouldBeEmpty("the lean runtime must not contain the disabled PCL/VTK point-cloud stack");
    }
#endif

    [Fact]
    public void AbbIrb2400EnvironmentSupportsForwardJacobianAndInverseKinematics()
    {
        var assetRoot = Path.Combine(AppContext.BaseDirectory, "Assets");
        var fixtureRoot = Path.Combine(assetRoot, "darp_test");
        var urdf = File.ReadAllText(Path.Combine(fixtureRoot, "abb_irb2400.urdf"));
        var srdf = File.ReadAllText(Path.Combine(fixtureRoot, "abb_irb2400.srdf"));

        using var locator = new GeneralResourceLocator();
        locator.addPath(assetRoot).ShouldBeTrue();

        using var sceneGraph = TesseractNative.parseURDFString(urdf, locator);
        sceneGraph.ShouldNotBeNull();
        sceneGraph.getRoot().ShouldBe("base_link");

        using var srdfModel = new SRDFModel();
        srdfModel.initString(sceneGraph, srdf, locator);

        using var environment = new TesseractEnvironment();
        environment.init(sceneGraph, srdfModel).ShouldBeTrue();
        environment.isInitialized().ShouldBeTrue();

        using (var stateSolver = environment.getStateSolver())
        {
            stateSolver.ShouldNotBeNull();
            stateSolver.getBaseLinkName().ShouldBe("base_link");
        }

        using var group = environment.getKinematicGroup("manipulator");
        group.ShouldNotBeNull();
        group.numJoints().ShouldBe(6);
        group.getBaseLinkName().ShouldBe("base_link");

        using var activeLinks = group.getActiveLinkNames();
        var tipLink = activeLinks[^1];
        tipLink.ShouldBe("tool0");

        using var seed = new VectorXD(checked((int)group.numJoints()));

        using var transforms = group.calcFwdKin(seed);
        transforms.ContainsKey(tipLink).ShouldBeTrue();
        using var target = transforms[tipLink];

        using var jacobian = group.calcJacobian(seed, tipLink);
        jacobian.Rows.ShouldBe(6);
        jacobian.Columns.ShouldBe(6);
        foreach (var value in jacobian.ToArray())
            double.IsFinite(value).ShouldBeTrue();

        using var input = new KinGroupIKInput(target, group.getBaseLinkName(), tipLink);
        using var solutions = group.calcInvKin(input, seed);
        solutions.Count.ShouldBeGreaterThan(0);

        var roundTripMatched = false;
        for (var index = 0; index < solutions.Count; index++)
        {
            using var solution = solutions[index];
            solution.Count.ShouldBe(seed.Count);
            foreach (var joint in solution.ToArray())
                double.IsFinite(joint).ShouldBeTrue();

            using var candidateTransforms = group.calcFwdKin(solution);
            using var candidatePose = candidateTransforms[tipLink];
            if (PosesApproximatelyEqual(target, candidatePose, 1e-6))
            {
                roundTripMatched = true;
                break;
            }
        }

        roundTripMatched.ShouldBeTrue("at least one IK solution should reproduce the requested FK pose");
    }

    [Fact]
    public void EmbeddedCollisionFactoriesLoadFromUnchangedPluginConfiguration()
    {
        using var environment = CreateEnvironment();

        environment.setActiveDiscreteContactManager("BulletDiscreteBVHManager").ShouldBeTrue();
        using (var bullet = environment.getDiscreteContactManager())
        {
            bullet.ShouldNotBeNull();
            bullet.getName().ShouldContain("Bullet", Case.Insensitive);
            using var collisionObjects = bullet.getCollisionObjects();
            collisionObjects.Count.ShouldBeGreaterThan(0);
        }

        environment.setActiveDiscreteContactManager("FCLDiscreteBVHManager").ShouldBeTrue();
        using (var fcl = environment.getDiscreteContactManager())
        {
            fcl.ShouldNotBeNull();
            fcl.getName().ShouldContain("FCL", Case.Insensitive);
            using var collisionObjects = fcl.getCollisionObjects();
            collisionObjects.Count.ShouldBeGreaterThan(0);
        }

        environment.setActiveContinuousContactManager("BulletCastBVHManager").ShouldBeTrue();
        using var continuous = environment.getContinuousContactManager();
        continuous.ShouldNotBeNull();
        continuous.getName().ShouldContain("Bullet", Case.Insensitive);
    }

    [Fact]
    public void EmbeddedOpwFactoryLoadsFromUnchangedPluginConfiguration()
    {
        using var environment = CreateEnvironment();
        using var group = environment.getKinematicGroup("manipulator");

        group.ShouldNotBeNull();
        using var inverseKinematics = group.getInverseKinematics();
        inverseKinematics.getSolverName().ShouldBe("OPWInvKin");
    }

    [Fact]
    public void EnvironmentSupportsSingleAndBatchCommands()
    {
        using var environment = CreateEnvironment();
        using var link = new Link("fixture");
        using var add = new AddLinkCommand(link);

        environment.applyCommand(add).ShouldBeTrue();
        using var added = environment.getLink("fixture");
        added.ShouldNotBeNull();
        added.getName().ShouldBe("fixture");

        using var remove = new RemoveLinkCommand("fixture");
        using var commands = new CommandVector { remove };

        environment.applyCommands(commands).ShouldBeTrue();
        environment.getLink("fixture").ShouldBeNull();
    }

    private static bool PosesApproximatelyEqual(IReadOnlyIsometry3D expected, IReadOnlyIsometry3D actual, double tolerance)
    {
        using var a = expected.AsReadOnlyMatrix();
        using var b = actual.AsReadOnlyMatrix();
        for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
                if (Math.Abs(a[row, column] - b[row, column]) > tolerance) return false;
        return true;
    }

    [Fact]
    public void StridedJointInputAndRefOutputsMatchValueOverloads()
    {
        using var environment = CreateEnvironment();
        using var group = environment.getKinematicGroup("manipulator");
        using var joints = new VectorXD(0.1, -0.2, 0.3, 0.1, 0.2, -0.1);
        var storage = new double[12];
        using var strided = VectorXD.CreateFromMemory(storage, 6, 2);
        for (int i = 0; i < 6; i++) { strided[i] = joints[i]; storage[i * 2 + 1] = 99; }
        using var expectedPoses = group.calcFwdKin(joints);
        using var expected = expectedPoses["tool0"];
        using var zero = new VectorXD(6);
        using var originalPoses = group.calcFwdKin(zero);
        var poses = originalPoses;
        using var earlier = poses["tool0"];
        using var earlierMatrix = earlier.AsReadOnlyMatrix();
        var earlierCopy = earlierMatrix.ToArray();
        group.calcFwdKin(ref poses, strided);
        using var updatedPoses = poses;
        originalPoses.Dispose();
        using var updated = poses["tool0"];
        PosesApproximatelyEqual(expected, updated, 1e-10).ShouldBeTrue();
        earlierMatrix.ToArray().ShouldBe(earlierCopy);
        for (int i = 0; i < 6; i++) storage[i * 2 + 1].ShouldBe(99);
    }

    [Fact]
    public void MatrixPropertiesPreserveRowMajorLayoutAndRejectWrongShape()
    {
        using var limits = new KinematicLimits();
        using var matrix = MatrixXD.CreateFromMemory(new double[] { -1, 1, -2, 2, -3, 3 }, 3, 2, columnStride: 1, rowStride: 2);
        using var readable = matrix.AsReadOnly();
        limits.joint_limits = readable;
        using var saved = limits.joint_limits;
        saved.Rows.ShouldBe(3);
        saved.Columns.ShouldBe(2);
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < 2; column++)
                saved[row, column].ShouldBe(matrix[row, column]);
        using var wrongShape = new MatrixXD(3, 3);
        using var wrongShapeView = wrongShape.AsReadOnly();
        Should.Throw<ArgumentException>(() => limits.joint_limits = wrongShapeView);
        using var current = limits.joint_limits;
        current[2, 1].ShouldBe(3);
        saved[0, 0].ShouldBe(-1);
    }

    [Fact]
    public void JointStateSnapshotsSurviveMutationDisposalAndCollection()
    {
        using var snapshot = CreateJointSnapshot();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        snapshot.ToArray().ShouldBe(new double[] { 1, 2, 3 });
        snapshot.Norm().ShouldBe(Math.Sqrt(14), 1e-12);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static IReadOnlyVectorXD CreateJointSnapshot()
    {
        using var names = new StringVector { "a", "b", "c" };
        using var initial = new VectorXD(1.0, 2.0, 3.0);
        using var state = new JointState(names, initial);
        using var velocity = state.velocity;
        velocity.Count.ShouldBe(0);
        using var empty = new VectorXD(0);
        using var emptyView = empty.AsReadOnly();
        state.velocity = emptyView;
        using var updatedVelocity = state.velocity;
        updatedVelocity.Count.ShouldBe(0);
        var saved = state.position;
        using var replacement = new VectorXD(4.0, 5.0, 6.0);
        using var replacementView = replacement.AsReadOnly();
        state.position = replacementView;
        using var current = state.position;
        current.ToArray().ShouldBe(new double[] { 4, 5, 6 });
        return saved;
    }

    [Fact]
    public void ContainerElementRetainsNativeStorageAfterContainerDisposalAndCollection()
    {
        using var pose = CreateDetachedPose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        using var translation = pose.Translation;
        translation.X.ShouldBe(1);
        translation.Y.ShouldBe(2);
        translation.Z.ShouldBe(3);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static IReadOnlyIsometry3D CreateDetachedPose()
    {
        using var pose = Isometry3D.Identity;
        using var translation = pose.Translation;
        translation.X = 1; translation.Y = 2; translation.Z = 3;
        using var readable = pose.AsReadOnly();
        using var map = new TransformMap(new Dictionary<string, IReadOnlyIsometry3D> { ["pose"] = readable });
        return map["pose"];
    }

    private static TesseractEnvironment CreateEnvironment()
    {
        var assetRoot = Path.Combine(AppContext.BaseDirectory, "Assets");
        var fixtureRoot = Path.Combine(assetRoot, "darp_test");
        var urdf = File.ReadAllText(Path.Combine(fixtureRoot, "abb_irb2400.urdf"));
        var srdf = File.ReadAllText(Path.Combine(fixtureRoot, "abb_irb2400.srdf"));

        using var locator = new GeneralResourceLocator();
        locator.addPath(assetRoot).ShouldBeTrue();
        using var sceneGraph = TesseractNative.parseURDFString(urdf, locator);
        sceneGraph.ShouldNotBeNull();
        using var srdfModel = new SRDFModel();
        srdfModel.initString(sceneGraph, srdf, locator);

        var environment = new TesseractEnvironment();
        environment.init(sceneGraph, srdfModel).ShouldBeTrue();
        return environment;
    }
}
