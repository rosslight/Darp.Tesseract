namespace Darp.Tesseract.Native;

/// <summary>A mutable trajectory view over the state waypoints in a program.</summary>
/// <remarks>
/// Retains the program and resolves its current instructions on every call.
/// Returned vectors are copies. Disposing the program invalidates the view.
/// Program mutation and disposal must not run concurrently with view operations.
/// </remarks>
public sealed class InstructionsTrajectory
{
    private readonly CompositeInstruction _program;

    public InstructionsTrajectory(CompositeInstruction program)
    {
        ArgumentNullException.ThrowIfNull(program);
        _program = program;
    }

    public int size() => DarpInstructionsTrajectoryInterop.size(Program);
    public int dof() => DarpInstructionsTrajectoryInterop.dof(Program);
    public bool empty() => size() == 0;
    public double[] getPosition(int i) => DarpInstructionsTrajectoryInterop.getPosition(Program, i);
    public double[] getVelocity(int i) => DarpInstructionsTrajectoryInterop.getVelocity(Program, i);
    public double[] getAcceleration(int i) => DarpInstructionsTrajectoryInterop.getAcceleration(Program, i);
    public double getTimeFromStart(int i) => DarpInstructionsTrajectoryInterop.getTimeFromStart(Program, i);
    public bool isTimeStrictlyIncreasing() => DarpInstructionsTrajectoryInterop.isTimeStrictlyIncreasing(Program);

    public void setData(int i, double[] velocity, double[] acceleration, double time) =>
        DarpInstructionsTrajectoryInterop.setData(Program, i, velocity, acceleration, time);

    private CompositeInstruction Program
    {
        get
        {
            ObjectDisposedException.ThrowIf(CompositeInstruction.getCPtr(_program).Handle == IntPtr.Zero, _program);
            return _program;
        }
    }
}
