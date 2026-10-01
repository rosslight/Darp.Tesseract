using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;
using TesseractEnvironment = Darp.Tesseract.Native.Environment;

namespace Darp.Tesseract.Native.IntegrationTests;

public sealed class TimeParameterizationTests
{
    private const string ProfileName = "RETIME";

    [Theory]
    [InlineData("ISP")]
    [InlineData("TOTG")]
    [InlineData("ConstantTCP")]
    public void StandaloneRetimingScalesTimesVelocitiesAndAccelerations(string kind)
    {
        using var environment = StockPipelineTests.CreateEnvironment();
        using var nominal = CreateProgram(environment);
        using var scaled = CreateProgram(environment);
        using TimeParameterization solver = kind switch
        {
            "ISP" => new IterativeSplineParameterization("retime"),
            "TOTG" => new TimeOptimalTrajectoryGeneration("retime"),
            _ => new ConstantTCPSpeedParameterization("retime"),
        };
        using var nominalProfiles = CreateProfiles(kind, solver.getName(), 1);
        using var scaledProfiles = CreateProfiles(kind, solver.getName(), 0.5);
        var nominalView = new InstructionsTrajectory(nominal);
        var scaledView = new InstructionsTrajectory(scaled);

        solver.compute(nominal, environment, nominalProfiles).ShouldBeTrue();
        solver.compute(scaled, environment, scaledProfiles).ShouldBeTrue();
        AssertTimedTrajectory(nominalView);
        AssertTimedTrajectory(scaledView);
        scaledView.size().ShouldBe(nominalView.size());
        var duration = nominalView.getTimeFromStart(nominalView.size() - 1);
        scaledView.getTimeFromStart(scaledView.size() - 1).ShouldBe(duration * 2, duration * 0.06);

        var nominalVelocity = PeakDerivative(nominalView, acceleration: false);
        var nominalAcceleration = PeakDerivative(nominalView, acceleration: true);
        nominalVelocity.ShouldBeGreaterThan(1e-6);
        nominalAcceleration.ShouldBeGreaterThan(1e-6);
        PeakDerivative(scaledView, acceleration: false).ShouldBe(nominalVelocity * 0.5, nominalVelocity * 0.06);
        PeakDerivative(scaledView, acceleration: true).ShouldBe(nominalAcceleration * 0.25, nominalAcceleration * 0.06);
        for (var i = 0; i < nominalView.size(); i++)
            scaledView.getPosition(i).ShouldBe(nominalView.getPosition(i));
    }

    [Fact]
    public void CustomTotgPipelinePlansAndRetimes()
    {
        using var environment = StockPipelineTests.CreateEnvironment();
        using var program = CreateProgram(environment);
        using var profiles = CreateProfiles("TOTG", StockProfileNamespaces.TimeOptimalParameterization, 0.5);
        using var result = StockPipelineTests.Run(
            environment, profiles, program, "TOTGPipeline",
            Path.Combine(AppContext.BaseDirectory, "Assets", "darp_test", "timing_tasks.yaml"), "input_data"
        );
        AssertTimedTrajectory(new InstructionsTrajectory(result));
    }

    [Fact]
    public void TrajectoryViewHandlesStructuralChangesAndRejectsDisposedPrograms()
    {
        using var environment = StockPipelineTests.CreateEnvironment();
        using var program = CreateProgram(environment, count: 2);
        var trajectory = new InstructionsTrajectory(program);
        trajectory.setData(0, new double[6], new double[6], 0);
        trajectory.setData(1, new double[6], new double[6], 1);
        trajectory.isTimeStrictlyIncreasing().ShouldBeTrue();
        trajectory.setData(1, new double[6], new double[6], 0);
        trajectory.isTimeStrictlyIncreasing().ShouldBeFalse();
        var position = trajectory.getPosition(0);
        position[0] = 999;
        trajectory.getPosition(0)[0].ShouldNotBe(999);
        Should.Throw<IndexOutOfRangeException>(() => trajectory.getPosition(-1));
        Should.Throw<IndexOutOfRangeException>(() => trajectory.getPosition(2));
        Should.Throw<ArgumentException>(() => trajectory.setData(0, [1], [1], 0));

        using var group = environment.getJointGroup("manipulator");
        using var names = group.getJointNames();
        for (var i = 0; i < 100; i++)
            AppendState(program, names, new double[6]);
        trajectory.size().ShouldBe(102);
        trajectory.getPosition(101).ShouldBe(new double[6]);
        program.clear();
        trajectory.empty().ShouldBeTrue();
        Should.Throw<IndexOutOfRangeException>(() => trajectory.getPosition(0));
        program.Dispose();
        Should.Throw<ObjectDisposedException>(() => trajectory.size());
    }

    [Fact]
    public void TrajectoryViewRetainsItsProgramAcrossGarbageCollection()
    {
        using var environment = StockPipelineTests.CreateEnvironment();
        var trajectory = CreateRetainedView(environment);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        trajectory.size().ShouldBe(21);
        trajectory.getPosition(20).Length.ShouldBe(6);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static InstructionsTrajectory CreateRetainedView(TesseractEnvironment environment) =>
        new(CreateProgram(environment));

    private static ProfileDictionary CreateProfiles(string kind, string profileNamespace, double scale)
    {
        var profiles = new ProfileDictionary();
        using Profile profile = kind switch
        {
            "ISP" => new IterativeSplineParameterizationCompositeProfile(scale, scale * scale),
            "TOTG" => new TimeOptimalTrajectoryGenerationCompositeProfile(scale, scale * scale, 0.01, 0.001),
            _ => new ConstantTCPSpeedParameterizationCompositeProfile(0.1, 0.3, 0.2, 0.6, scale, scale * scale),
        };
        profiles.addProfile(profileNamespace, ProfileName, profile);
        return profiles;
    }

    private static void AssertTimedTrajectory(InstructionsTrajectory trajectory)
    {
        trajectory.dof().ShouldBe(6);
        trajectory.getTimeFromStart(trajectory.size() - 1).ShouldBeGreaterThan(0);
        trajectory.isTimeStrictlyIncreasing().ShouldBeTrue();
        for (var i = 0; i < trajectory.size(); i++)
        {
            double.IsFinite(trajectory.getTimeFromStart(i)).ShouldBeTrue();
            var velocity = trajectory.getVelocity(i);
            var acceleration = trajectory.getAcceleration(i);
            velocity.Length.ShouldBe(6);
            acceleration.Length.ShouldBe(6);
            velocity.All(double.IsFinite).ShouldBeTrue();
            acceleration.All(double.IsFinite).ShouldBeTrue();
        }
    }

    private static double PeakDerivative(InstructionsTrajectory trajectory, bool acceleration) =>
        Enumerable.Range(0, trajectory.size())
            .SelectMany(i => acceleration ? trajectory.getAcceleration(i) : trajectory.getVelocity(i))
            .Max(Math.Abs);

    private static CompositeInstruction CreateProgram(TesseractEnvironment environment, int count = 21)
    {
        using var manipulator = new ManipulatorInfo("manipulator", "base_link", "tool0");
        var program = new CompositeInstruction(ProfileName, manipulator);
        using var group = environment.getJointGroup("manipulator");
        using var names = group.getJointNames();
        for (var i = 0; i < count; i++)
        {
            var fraction = (double)i / (count - 1);
            AppendState(program, names, [0.1 + fraction * 0.3, -0.4 + fraction * 0.2,
                0.3 - fraction * 0.1, 0.2 + fraction * 0.2, 0.3 + fraction * 0.1, 0.1 + fraction * 0.2]);
        }
        return program;
    }

    private static void AppendState(CompositeInstruction program, StringVector names, double[] position)
    {
        using var state = new StateWaypoint(names, position, new double[6], new double[6], 0);
        using var waypoint = new WaypointPoly(state);
        using var move = new MoveInstruction(waypoint, MoveInstructionType.FREESPACE, ProfileName);
        TesseractNative.appendMoveInstruction(program, move);
    }
}
