using Shouldly;
using Xunit;
using TesseractEnvironment = Darp.Tesseract.Native.Environment;

namespace Darp.Tesseract.Native.IntegrationTests;

public sealed class StockPipelineTests
{
    private const string ProfileName = "CUSTOM";

    [Fact]
    public void DescartesPipelineUsesCompositeAndMoveTimeScalingAfterProfileDisposal()
    {
        using var environment = CreateEnvironment();
        using var program = CreateProgram(environment);
        var nominal = RunDescartes(environment, program, 1, null);
        var compositeScaled = RunDescartes(environment, program, 0.5, null);
        var moveScaled = RunDescartes(environment, program, 1, 0.5);

        nominal.ShouldBeGreaterThan(0);
        compositeScaled.ShouldBeGreaterThan(nominal * 1.3);
        moveScaled.ShouldBeGreaterThan(nominal * 1.3);
    }

    [Fact]
    public void SimplePlannerUsesFixedSizeAndLongestValidSegmentProfiles()
    {
        using var environment = CreateEnvironment();
        using var program = CreateProgram(environment);
        using var fixedSmall = new SimplePlannerFixedSizeMoveProfile(3, 3);
        using var fixedLarge = new SimplePlannerFixedSizeMoveProfile(12, 12);
        using var coarse = new SimplePlannerLVSNoIKMoveProfile(0.1);
        using var fine = new SimplePlannerLVSNoIKMoveProfile(0.01);

        RunSimple(environment, program, fixedLarge).ShouldBeGreaterThan(RunSimple(environment, program, fixedSmall));
        RunSimple(environment, program, fine).ShouldBeGreaterThan(RunSimple(environment, program, coarse));
    }

    [Theory]
    [InlineData("OMPLPipeline")]
    [InlineData("TrajOptPipeline")]
    [InlineData("FreespacePipeline")]
    public void StockPipelinesRunWithRegisteredOmplAndTrajOptProfiles(string pipeline)
    {
        using var environment = CreateEnvironment();
        using var program = CreateProgram(environment);
        using var profiles = new ProfileDictionary();
        using var ompl = new OMPLRealVectorMoveProfile();
        using var trajOptMove = new TrajOptDefaultMoveProfile();
        using var trajOptComposite = new TrajOptDefaultCompositeProfile();
        using var trajOptSolver = new TrajOptOSQPSolverProfile();
        using OMPLSolverConfig solver = ompl.solver_config;
        solver.planning_time = 1;
        profiles.addProfile(StockProfileNamespaces.OmplMotionPlanner, ProfileName, ompl);
        profiles.addProfile(StockProfileNamespaces.TrajOptMotionPlanner, ProfileName, trajOptMove);
        profiles.addProfile(StockProfileNamespaces.TrajOptMotionPlanner, ProfileName, trajOptComposite);
        profiles.addProfile(StockProfileNamespaces.TrajOptMotionPlanner, ProfileName, trajOptSolver);

        using var result = Run(environment, profiles, program, pipeline);
        TesseractNative.instructionCount(result).ShouldBeGreaterThan(1u);
        using var last = result.getLastMoveInstruction();
        using var waypoint = last.getWaypoint();
        using var state = TesseractNative.asStateWaypoint(waypoint);
        state.getTime().ShouldBeGreaterThan(0);
    }

    private static double RunDescartes(
        TesseractEnvironment environment,
        CompositeInstruction program,
        double compositeScale,
        double? moveScale
    )
    {
        using var profiles = new ProfileDictionary();
        using (var composite = new IterativeSplineParameterizationCompositeProfile(compositeScale, compositeScale))
            profiles.addProfile(StockProfileNamespaces.IterativeSplineParameterization, ProfileName, composite);
        if (moveScale is { } scale)
        {
            using var move = new IterativeSplineParameterizationMoveProfile(scale, scale);
            profiles.addProfile(StockProfileNamespaces.IterativeSplineParameterization, ProfileName, move);
        }

        using var result = Run(environment, profiles, program, "DescartesDPipeline");
        using var last = result.getLastMoveInstruction();
        using var waypoint = last.getWaypoint();
        using var state = TesseractNative.asStateWaypoint(waypoint);
        return state.getTime();
    }

    private static uint RunSimple(TesseractEnvironment environment, CompositeInstruction program, Profile profile)
    {
        using var profiles = new ProfileDictionary();
        profiles.addProfile(StockProfileNamespaces.SimpleMotionPlanner, ProfileName, profile);
        using var result = Run(
            environment,
            profiles,
            program,
            "SimpleMotionPlannerTask",
            Path.Combine(AppContext.BaseDirectory, "Assets", "darp_test", "profile_tasks.yaml"),
            "input_data"
        );
        return TesseractNative.instructionCount(result);
    }

    private static CompositeInstruction Run(
        TesseractEnvironment environment,
        ProfileDictionary profiles,
        CompositeInstruction program,
        string taskName,
        string? configPath = null,
        string inputKey = "planning_input"
    )
    {
        configPath ??= Path.Combine(AppContext.BaseDirectory, "Tesseract", "task_composer_plugins.yaml");
        using var locator = new GeneralResourceLocator();
        using var factory = TesseractNative.createTaskComposerPluginFactory(configPath, locator);
        using var task = factory.createTaskComposerNode(taskName);
        using var executor = factory.createTaskComposerExecutor("TaskflowExecutor");
        using var storage = new TaskComposerDataStorage();
        using var input = TesseractNative.wrapCompositeInstruction(program);
        using var wrappedEnvironment = TesseractNative.wrapEnvironment(environment);
        using var wrappedProfiles = TesseractNative.wrapProfileDictionary(profiles);
        TesseractNative.setData(storage, inputKey, input);
        TesseractNative.setData(storage, "environment", wrappedEnvironment);
        TesseractNative.setData(storage, "profiles", wrappedProfiles);
        using var context = TesseractNative.createTaskComposerContext(taskName, storage);
        using var future = executor.run(task, context);
        future.wait();

        using var infos = context.task_infos;
        using var nodes = TesseractNative.getTaskComposerNodeInfos(infos);
        List<string> failures = [];
        foreach (var node in nodes)
        {
            using (node)
            {
                if (node.status_code == 0)
                    failures.Add($"{node.name}: {node.status_message}");
            }
        }
        context.isSuccessful().ShouldBeTrue(string.Join(System.Environment.NewLine, failures));
        failures.ShouldBeEmpty();
        using var output = TesseractNative.getContextData(context, "output_data");
        return TesseractNative.asCompositeInstruction(output);
    }

    private static CompositeInstruction CreateProgram(TesseractEnvironment environment)
    {
        using var manipulator = new ManipulatorInfo("manipulator", "base_link", "tool0");
        var program = new CompositeInstruction(ProfileName, manipulator);
        using var group = environment.getJointGroup("manipulator");
        using var names = group.getJointNames();
        using var start = new StateWaypoint(names, new double[6]);
        using var startWaypoint = new WaypointPoly(start);
        using var startMove = new MoveInstruction(startWaypoint, MoveInstructionType.FREESPACE, ProfileName);
        TesseractNative.appendMoveInstruction(program, startMove);
        using var end = new JointWaypoint(names, [0.3, -0.1, 0.1, 0, 0.2, 0]);
        using var endWaypoint = new WaypointPoly(end);
        using var endMove = new MoveInstruction(endWaypoint, MoveInstructionType.FREESPACE, ProfileName);
        TesseractNative.appendMoveInstruction(program, endMove);
        return program;
    }

    private static TesseractEnvironment CreateEnvironment()
    {
        var assetRoot = Path.Combine(AppContext.BaseDirectory, "Assets");
        var fixtureRoot = Path.Combine(assetRoot, "darp_test");
        using var locator = new GeneralResourceLocator();
        locator.addPath(assetRoot).ShouldBeTrue();
        using var sceneGraph = TesseractNative.parseURDFString(
            File.ReadAllText(Path.Combine(fixtureRoot, "abb_irb2400.urdf")), locator
        );
        using var srdf = new SRDFModel();
        srdf.initString(sceneGraph, File.ReadAllText(Path.Combine(fixtureRoot, "abb_irb2400.srdf")), locator);
        var environment = new TesseractEnvironment();
        environment.init(sceneGraph, srdf).ShouldBeTrue();
        return environment;
    }
}
