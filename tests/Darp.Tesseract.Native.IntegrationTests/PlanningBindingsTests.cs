using Shouldly;
using Xunit;

namespace Darp.Tesseract.Native.IntegrationTests;

public sealed class PlanningBindingsTests
{
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
        pipelines.ShouldContain("FreespacePipeline");
    }
}
