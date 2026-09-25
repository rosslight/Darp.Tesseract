/* Upstream Tesseract Planning types used by the high-level C# API. The small
 * helpers below are the same type-erasure bridge required by the official
 * Python bindings; planning remains implemented by Tesseract itself. */

%ignore tesseract::common::Profile::getKey;
%ignore tesseract::common::ProfileDictionary::getProfile;
%ignore tesseract::common::ProfileDictionary::getProfileEntry;
%ignore tesseract::common::ProfileDictionary::getAllProfileEntries;
%shared_ptr(tesseract::common::Profile)
%shared_ptr(tesseract::common::ProfileDictionary)
%include <tesseract/common/profile.h>
%include <tesseract/common/profile_dictionary.h>

namespace tesseract::common
{
class AnyPoly {};
}

%ignore tesseract::command_language::WaypointPoly::getType;
%ignore tesseract::command_language::WaypointPoly::getTypeErased;
%ignore tesseract::command_language::InstructionPoly::getType;
%ignore tesseract::command_language::InstructionPoly::getTypeErased;
%ignore tesseract::command_language::MoveInstructionPoly::getType;
%ignore tesseract::command_language::MoveInstructionPoly::getTypeErased;
%ignore tesseract::command_language::MoveInstructionPoly::createCartesianWaypoint;
%ignore tesseract::command_language::MoveInstructionPoly::createJointWaypoint;
%ignore tesseract::command_language::MoveInstructionPoly::createStateWaypoint;
%ignore tesseract::command_language::MoveInstructionInterface::createCartesianWaypoint;
%ignore tesseract::command_language::MoveInstructionInterface::createJointWaypoint;
%ignore tesseract::command_language::MoveInstructionInterface::createStateWaypoint;
%ignore tesseract::command_language::StateWaypointPoly::getType;
%ignore tesseract::command_language::StateWaypointPoly::getTypeErased;
%ignore tesseract::command_language::CartesianWaypoint::clone;
%ignore tesseract::command_language::CartesianWaypoint::getTransform;
%ignore tesseract::command_language::CartesianWaypoint::getUpperTolerance;
%ignore tesseract::command_language::CartesianWaypoint::getLowerTolerance;
%ignore tesseract::command_language::StateWaypoint::clone;
%ignore tesseract::command_language::StateWaypoint::getPosition;
%ignore tesseract::command_language::StateWaypoint::getVelocity;
%ignore tesseract::command_language::StateWaypoint::getAcceleration;
%ignore tesseract::command_language::StateWaypoint::getEffort;
%ignore tesseract::command_language::StateWaypointInterface::getPosition;
%ignore tesseract::command_language::StateWaypointInterface::getVelocity;
%ignore tesseract::command_language::StateWaypointInterface::getAcceleration;
%ignore tesseract::command_language::StateWaypointInterface::getEffort;
%ignore tesseract::command_language::StateWaypointPoly::getPosition;
%ignore tesseract::command_language::StateWaypointPoly::getVelocity;
%ignore tesseract::command_language::StateWaypointPoly::getAcceleration;
%ignore tesseract::command_language::StateWaypointPoly::getEffort;
%ignore tesseract::command_language::JointWaypoint::clone;
%ignore tesseract::command_language::JointWaypoint::JointWaypoint(std::initializer_list<std::string>, std::initializer_list<double>);
%ignore tesseract::command_language::JointWaypoint::JointWaypoint(std::initializer_list<std::string>, std::initializer_list<double>, bool);
%ignore tesseract::command_language::JointWaypoint::JointWaypoint(std::initializer_list<std::string>, std::initializer_list<double>, std::initializer_list<double>, std::initializer_list<double>);
%ignore tesseract::command_language::JointWaypoint::getPosition;
%ignore tesseract::command_language::JointWaypoint::getUpperTolerance;
%ignore tesseract::command_language::JointWaypoint::getLowerTolerance;
%ignore tesseract::command_language::JointWaypointInterface::clone;
%ignore tesseract::command_language::JointWaypointInterface::getPosition;
%ignore tesseract::command_language::JointWaypointInterface::getUpperTolerance;
%ignore tesseract::command_language::JointWaypointInterface::getLowerTolerance;
%ignore tesseract::command_language::JointWaypointPoly::clone;
%ignore tesseract::command_language::JointWaypointPoly::getType;
%ignore tesseract::command_language::JointWaypointPoly::getPosition;
%ignore tesseract::command_language::JointWaypointPoly::getUpperTolerance;
%ignore tesseract::command_language::JointWaypointPoly::getLowerTolerance;
%ignore tesseract::command_language::MoveInstruction::clone;
%ignore tesseract::command_language::MoveInstruction::getUUID;
%ignore tesseract::command_language::MoveInstruction::setUUID;
%ignore tesseract::command_language::MoveInstruction::getParentUUID;
%ignore tesseract::command_language::MoveInstruction::setParentUUID;
%ignore tesseract::command_language::MoveInstruction::getProfileOverrides;
%ignore tesseract::command_language::MoveInstruction::setProfileOverrides;
%ignore tesseract::command_language::MoveInstruction::getPathProfileOverrides;
%ignore tesseract::command_language::MoveInstruction::setPathProfileOverrides;
%ignore tesseract::command_language::MoveInstruction::createCartesianWaypoint;
%ignore tesseract::command_language::MoveInstruction::createJointWaypoint;
%ignore tesseract::command_language::MoveInstruction::createStateWaypoint;
%ignore tesseract::command_language::CompositeInstruction::clone;
%ignore tesseract::command_language::CompositeInstruction::getUUID;
%ignore tesseract::command_language::CompositeInstruction::setUUID;
%ignore tesseract::command_language::CompositeInstruction::getParentUUID;
%ignore tesseract::command_language::CompositeInstruction::setParentUUID;
%ignore tesseract::command_language::CompositeInstruction::getProfileOverrides;
%ignore tesseract::command_language::CompositeInstruction::setProfileOverrides;
%ignore tesseract::command_language::CompositeInstruction::getUserData;
%ignore tesseract::command_language::CompositeInstruction::flatten;
%ignore tesseract::command_language::CompositeInstruction::getFirstInstruction;
%ignore tesseract::command_language::CompositeInstruction::getLastInstruction;
%ignore tesseract::command_language::CompositeInstruction::getInstructionCount;
%ignore tesseract::command_language::CompositeInstruction::insert;
%ignore tesseract::command_language::CompositeInstruction::emplace;
%ignore tesseract::command_language::CompositeInstruction::emplace_back;
%ignore tesseract::command_language::CompositeInstruction::erase;
%ignore tesseract::command_language::CompositeInstruction::swap;
%ignore tesseract::command_language::CompositeInstruction::begin;
%ignore tesseract::command_language::CompositeInstruction::end;
%ignore tesseract::command_language::CompositeInstruction::cbegin;
%ignore tesseract::command_language::CompositeInstruction::cend;
%ignore tesseract::command_language::CompositeInstruction::rbegin;
%ignore tesseract::command_language::CompositeInstruction::rend;
%ignore tesseract::command_language::CompositeInstruction::crbegin;
%ignore tesseract::command_language::CompositeInstruction::crend;
%ignore tesseract::command_language::CompositeInstruction::data;
%ignore tesseract::command_language::CompositeInstruction::operator[];
%ignore tesseract::command_language::CompositeInstruction::front;
%ignore tesseract::command_language::CompositeInstruction::back;
%ignore tesseract::command_language::CompositeInstruction::at;
%ignore tesseract::command_language::CompositeInstruction::push_back;
%ignore tesseract::command_language::CompositeInstruction::setInstructions;
%ignore tesseract::command_language::CompositeInstruction::getInstructions;

%include <tesseract/command_language/types.h>
%include <tesseract/command_language/instruction_type.h>
%include <tesseract/command_language/poly/waypoint_poly.h>
%include <tesseract/command_language/poly/joint_waypoint_poly.h>
%include <tesseract/command_language/poly/state_waypoint_poly.h>
%include <tesseract/command_language/poly/instruction_poly.h>
%include <tesseract/command_language/poly/move_instruction_poly.h>
%include <tesseract/command_language/cartesian_waypoint.h>
%include <tesseract/command_language/joint_waypoint.h>
%include <tesseract/command_language/state_waypoint.h>
%include <tesseract/command_language/move_instruction.h>
%include <tesseract/command_language/composite_instruction.h>

%ignore tesseract::motion_planners::OMPLMoveProfile::createSolverConfig;
%ignore tesseract::motion_planners::OMPLMoveProfile::createStateExtractor;
%ignore tesseract::motion_planners::OMPLMoveProfile::createSimpleSetup;
%ignore tesseract::motion_planners::OMPLPlannerConfigurator::create;
%ignore tesseract::motion_planners::SBLConfigurator::create;
%ignore tesseract::motion_planners::ESTConfigurator::create;
%ignore tesseract::motion_planners::LBKPIECE1Configurator::create;
%ignore tesseract::motion_planners::BKPIECE1Configurator::create;
%ignore tesseract::motion_planners::KPIECE1Configurator::create;
%ignore tesseract::motion_planners::BiTRRTConfigurator::create;
%ignore tesseract::motion_planners::RRTConfigurator::create;
%ignore tesseract::motion_planners::RRTConnectConfigurator::create;
%ignore tesseract::motion_planners::RRTstarConfigurator::create;
%ignore tesseract::motion_planners::TRRTConfigurator::create;
%ignore tesseract::motion_planners::PRMConfigurator::create;
%ignore tesseract::motion_planners::PRMstarConfigurator::create;
%ignore tesseract::motion_planners::LazyPRMstarConfigurator::create;
%ignore tesseract::motion_planners::SPARSConfigurator::create;
%ignore tesseract::motion_planners::OMPLSolverConfig::operator==;
%ignore tesseract::motion_planners::OMPLSolverConfig::operator!=;
%shared_ptr(tesseract::motion_planners::OMPLPlannerConfigurator)
%shared_ptr(tesseract::motion_planners::SBLConfigurator)
%shared_ptr(tesseract::motion_planners::ESTConfigurator)
%shared_ptr(tesseract::motion_planners::LBKPIECE1Configurator)
%shared_ptr(tesseract::motion_planners::BKPIECE1Configurator)
%shared_ptr(tesseract::motion_planners::KPIECE1Configurator)
%shared_ptr(tesseract::motion_planners::BiTRRTConfigurator)
%shared_ptr(tesseract::motion_planners::RRTConfigurator)
%shared_ptr(tesseract::motion_planners::RRTConnectConfigurator)
%shared_ptr(tesseract::motion_planners::RRTstarConfigurator)
%shared_ptr(tesseract::motion_planners::TRRTConfigurator)
%shared_ptr(tesseract::motion_planners::PRMConfigurator)
%shared_ptr(tesseract::motion_planners::PRMstarConfigurator)
%shared_ptr(tesseract::motion_planners::LazyPRMstarConfigurator)
%shared_ptr(tesseract::motion_planners::SPARSConfigurator)
%shared_ptr(tesseract::motion_planners::OMPLSolverConfig)
%include <tesseract/motion_planners/ompl/ompl_planner_configurator.h>
%template(OMPLPlannerConfiguratorVector) std::vector<std::shared_ptr<const tesseract::motion_planners::OMPLPlannerConfigurator>>;
%include <tesseract/motion_planners/ompl/ompl_solver_config.h>
%ignore tesseract::motion_planners::OMPLRealVectorMoveProfile::OMPLRealVectorMoveProfile(const YAML::Node&, const tesseract::common::ProfilePluginFactory&);
%ignore tesseract::motion_planners::OMPLRealVectorMoveProfile::createSolverConfig;
%ignore tesseract::motion_planners::OMPLRealVectorMoveProfile::createStateExtractor;
%ignore tesseract::motion_planners::OMPLRealVectorMoveProfile::createSimpleSetup;
%ignore tesseract::motion_planners::OMPLRealVectorMoveProfile::operator==;
%ignore tesseract::motion_planners::OMPLRealVectorMoveProfile::operator!=;
%shared_ptr(tesseract::motion_planners::OMPLMoveProfile)
%shared_ptr(tesseract::motion_planners::OMPLRealVectorMoveProfile)
%include <tesseract/motion_planners/ompl/profile/ompl_profile.h>
%include <tesseract/motion_planners/ompl/profile/ompl_real_vector_move_profile.h>

%ignore tesseract::motion_planners::DescartesDefaultMoveProfile<double>::createWaypointSampler;
%ignore tesseract::motion_planners::DescartesDefaultMoveProfile<double>::createEdgeEvaluator;
%ignore tesseract::motion_planners::DescartesDefaultMoveProfile<double>::createStateEvaluator;
%shared_ptr(tesseract::motion_planners::DescartesMoveProfile<double>)
%shared_ptr(tesseract::motion_planners::DescartesDefaultMoveProfile<double>)
%shared_ptr(tesseract::motion_planners::DescartesSolverProfile<double>)
%shared_ptr(tesseract::motion_planners::DescartesLadderGraphSolverProfile<double>)
%include <tesseract/motion_planners/descartes/profile/descartes_profile.h>
%include <tesseract/motion_planners/descartes/profile/descartes_default_move_profile.h>
%include <tesseract/motion_planners/descartes/profile/descartes_ladder_graph_solver_profile.h>
%template(DescartesDefaultMoveProfileD) tesseract::motion_planners::DescartesDefaultMoveProfile<double>;
%template(DescartesLadderGraphSolverProfileD) tesseract::motion_planners::DescartesLadderGraphSolverProfile<double>;

%ignore tesseract::motion_planners::TrajOptTermInfos;
%ignore tesseract::motion_planners::TrajOptWaypointInfo;
namespace sco
{
struct BasicTrustRegionSQPParameters
{
  double improve_ratio_threshold;
  double min_trust_box_size;
  double min_approx_improve;
  double min_approx_improve_frac;
  int max_iter;
  double trust_shrink_ratio;
  double trust_expand_ratio;
  double cnt_tolerance;
  double max_merit_coeff_increases;
  int max_qp_solver_failures;
  double merit_coeff_increase_ratio;
  double max_time;
  double initial_merit_error_coeff;
  bool inflate_constraints_individually;
  double trust_box_size;
  bool log_results;
  std::string log_dir;
  int num_threads;
};
}
%ignore trajopt_common::CollisionCoeffData::getCollisionCoeffPairData;
%ignore trajopt_common::CollisionCoeffData::getPairsWithZeroCoeff;
%ignore trajopt_common::CollisionCoeffData::operator==;
%ignore trajopt_common::CollisionCoeffData::operator!=;
%ignore trajopt_common::TrajOptCollisionConfig::operator==;
%ignore trajopt_common::TrajOptCollisionConfig::operator!=;
%ignore trajopt_common::LinkGradientResults;
%ignore trajopt_common::GradientResults;
%ignore trajopt_common::LinkMaxError;
%ignore trajopt_common::GradientResultsSet;
%ignore trajopt_common::CollisionCacheData;
%include <trajopt_common/collision_types.h>
%ignore tesseract::motion_planners::TrajOptCartesianWaypointConfig::operator==;
%ignore tesseract::motion_planners::TrajOptCartesianWaypointConfig::operator!=;
%ignore tesseract::motion_planners::TrajOptJointWaypointConfig::operator==;
%ignore tesseract::motion_planners::TrajOptJointWaypointConfig::operator!=;
%include <tesseract/motion_planners/trajopt/trajopt_waypoint_config.h>
%ignore tesseract::motion_planners::TrajOptMoveProfile::create;
%ignore tesseract::motion_planners::TrajOptCompositeProfile::create;
%ignore tesseract::motion_planners::TrajOptSolverProfile::callbacks;
%ignore tesseract::motion_planners::TrajOptSolverProfile::getSolverType;
%ignore tesseract::motion_planners::TrajOptSolverProfile::createSolverConfig;
%ignore tesseract::motion_planners::TrajOptSolverProfile::createOptimizationParameters;
%ignore tesseract::motion_planners::TrajOptSolverProfile::createOptimizationCallbacks;
%ignore tesseract::motion_planners::TrajOptDefaultMoveProfile::TrajOptDefaultMoveProfile(const YAML::Node&, const tesseract::common::ProfilePluginFactory&);
%ignore tesseract::motion_planners::TrajOptDefaultMoveProfile::create;
%ignore tesseract::motion_planners::TrajOptDefaultMoveProfile::operator==;
%ignore tesseract::motion_planners::TrajOptDefaultMoveProfile::operator!=;
%ignore tesseract::motion_planners::TrajOptDefaultCompositeProfile::TrajOptDefaultCompositeProfile(const YAML::Node&, const tesseract::common::ProfilePluginFactory&);
%ignore tesseract::motion_planners::TrajOptDefaultCompositeProfile::create;
%ignore tesseract::motion_planners::TrajOptDefaultCompositeProfile::computeLongestValidSegmentLength;
%ignore tesseract::motion_planners::TrajOptDefaultCompositeProfile::operator==;
%ignore tesseract::motion_planners::TrajOptDefaultCompositeProfile::operator!=;
%ignore tesseract::motion_planners::TrajOptOSQPSolverProfile::TrajOptOSQPSolverProfile(const YAML::Node&, const tesseract::common::ProfilePluginFactory&);
%ignore tesseract::motion_planners::TrajOptOSQPSolverProfile::getSolverType;
%ignore tesseract::motion_planners::TrajOptOSQPSolverProfile::createSolverConfig;
%ignore tesseract::motion_planners::TrajOptOSQPSolverProfile::operator==;
%ignore tesseract::motion_planners::TrajOptOSQPSolverProfile::operator!=;
enum osqp_linsys_solver_type
{
  OSQP_UNKNOWN_SOLVER = 0,
  OSQP_DIRECT_SOLVER,
  OSQP_INDIRECT_SOLVER
};
enum osqp_precond_type
{
  OSQP_NO_PRECONDITIONER = 0,
  OSQP_DIAGONAL_PRECONDITIONER
};
%nodefaultctor OSQPSettings;
struct OSQPSettings
{
  long long device;
  osqp_linsys_solver_type linsys_solver;
  long long allocate_solution;
  long long verbose;
  long long profiler_level;
  long long warm_starting;
  long long scaling;
  long long polishing;
  double rho;
  long long rho_is_vec;
  double sigma;
  double alpha;
  long long cg_max_iter;
  long long cg_tol_reduction;
  double cg_tol_fraction;
  osqp_precond_type cg_precond;
  long long adaptive_rho;
  long long adaptive_rho_interval;
  double adaptive_rho_fraction;
  double adaptive_rho_tolerance;
  long long max_iter;
  double eps_abs;
  double eps_rel;
  double eps_prim_inf;
  double eps_dual_inf;
  long long scaled_termination;
  long long check_termination;
  long long check_dualgap;
  double time_limit;
  double delta;
  long long polish_refine_iter;
};
%shared_ptr(tesseract::motion_planners::TrajOptMoveProfile)
%shared_ptr(tesseract::motion_planners::TrajOptCompositeProfile)
%shared_ptr(tesseract::motion_planners::TrajOptSolverProfile)
%shared_ptr(tesseract::motion_planners::TrajOptDefaultMoveProfile)
%shared_ptr(tesseract::motion_planners::TrajOptDefaultCompositeProfile)
%shared_ptr(tesseract::motion_planners::TrajOptOSQPSolverProfile)
%include <tesseract/motion_planners/trajopt/profile/trajopt_profile.h>
%include <tesseract/motion_planners/trajopt/profile/trajopt_default_move_profile.h>
%include <tesseract/motion_planners/trajopt/profile/trajopt_default_composite_profile.h>
%include <tesseract/motion_planners/trajopt/profile/trajopt_osqp_solver_profile.h>

%ignore tesseract::task_composer::TaskComposerContext::TaskComposerContext;
%ignore tesseract::task_composer::TaskComposerContext::name;
%ignore tesseract::task_composer::TaskComposerContext::dotgraph;
%ignore tesseract::task_composer::TaskComposerContext::data_storage;
%ignore tesseract::task_composer::TaskComposerContext::abort;
%ignore tesseract::task_composer::TaskComposerContext::operator==;
%ignore tesseract::task_composer::TaskComposerContext::operator!=;
%ignore tesseract::task_composer::TaskComposerNode::TaskComposerNode;
%ignore tesseract::task_composer::TaskComposerNode::run;
%ignore tesseract::task_composer::TaskComposerNode::setName;
%ignore tesseract::task_composer::TaskComposerNode::getName;
%ignore tesseract::task_composer::TaskComposerNode::setNamespace;
%ignore tesseract::task_composer::TaskComposerNode::getNamespace;
%ignore tesseract::task_composer::TaskComposerNode::getType;
%ignore tesseract::task_composer::TaskComposerNode::getUUID;
%ignore tesseract::task_composer::TaskComposerNode::getUUIDString;
%ignore tesseract::task_composer::TaskComposerNode::getParentUUID;
%ignore tesseract::task_composer::TaskComposerNode::getParentUUIDString;
%ignore tesseract::task_composer::TaskComposerNode::isConditional;
%ignore tesseract::task_composer::TaskComposerNode::validatePorts;
%ignore tesseract::task_composer::TaskComposerNode::getOutboundEdges;
%ignore tesseract::task_composer::TaskComposerNode::getInboundEdges;
%ignore tesseract::task_composer::TaskComposerNode::setInputKeys;
%ignore tesseract::task_composer::TaskComposerNode::setOutputKeys;
%ignore tesseract::task_composer::TaskComposerNode::getPorts;
%ignore tesseract::task_composer::TaskComposerNode::getDotgraph;
%ignore tesseract::task_composer::TaskComposerNode::saveDotgraph;
%ignore tesseract::task_composer::TaskComposerNode::getDataStorage;
%ignore tesseract::task_composer::TaskComposerNode::setConditional;
%ignore tesseract::task_composer::TaskComposerNode::dump;
%ignore tesseract::task_composer::TaskComposerFuture::TaskComposerFuture;
%ignore tesseract::task_composer::TaskComposerFuture::context;
%ignore tesseract::task_composer::TaskComposerFuture::clear;
%ignore tesseract::task_composer::TaskComposerFuture::valid;
%ignore tesseract::task_composer::TaskComposerFuture::ready;
%ignore tesseract::task_composer::TaskComposerFuture::waitFor;
%ignore tesseract::task_composer::TaskComposerFuture::waitUntil;
%ignore tesseract::task_composer::TaskComposerFuture::copy;
%ignore tesseract::task_composer::TaskComposerExecutor::TaskComposerExecutor;
%ignore tesseract::task_composer::TaskComposerExecutor::getName;
%ignore tesseract::task_composer::TaskComposerExecutor::getWorkerCount;
%ignore tesseract::task_composer::TaskComposerExecutor::getTaskCount;
%ignore tesseract::task_composer::TaskComposerDataStorage::getData;
%ignore tesseract::task_composer::TaskComposerDataStorage::copyAsInputData;
%ignore tesseract::task_composer::TaskComposerDataStorage::copyAsOutputData;
%ignore tesseract::task_composer::TaskComposerDataStorage::remapData;
%ignore tesseract::task_composer::TaskComposerDataStorage::operator==;
%ignore tesseract::task_composer::TaskComposerDataStorage::operator!=;
%ignore tesseract::task_composer::TaskComposerPluginFactory::TaskComposerPluginFactory;
%ignore tesseract::task_composer::TaskComposerPluginFactory::TaskComposerPluginFactory(YAML::Node const &, tesseract::common::ResourceLocator const &);
%ignore tesseract::task_composer::TaskComposerPluginFactory::TaskComposerPluginFactory(std::string const &, tesseract::common::ResourceLocator const &);
%ignore tesseract::task_composer::TaskComposerPluginFactory::TaskComposerPluginFactory(tesseract::task_composer::TaskComposerPluginFactory &&);
%ignore tesseract::task_composer::TaskComposerPluginFactory::TaskComposerPluginFactory(TaskComposerPluginFactory &&);
%ignore tesseract::task_composer::TaskComposerPluginFactory::loadConfig;
%ignore tesseract::task_composer::TaskComposerPluginFactory::getConfig;
%ignore tesseract::task_composer::TaskComposerPluginFactory::saveConfig;
%ignore tesseract::task_composer::TaskComposerPluginFactory::getTaskComposerExecutorPlugins;
%ignore tesseract::task_composer::TaskComposerPluginFactory::getTaskComposerNodePlugins;
%ignore tesseract::task_composer::TaskComposerPluginFactory::createTaskComposerExecutor(std::string const &, tesseract::common::PluginInfo const &) const;
%ignore tesseract::task_composer::TaskComposerPluginFactory::createTaskComposerNode(std::string const &, tesseract::common::PluginInfo const &) const;

%ignore tesseract::task_composer::TaskComposerNodeInfo::TaskComposerNodeInfo(const TaskComposerNode&);
%ignore tesseract::task_composer::TaskComposerNodeInfo::uuid;
%ignore tesseract::task_composer::TaskComposerNodeInfo::root_uuid;
%ignore tesseract::task_composer::TaskComposerNodeInfo::parent_uuid;
%ignore tesseract::task_composer::TaskComposerNodeInfo::type;
%ignore tesseract::task_composer::TaskComposerNodeInfo::type_hash_code;
%ignore tesseract::task_composer::TaskComposerNodeInfo::conditional;
%ignore tesseract::task_composer::TaskComposerNodeInfo::inbound_edges;
%ignore tesseract::task_composer::TaskComposerNodeInfo::outbound_edges;
%ignore tesseract::task_composer::TaskComposerNodeInfo::input_keys;
%ignore tesseract::task_composer::TaskComposerNodeInfo::output_keys;
%ignore tesseract::task_composer::TaskComposerNodeInfo::terminals;
%ignore tesseract::task_composer::TaskComposerNodeInfo::start_time;
%ignore tesseract::task_composer::TaskComposerNodeInfo::data_storage;
%ignore tesseract::task_composer::TaskComposerNodeInfo::operator==;
%ignore tesseract::task_composer::TaskComposerNodeInfo::operator!=;
%ignore tesseract::task_composer::TaskComposerNodeInfoContainer::getInfo;
%ignore tesseract::task_composer::TaskComposerNodeInfoContainer::find;
%ignore tesseract::task_composer::TaskComposerNodeInfoContainer::getInfoMap;
%ignore tesseract::task_composer::TaskComposerNodeInfoContainer::insertInfoMap;
%ignore tesseract::task_composer::TaskComposerNodeInfoContainer::mergeInfoMap;
%ignore tesseract::task_composer::TaskComposerNodeInfoContainer::setRootNode;
%ignore tesseract::task_composer::TaskComposerNodeInfoContainer::getRootNode;
%ignore tesseract::task_composer::TaskComposerNodeInfoContainer::setAborted;
%ignore tesseract::task_composer::TaskComposerNodeInfoContainer::getAbortingNode;
%ignore tesseract::task_composer::TaskComposerNodeInfoContainer::prune;
%ignore tesseract::task_composer::TaskComposerNodeInfoContainer::operator==;
%ignore tesseract::task_composer::TaskComposerNodeInfoContainer::operator!=;

%shared_ptr(tesseract::task_composer::TaskComposerNodeInfo)
%shared_ptr(tesseract::task_composer::TaskComposerNodeInfoContainer)
%shared_ptr(tesseract::task_composer::TaskComposerContext)
%shared_ptr(tesseract::task_composer::TaskComposerDataStorage)
%shared_ptr(tesseract::task_composer::TaskComposerExecutor)
%shared_ptr(tesseract::task_composer::TaskComposerFuture)
%shared_ptr(tesseract::task_composer::TaskComposerNode)
%shared_ptr(tesseract::task_composer::TaskComposerPluginFactory)
DARP_UNIQUE_PTR_TO_SHARED(tesseract::task_composer::TaskComposerExecutor)
DARP_UNIQUE_PTR_TO_SHARED(tesseract::task_composer::TaskComposerFuture)
DARP_UNIQUE_PTR_TO_SHARED(tesseract::task_composer::TaskComposerNode)

%include <tesseract/task_composer/task_composer_keys.h>
%template(get) tesseract::task_composer::TaskComposerKeys::get<std::string>;
%include <tesseract/task_composer/task_composer_data_storage.h>
%include <tesseract/task_composer/task_composer_node_info.h>
%template(TaskComposerNodeInfoVector) std::vector<tesseract::task_composer::TaskComposerNodeInfo>;
%include <tesseract/task_composer/task_composer_context.h>
%include <tesseract/task_composer/task_composer_node.h>
%include <tesseract/task_composer/task_composer_future.h>
%include <tesseract/task_composer/task_composer_executor.h>
%include <tesseract/task_composer/task_composer_plugin_factory.h>

%inline %{
namespace darp_tesseract_bindings
{
tesseract::command_language::WaypointPoly wrapCartesianWaypoint(
    const tesseract::command_language::CartesianWaypoint& waypoint)
{
  return tesseract::command_language::WaypointPoly(waypoint);
}

void appendMoveInstruction(tesseract::command_language::CompositeInstruction& program,
                           const tesseract::command_language::MoveInstruction& instruction)
{
  program.push_back(tesseract::command_language::InstructionPoly(instruction));
}

std::size_t instructionCount(const tesseract::command_language::CompositeInstruction& program)
{
  return program.size();
}

tesseract::command_language::InstructionPoly instructionAt(
    tesseract::command_language::CompositeInstruction& program,
    std::size_t index)
{
  return program.at(index);
}

tesseract::command_language::MoveInstructionPoly asMoveInstruction(
    tesseract::command_language::InstructionPoly& instruction)
{
  return instruction.as<tesseract::command_language::MoveInstructionPoly>();
}

tesseract::command_language::StateWaypointPoly asStateWaypoint(
    tesseract::command_language::WaypointPoly& waypoint)
{
  return waypoint.as<tesseract::command_language::StateWaypointPoly>();
}

tesseract::command_language::JointWaypointPoly asJointWaypoint(
    tesseract::command_language::WaypointPoly& waypoint)
{
  return waypoint.as<tesseract::command_language::JointWaypointPoly>();
}

Eigen::VectorXd statePosition(const tesseract::command_language::StateWaypointPoly& waypoint)
{
  return waypoint.getPosition();
}

Eigen::VectorXd jointPosition(const tesseract::command_language::JointWaypointPoly& waypoint)
{
  return waypoint.getPosition();
}

Eigen::VectorXd jointLowerTolerance(const tesseract::command_language::JointWaypointPoly& waypoint)
{
  return waypoint.getLowerTolerance();
}

Eigen::VectorXd jointUpperTolerance(const tesseract::command_language::JointWaypointPoly& waypoint)
{
  return waypoint.getUpperTolerance();
}

Eigen::VectorXd stateVelocity(const tesseract::command_language::StateWaypointPoly& waypoint)
{
  return waypoint.getVelocity();
}

Eigen::VectorXd stateAcceleration(const tesseract::command_language::StateWaypointPoly& waypoint)
{
  return waypoint.getAcceleration();
}

std::string uuidString(const tesseract::command_language::MoveInstructionPoly& instruction)
{
  return boost::uuids::to_string(instruction.getUUID());
}

std::string uuidString(const tesseract::command_language::MoveInstruction& instruction)
{
  return boost::uuids::to_string(instruction.getUUID());
}

std::string parentUuidString(const tesseract::command_language::MoveInstructionPoly& instruction)
{
  return boost::uuids::to_string(instruction.getParentUUID());
}

tesseract::common::AnyPoly wrapCompositeInstruction(
    const tesseract::command_language::CompositeInstruction& program)
{
  return tesseract::common::AnyPoly(program);
}

tesseract::common::AnyPoly wrapEnvironment(
    const std::shared_ptr<tesseract::environment::Environment>& environment)
{
  std::shared_ptr<const tesseract::environment::Environment> const_environment = environment;
  return tesseract::common::AnyPoly(const_environment);
}

tesseract::common::AnyPoly wrapProfileDictionary(
    const std::shared_ptr<tesseract::common::ProfileDictionary>& profiles)
{
  return tesseract::common::AnyPoly(profiles);
}

tesseract::command_language::CompositeInstruction asCompositeInstruction(
    tesseract::common::AnyPoly& value)
{
  return value.as<tesseract::command_language::CompositeInstruction>();
}

void setData(tesseract::task_composer::TaskComposerDataStorage& storage,
             const std::string& key,
             tesseract::common::AnyPoly value)
{
  storage.setData(key, std::move(value));
}

tesseract::common::AnyPoly getData(const tesseract::task_composer::TaskComposerDataStorage& storage,
                                   const std::string& key)
{
  return storage.getData(key);
}

std::shared_ptr<tesseract::task_composer::TaskComposerPluginFactory> createTaskComposerPluginFactory(
    const std::string& config,
    const std::shared_ptr<tesseract::common::ResourceLocator>& locator)
{
  if (locator == nullptr)
    throw std::invalid_argument("The resource locator is null.");

  return std::make_shared<tesseract::task_composer::TaskComposerPluginFactory>(
      std::filesystem::path(config), *locator);
}

std::shared_ptr<tesseract::task_composer::TaskComposerContext> createTaskComposerContext(
    const std::string& name,
    const std::shared_ptr<tesseract::task_composer::TaskComposerDataStorage>& storage)
{
  if (storage == nullptr)
    throw std::invalid_argument("The task composer data storage is null.");

  return std::make_shared<tesseract::task_composer::TaskComposerContext>(name, storage);
}

std::vector<tesseract::task_composer::TaskComposerNodeInfo> getTaskComposerNodeInfos(
    const tesseract::task_composer::TaskComposerNodeInfoContainer& task_infos)
{
  const auto info_map = task_infos.getInfoMap();
  std::vector<tesseract::task_composer::TaskComposerNodeInfo> result;
  result.reserve(info_map.size());
  for (const auto& entry : info_map)
    result.push_back(entry.second);
  return result;
}

std::shared_ptr<tesseract::task_composer::TaskComposerNodeInfo> getAbortingTaskComposerNodeInfo(
    const tesseract::task_composer::TaskComposerNodeInfoContainer& task_infos)
{
  const auto aborting_node = task_infos.getAbortingNode();
  if (aborting_node.is_nil())
    return nullptr;

  const auto info = task_infos.getInfo(aborting_node);
  if (!info.has_value())
    return nullptr;

  return std::make_shared<tesseract::task_composer::TaskComposerNodeInfo>(*info);
}

std::vector<std::string> getConfiguredTaskComposerNodeNames(
    const tesseract::task_composer::TaskComposerPluginFactory& factory)
{
  const auto plugins = factory.getTaskComposerNodePlugins();
  std::vector<std::string> result;
  result.reserve(plugins.size());
  for (const auto& entry : plugins)
    result.push_back(entry.first);
  return result;
}

tesseract::common::AnyPoly getContextData(
    const tesseract::task_composer::TaskComposerContext& context,
    const std::string& key)
{
  if (context.data_storage == nullptr)
    throw std::runtime_error("The task composer context has no data storage.");

  return context.data_storage->getData(key);
}

std::shared_ptr<tesseract::common::Profile> asProfile(
    const std::shared_ptr<tesseract::motion_planners::DescartesDefaultMoveProfile<double>>& profile)
{
  return profile;
}

std::shared_ptr<tesseract::common::Profile> asProfile(
    const std::shared_ptr<tesseract::motion_planners::DescartesLadderGraphSolverProfile<double>>& profile)
{
  return profile;
}

void addProfile(tesseract::common::ProfileDictionary& profiles,
                const std::string& profile_namespace,
                const std::string& profile_name,
                const std::shared_ptr<tesseract::common::Profile>& profile)
{
  profiles.addProfile(profile_namespace, profile_name, profile);
}
}
%}

%include <tesseract/motion_planners/utils.h>
