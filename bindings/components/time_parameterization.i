/* Standalone retiming uses the same native profiles as Task Composer. */
%shared_ptr(tesseract::time_parameterization::TimeParameterization)
%shared_ptr(tesseract::time_parameterization::IterativeSplineParameterization)
%shared_ptr(tesseract::time_parameterization::TimeOptimalTrajectoryGeneration)
%shared_ptr(tesseract::time_parameterization::ConstantTCPSpeedParameterization)
%ignore tesseract::time_parameterization::TimeParameterization::TimeParameterization;
%include <tesseract/time_parameterization/time_parameterization.h>
%include <tesseract/time_parameterization/isp/iterative_spline_parameterization.h>
/* TOTG's internal path solver is not part of the managed retiming API. */
%ignore tesseract::time_parameterization::totg::PathSegment;
%ignore tesseract::time_parameterization::totg::Path;
%ignore tesseract::time_parameterization::totg::PathData;
%ignore tesseract::time_parameterization::totg::Trajectory;
%include <tesseract/time_parameterization/totg/time_optimal_trajectory_generation.h>
%include <tesseract/time_parameterization/kdl/constant_tcp_speed_parameterization.h>

%shared_ptr(tesseract::time_parameterization::TimeOptimalTrajectoryGenerationCompositeProfile)
%shared_ptr(tesseract::time_parameterization::ConstantTCPSpeedParameterizationCompositeProfile)
%ignore tesseract::time_parameterization::TimeOptimalTrajectoryGenerationCompositeProfile::operator==;
%ignore tesseract::time_parameterization::TimeOptimalTrajectoryGenerationCompositeProfile::operator!=;
%ignore tesseract::time_parameterization::ConstantTCPSpeedParameterizationCompositeProfile::operator==;
%ignore tesseract::time_parameterization::ConstantTCPSpeedParameterizationCompositeProfile::operator!=;
%include <tesseract/time_parameterization/totg/time_optimal_trajectory_generation_profiles.h>
%include <tesseract/time_parameterization/kdl/constant_tcp_speed_parameterization_profiles.h>

/* No borrowed instruction or Eigen references survive a managed call. */
%typemap(csclassmodifiers) DarpInstructionsTrajectoryInterop "internal class";
%nodefaultctor DarpInstructionsTrajectoryInterop;
%nodefaultdtor DarpInstructionsTrajectoryInterop;
%inline %{
class DarpInstructionsTrajectoryInterop
{
public:
  static int size(tesseract::command_language::CompositeInstruction& program)
  {
    const auto count = program.flatten(tesseract::command_language::moveFilter).size();
    if (count > static_cast<std::size_t>(std::numeric_limits<int>::max()))
      throw std::overflow_error("Trajectory size exceeds Int32.");
    return static_cast<int>(count);
  }

  static int dof(tesseract::command_language::CompositeInstruction& program)
  {
    return static_cast<int>(tesseract::time_parameterization::InstructionsTrajectory(program).dof());
  }

  static Eigen::VectorXd getPosition(tesseract::command_language::CompositeInstruction& program, int i)
  {
    return checked(program, i).getPosition(i);
  }

  static Eigen::VectorXd getVelocity(tesseract::command_language::CompositeInstruction& program, int i)
  {
    return checked(program, i).getVelocity(i);
  }

  static Eigen::VectorXd getAcceleration(tesseract::command_language::CompositeInstruction& program, int i)
  {
    return checked(program, i).getAcceleration(i);
  }

  static double getTimeFromStart(tesseract::command_language::CompositeInstruction& program, int i)
  {
    return checked(program, i).getTimeFromStart(i);
  }

  static void setData(tesseract::command_language::CompositeInstruction& program,
                      int i, const Eigen::VectorXd& velocity, const Eigen::VectorXd& acceleration, double time)
  {
    auto trajectory = checked(program, i);
    if (velocity.size() != trajectory.dof() || acceleration.size() != trajectory.dof())
      throw std::invalid_argument("Velocity and acceleration must match the trajectory's joint count.");
    trajectory.setData(i, velocity, acceleration, time);
  }

  static bool isTimeStrictlyIncreasing(tesseract::command_language::CompositeInstruction& program)
  {
    if (size(program) < 2)
      return true;
    const tesseract::time_parameterization::InstructionsTrajectory trajectory(program);
    // The upstream 0.35 check omits the final waypoint.
    for (Eigen::Index i = 1; i < trajectory.size(); ++i)
      if (!(trajectory.getTimeFromStart(i) > trajectory.getTimeFromStart(i - 1)))
        return false;
    return true;
  }

private:
  static tesseract::time_parameterization::InstructionsTrajectory checked(
      tesseract::command_language::CompositeInstruction& program, int i)
  {
    if (i < 0 || i >= size(program))
      throw std::out_of_range("Trajectory index is out of range.");
    return tesseract::time_parameterization::InstructionsTrajectory(program);
  }
};
%}
