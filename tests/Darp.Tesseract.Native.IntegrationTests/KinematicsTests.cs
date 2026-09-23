using Aardvark.Base;
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

        var seed = new double[checked((int)group.numJoints())];

        using var transforms = group.calcFwdKin(seed);
        transforms.ContainsKey(tipLink).ShouldBeTrue();
        var target = transforms[tipLink];

        var jacobian = group.calcJacobian(seed, tipLink);
        jacobian.GetLength(0).ShouldBe(6);
        jacobian.GetLength(1).ShouldBe(6);
        foreach (var value in jacobian)
            double.IsFinite(value).ShouldBeTrue();

        using var input = new KinGroupIKInput(target, group.getBaseLinkName(), tipLink);
        using var solutions = group.calcInvKin(input, seed);
        solutions.Count.ShouldBeGreaterThan(0);

        var roundTripMatched = false;
        for (var index = 0; index < solutions.Count; index++)
        {
            var solution = solutions[index];
            solution.Length.ShouldBe(seed.Length);
            foreach (var joint in solution)
                double.IsFinite(joint).ShouldBeTrue();

            using var candidateTransforms = group.calcFwdKin(solution);
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

    private static bool PosesApproximatelyEqual(Euclidean3d expected, Euclidean3d actual, double tolerance)
    {
        var a = (M44d)expected;
        var b = (M44d)actual;
        for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
                if (Math.Abs(a[row, column] - b[row, column]) > tolerance) return false;
        return true;
    }

    [Fact]
    public void CopiedJointInputAndRefOutputsMatchValueOverloads()
    {
        using var environment = CreateEnvironment();
        using var group = environment.getKinematicGroup("manipulator");
        double[] joints = [0.1, -0.2, 0.3, 0.1, 0.2, -0.1];
        using var expectedPoses = group.calcFwdKin(joints);
        var expected = expectedPoses["tool0"];
        using var originalPoses = group.calcFwdKin(new double[6]);
        var poses = originalPoses;
        var earlier = poses["tool0"];
        group.calcFwdKin(ref poses, joints);
        using var updatedPoses = poses;
        originalPoses.Dispose();
        PosesApproximatelyEqual(expected, poses["tool0"], 1e-10).ShouldBeTrue();
        earlier.Trans.X.ShouldBe(0);
    }

    [Fact]
    public void MatrixPropertiesCopyRowMajorInputsAndRejectWrongShape()
    {
        using var limits = new KinematicLimits();
        double[,] matrix = { { -1, 1 }, { -2, 2 }, { -3, 3 } };
        limits.joint_limits = matrix;
        matrix[0, 0] = 99;
        var saved = limits.joint_limits;
        saved.GetLength(0).ShouldBe(3);
        saved.GetLength(1).ShouldBe(2);
        saved[0, 0].ShouldBe(-1);
        saved[2, 1].ShouldBe(3);
        Should.Throw<ArgumentException>(() => limits.joint_limits = new double[3, 3]);
    }

    [Fact]
    public void JointStateSnapshotsSurviveMutationDisposalAndCollection()
    {
        var snapshot = CreateJointSnapshot();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        snapshot.ShouldBe(new double[] { 1, 2, 3 });
        Math.Sqrt(snapshot.Sum(value => value * value)).ShouldBe(Math.Sqrt(14), 1e-12);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static double[] CreateJointSnapshot()
    {
        using var names = new StringVector { "a", "b", "c" };
        double[] initial = [1.0, 2.0, 3.0];
        using var state = new JointState(names, initial);
        var velocity = state.velocity;
        velocity.Length.ShouldBe(0);
        state.velocity = [];
        var updatedVelocity = state.velocity;
        updatedVelocity.Length.ShouldBe(0);
        var saved = state.position;
        state.position = [4.0, 5.0, 6.0];
        var current = state.position;
        current.ShouldBe(new double[] { 4, 5, 6 });
        return saved;
    }

    [Fact]
    public void ContainerElementRetainsNativeStorageAfterContainerDisposalAndCollection()
    {
        var pose = CreateDetachedPose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        pose.Trans.X.ShouldBe(1);
        pose.Trans.Y.ShouldBe(2);
        pose.Trans.Z.ShouldBe(3);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static Euclidean3d CreateDetachedPose()
    {
        var pose = new Euclidean3d(Rot3d.Identity, new V3d(1, 2, 3));
        using var map = new TransformMap(new Dictionary<string, Euclidean3d> { ["pose"] = pose });
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
