%include <tesseract/common/types.h>
%template(ByteVector) std::vector<uint8_t>;
%include <tesseract/common/resource_locator.h>
%include <tesseract/common/manipulator_info.h>
%include "support/environment_values.i"
%template(JointStateVector) std::vector<tesseract::common::JointState>;
%include <tesseract/common/joint_state.h>
%include <tesseract/common/kinematic_limits.h>
%include <tesseract/common/plugin_info.h>

/* YAML nodes and nested maps stay native; these helpers copy configuration. */
%{
namespace
{
tesseract::common::PluginInfoContainer darpCopyPluginContainer(const tesseract::common::PluginInfoContainer& source)
{
  auto result = source;
  for (auto& entry : result.plugins)
    entry.second.config.reset(YAML::Clone(entry.second.config));
  return result;
}
}
%}
%extend tesseract::common::PluginInfo
{
  void setConfigString(const std::string& yaml)
  {
    $self->config.reset(YAML::Load(yaml));
  }
}
%extend tesseract::common::PluginInfoContainer
{
  void addPlugin(const std::string& name, const tesseract::common::PluginInfo& plugin)
  {
    auto& stored = $self->plugins[name];
    stored.class_name = plugin.class_name;
    stored.config.reset(YAML::Clone(plugin.config));
  }
  tesseract::common::PluginInfo getPlugin(const std::string& name) const
  {
    auto result = $self->plugins.at(name);
    result.config.reset(YAML::Clone(result.config));
    return result;
  }
}
%extend tesseract::common::KinematicsPluginInfo
{
  void setFwdPluginInfo(const std::string& group, const tesseract::common::PluginInfoContainer& plugins)
  {
    $self->fwd_plugin_infos[group] = darpCopyPluginContainer(plugins);
  }
  void setInvPluginInfo(const std::string& group, const tesseract::common::PluginInfoContainer& plugins)
  {
    $self->inv_plugin_infos[group] = darpCopyPluginContainer(plugins);
  }
  tesseract::common::PluginInfoContainer getFwdPluginInfo(const std::string& group) const
  {
    return darpCopyPluginContainer($self->fwd_plugin_infos.at(group));
  }
  tesseract::common::PluginInfoContainer getInvPluginInfo(const std::string& group) const
  {
    return darpCopyPluginContainer($self->inv_plugin_infos.at(group));
  }
}
