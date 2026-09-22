/* Concrete geometry mappings. Conversion and ownership rules are shared in typemaps.i. */
%include "geometry/typemaps.i"
DARP_TENSOR(Eigen::Vector2d, IReadOnlyVectorXD, Vector, VectorXD, TakeVectorXD)
DARP_TENSOR(Eigen::Vector3d, IReadOnlyVector3D, Vector3, Vector3D, TakeVector3D)
DARP_TENSOR(Eigen::Vector4d, IReadOnlyVectorXD, Vector, VectorXD, TakeVectorXD)
DARP_TENSOR(Eigen::VectorXd, IReadOnlyVectorXD, Vector, VectorXD, TakeVectorXD)
DARP_TENSOR(Eigen::MatrixXd, IReadOnlyMatrixD, Matrix, MatrixXD, TakeMatrixXD)
DARP_TENSOR(Eigen::MatrixX2d, IReadOnlyMatrixD, Matrix, MatrixXD, TakeMatrixXD)
DARP_TENSOR(Eigen::Quaterniond, IReadOnlyQuaternionD, Quaternion, QuaternionD, TakeQuaternionD)
DARP_TENSOR(Eigen::Isometry3d, IReadOnlyIsometry3D, Isometry, Isometry3D, TakeIsometry3D)
DARP_TENSOR_REF(Eigen::VectorXd, IReadOnlyVectorXD, VectorXD)
DARP_TENSOR_REF(Eigen::MatrixXd, IReadOnlyMatrixD, MatrixXD)
DARP_CONTAINER(tesseract::common::TransformMap, TransformMap)
DARP_MAP_PROXY(tesseract::common::TransformMap, TransformMap, IReadOnlyIsometry3D, 1, Isometry)
DARP_CONTAINER(tesseract::common::VectorIsometry3d, VectorIsometry3d)
DARP_LIST_PROXY(tesseract::common::VectorIsometry3d, VectorIsometry3d, IReadOnlyIsometry3D, 2, Isometry)
DARP_CONTAINER(tesseract::kinematics::IKSolutions, IKSolutions)
DARP_LIST_PROXY(tesseract::kinematics::IKSolutions, IKSolutions, IReadOnlyVectorXD, 3, Vector)
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
    default: throw std::invalid_argument("Unknown container kind."); } }
  static int count(darp_geometry::Value* value, int kind) { std::size_t size; switch(kind) {
    case 1: size = darp_geometry::container<tesseract::common::TransformMap>(value).size(); break;
    case 2: size = darp_geometry::container<tesseract::common::VectorIsometry3d>(value).size(); break;
    case 3: size = darp_geometry::container<tesseract::kinematics::IKSolutions>(value).size(); break;
    default: throw std::invalid_argument("Unknown container kind."); } if (size > std::numeric_limits<int>::max()) throw std::overflow_error("Container size exceeds Int32."); return static_cast<int>(size); }
  static darp_geometry::Value* element(darp_geometry::Value* value, int kind, int index, const std::string& key) { switch(kind) {
    case 1: return new darp_geometry::Value(darp_geometry::view(value->owner, darp_geometry::container<tesseract::common::TransformMap>(value).at(key)));
    case 2: return new darp_geometry::Value(darp_geometry::view(value->owner, darp_geometry::container<tesseract::common::VectorIsometry3d>(value).at(static_cast<std::size_t>(index))));
    case 3: return new darp_geometry::Value(darp_geometry::view(value->owner, darp_geometry::container<tesseract::kinematics::IKSolutions>(value).at(static_cast<std::size_t>(index))));
    default: throw std::invalid_argument("Unknown container kind."); } }
  static void add(darp_geometry::Value* value, int kind, const std::string& key, darp_geometry::Value* tensor) { switch(kind) {
    case 1: darp_geometry::container<tesseract::common::TransformMap>(value).insert_or_assign(key, darp_geometry::read<Eigen::Isometry3d>(tensor)); return;
    case 2: darp_geometry::container<tesseract::common::VectorIsometry3d>(value).push_back(darp_geometry::read<Eigen::Isometry3d>(tensor)); return;
    case 3: darp_geometry::container<tesseract::kinematics::IKSolutions>(value).push_back(darp_geometry::read<Eigen::VectorXd>(tensor)); return;
    default: throw std::invalid_argument("Unknown container kind."); } }
  static std::vector<std::string> keys(darp_geometry::Value* value, int kind) { std::vector<std::string> result; switch(kind) {
    case 1: for (const auto& entry : darp_geometry::container<tesseract::common::TransformMap>(value)) result.push_back(entry.first); return result;
    default: throw std::invalid_argument("Expected a map."); } }
  static bool contains(darp_geometry::Value* value, int kind, const std::string& key) { switch(kind) {
    case 1: { auto& map = darp_geometry::container<tesseract::common::TransformMap>(value); return map.find(key) != map.end(); }
    default: throw std::invalid_argument("Expected a map."); } }
};
%}
