/* Concrete geometry mappings. Conversion and ownership rules are shared in typemaps.i. */
%include "geometry/typemaps.i"
%define DARP_MATRIX_ARRAY double[,] %enddef
DARP_TENSOR(Eigen::Vector2d, global::Aardvark.Base.V2d, Vector2, global::Aardvark.Base.V2d, TakeVector2)
DARP_TENSOR(Eigen::Vector3d, global::Aardvark.Base.V3d, Vector3, global::Aardvark.Base.V3d, TakeVector3)
DARP_TENSOR(Eigen::Vector4d, global::Aardvark.Base.V4d, Vector4, global::Aardvark.Base.V4d, TakeVector4)
DARP_TENSOR(Eigen::VectorXd, double[], Vector, double[], TakeVector)
DARP_TENSOR(Eigen::MatrixXd, DARP_MATRIX_ARRAY, Matrix, DARP_MATRIX_ARRAY, TakeMatrix)
DARP_TENSOR(Eigen::MatrixX2d, DARP_MATRIX_ARRAY, Matrix, DARP_MATRIX_ARRAY, TakeMatrix)
DARP_TENSOR(Eigen::Quaterniond, global::Aardvark.Base.QuaternionD, Quaternion, global::Aardvark.Base.QuaternionD, TakeQuaternion)
DARP_TENSOR(Eigen::Isometry3d, global::Aardvark.Base.Euclidean3d, Isometry, global::Aardvark.Base.Euclidean3d, TakeIsometry)
DARP_TENSOR_REF(Eigen::VectorXd, double[], double[])
DARP_TENSOR_REF(Eigen::MatrixXd, DARP_MATRIX_ARRAY, DARP_MATRIX_ARRAY)
DARP_CONTAINER(tesseract::common::TransformMap, TransformMap)
DARP_MAP_PROXY(tesseract::common::TransformMap, TransformMap, global::Aardvark.Base.Euclidean3d, 1, Isometry)
DARP_CONTAINER(tesseract::common::VectorIsometry3d, VectorIsometry3d)
DARP_LIST_PROXY(tesseract::common::VectorIsometry3d, VectorIsometry3d, global::Aardvark.Base.Euclidean3d, 2, Isometry)
DARP_CONTAINER(tesseract::kinematics::IKSolutions, IKSolutions)
DARP_LIST_PROXY(tesseract::kinematics::IKSolutions, IKSolutions, double[], 3, Vector)
DARP_CONTAINER(tesseract::common::VectorVector2d, VectorVector2d)
DARP_SHARED_CONTAINER_RESULT(tesseract::common::VectorVector2d, VectorVector2d)
DARP_LIST_PROXY(tesseract::common::VectorVector2d, VectorVector2d, global::Aardvark.Base.V2d, 4, Vector2)
DARP_CONTAINER(tesseract::common::VectorVector3d, VectorVector3d)
DARP_SHARED_CONTAINER_RESULT(tesseract::common::VectorVector3d, VectorVector3d)
DARP_LIST_PROXY(tesseract::common::VectorVector3d, VectorVector3d, global::Aardvark.Base.V3d, 5, Vector3)
DARP_CONTAINER(tesseract::common::VectorVector4d, VectorVector4d)
DARP_SHARED_CONTAINER_RESULT(tesseract::common::VectorVector4d, VectorVector4d)
DARP_LIST_PROXY(tesseract::common::VectorVector4d, VectorVector4d, global::Aardvark.Base.V4d, 6, Vector4)
%typemap(csclassmodifiers) DarpGeometryInterop "internal class";
%nodefaultctor DarpGeometryInterop;
%nodefaultdtor DarpGeometryInterop;
%inline %{
class DarpGeometryInterop {
public:
  static darp_geometry::Value* tensor(unsigned long long address, int rows, int columns, int rowStride, int columnStride) {
    return new darp_geometry::Value{nullptr, reinterpret_cast<double*>(static_cast<std::uintptr_t>(address)), rows, columns, rowStride, columnStride, false};
  }
  static darp_geometry::Value* argument(darp_geometry::Value* source) {
    auto result = std::make_unique<darp_geometry::Value>(*source);
    result->changed = false;
    return result.release();
  }
  static void release(darp_geometry::Value* value) { delete value; }
  static bool changed(darp_geometry::Value* value) { return value->changed; }
  static unsigned long long data(darp_geometry::Value* value) { return static_cast<unsigned long long>(reinterpret_cast<std::uintptr_t>(value->data)); }
  static int rows(darp_geometry::Value* value) { return value->rows; }
  static int columns(darp_geometry::Value* value) { return value->columns; }
  static int rowStride(darp_geometry::Value* value) { return value->row_stride; }
  static int columnStride(darp_geometry::Value* value) { return value->column_stride; }
  static darp_geometry::Value* create(int kind) { switch(kind) {
    case 1: return new darp_geometry::Value(darp_geometry::owned_container(tesseract::common::TransformMap{}));
    case 2: return new darp_geometry::Value(darp_geometry::owned_container(tesseract::common::VectorIsometry3d{}));
    case 3: return new darp_geometry::Value(darp_geometry::owned_container(tesseract::kinematics::IKSolutions{}));
    case 4: return new darp_geometry::Value(darp_geometry::owned_container(tesseract::common::VectorVector2d{}));
    case 5: return new darp_geometry::Value(darp_geometry::owned_container(tesseract::common::VectorVector3d{}));
    case 6: return new darp_geometry::Value(darp_geometry::owned_container(tesseract::common::VectorVector4d{}));
    default: throw std::invalid_argument("Unknown container kind."); } }
  static int count(darp_geometry::Value* value, int kind) { std::size_t size; switch(kind) {
    case 1: size = darp_geometry::container<tesseract::common::TransformMap>(value).size(); break;
    case 2: size = darp_geometry::container<tesseract::common::VectorIsometry3d>(value).size(); break;
    case 3: size = darp_geometry::container<tesseract::kinematics::IKSolutions>(value).size(); break;
    case 4: size = darp_geometry::container<tesseract::common::VectorVector2d>(value).size(); break;
    case 5: size = darp_geometry::container<tesseract::common::VectorVector3d>(value).size(); break;
    case 6: size = darp_geometry::container<tesseract::common::VectorVector4d>(value).size(); break;
    default: throw std::invalid_argument("Unknown container kind."); } if (size > std::numeric_limits<int>::max()) throw std::overflow_error("Container size exceeds Int32."); return static_cast<int>(size); }
  static darp_geometry::Value* element(darp_geometry::Value* value, int kind, int index, const std::string& key) { switch(kind) {
    case 1: return new darp_geometry::Value(darp_geometry::view(value->owner, darp_geometry::container<tesseract::common::TransformMap>(value).at(key)));
    case 2: return new darp_geometry::Value(darp_geometry::view(value->owner, darp_geometry::container<tesseract::common::VectorIsometry3d>(value).at(static_cast<std::size_t>(index))));
    case 3: return new darp_geometry::Value(darp_geometry::view(value->owner, darp_geometry::container<tesseract::kinematics::IKSolutions>(value).at(static_cast<std::size_t>(index))));
    case 4: return new darp_geometry::Value(darp_geometry::view(value->owner, darp_geometry::container<tesseract::common::VectorVector2d>(value).at(static_cast<std::size_t>(index))));
    case 5: return new darp_geometry::Value(darp_geometry::view(value->owner, darp_geometry::container<tesseract::common::VectorVector3d>(value).at(static_cast<std::size_t>(index))));
    case 6: return new darp_geometry::Value(darp_geometry::view(value->owner, darp_geometry::container<tesseract::common::VectorVector4d>(value).at(static_cast<std::size_t>(index))));
    default: throw std::invalid_argument("Unknown container kind."); } }
  static void add(darp_geometry::Value* value, int kind, const std::string& key, darp_geometry::Value* tensor) { switch(kind) {
    case 1: darp_geometry::container<tesseract::common::TransformMap>(value).insert_or_assign(key, darp_geometry::read<Eigen::Isometry3d>(tensor)); return;
    case 2: darp_geometry::container<tesseract::common::VectorIsometry3d>(value).push_back(darp_geometry::read<Eigen::Isometry3d>(tensor)); return;
    case 3: darp_geometry::container<tesseract::kinematics::IKSolutions>(value).push_back(darp_geometry::read<Eigen::VectorXd>(tensor)); return;
    case 4: darp_geometry::container<tesseract::common::VectorVector2d>(value).push_back(darp_geometry::read<Eigen::Vector2d>(tensor)); return;
    case 5: darp_geometry::container<tesseract::common::VectorVector3d>(value).push_back(darp_geometry::read<Eigen::Vector3d>(tensor)); return;
    case 6: darp_geometry::container<tesseract::common::VectorVector4d>(value).push_back(darp_geometry::read<Eigen::Vector4d>(tensor)); return;
    default: throw std::invalid_argument("Unknown container kind."); } }
  static std::vector<std::string> keys(darp_geometry::Value* value, int kind) { std::vector<std::string> result; switch(kind) {
    case 1: for (const auto& entry : darp_geometry::container<tesseract::common::TransformMap>(value)) result.push_back(entry.first); return result;
    default: throw std::invalid_argument("Expected a map."); } }
  static bool contains(darp_geometry::Value* value, int kind, const std::string& key) { switch(kind) {
    case 1: { auto& map = darp_geometry::container<tesseract::common::TransformMap>(value); return map.find(key) != map.end(); }
    default: throw std::invalid_argument("Expected a map."); } }
};
%}
