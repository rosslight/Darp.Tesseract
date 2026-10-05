# Darp.Tesseract.Native

Generated .NET 10 bindings for Tesseract Robotics. The package includes the native
runtime and uses `Aardvark.Base` for fixed-size vectors, quaternions and transforms.

Method names follow Tesseract's C++ API. Look in [Generated](Generated) for the
available C# signatures. For build prerequisites and platform support, see the
[repository README](../../README.md).

## Load a robot and compute kinematics

This example uses the ABB robot fixture included in the repository. Run it from
the repository root in an application referencing the binding package or project.

```csharp
using Aardvark.Base;
using Darp.Tesseract.Native;
using TesseractEnvironment = Darp.Tesseract.Native.Environment;

var assets = Path.GetFullPath(
    "tests/Darp.Tesseract.Native.IntegrationTests/Assets");
var robot = Path.Combine(assets, "darp_test");

using var locator = new GeneralResourceLocator();
locator.addPath(assets);

using var sceneGraph = TesseractNative.parseURDFString(
    File.ReadAllText(Path.Combine(robot, "abb_irb2400.urdf")), locator);
using var srdf = new SRDFModel();
srdf.initString(
    sceneGraph,
    File.ReadAllText(Path.Combine(robot, "abb_irb2400.srdf")),
    locator);

using var environment = new TesseractEnvironment();
if (!environment.init(sceneGraph, srdf))
    throw new InvalidOperationException("Could not initialize the robot.");

using var group = environment.getKinematicGroup("manipulator");
var seed = new double[checked((int)group.numJoints())];
using var poses = group.calcFwdKin(seed);
var tool = poses["tool0"];
var position = tool.Trans;
var jacobian = group.calcJacobian(seed, "tool0");

Console.WriteLine(position);
Console.WriteLine($"Jacobian: {jacobian.GetLength(0)} by {jacobian.GetLength(1)}");

var target = new KinGroupIKInput(
    tool, group.getBaseLinkName(), "tool0");
using var solutions = group.calcInvKin(target, seed);
foreach (var solution in solutions)
    Console.WriteLine(string.Join(", ", solution));
```

For your robot, change the URDF, SRDF, resource path, group name and tip link.
The SRDF and referenced YAML configure the kinematics plugins.
[The integration tests](../../tests/Darp.Tesseract.Native.IntegrationTests/KinematicsTests.cs)
also show collision-manager setup, environment commands and joint-state access.

## Configure stock planning pipelines

The package copies `Tesseract/task_composer_plugins.yaml` to the application's
output directory. It contains the stock Descartes, OMPL, TrajOpt, Cartesian,
Freespace and raster pipelines supported by the embedded factories. Ifopt
pipelines are excluded because this package does not build that planner.

Register profiles under the node's namespace and the profile name used by the
program. `StockProfileNamespaces` provides the default node namespaces. A custom
YAML `namespace` overrides these defaults.

```csharp
using var profiles = new ProfileDictionary();
using var timing = new IterativeSplineParameterizationCompositeProfile(0.5, 0.5);
profiles.addProfile(
    StockProfileNamespaces.IterativeSplineParameterization, "DEFAULT", timing);

using var interpolation = new SimplePlannerLVSNoIKMoveProfile
{
    translation_longest_valid_segment_length = 0.002,
};
profiles.addProfile(
    StockProfileNamespaces.SimpleMotionPlanner, "DEFAULT", interpolation);

using var ompl = new OMPLRealVectorMoveProfile();
profiles.addProfile(StockProfileNamespaces.OmplMotionPlanner, "DEFAULT", ompl);
using var trajOpt = new TrajOptDefaultCompositeProfile();
profiles.addProfile(StockProfileNamespaces.TrajOptMotionPlanner, "DEFAULT", trajOpt);
```

ISP move profiles can override the composite's velocity and acceleration scaling
for individual instructions. Simple planner profiles control interpolation in
`SimpleMotionPlannerTask`, which the stock raster pipelines use. The stock
`CartesianPipeline` uses Descartes and TrajOpt. Upsampling, minimum trajectory
length, bounds repair, collision repair, kinematic checks and profile switching
also have native profile classes. Their fields follow the upstream API.

`ProfileDictionary` retains shared ownership of registered profiles, so disposing
the original managed profile wrapper does not remove the registration. Descartes
template profiles use the existing `TesseractNative.asProfile` bridge; the other
stock profiles inherit `Profile` directly. The
[pipeline integration tests](../../tests/Darp.Tesseract.Native.IntegrationTests/StockPipelineTests.cs)
show program construction, execution and result extraction.

Nested configuration objects and the collision correction workflow are mutable
native views. Keep the profile alive while using those views.

Collision-aware pipelines need a default contact manager configured in the SRDF.
For a robot whose resources live in a `my_robot` resource directory, a minimal
SRDF can reference both plugin configurations:

```xml
<robot name="my_robot">
  <group name="manipulator">
    <chain base_link="base_link" tip_link="tool0"/>
  </group>
  <kinematics_plugin_config filename="package://my_robot/kinematics_plugins.yaml"/>
  <contact_managers_plugin_config filename="package://my_robot/contact_manager_plugins.yaml"/>
</robot>
```

Keep the kinematics configuration appropriate for that robot. The contact-manager
configuration can use the embedded Bullet factories:

```yaml
contact_manager_plugins:
  search_libraries:
    - tesseract_collision_bullet_factories
  discrete_plugins:
    default: BulletDiscreteBVHManager
    plugins:
      BulletDiscreteBVHManager:
        class: BulletDiscreteBVHManagerFactory
  continuous_plugins:
    default: BulletCastBVHManager
    plugins:
      BulletCastBVHManager:
        class: BulletCastBVHManagerFactory
```

Add the resources' parent directory to `GeneralResourceLocator` before parsing
the SRDF. The configuration uses embedded factories and requires no absolute
native-library search path. Add the robot's allowed collision pairs to its SRDF
as needed; the ABB fixture demonstrates this.

## Retime a planned program

Standalone solvers update a `CompositeInstruction` in place. Its moves must use
state waypoints, as produced by the stock planning pipelines. Register timing
profiles under the solver's name and the program's profile name:

```csharp
using var solver = new TimeOptimalTrajectoryGeneration("retime");
using var profiles = new ProfileDictionary();
using var timing = new TimeOptimalTrajectoryGenerationCompositeProfile
{
    max_velocity_scaling_factor = 0.5,
    max_acceleration_scaling_factor = 0.25,
    path_tolerance = 0.01,
};
profiles.addProfile(solver.getName(), "DEFAULT", timing);

if (!solver.compute(program, environment, profiles))
    throw new InvalidOperationException("Could not retime the program.");

var trajectory = new InstructionsTrajectory(program);
Console.WriteLine(trajectory.getTimeFromStart(trajectory.size() - 1));
var velocity = trajectory.getVelocity(1);
var acceleration = trajectory.getAcceleration(1);
```

`IterativeSplineParameterization` uses the ISP profiles described above.
`ConstantTCPSpeedParameterization` uses
`ConstantTCPSpeedParameterizationCompositeProfile`, with translational and
rotational velocity/acceleration limits and scaling factors. The pinned upstream
constant-TCP implementation is experimental and does not guarantee enforcement
of joint limits; use it for evaluation and validate the resulting trajectory.

`InstructionsTrajectory` retains its managed program and creates a short native
view for each operation. Reads return vector copies; `setData` writes velocity,
acceleration and time back to the current waypoint. Structural changes are
resolved on the next call. Explicitly disposing the program invalidates the
view, and subsequent calls throw `ObjectDisposedException`. The view owns no
native resource and needs no disposal. Do not mutate or dispose the program
concurrently with a solver or view operation. Each view call flattens the program;
this interface is intended for trajectory inspection and editing.
An empty program has size zero; `dof()` requires at least one state waypoint and
throws a managed `ApplicationException` from the native constructor's empty check.

The runtime also embeds `TimeOptimalParameterizationTaskFactory` and
`ConstantTCPSpeedParameterizationTaskFactory`. Configure them in custom Task
Composer YAML with `program`, `environment` and `profiles` inputs and a `program`
output. Their default profile namespaces are
`StockProfileNamespaces.TimeOptimalParameterization` and
`StockProfileNamespaces.ConstantTCPSpeedParameterization`.
[The timing YAML fixture](../../tests/Darp.Tesseract.Native.IntegrationTests/Assets/darp_test/timing_tasks.yaml)
shows a Simple-planner/TOTG pipeline, and
[the timing tests](../../tests/Darp.Tesseract.Native.IntegrationTests/TimeParameterizationTests.cs)
exercise standalone retiming and trajectory lifetime handling.

## Configure robot plugins at runtime

`PluginInfo.setConfigString` parses a YAML document directly. The existing
`getConfigString` serializes it. `addPlugin` and `setFwdPluginInfo`/`setInvPluginInfo`
copy their inputs. `getPlugin` and `getFwdPluginInfo`/`getInvPluginInfo` return
detached copies, including YAML data:

```csharp
using var plugin = new PluginInfo { class_name = "URInvKinFactory" };
plugin.setConfigString("""
    base_link: base_link
    tip_link: tool0
    params:
      d1: 0.1273
      a2: -0.612
      a3: -0.5723
      d4: 0.163941
      d5: 0.1157
      d6: 0.0922
    """);
using var plugins = new PluginInfoContainer { default_plugin = "RuntimeUR" };
plugins.addPlugin("RuntimeUR", plugin);
using var information = new KinematicsPluginInfo();
information.setInvPluginInfo("manipulator", plugins);
using var kinematics = new KinematicsInformation { kinematics_plugin_info = information };
using var command = new AddKinematicsInformationCommand(kinematics);
if (!environment.applyCommand(command))
    throw new InvalidOperationException("Could not configure robot kinematics.");
using var group = environment.getKinematicGroup("manipulator", "RuntimeUR");
```

These parameters describe the UR10 fixture; use parameters and frames matching
the actual robot. `setFwdPluginInfo` configures forward-kinematics plugins.
`PluginInfoContainer.addPlugin` also works with the discrete/continuous containers
in `ContactManagersPluginInfo`, applied through
`AddContactManagersPluginInfoCommand`. Keep the parent `ContactManagersPluginInfo`
alive while editing its discrete/continuous container views. Factories are
embedded in the runtime.
Malformed YAML throws a managed exception and leaves the previous configuration
intact. Temporary configuration wrappers can be disposed after their data has
been copied into the command.

## Clone environments and use kinematics utilities

`Environment.clone()` owns an independent native environment, including state,
scene commands and plugin configuration. Dispose the clone when finished. It
can be used after disposing the source environment; separate clones allow
planning and live kinematics to use different state.

`KinGroupIKInputs` stores copies of `KinGroupIKInput` values. Add inputs with
`push_back`, inspect them with `at` (an owned copy), and pass the container to
`KinematicGroup.calcInvKinMultiple`. The native API solves all supplied tip
constraints together; provide one input per IK solver tip. A single-tool robot
therefore needs one input. Both returning and `ref IKSolutions` forms are bound.

`TesseractNative.getRedundantSolutions(solution, limits, indices)` returns
additional configurations obtained by adding or subtracting full turns within
joint limits. The original solution is excluded. Supply a two-column limit
matrix with one row per joint and an `IndexVector` of redundancy-capable joints.

### UR and OPW frame conventions

`KinGroupIKInput.pose` is expressed in its `working_frame` and targets its
`tip_link_name`. The group converts supported working frames and fixed tool
offsets into the solver's configured base and tip frames. Use the group's
`getAllValidWorkingFrames` and `getAllPossibleTipLinkNames` to inspect them.

The UR solver uses the conventional UR chain ending at `tool0`; it applies a
180-degree rotation around the configured base's Z axis internally. The OPW
solver passes the configured base-to-tip pose directly to OPW and uses its
parameter offsets/sign corrections for the robot's joint conventions. A UR
parameter set applied to an arbitrary six-joint URDF need not reproduce its FK.
Verify the configured model with an FK/IK round trip, including the actual TCP.
[The UR10 fixture](../../tests/Darp.Tesseract.Native.IntegrationTests/Assets/darp_test/ur10.urdf)
and [robot construction tests](../../tests/Darp.Tesseract.Native.IntegrationTests/RobotConstructionTests.cs)
exercise runtime configuration and an offset weld TCP.

## Construct scene geometry

`Mesh(vertices, faces)` and `ConvexMesh(vertices, faces)` own copies of a
`VectorVector3d` and an `IntVector`. Faces use the native encoding: the vertex
count followed by that many vertex indices, repeated for each face. `Mesh`
requires triangles; `ConvexMesh` accepts polygon faces and assumes the supplied
geometry is convex. Invalid face encoding or indices throw managed exceptions.
The input containers can be disposed after construction.

`TesseractNative.createMeshFromPath` and `createMeshFromResource` load meshes with
the native scale and import options and return a `MeshVector`. Both loaders
triangulate by default because `Mesh` requires triangles, matching nanobind.
Meshes retrieved
from the vector retain shared native ownership after the vector or resource is
disposed. `TesseractNative.makeConvexMesh(mesh)` computes a convex hull using the
already-built Bullet implementation.

Set a `Collision` or `Visual` object's `geometry`, then attach it with
`Link.addCollision` or `Link.addVisual`. These methods retain shared ownership;
`getCollisions` and `getVisuals` return owned vectors of shared objects. Apply
`AddLinkCommand(link, joint)` to connect the link to an environment.
[The scene construction tests](../../tests/Darp.Tesseract.Native.IntegrationTests/SceneConstructionTests.cs)
demonstrate mesh loading, lifetime handling and collision detection with newly
constructed geometry.

## Geometry inputs and results

Fixed-size Eigen values map to Aardvark.Base values: `V2d`, `V3d`, `V4d`,
`QuaternionD`, and `Euclidean3d` for rigid transforms. Dynamic vectors use `double[]`;
dynamic matrices use `double[,]` with `[row, column]` indexing. The API checks
native shapes when converting results and inputs.

Every call copies geometry inputs into a temporary column-major buffer. Every
result copies coefficients out of native memory before returning. You can keep a
result after disposing its native collection. Changing a returned array does not
change Tesseract state or other results.

```csharp
var poses = group.calcFwdKin(seed);
Euclidean3d tool = poses["tool0"];  // Native coefficients have already been copied.
poses.Dispose();
Console.WriteLine(tool.Trans.X); // Translation X remains available.
```

Native collections such as `TransformMap` and `IKSolutions` still own native
resources and must be disposed. Their elements are managed copies. Writable
`ref T` outputs replace the caller's value after the call. Writable `Eigen::Ref`
outputs copy into an existing `double[]` or `double[,]` and cannot resize it.

Aardvark's structs have writable fields, and arrays are mutable. They provide
read-only *native access* here: mutations to a returned value never write back
to Tesseract. For transform inputs, use `Euclidean3d` to represent rotation and translation directly.

## Change or extend a binding

Edit the SWIG inputs, then regenerate both managed and native output:

```powershell
pixi run -e bindings generate-bindings
pixi run build-native
dotnet build src/Darp.Tesseract.Native/Darp.Tesseract.Native.csproj
```

Run these commands from the repository root. Rebuilding the native wrapper matters
when a change alters the generated interop signatures.

| Change | Source |
| --- | --- |
| Expose an upstream header or exclude a signature | [bindings/components](../../bindings/components) |
| Add an Eigen type or geometry container mapping | [mappings.i](../../bindings/geometry/mappings.i) |
| Change generated C# conversion or ownership code | [typemaps.i](../../bindings/geometry/typemaps.i) |
| Change native Eigen conversion behavior | [runtime.h](../../bindings/geometry/runtime.h) |
| Change managed copying or containers | [Runtime](Runtime) |
| Change shared SWIG behavior for other C++ types | [bindings/support](../../bindings/support) |

SWIG writes C# files to `src/Darp.Tesseract.Native/Generated/` and the C++ wrapper
to `bindings/generated/`. Review and commit both outputs with the input changes.
The generated files are not the place to fix conversion behavior.
