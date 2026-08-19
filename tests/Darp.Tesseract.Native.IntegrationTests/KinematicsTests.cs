using Shouldly;
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

        var requiredNativeAssets = OperatingSystem.IsWindows()
            ? new[]
            {
                "tesseract_csharp.dll",
                "tesseract_environment.dll",
                "tesseract_kinematics_kdl_factories.dll",
                "boost_plugin_loader.dll",
            }
            : OperatingSystem.IsMacOS()
                ? new[]
                {
                    "libtesseract_csharp.dylib",
                    "libtesseract_environment.dylib",
                    "libtesseract_kinematics_kdl_factories.dylib",
                    "libboost_plugin_loader.dylib",
                }
                : new[]
                {
                    "libtesseract_csharp.so",
                    "libtesseract_environment.so",
                    "libtesseract_kinematics_kdl_factories.so",
                    "libboost_plugin_loader.so",
                };

        foreach (var asset in requiredNativeAssets)
            File.Exists(Path.Combine(AppContext.BaseDirectory, asset)).ShouldBeTrue($"missing packaged native asset '{asset}'");
    }
#endif

    [Fact]
    public void AbbIrb2400EnvironmentSupportsForwardJacobianAndInverseKinematics()
    {
        var repositoryRoot = FindRepositoryRoot();
        var tesseractRoot = Path.Combine(repositoryRoot, "native", "tesseract");
        var urdfDirectory = Path.Combine(tesseractRoot, "support", "urdf");
        var urdf = File.ReadAllText(Path.Combine(urdfDirectory, "abb_irb2400.urdf"));
        var srdf = File.ReadAllText(Path.Combine(urdfDirectory, "abb_irb2400.srdf"));

        using var locator = new GeneralResourceLocator();
        locator.addPath(Path.GetDirectoryName(tesseractRoot)!).ShouldBeTrue();

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

    private static string FindRepositoryRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "native", "tesseract", "package.xml")))
                    return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the Darp.Tesseract.Native repository root.");
    }
}
