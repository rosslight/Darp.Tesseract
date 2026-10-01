/* Constructors own immutable snapshots of managed vertex and face inputs. */
%{
namespace
{
std::shared_ptr<const Eigen::VectorXi> darpMeshFaces(
    const tesseract::common::VectorVector3d& vertices, const std::vector<int>& faces, bool triangles)
{
  if (vertices.empty() || faces.empty())
    throw std::invalid_argument("A mesh requires vertices and faces.");
  if (vertices.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()) ||
      faces.size() > static_cast<std::size_t>(std::numeric_limits<int>::max()))
    throw std::overflow_error("Mesh data exceeds Int32.");
  for (std::size_t i = 0; i < faces.size();)
  {
    const int count = faces[i++];
    if (count < 3 || (triangles && count != 3) || static_cast<std::size_t>(count) > faces.size() - i)
      throw std::invalid_argument("Faces must contain a vertex count followed by that many indices.");
    for (int j = 0; j < count; ++j)
    {
      const int index = faces[i++];
      if (index < 0 || static_cast<std::size_t>(index) >= vertices.size())
        throw std::out_of_range("Mesh vertex index is out of range.");
    }
  }
  auto result = std::make_shared<Eigen::VectorXi>(static_cast<Eigen::Index>(faces.size()));
  for (std::size_t i = 0; i < faces.size(); ++i)
    (*result)[static_cast<Eigen::Index>(i)] = faces[i];
  return result;
}
}
%}

%rename(Mesh) tesseract::geometry::Mesh::Mesh(
    const tesseract::common::VectorVector3d&, const std::vector<int>&);
%extend tesseract::geometry::Mesh
{
  Mesh(const tesseract::common::VectorVector3d& vertices, const std::vector<int>& faces)
  {
    auto data = darpMeshFaces(vertices, faces, true);
    return new tesseract::geometry::Mesh(
        std::make_shared<const tesseract::common::VectorVector3d>(vertices), std::move(data));
  }
}
%rename(ConvexMesh) tesseract::geometry::ConvexMesh::ConvexMesh(
    const tesseract::common::VectorVector3d&, const std::vector<int>&);
%extend tesseract::geometry::ConvexMesh
{
  ConvexMesh(const tesseract::common::VectorVector3d& vertices, const std::vector<int>& faces)
  {
    auto data = darpMeshFaces(vertices, faces, false);
    return new tesseract::geometry::ConvexMesh(
        std::make_shared<const tesseract::common::VectorVector3d>(vertices), std::move(data));
  }
}

%template(MeshVector) std::vector<std::shared_ptr<tesseract::geometry::Mesh>>;
%rename(createMeshFromPath) tesseract::geometry::createMeshFromPathForBinding;
%rename(createMeshFromResource) tesseract::geometry::createMeshFromResourceForBinding;
%rename(makeConvexMesh) tesseract::collision::makeConvexMeshForBinding;
%inline %{
namespace tesseract::geometry
{
std::vector<std::shared_ptr<Mesh>> createMeshFromPathForBinding(
    const std::string& path, const Eigen::Vector3d& scale = Eigen::Vector3d(1, 1, 1),
    bool triangulate = true, bool flatten = false, bool normals = false,
    bool vertex_colors = false, bool material_and_texture = false)
{
  return createMeshFromPath<Mesh>(path, scale, triangulate, flatten, normals, vertex_colors, material_and_texture);
}
std::vector<std::shared_ptr<Mesh>> createMeshFromResourceForBinding(
    tesseract::common::Resource::Ptr resource, const Eigen::Vector3d& scale = Eigen::Vector3d(1, 1, 1),
    bool triangulate = true, bool flatten = false, bool normals = false,
    bool vertex_colors = false, bool material_and_texture = false)
{
  return createMeshFromResource<Mesh>(std::move(resource), scale, triangulate, flatten, normals,
                                      vertex_colors, material_and_texture);
}
}
namespace tesseract::collision
{
tesseract::geometry::ConvexMesh::Ptr makeConvexMeshForBinding(const tesseract::geometry::Mesh& mesh)
{
  if (mesh.getVertexCount() == 0)
    throw std::invalid_argument("A convex hull requires mesh vertices.");
  return makeConvexMesh(mesh);
}
}
%}

%template(CollisionVector) std::vector<std::shared_ptr<tesseract::scene_graph::Collision>>;
%extend tesseract::scene_graph::Link
{
  void addCollision(const std::shared_ptr<tesseract::scene_graph::Collision>& collision)
  {
    if (!collision)
      throw std::invalid_argument("Collision must not be null.");
    $self->collision.push_back(collision);
  }
  void addVisual(const std::shared_ptr<tesseract::scene_graph::Visual>& visual)
  {
    if (!visual)
      throw std::invalid_argument("Visual must not be null.");
    $self->visual.push_back(visual);
  }
  std::vector<std::shared_ptr<tesseract::scene_graph::Collision>> getCollisions() const
  {
    return $self->collision;
  }
}
