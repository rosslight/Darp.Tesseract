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
