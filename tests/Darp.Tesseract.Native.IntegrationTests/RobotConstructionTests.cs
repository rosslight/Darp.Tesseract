using System.Runtime.CompilerServices;
using Aardvark.Base;
using Shouldly;
using Xunit;
using TesseractEnvironment = Darp.Tesseract.Native.Environment;

namespace Darp.Tesseract.Native.IntegrationTests;

public sealed class RobotConstructionTests
{
    private const string UrConfig = """
        base_link: base_link
        tip_link: tool0
        params:
          d1: 0.1273
          a2: -0.612
          a3: -0.5723
          d4: 0.163941
          d5: 0.1157
          d6: 0.0922
        """;

    [Fact]
    public void PluginConfigurationIsCopiedAndMalformedYamlPreservesThePreviousValue()
    {
        using var plugin = new PluginInfo { class_name = "URInvKinFactory" };
        plugin.setConfigString(UrConfig);
        var before = plugin.getConfigString();
        Should.Throw<ApplicationException>(() => plugin.setConfigString("params: ["));
        plugin.getConfigString().ShouldBe(before);
        using var plugins = new PluginInfoContainer { default_plugin = "RuntimeUR" };
        plugins.addPlugin("RuntimeUR", plugin);
        plugin.setConfigString("model: UR5");
        using var configuration = new KinematicsPluginInfo();
        configuration.setInvPluginInfo("manipulator", plugins);
        configuration.setFwdPluginInfo("manipulator", plugins);
        plugins.clear();
        using var saved = configuration.getInvPluginInfo("manipulator");
        using var savedPlugin = saved.getPlugin("RuntimeUR");
        savedPlugin.getConfigString().ShouldBe(before);
        savedPlugin.setConfigString("model: UR3");
        using var unchanged = saved.getPlugin("RuntimeUR");
        unchanged.getConfigString().ShouldBe(before);
        configuration.Dispose();
        using var detached = saved.getPlugin("RuntimeUR");
        detached.getConfigString().ShouldBe(before);
        Should.Throw<IndexOutOfRangeException>(() => saved.getPlugin("missing"));
    }

    [Fact]
    public void RuntimeUrPluginAndMultipleIkInputsReproduceTheRequestedTcpPose()
    {
        using var environment = CreateUrEnvironment();
        AssertUrRoundTrip(environment);
    }

    [Fact]
    public void ClonedEnvironmentRetainsRuntimePluginsAfterSourceDisposalAndCollection()
    {
        using var clone = CreateDetachedClone();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        AssertUrRoundTrip(clone);
    }

    [Fact]
    public void EnvironmentCloneHasIndependentStateAndSceneCommands()
    {
        using var original = StockPipelineTests.CreateEnvironment();
        using var clone = original.clone();
        using var group = clone.getJointGroup("manipulator");
        using var names = group.getJointNames();
        clone.setState(names, [0.2, -0.3, 0.1, 0.4, 0.2, -0.1]);
        using var originalState = original.getState();
        using var cloneState = clone.getState();
        using var originalJoints = originalState.joints;
        using var cloneJoints = cloneState.joints;
        originalJoints[names[0]].ShouldBe(0);
        cloneJoints[names[0]].ShouldBe(0.2);
        using var link = new Link("clone_only");
        using var command = new AddLinkCommand(link);
        clone.applyCommand(command).ShouldBeTrue();
        original.getLink("clone_only").ShouldBeNull();
        using var added = clone.getLink("clone_only");
        added.getName().ShouldBe("clone_only");
        clone.getRevision().ShouldBe(original.getRevision() + 1);
    }

    [Fact]
    public void RedundantSolutionsAddFullTurnsWithinJointLimits()
    {
        double[] solution = [0.2, -0.1];
        double[,] limits = { { -7, 7 }, { -1, 1 } };
        using var indices = new IndexVector { 0 };
        using var redundant = TesseractNative.getRedundantSolutions(solution, limits, indices);
        redundant.Count.ShouldBe(2);
        redundant.Select(q => q[0]).Order().ShouldBe(new[] { 0.2 - 2 * Math.PI, 0.2 + 2 * Math.PI });
        foreach (var q in redundant)
        {
            q[1].ShouldBe(-0.1);
            q[0].ShouldBeInRange(-7, 7);
        }
        Should.Throw<ArgumentException>(() => TesseractNative.getRedundantSolutions(solution, new double[1, 2], indices));
        using var negative = new IndexVector { -1 };
        Should.Throw<IndexOutOfRangeException>(() => TesseractNative.getRedundantSolutions(solution, limits, negative));
    }

    [Fact]
    public void ContactManagerPluginsCanBeConfiguredThroughManagedCommands()
    {
        using var environment = StockPipelineTests.CreateEnvironment();
        using var command = CreateContactCommand();
        environment.applyCommand(command).ShouldBeTrue();
        environment.setActiveDiscreteContactManager("RuntimeBullet").ShouldBeTrue();
        using var manager = environment.getDiscreteContactManager();
        manager.getName().ShouldBe("RuntimeBullet");
        using var objects = manager.getCollisionObjects();
        objects.Count.ShouldBeGreaterThan(0);
    }

    private static AddContactManagersPluginInfoCommand CreateContactCommand()
    {
        using var info = new ContactManagersPluginInfo();
        using var plugins = info.discrete_plugin_infos;
        plugins.default_plugin = "RuntimeBullet";
        using var plugin = new PluginInfo { class_name = "BulletDiscreteBVHManagerFactory" };
        plugin.setConfigString("{}");
        plugins.addPlugin("RuntimeBullet", plugin);
        return new AddContactManagersPluginInfoCommand(info);
    }

    internal static TesseractEnvironment CreateUrEnvironment()
    {
        using var locator = new GeneralResourceLocator();
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "darp_test", "ur10.urdf");
        using var graph = TesseractNative.parseURDFString(File.ReadAllText(path), locator);
        using var srdf = new SRDFModel();
        srdf.initString(graph, """
            <robot name="ur10_test"><group name="manipulator">
              <chain base_link="base_link" tip_link="weld_tcp" />
            </group></robot>
            """, locator);
        var environment = new TesseractEnvironment();
        environment.init(graph, srdf).ShouldBeTrue();
        using var plugin = new PluginInfo { class_name = "URInvKinFactory" };
        plugin.setConfigString(UrConfig);
        using var plugins = new PluginInfoContainer { default_plugin = "RuntimeUR" };
        plugins.addPlugin("RuntimeUR", plugin);
        using var information = new KinematicsPluginInfo();
        information.setInvPluginInfo("manipulator", plugins);
        using var kinematics = new KinematicsInformation { kinematics_plugin_info = information };
        using var command = new AddKinematicsInformationCommand(kinematics);
        environment.applyCommand(command).ShouldBeTrue();
        return environment;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static TesseractEnvironment CreateDetachedClone()
    {
        using var source = CreateUrEnvironment();
        return source.clone();
    }

    private static void AssertUrRoundTrip(TesseractEnvironment environment)
    {
        using var group = environment.getKinematicGroup("manipulator", "RuntimeUR");
        using var inverse = group.getInverseKinematics();
        inverse.getSolverName().ShouldBe("RuntimeUR");
        double[] seed = [0.3, -1.1, 1.3, -0.7, 0.9, 0.4];
        using var poses = group.calcFwdKin(seed);
        var target = poses["weld_tcp"];
        using var input = new KinGroupIKInput(target, "base_link", "weld_tcp");
        using var inputs = new KinGroupIKInputs();
        inputs.push_back(input);
        input.Dispose();
        using var copiedInput = inputs.at(0);
        PosesMatch(target, copiedInput.pose).ShouldBeTrue();
        using var solutions = group.calcInvKinMultiple(inputs, seed);
        solutions.Count.ShouldBeGreaterThan(0);
        using var originalResult = new IKSolutions();
        var result = originalResult;
        group.calcInvKinMultiple(ref result, inputs, seed);
        using var updated = result;
        updated.Count.ShouldBe(solutions.Count);
        inputs.Dispose();
        copiedInput.tip_link_name.ShouldBe("weld_tcp");
        solutions.Any(q =>
        {
            using var candidate = group.calcFwdKin(q);
            return PosesMatch(target, candidate["weld_tcp"]);
        }).ShouldBeTrue();
    }

    private static bool PosesMatch(Euclidean3d expected, Euclidean3d actual)
    {
        var a = (M44d)expected;
        var b = (M44d)actual;
        for (var row = 0; row < 4; row++)
            for (var column = 0; column < 4; column++)
                if (Math.Abs(a[row, column] - b[row, column]) > 1e-6)
                    return false;
        return true;
    }
}
