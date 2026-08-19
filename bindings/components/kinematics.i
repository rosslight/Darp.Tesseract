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
%include <tesseract/kinematics/kinematics_plugin_factory.h>
