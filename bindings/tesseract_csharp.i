/**
 * Generated C# bindings for the pinned native Tesseract API. Only reusable
 * language interop rules live in this repository; class and method
 * declarations come from the unmodified upstream headers.
 */
%module TesseractNative

%include <stdint.i>
%include <std_string.i>
%include <std_vector.i>
%include <std_set.i>
%include <std_map.i>
%include <std_unordered_map.i>
%include <std_pair.i>
%include <std_shared_ptr.i>
%include <std_unique_ptr.i>

%include "support/exceptions.i"
%include "support/ownership.i"

%pragma(csharp) imclasscode=%{
  [global::System.Runtime.InteropServices.DllImport("tesseract_csharp", EntryPoint="darp_tesseract_initialize_plugins")]
  private static extern int darp_tesseract_initialize_plugins();

  private static readonly bool darpPluginsInitialized = InitializeDarpPlugins();

  private static bool InitializeDarpPlugins() {
    if (darp_tesseract_initialize_plugins() != 0)
      throw new global::System.InvalidOperationException("Could not initialize embedded Tesseract plugins.");
    return true;
  }
%}

%{
#include <cstdint>
#include <filesystem>
#include <limits>
#include <optional>
#include <stdexcept>
#include <Eigen/Core>
#include <Eigen/Geometry>

#include <tesseract/common/types.h>
#include <tesseract/common/eigen_types.h>
#include <tesseract/common/resource_locator.h>
#include <tesseract/common/manipulator_info.h>
#include <tesseract/common/joint_state.h>
#include <tesseract/common/kinematic_limits.h>
#include <tesseract/common/plugin_info.h>

#include <tesseract/geometry/geometry.h>
#include <tesseract/geometry/geometries.h>
#include <tesseract/geometry/impl/mesh_material.h>
#include <tesseract/geometry/mesh_parser.h>
#include <tesseract/geometry/utils.h>

#include <tesseract/scene_graph/joint.h>
#include <tesseract/scene_graph/link.h>
#include <tesseract/scene_graph/graph.h>
#include <tesseract/scene_graph/scene_state.h>

#include <tesseract/urdf/urdf_parser.h>
#include <tesseract/srdf/kinematics_information.h>
#include <tesseract/srdf/srdf_model.h>
#include <tesseract/srdf/utils.h>

#include <tesseract/state_solver/state_solver.h>
#include <tesseract/state_solver/mutable_state_solver.h>

#include <tesseract/collision/discrete_contact_manager.h>
#include <tesseract/collision/continuous_contact_manager.h>

#include <tesseract/kinematics/types.h>
#include <tesseract/kinematics/forward_kinematics.h>
#include <tesseract/kinematics/inverse_kinematics.h>
#include <tesseract/kinematics/joint_group.h>
#include <tesseract/kinematics/kinematic_group.h>
#include <tesseract/kinematics/kinematics_plugin_factory.h>

#include <tesseract/environment/environment.h>
%}

/* Export/serialization macros are irrelevant to the SWIG parser. */
#define TESSERACT_COMMON_PUBLIC
#define TESSERACT_GEOMETRY_PUBLIC
#define TESSERACT_SCENE_GRAPH_PUBLIC
#define TESSERACT_SRDF_PUBLIC
#define TESSERACT_URDF_PUBLIC
#define TESSERACT_STATE_SOLVER_PUBLIC
#define TESSERACT_COLLISION_PUBLIC
#define TESSERACT_KINEMATICS_PUBLIC
#define TESSERACT_ENVIRONMENT_PUBLIC
#define EIGEN_MAKE_ALIGNED_OPERATOR_NEW
#define TESSERACT_COMMON_IGNORE_WARNINGS_PUSH
#define TESSERACT_COMMON_IGNORE_WARNINGS_POP
#define BOOST_CLASS_EXPORT_KEY(a)
#define BOOST_CLASS_EXPORT_KEY2(a,b)
#define BOOST_CLASS_VERSION(a,b)
#define BOOST_CLASS_TRACKING(a,b)
#define BOOST_SERIALIZATION_ASSUME_ABSTRACT(a)

%include "support/eigen.i"
%include "support/filesystem.i"
%include "support/containers.i"

/* Resolve this upstream alias before overload-specific feature matching. */
namespace tesseract
{
namespace common
{
template <typename T> class AlignedVector;
}
namespace kinematics
{
struct KinGroupIKInput;
using KinGroupIKInputs = tesseract::common::AlignedVector<KinGroupIKInput>;
}
}

/* Managed subclassing/callbacks are outside this native-only slice. */
%ignore tesseract::common::ResourceLocator::locateResource;
%ignore tesseract::common::Resource::getResourceContents;
%ignore tesseract::common::Resource::getResourceContentStream;
%ignore tesseract::common::SimpleLocatedResource::getResourceContents;
%ignore tesseract::common::SimpleLocatedResource::getResourceContentStream;
%ignore tesseract::common::BytesResource;
%ignore tesseract::common::ProfilesPluginInfo;
%ignore tesseract::common::ManipulatorInfo::tcp_offset;
%ignore tesseract::common::ManipulatorInfo::ManipulatorInfo(
  std::string,
  std::string,
  std::string,
  std::variant<std::string, Eigen::Isometry3d>);
%ignore tesseract::common::GeneralResourceLocator::GeneralResourceLocator(
  std::vector<std::filesystem::path> const &,
  std::vector<std::string> const &);
%ignore tesseract::common::GeneralResourceLocator::GeneralResourceLocator(
  std::vector<std::filesystem::path> const &);
%ignore tesseract::common::JointTrajectory;
%ignore tesseract::scene_graph::Joint::Joint(tesseract::scene_graph::Joint&&);
%ignore tesseract::scene_graph::Joint::Joint(tesseract::scene_graph::Joint);
%ignore tesseract::scene_graph::Link::Link(tesseract::scene_graph::Link&&);
%ignore tesseract::scene_graph::Link::Link(tesseract::scene_graph::Link);
%ignore tesseract::scene_graph::SceneGraph::SceneGraph(tesseract::scene_graph::SceneGraph&&);
%ignore tesseract::scene_graph::SceneGraph::SceneGraph(tesseract::scene_graph::SceneGraph);
%ignore tesseract::scene_graph::SceneGraph::clone;
%ignore tesseract::environment::Environment::addFindTCPOffsetCallback;
%ignore tesseract::environment::Environment::getFindTCPOffsetCallbacks;
%ignore tesseract::environment::Environment::addEventCallback;
%ignore tesseract::environment::Environment::getEventCallbacks;
%ignore tesseract::environment::Environment::lockRead;
%ignore tesseract::environment::Environment::clone;
%ignore tesseract::environment::Environment::getCommandHistory;
%ignore tesseract::environment::Environment::applyCommands;
%ignore tesseract::environment::Environment::applyCommand;
%ignore tesseract::environment::Environment::init(std::vector<std::shared_ptr<const Command>> const &);
%ignore tesseract::environment::Environment::init(
  std::filesystem::path const &,
  std::shared_ptr<tesseract::common::ResourceLocator const> const &);
%ignore tesseract::environment::Environment::init(
  std::filesystem::path const &,
  std::filesystem::path const &,
  std::shared_ptr<tesseract::common::ResourceLocator const> const &);
%ignore tesseract::environment::Environment::Environment(std::unique_ptr<Implementation>);
%ignore tesseract::kinematics::ForwardKinematics::clone;
%ignore tesseract::kinematics::InverseKinematics::clone;
%ignore tesseract::kinematics::KinematicGroup::KinematicGroup;
%ignore tesseract::kinematics::KinematicsPluginFactory;
%ignore tesseract::kinematics::FwdKinFactory;
%ignore tesseract::kinematics::InvKinFactory;

/* Mesh data needs a dedicated aligned-vector/indexed-face layer. Robot
 * loading still uses it internally; defer only its public construction API. */
%ignore tesseract::geometry::PolygonMesh;
%ignore tesseract::geometry::Mesh;
%ignore tesseract::geometry::ConvexMesh;
%ignore tesseract::geometry::SDFMesh;
%ignore tesseract::geometry::CompoundMesh;
%ignore tesseract::geometry::MeshMaterial;
%ignore tesseract::geometry::MeshTexture;
%ignore tesseract::geometry::Geometry::setUUID;
%ignore tesseract::geometry::Geometry::getUUID;
%ignore tesseract::geometry::extractVertices;

%ignore tesseract::common::makeOrderedLinkPair;
%ignore tesseract::srdf::compareLinkPairAlphabetically;
%ignore tesseract::environment::Environment::getTimestamp;
%ignore tesseract::environment::Environment::getCurrentStateTimestamp;
%ignore tesseract::srdf::SRDFModel::version;
%ignore tesseract::srdf::SRDFModel::calibration_info;
%ignore tesseract::kinematics::KinematicGroup::calcInvKin(
  KinGroupIKInputs const &,
  Eigen::Ref<Eigen::VectorXd const> const &) const;
%ignore tesseract::kinematics::KinematicGroup::calcInvKin(
  IKSolutions &,
  KinGroupIKInputs const &,
  Eigen::Ref<Eigen::VectorXd const> const &) const;

/* Keep this first slice free of public SWIGTYPE placeholders. These members
 * return nested containers/YAML nodes that will be enabled by later reusable
 * container typemaps. Scalar and already-typed alternatives remain exposed. */
%ignore tesseract::common::PluginInfo::config;
%ignore tesseract::common::PluginInfoContainer::plugins;
%ignore tesseract::common::KinematicsPluginInfo::fwd_plugin_infos;
%ignore tesseract::common::KinematicsPluginInfo::inv_plugin_infos;
%ignore tesseract::srdf::KinematicsInformation::chain_groups;
%ignore tesseract::srdf::KinematicsInformation::group_states;
%ignore tesseract::srdf::KinematicsInformation::group_tcps;
%ignore tesseract::srdf::KinematicsInformation::addChainGroup;
%ignore tesseract::scene_graph::Link::visual;
%ignore tesseract::scene_graph::Link::collision;
%ignore tesseract::scene_graph::SceneGraph::getLinks;
%ignore tesseract::scene_graph::SceneGraph::getLeafLinks;
%ignore tesseract::scene_graph::SceneGraph::getJoints;
%ignore tesseract::scene_graph::SceneGraph::getActiveJoints;
%ignore tesseract::scene_graph::SceneGraph::getInboundJoints;
%ignore tesseract::scene_graph::SceneGraph::getOutboundJoints;

/* Complex collision configuration shapes remain deferred for now. */
%ignore tesseract::common::ContactManagersPluginInfo;
%ignore tesseract::common::TaskComposerPluginInfo;
%ignore tesseract::environment::EnvironmentContactAllowedValidator;
%ignore tesseract::srdf::SRDFModel::contact_managers_plugin_info;
%ignore tesseract::srdf::SRDFModel::acm;
%ignore tesseract::srdf::SRDFModel::collision_margin_data;
%ignore tesseract::srdf::processSRDFAllowedCollisions;
%ignore tesseract::srdf::getAlphabeticalACMKeys;
%ignore tesseract::scene_graph::SceneGraph::setLinkCollisionEnabled;
%ignore tesseract::scene_graph::SceneGraph::getLinkCollisionEnabled;
%ignore tesseract::scene_graph::SceneGraph::setAllowedCollisionMatrix;
%ignore tesseract::scene_graph::SceneGraph::addAllowedCollision;
%ignore tesseract::scene_graph::SceneGraph::removeAllowedCollision;
%ignore tesseract::scene_graph::SceneGraph::clearAllowedCollisions;
%ignore tesseract::scene_graph::SceneGraph::isCollisionAllowed;
%ignore tesseract::scene_graph::SceneGraph::getAllowedCollisionMatrix;
%ignore tesseract::environment::Environment::getLinkCollisionEnabled;
%ignore tesseract::environment::Environment::getAllowedCollisionMatrix;
%ignore tesseract::environment::Environment::getCollisionMarginData;
%ignore tesseract::environment::Environment::clearCachedDiscreteContactManager;
%ignore tesseract::environment::Environment::clearCachedContinuousContactManager;
%ignore tesseract::environment::Environment::getContactManagersPluginInfo;

/* shared_ptr ownership annotations must precede the corresponding headers. */
%shared_ptr(tesseract::common::Resource)
%shared_ptr(tesseract::common::BytesResource)
%shared_ptr(tesseract::common::SimpleLocatedResource)
%shared_ptr(tesseract::common::ResourceLocator)
%shared_ptr(tesseract::common::GeneralResourceLocator)
%shared_ptr(tesseract::geometry::Geometry)
%shared_ptr(tesseract::geometry::Box)
%shared_ptr(tesseract::geometry::Sphere)
%shared_ptr(tesseract::geometry::Cylinder)
%shared_ptr(tesseract::geometry::Capsule)
%shared_ptr(tesseract::geometry::Cone)
%shared_ptr(tesseract::geometry::Plane)
%shared_ptr(tesseract::geometry::PolygonMesh)
%shared_ptr(tesseract::geometry::Mesh)
%shared_ptr(tesseract::geometry::ConvexMesh)
%shared_ptr(tesseract::geometry::SDFMesh)
%shared_ptr(tesseract::geometry::CompoundMesh)
%shared_ptr(tesseract::geometry::MeshMaterial)
%shared_ptr(tesseract::geometry::MeshTexture)

%shared_ptr(tesseract::scene_graph::JointDynamics)
%shared_ptr(tesseract::scene_graph::JointLimits)
%shared_ptr(tesseract::scene_graph::JointSafety)
%shared_ptr(tesseract::scene_graph::JointMimic)
%shared_ptr(tesseract::scene_graph::JointCalibration)
%shared_ptr(tesseract::scene_graph::Joint)
%shared_ptr(tesseract::scene_graph::Material)
%shared_ptr(tesseract::scene_graph::Inertial)
%shared_ptr(tesseract::scene_graph::Visual)
%shared_ptr(tesseract::scene_graph::Collision)
%shared_ptr(tesseract::scene_graph::Link)
%shared_ptr(tesseract::scene_graph::SceneGraph)
%shared_ptr(tesseract::scene_graph::SceneState)
%shared_ptr(tesseract::scene_graph::StateSolver)
%shared_ptr(tesseract::scene_graph::MutableStateSolver)

%shared_ptr(tesseract::collision::DiscreteContactManager)
%shared_ptr(tesseract::collision::ContinuousContactManager)

%shared_ptr(tesseract::kinematics::ForwardKinematics)
%shared_ptr(tesseract::kinematics::InverseKinematics)
%shared_ptr(tesseract::kinematics::JointGroup)
%shared_ptr(tesseract::kinematics::KinematicGroup)
%shared_ptr(tesseract::kinematics::KinematicsPluginFactory)
%shared_ptr(tesseract::srdf::SRDFModel)
%shared_ptr(tesseract::environment::Environment)

DARP_UNIQUE_PTR_TO_SHARED(tesseract::scene_graph::SceneGraph)
DARP_UNIQUE_PTR_TO_SHARED(tesseract::scene_graph::StateSolver)
DARP_UNIQUE_PTR_TO_SHARED(tesseract::kinematics::ForwardKinematics)
DARP_UNIQUE_PTR_TO_SHARED(tesseract::kinematics::InverseKinematics)
DARP_UNIQUE_PTR_TO_SHARED(tesseract::collision::DiscreteContactManager)
DARP_UNIQUE_PTR_TO_SHARED(tesseract::collision::ContinuousContactManager)

DARP_MOVE_ONLY_VALUE_TO_SHARED(tesseract::scene_graph::Joint)
DARP_MOVE_ONLY_VALUE_TO_SHARED(tesseract::scene_graph::Link)

/* Apply these after %shared_ptr: that macro installs ownership features for
 * the type and otherwise supersedes an earlier overload-specific ignore. */
%ignore tesseract::scene_graph::Joint::Joint(tesseract::scene_graph::Joint&&);
%ignore tesseract::scene_graph::Joint::Joint(Joint&&);
%ignore tesseract::scene_graph::Link::Link(tesseract::scene_graph::Link&&);
%ignore tesseract::scene_graph::Link::Link(Link&&);
%ignore tesseract::scene_graph::SceneGraph::SceneGraph(tesseract::scene_graph::SceneGraph&&);
%ignore tesseract::scene_graph::SceneGraph::SceneGraph(SceneGraph&&);

/* Groups returned by Environment can contain plugin-defined implementations.
 * Retaining the Environment keeps its plugin loader alive for the entire
 * managed lifetime of each group. This is a type-level ownership rule, not a
 * hand-written forwarding API. */
%typemap(cscode) tesseract::kinematics::JointGroup %{
  private object swigOwner;
  internal void swigRetainOwner(object owner) => swigOwner = owner;
%}
%typemap(csout, excode=SWIGEXCODE) std::shared_ptr<const tesseract::kinematics::JointGroup> {
    global::System.IntPtr cPtr = $imcall;
    JointGroup ret = (cPtr == global::System.IntPtr.Zero) ? null : new JointGroup(cPtr, true);$excode
    ret?.swigRetainOwner(this);
    return ret;
  }

%typemap(csout, excode=SWIGEXCODE) std::shared_ptr<const tesseract::kinematics::KinematicGroup> {
    global::System.IntPtr cPtr = $imcall;
    KinematicGroup ret = (cPtr == global::System.IntPtr.Zero) ? null : new KinematicGroup(cPtr, true);$excode
    ret?.swigRetainOwner(this);
    return ret;
  }

%include "components/common.i"
%include "components/geometry.i"
%include "components/scene_graph.i"
%include "components/robot_description.i"
%include "components/state_solver.i"
%include "components/collision.i"
%include "components/kinematics.i"
%include "components/environment.i"
