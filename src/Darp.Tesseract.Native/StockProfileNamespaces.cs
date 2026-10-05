namespace Darp.Tesseract.Native;

/// <summary>
/// Default profile namespaces used by Tesseract's stock Task Composer nodes.
/// A node's configured namespace takes precedence when it is customized.
/// </summary>
public static class StockProfileNamespaces
{
    public const string DescartesMotionPlanner = "DescartesMotionPlannerTask";
    public const string OmplMotionPlanner = "OMPLMotionPlannerTask";
    public const string TrajOptMotionPlanner = "TrajOptMotionPlannerTask";
    public const string SimpleMotionPlanner = "SimpleMotionPlannerTask";
    public const string IterativeSplineParameterization = "IterativeSplineParameterizationTask";
    public const string TimeOptimalParameterization = "TimeOptimalParameterizationTask";
    public const string ConstantTCPSpeedParameterization = "ConstantTCPSpeedParameterizationTask";
    public const string UpsampleTrajectory = "UpsampleTrajectoryTask";
    public const string MinLength = "MinLengthTask";
    public const string KinematicLimitsCheck = "KinematicLimitsCheckTask";
    public const string FixStateBounds = "FixStateBoundsTask";
    public const string FixStateCollision = "FixStateCollisionTask";
    public const string ProfileSwitch = "ProfileSwitchTask";
    public const string DiscreteContactCheck = "DiscreteContactCheckTask";
    public const string ContinuousContactCheck = "ContinuousContactCheckTask";
}
