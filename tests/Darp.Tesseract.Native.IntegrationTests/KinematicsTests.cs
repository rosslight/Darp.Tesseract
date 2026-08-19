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

        var nativeTesseractPrefix = OperatingSystem.IsWindows() ? "tesseract_" : "libtesseract_";
        nativeFiles
            .Where(path => !string.Equals(Path.GetFileName(path), wrapperName, StringComparison.OrdinalIgnoreCase))
            .Where(path => Path.GetFileName(path).StartsWith(nativeTesseractPrefix, StringComparison.OrdinalIgnoreCase))
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

        using var seed = new VectorXd(checked((int)group.numJoints()));
        seed.AsSpan().Clear();

        using var transforms = group.calcFwdKin(seed);
        transforms.contains(tipLink).ShouldBeTrue();
        using var target = transforms.get(tipLink);

        using var jacobian = group.calcJacobian(seed, tipLink);
        jacobian.rows().ShouldBe(6);
        jacobian.columns().ShouldBe(6);
        foreach (var value in jacobian.AsSpan())
            double.IsFinite(value).ShouldBeTrue();

        using var input = new KinGroupIKInput(target, group.getBaseLinkName(), tipLink);
        using var solutions = group.calcInvKin(input, seed);
        solutions.size().ShouldBeGreaterThan(0);

        var roundTripMatched = false;
        using var candidate = new VectorXd(checked((int)group.numJoints()));
        for (var index = 0; index < solutions.size(); index++)
        {
            var solution = solutions.GetSolutionSpan(index);
            solution.Length.ShouldBe(candidate.size());
            foreach (var joint in solution)
                double.IsFinite(joint).ShouldBeTrue();

            solution.CopyTo(candidate.AsSpan());
            using var candidateTransforms = group.calcFwdKin(candidate);
            using var candidatePose = candidateTransforms.get(tipLink);
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
    public void GeneratedNativeViewsValidateBoundsWithoutManagedArrays()
    {
        using var vector = new VectorXd(6);
        vector.AsSpan().Fill(0.25);

        vector.get(0).ShouldBe(0.25);
        vector.get(5).ShouldBe(0.25);
        Should.Throw<IndexOutOfRangeException>(() => vector.get(6));
    }

    private static bool PosesApproximatelyEqual(Isometry3d expected, Isometry3d actual, double tolerance)
    {
        var translationMatches =
            Math.Abs(expected.translationX() - actual.translationX()) <= tolerance &&
            Math.Abs(expected.translationY() - actual.translationY()) <= tolerance &&
            Math.Abs(expected.translationZ() - actual.translationZ()) <= tolerance;

        var quaternionDot =
            expected.quaternionX() * actual.quaternionX() +
            expected.quaternionY() * actual.quaternionY() +
            expected.quaternionZ() * actual.quaternionZ() +
            expected.quaternionW() * actual.quaternionW();

        return translationMatches && Math.Abs(Math.Abs(quaternionDot) - 1) <= tolerance;
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
