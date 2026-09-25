using Shouldly;
using Xunit;

namespace Darp.Tesseract.Native.IntegrationTests;

public sealed class PlanningBindingsTests
{
    [Fact]
    public void StockPlannerProfilesCanBeAddedToTheProfileDictionary()
    {
        using var profiles = new ProfileDictionary();
        using var ompl = new OMPLRealVectorMoveProfile();
        using var trajOptMove = new TrajOptDefaultMoveProfile();
        using var trajOptComposite = new TrajOptDefaultCompositeProfile();
        using var trajOptSolver = new TrajOptOSQPSolverProfile();
        using var contactCheck = new ContactCheckProfile();

        profiles.addProfile("OMPLMotionPlannerTask", "DEFAULT", ompl);
        profiles.addProfile("TrajOptMotionPlannerTask", "DEFAULT", trajOptMove);
        profiles.addProfile("TrajOptMotionPlannerTask", "DEFAULT", trajOptComposite);
        profiles.addProfile("TrajOptMotionPlannerTask", "DEFAULT", trajOptSolver);
        profiles.addProfile("DiscreteContactCheckTask", "DEFAULT", contactCheck);
    }

    [Fact]
    public void PlannerProfilesExposeTheirNativeConfiguration()
    {
        using var ompl = new OMPLRealVectorMoveProfile();
        using OMPLSolverConfig omplSolver = ompl.solver_config;
        using OMPLPlannerConfiguratorVector planners = omplSolver.planners;
        using var rrtConnect = new RRTConnectConfigurator { range = 0.25 };
        planners.Clear();
        planners.Add(rrtConnect);
        omplSolver.planning_time = 2.5;

        using ContactManagerConfig contactManager = ompl.contact_manager_config;
        using var margin = new OptionalDouble(0.02);
        contactManager.default_margin = margin;
        using StringBoolMap enabledObjects = contactManager.modify_object_enabled;
        enabledObjects["fixture"] = false;

        using CollisionCheckConfig collisionCheck = ompl.collision_check_config;
        collisionCheck.type = CollisionEvaluatorType.LVS_CONTINUOUS;
        collisionCheck.longest_valid_segment_length = 0.01;

        using var trajOptMove = new TrajOptDefaultMoveProfile();
        using TrajOptCartesianWaypointConfig cartesianCost = trajOptMove.cartesian_cost_config;
        cartesianCost.coeff = [1, 2, 3, 4, 5, 6];
        cartesianCost.use_tolerance_override = true;

        using var trajOptComposite = new TrajOptDefaultCompositeProfile();
        using TrajOptCollisionConfig collisionCost = trajOptComposite.collision_cost_config;
        using CollisionCoeffData collisionCoefficients = collisionCost.collision_coeff_data;
        collisionCoefficients.setCollisionCoeff("tool", "fixture", 7.5);

        using var trajOptSolver = new TrajOptOSQPSolverProfile();
        using BasicTrustRegionSQPParameters sqp = trajOptSolver.opt_params;
        using OSQPSettings osqp = trajOptSolver.settings;
        sqp.max_iter = 25;
        osqp.max_iter = 1_000;

        omplSolver.planning_time.ShouldBe(2.5);
        planners.Single().getType().ShouldBe(OMPLPlannerType.RRTConnect);
        contactManager.default_margin.value().ShouldBe(0.02);
        enabledObjects["fixture"].ShouldBeFalse();
        collisionCheck.type.ShouldBe(CollisionEvaluatorType.LVS_CONTINUOUS);
        collisionCheck.longest_valid_segment_length.ShouldBe(0.01);
        cartesianCost.coeff.ShouldBe([1, 2, 3, 4, 5, 6]);
        cartesianCost.use_tolerance_override.ShouldBeTrue();
        collisionCoefficients.getCollisionCoeff("fixture", "tool").ShouldBe(7.5);
        sqp.max_iter.ShouldBe(25);
        osqp.max_iter.ShouldBe(1_000);
    }

    [Fact]
    public void PlanningWaypointsPreserveJointAndTrajectoryData()
    {
        using var names = new StringVector { "joint_1", "joint_2" };
        using var joint = new JointWaypoint(names, [1.0, 2.0], [-0.1, -0.2], [0.1, 0.2]);
        using var wrappedJoint = new WaypointPoly(joint);
        using JointWaypointPoly jointWaypoint = TesseractNative.asJointWaypoint(wrappedJoint);

        TesseractNative.jointPosition(jointWaypoint).ShouldBe([1.0, 2.0]);
        TesseractNative.jointLowerTolerance(jointWaypoint).ShouldBe([-0.1, -0.2]);
        TesseractNative.jointUpperTolerance(jointWaypoint).ShouldBe([0.1, 0.2]);

        using var state = new StateWaypoint(names, [3.0, 4.0], [0.3, 0.4], [0.03, 0.04], 1.5);
        using var wrappedState = new WaypointPoly(state);
        using StateWaypointPoly stateWaypoint = TesseractNative.asStateWaypoint(wrappedState);

        TesseractNative.statePosition(stateWaypoint).ShouldBe([3.0, 4.0]);
        TesseractNative.stateVelocity(stateWaypoint).ShouldBe([0.3, 0.4]);
        TesseractNative.stateAcceleration(stateWaypoint).ShouldBe([0.03, 0.04]);
        stateWaypoint.getTime().ShouldBe(1.5);
    }

    [Fact]
    public void TaskComposerDiagnosticsExposeNodeFailures()
    {
        using var container = new TaskComposerNodeInfoContainer();
        using var node = new TaskComposerNodeInfo
        {
            name = "CollisionCheckTask",
            status_code = 0,
            status_message = "Trajectory is in collision",
        };
        container.addInfo(node);

        using TaskComposerNodeInfoVector infos = TesseractNative.getTaskComposerNodeInfos(container);
        using TaskComposerNodeInfo actual = infos.Single();

        actual.name.ShouldBe("CollisionCheckTask");
        actual.status_code.ShouldBe(0);
        actual.status_message.ShouldBe("Trajectory is in collision");
        TesseractNative.getAbortingTaskComposerNodeInfo(container).ShouldBeNull();
    }

    [Fact]
    public void ConfiguredTaskComposerPipelinesCanBeEnumerated()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "Tesseract", "task_composer_plugins.yaml");
        using var locator = new GeneralResourceLocator();
        using TaskComposerPluginFactory factory =
            TesseractNative.createTaskComposerPluginFactory(configPath, locator)
            ?? throw new InvalidOperationException("Tesseract did not create the Task Composer plugin factory.");
        using StringVector pipelines = TesseractNative.getConfiguredTaskComposerNodeNames(factory);

        pipelines.ShouldContain("DescartesDPipeline");
        pipelines.ShouldContain("OMPLPipeline");
        pipelines.ShouldContain("FreespacePipeline");

        using TaskComposerNode ompl = factory.createTaskComposerNode("OMPLPipeline");
        using TaskComposerNode freespace = factory.createTaskComposerNode("FreespacePipeline");
        ompl.ShouldNotBeNull();
        freespace.ShouldNotBeNull();
    }
}
