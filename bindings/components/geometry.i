%include <tesseract/geometry/geometry.h>
%include <tesseract/geometry/impl/mesh_material.h>
%template(MeshTextureVector) std::vector<std::shared_ptr<tesseract::geometry::MeshTexture>>;

%ignore tesseract::geometry::PolygonMesh::getFaces;
%ignore tesseract::geometry::PolygonMesh::getTextures;
%rename(getFaces) tesseract::geometry::PolygonMesh::getFacesForBinding;
%rename(getTextures) tesseract::geometry::PolygonMesh::getTexturesForBinding;
%extend tesseract::geometry::PolygonMesh
{
  std::vector<int> getFacesForBinding() const
  {
    const auto& faces = $self->getFaces();
    if (!faces)
      return {};
    return { faces->data(), faces->data() + faces->size() };
  }

  std::vector<std::shared_ptr<tesseract::geometry::MeshTexture>> getTexturesForBinding() const
  {
    const auto& textures = $self->getTextures();
    return textures ? *textures : std::vector<std::shared_ptr<tesseract::geometry::MeshTexture>>{};
  }
}

%template(IntVector) std::vector<int>;
%include <tesseract/geometry/impl/box.h>
%include <tesseract/geometry/impl/sphere.h>
%include <tesseract/geometry/impl/cylinder.h>
%include <tesseract/geometry/impl/capsule.h>
%include <tesseract/geometry/impl/cone.h>
%include <tesseract/geometry/impl/plane.h>
%include <tesseract/geometry/impl/polygon_mesh.h>
%include <tesseract/geometry/impl/mesh.h>
%include <tesseract/geometry/impl/convex_mesh.h>
%include <tesseract/geometry/impl/sdf_mesh.h>
%include <tesseract/geometry/impl/compound_mesh.h>
%template(PolygonMeshVector) std::vector<std::shared_ptr<tesseract::geometry::PolygonMesh>>;

%rename(OctomapTree) octomap::OcTree;
%rename(writeBinary) octomap::OcTree::writeBinaryConst;
%nodefaultctor octomap::OcTree;
%shared_ptr(octomap::OcTree)
namespace octomap
{
class OcTree
{
public:
  bool writeBinaryConst(const std::string& filename) const;
};
}
%include <tesseract/geometry/impl/octree.h>

%inline %{
namespace tesseract::geometry
{
template <typename T>
std::shared_ptr<const T> requireGeometry(const std::shared_ptr<const Geometry>& geometry, GeometryType expected)
{
  if (!geometry || geometry->getType() != expected)
    throw std::invalid_argument("Geometry has an unexpected type.");
  return std::static_pointer_cast<const T>(geometry);
}

std::shared_ptr<const Box> asBox(const std::shared_ptr<const Geometry>& geometry)
{
  return requireGeometry<Box>(geometry, GeometryType::BOX);
}
std::shared_ptr<const Sphere> asSphere(const std::shared_ptr<const Geometry>& geometry)
{
  return requireGeometry<Sphere>(geometry, GeometryType::SPHERE);
}
std::shared_ptr<const Cylinder> asCylinder(const std::shared_ptr<const Geometry>& geometry)
{
  return requireGeometry<Cylinder>(geometry, GeometryType::CYLINDER);
}
std::shared_ptr<const Capsule> asCapsule(const std::shared_ptr<const Geometry>& geometry)
{
  return requireGeometry<Capsule>(geometry, GeometryType::CAPSULE);
}
std::shared_ptr<const Cone> asCone(const std::shared_ptr<const Geometry>& geometry)
{
  return requireGeometry<Cone>(geometry, GeometryType::CONE);
}
std::shared_ptr<const Plane> asPlane(const std::shared_ptr<const Geometry>& geometry)
{
  return requireGeometry<Plane>(geometry, GeometryType::PLANE);
}
std::shared_ptr<const PolygonMesh> asPolygonMesh(const std::shared_ptr<const Geometry>& geometry)
{
  const auto type = geometry ? geometry->getType() : GeometryType::UNINITIALIZED;
  if (type != GeometryType::MESH && type != GeometryType::CONVEX_MESH && type != GeometryType::SDF_MESH &&
      type != GeometryType::POLYGON_MESH)
    throw std::invalid_argument("Geometry is not a polygon mesh.");
  return std::static_pointer_cast<const PolygonMesh>(geometry);
}
std::shared_ptr<const CompoundMesh> asCompoundMesh(const std::shared_ptr<const Geometry>& geometry)
{
  return requireGeometry<CompoundMesh>(geometry, GeometryType::COMPOUND_MESH);
}
std::shared_ptr<const Octree> asOctree(const std::shared_ptr<const Geometry>& geometry)
{
  return requireGeometry<Octree>(geometry, GeometryType::OCTREE);
}
}
%}
%include <tesseract/geometry/geometries.h>
%include <tesseract/geometry/utils.h>
%include <tesseract/geometry/mesh_parser.h>
