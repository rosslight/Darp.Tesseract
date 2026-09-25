using Shouldly;
using Xunit;

namespace Darp.Tesseract.Native.IntegrationTests;

public sealed class VisualizationBindingsTests
{
    [Fact]
    public void UrdfVisualGeometryIsAvailableForRendering()
    {
        const string urdf = """
            <robot name="visual_test" xmlns:tesseract="https://github.com/tesseract-robotics/tesseract" tesseract:make_convex="false">
              <link name="base_link">
                <visual name="body">
                  <origin xyz="1 2 3" rpy="0 0 0" />
                  <geometry>
                    <box size="0.1 0.2 0.3" />
                  </geometry>
                  <material name="paint">
                    <color rgba="0.1 0.2 0.3 0.4" />
                  </material>
                </visual>
              </link>
              <link name="tip_link" />
              <joint name="fixed_joint" type="fixed">
                <parent link="base_link" />
                <child link="tip_link" />
              </joint>
            </robot>
            """;

        using var locator = new GeneralResourceLocator();
        using var sceneGraph = TesseractNative.parseURDFString(urdf, locator);
        sceneGraph.ShouldNotBeNull();

        using var link = sceneGraph.getLink("base_link");
        link.ShouldNotBeNull();
        using var visuals = link.visual;
        visuals.Count.ShouldBe(1);

        using var visual = visuals[0];
        visual.name.ShouldBe("body");
        visual.origin.Trans.ShouldBe(new(1, 2, 3));

        using var geometry = visual.geometry;
        geometry.getType().ShouldBe(GeometryType.BOX);
        using var box = TesseractNative.asBox(geometry);
        box.getX().ShouldBe(0.1);
        box.getY().ShouldBe(0.2);
        box.getZ().ShouldBe(0.3);

        using var material = visual.material;
        material.ShouldNotBeNull();
        material.getName().ShouldBe("paint");
        material.color.ShouldBe(new(0.1, 0.2, 0.3, 0.4));
    }
}
