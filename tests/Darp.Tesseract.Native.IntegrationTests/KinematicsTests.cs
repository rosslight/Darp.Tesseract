using Darp.Geometry.Tensor2;
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

        var seed = new VectorXD(checked((int)group.numJoints()));

        var transforms = group.calcFwdKin(seed);
        transforms.ContainsKey(tipLink).ShouldBeTrue();
        var target = transforms[tipLink];

        var jacobian = group.calcJacobian(seed, tipLink);
        jacobian.Rows.ShouldBe(6);
        jacobian.Columns.ShouldBe(6);
        foreach (var value in jacobian.ToArray())
            double.IsFinite(value).ShouldBeTrue();

        using var input = new KinGroupIKInput(target, group.getBaseLinkName(), tipLink);
        var solutions = group.calcInvKin(input, seed);
        solutions.Count.ShouldBeGreaterThan(0);

        var roundTripMatched = false;
        for (var index = 0; index < solutions.Count; index++)
        {
            var solution = solutions[index];
            solution.Count.ShouldBe(seed.Count);
            foreach (var joint in solution.ToArray())
                double.IsFinite(joint).ShouldBeTrue();

            var candidateTransforms = group.calcFwdKin(solution);
            var candidatePose = candidateTransforms[tipLink];
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

    private static bool PosesApproximatelyEqual(ReadOnlyIsometry3D expected, ReadOnlyIsometry3D actual, double tolerance)
    {
        var a = expected.AsReadOnlyMatrix();
        var b = actual.AsReadOnlyMatrix();
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
        var joints = new VectorXD(0.1, -0.2, 0.3, 0.1, 0.2, -0.1);
        var storage = new double[12];
        var strided = VectorXD.Map(storage, 6, 2);
        for (int i = 0; i < 6; i++) { strided[i] = joints[i]; storage[i * 2 + 1] = 99; }
        var expected = group.calcFwdKin(joints)["tool0"];
        var poses = group.calcFwdKin(new VectorXD(6));
        var earlier = poses["tool0"];
        var earlierCopy = earlier.AsReadOnlyMatrix().ToArray();
        group.calcFwdKin(ref poses, strided);
        PosesApproximatelyEqual(expected, poses["tool0"], 1e-10).ShouldBeTrue();
        earlier.AsReadOnlyMatrix().ToArray().ShouldBe(earlierCopy);
        for (int i = 0; i < 6; i++) storage[i * 2 + 1].ShouldBe(99);
    }

    [Fact]
    public void MatrixPropertiesPreserveRowMajorLayoutAndRejectWrongShape()
    {
        using var limits = new KinematicLimits();
        var matrix = MatrixXD.Map(new double[] { -1, 1, -2, 2, -3, 3 }, 3, 2, columnStride: 1, rowStride: 2);
        limits.joint_limits = matrix;
        var saved = limits.joint_limits;
        saved.Rows.ShouldBe(3);
        saved.Columns.ShouldBe(2);
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < 2; column++)
                saved[row, column].ShouldBe(matrix[row, column]);
        Should.Throw<ArgumentException>(() => limits.joint_limits = new MatrixXD(3, 3));
        limits.joint_limits[2, 1].ShouldBe(3);
        saved[0, 0].ShouldBe(-1);
    }

    [Fact]
    public void JointStateSnapshotsSurviveMutationDisposalAndCollection()
    {
        var snapshot = CreateJointSnapshot();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        snapshot.ToArray().ShouldBe(new double[] { 1, 2, 3 });
        snapshot.Norm().ShouldBe(Math.Sqrt(14), 1e-12);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static ReadOnlyVectorXD CreateJointSnapshot()
    {
        using var names = new StringVector { "a", "b", "c" };
        using var state = new JointState(names, new VectorXD(1.0, 2.0, 3.0));
        state.velocity.Count.ShouldBe(0);
        state.velocity = new VectorXD(0);
        state.velocity.Count.ShouldBe(0);
        var saved = state.position;
        state.position = new VectorXD(4.0, 5.0, 6.0);
        state.position.ToArray().ShouldBe(new double[] { 4, 5, 6 });
        return saved;
    }

    [Fact]
    public void ContainerElementRetainsNativeStorageAfterContainerCollection()
    {
        var pose = CreateDetachedPose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        pose.Translation.X.ShouldBe(1);
        pose.Translation.Y.ShouldBe(2);
        pose.Translation.Z.ShouldBe(3);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static ReadOnlyIsometry3D CreateDetachedPose()
    {
        var pose = Isometry3D.Identity;
        var translation = pose.Translation;
        translation.X = 1; translation.Y = 2; translation.Z = 3;
        var map = new TransformMap(new Dictionary<string, ReadOnlyIsometry3D> { ["pose"] = pose });
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
