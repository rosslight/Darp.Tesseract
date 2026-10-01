using Aardvark.Base;
using Shouldly;
using Xunit;

namespace Darp.Tesseract.Native.IntegrationTests;

public sealed class SceneConstructionTests
{
    [Fact]
    public void MeshConstructorsOwnTheirInputsAndRejectMalformedFaces()
    {
        using var vertices = CreateVertices();
        using var faces = CreateFaces();
        using var mesh = new Mesh(vertices, faces);
        vertices.Count.ShouldBe(4);
        faces.Count.ShouldBe(16);
        using var convex = new ConvexMesh(vertices, faces);
        faces[1] = 99;
        vertices.Dispose();
        faces.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        using var savedVertices = mesh.getVertices();
        savedVertices[0].ShouldBe(V3d.Zero);
        mesh.getVertexCount().ShouldBe(4);
        mesh.getFaceCount().ShouldBe(4);
        convex.getVertexCount().ShouldBe(4);
        using var savedFaces = mesh.getFaces();
        savedFaces[1].ShouldBe(0);
        using var freshVertices = CreateVertices();
        using var truncated = new IntVector { 3, 0, 1 };
        Should.Throw<ArgumentException>(() => new Mesh(freshVertices, truncated));
        using var badIndex = new IntVector { 3, 0, 1, 9 };
        Should.Throw<IndexOutOfRangeException>(() => new Mesh(freshVertices, badIndex));
    }

    [Fact]
    public void MeshLoadersApplyScaleAndRetainMeshesAfterSourceDisposal()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "darp_test", "tetrahedron.obj");
        var scale = new V3d(2, 3, 4);
        using var fromPath = TesseractNative.createMeshFromPath(path, scale, true);
        fromPath.Count.ShouldBe(1);
        using var resource = new SimpleLocatedResource("file://tetrahedron.obj", path);
        using var fromResource = TesseractNative.createMeshFromResource(resource, scale, true);
        fromResource.Count.ShouldBe(1);
        using var mesh = fromResource[0];
        fromResource.Dispose();
        resource.Dispose();
        mesh.getScale().ShouldBe(scale);
        using var vertices = mesh.getVertices();
        vertices.ShouldContain(new V3d(2, 0, 0));
        vertices.ShouldContain(new V3d(0, 3, 0));
        vertices.ShouldContain(new V3d(0, 0, 4));
        using var convex = TesseractNative.makeConvexMesh(mesh);
        convex.getCreationMethod().ShouldBe(ConvexMesh.CreationMethod.CONVERTED);
        convex.getVertexCount().ShouldBe(4);
        using var expected = fromPath[0];
        using var expectedVertices = expected.getVertices();
        vertices.OrderBy(v => v.X).ThenBy(v => v.Y).ThenBy(v => v.Z)
            .ShouldBe(expectedVertices.OrderBy(v => v.X).ThenBy(v => v.Y).ThenBy(v => v.Z));
        var quadPath = Path.Combine(AppContext.BaseDirectory, "Assets", "darp_test", "quad.obj");
        using var quadFromPath = TesseractNative.createMeshFromPath(quadPath);
        using var quadResource = new SimpleLocatedResource("file://quad.obj", quadPath);
        using var quadFromResource = TesseractNative.createMeshFromResource(quadResource);
        using var triangulatedPath = quadFromPath[0];
        using var triangulatedResource = quadFromResource[0];
        triangulatedPath.getFaceCount().ShouldBe(2);
        triangulatedResource.getFaceCount().ShouldBe(2);
    }

    [Fact]
    public void ConstructedMeshCollisionGeometryCanBeAddedToAnEnvironment()
    {
        using var environment = StockPipelineTests.CreateEnvironment();
        using var vertices = CreateVertices();
        using var faces = CreateFaces();
        using var mesh = new Mesh(vertices, faces);
        using var convex = TesseractNative.makeConvexMesh(mesh);
        using var link = new Link("mesh_tool");
        using var collision = new Collision { geometry = convex };
        using var visual = new Visual { geometry = mesh };
        link.addCollision(collision);
        link.addVisual(visual);
        collision.Dispose();
        visual.Dispose();
        convex.Dispose();
        mesh.Dispose();
        using var joint = new Joint("mesh_tool_joint")
        {
            type = JointType.FIXED,
            parent_link_name = "base_link",
            child_link_name = "mesh_tool",
            parent_to_joint_origin_transform = new Euclidean3d(Rot3d.Identity, new V3d(2, 0, 0)),
        };
        using var add = new AddLinkCommand(link, joint);
        environment.applyCommand(add).ShouldBeTrue();
        link.Dispose();
        using var sceneLink = environment.getLink("mesh_tool");
        using var collisions = sceneLink.getCollisions();
        using var visuals = sceneLink.getVisuals();
        collisions.Count.ShouldBe(1);
        visuals.Count.ShouldBe(1);
        using var stored = collisions[0];
        using var geometry = stored.geometry;
        geometry.getType().ShouldBe(GeometryType.CONVEX_MESH);
        using var obstacleGeometry = new Sphere(0.1);
        using var obstacleCollision = new Collision { geometry = obstacleGeometry };
        using var obstacle = new Link("mesh_obstacle");
        obstacle.addCollision(obstacleCollision);
        using var obstacleJoint = new Joint("mesh_obstacle_joint")
        {
            type = JointType.FIXED,
            parent_link_name = "base_link",
            child_link_name = "mesh_obstacle",
            parent_to_joint_origin_transform = new Euclidean3d(Rot3d.Identity, new V3d(2.1, 0.1, 0.1)),
        };
        using var addObstacle = new AddLinkCommand(obstacle, obstacleJoint);
        environment.applyCommand(addObstacle).ShouldBeTrue();
        using var manager = environment.getDiscreteContactManager();
        using var active = new StringVector { "mesh_tool" };
        manager.setActiveCollisionObjects(active);
        using var request = new ContactRequest(ContactTestType.ALL);
        using var contacts = new ContactResultMap();
        manager.contactTest(contacts, request);
        using var results = TesseractNative.flattenContactResults(contacts);
        results.Any(contact =>
        {
            using (contact)
            using (var links = contact.link_names)
                return links.Contains("mesh_tool") && links.Contains("mesh_obstacle");
        }).ShouldBeTrue();
    }

    private static VectorVector3d CreateVertices() => new(new V3d[]
    {
        V3d.Zero, new(1, 0, 0), new(0, 1, 0), new(0, 0, 1),
    });

    private static IntVector CreateFaces() => new(new[]
    {
        3, 0, 2, 1, 3, 0, 1, 3, 3, 0, 3, 2, 3, 1, 2, 3,
    });
}
