%include <tesseract/common/types.h>
%include <tesseract/common/resource_locator.h>
%include <tesseract/common/manipulator_info.h>
%include "support/environment_values.i"
%template(JointStateVector) std::vector<tesseract::common::JointState>;
%include <tesseract/common/joint_state.h>
%include <tesseract/common/kinematic_limits.h>
%include <tesseract/common/plugin_info.h>
