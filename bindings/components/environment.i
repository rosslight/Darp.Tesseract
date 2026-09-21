/* Commands share native ownership with the environment and batch containers. */
/* Only concrete commands can be constructed: native dispatch casts by type. */
%ignore tesseract::environment::Command::Command;
%include <tesseract/environment/command.h>
%template(CommandVector) std::vector<std::shared_ptr<const tesseract::environment::Command>>;

%include <tesseract/environment/commands/add_contact_managers_plugin_info_command.h>
%include <tesseract/environment/commands/add_kinematics_information_command.h>
%include <tesseract/environment/commands/add_link_command.h>
%include <tesseract/environment/commands/add_scene_graph_command.h>
%include <tesseract/environment/commands/add_trajectory_link_command.h>
%include <tesseract/environment/commands/change_collision_margins_command.h>
%include <tesseract/environment/commands/change_joint_acceleration_limits_command.h>
%include <tesseract/environment/commands/change_joint_origin_command.h>
%include <tesseract/environment/commands/change_joint_position_limits_command.h>
%include <tesseract/environment/commands/change_joint_velocity_limits_command.h>
%include <tesseract/environment/commands/change_link_collision_enabled_command.h>
%include <tesseract/environment/commands/change_link_origin_command.h>
%include <tesseract/environment/commands/change_link_visibility_command.h>
%include <tesseract/environment/commands/modify_allowed_collisions_command.h>
%include <tesseract/environment/commands/move_joint_command.h>
%include <tesseract/environment/commands/move_link_command.h>
%include <tesseract/environment/commands/remove_allowed_collision_link_command.h>
%include <tesseract/environment/commands/remove_joint_command.h>
%include <tesseract/environment/commands/remove_link_command.h>
%include <tesseract/environment/commands/replace_joint_command.h>
%include <tesseract/environment/commands/set_active_continuous_contact_manager_command.h>
%include <tesseract/environment/commands/set_active_discrete_contact_manager_command.h>

%include <tesseract/environment/environment.h>
