/* types.h contains initialized namespace constants that SWIG cannot parse, so
 * declare its public UR parameter value type equivalently. */
namespace tesseract
{
namespace kinematics
{
struct URParameters
{
  URParameters();
  URParameters(double d1, double a2, double a3, double d4, double d5, double d6);
  double d1;
  double a2;
  double a3;
  double d4;
  double d5;
  double d6;
};
}
}
%include <tesseract/kinematics/forward_kinematics.h>
%include <tesseract/kinematics/inverse_kinematics.h>
%include <tesseract/kinematics/joint_group.h>
%include <tesseract/kinematics/kinematic_group.h>
/* Model the aligned native vector without exposing its allocator or references. */
namespace tesseract::common
{
template <typename T> class AlignedVector
{
public:
  AlignedVector();
  void push_back(const T& value);
  T at(std::size_t index) const;
  std::size_t size() const;
  void clear();
};
}
%template(KinGroupIKInputs) tesseract::common::AlignedVector<tesseract::kinematics::KinGroupIKInput>;

%inline %{
namespace tesseract::kinematics
{
IKSolutions getRedundantSolutions(const Eigen::VectorXd& solution,
                                 const Eigen::MatrixX2d& limits,
                                 const std::vector<Eigen::Index>& indices)
{
  if (limits.rows() != solution.size())
    throw std::invalid_argument("Joint limits must have one row per joint.");
  for (const auto index : indices)
    if (index < 0 || index >= solution.size())
      throw std::out_of_range("Redundant joint index is out of range.");
  return getRedundantSolutions<double>(solution, limits, indices);
}
}
%}
%include <tesseract/kinematics/kinematics_plugin_factory.h>
