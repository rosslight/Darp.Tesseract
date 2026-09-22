namespace Eigen {
struct Index;
class Vector2d {}; class Vector3d {}; class Vector4d {}; class VectorXd {};
class MatrixXd {}; class MatrixX2d {}; class Quaterniond {}; class Isometry3d {};
}
%apply long long { Eigen::Index };
%apply const long long& { const Eigen::Index& };
