%typemap(cscode) std::vector<double> %{
  /// <summary>Views the native vector storage. The span is invalidated by mutation or disposal.</summary>
  public unsafe global::System.Span<double> AsSpan() =>
    new global::System.Span<double>((void*)dataAddress(), checked((int)Count));
%}

/* Eigen::Index is a signed 64-bit integer, but its native spelling differs:
 * long long on Windows and long on LP64 platforms. Keep the STL helpers tied
 * to Eigen::Index instead of SWIG's generation-host integer spelling. */
namespace std
{
template<> class vector<Eigen::Index>
{
  SWIG_STD_VECTOR_MINIMUM_INTERNAL(IList, const Eigen::Index&, Eigen::Index)
  SWIG_STD_VECTOR_EXTRA_OP_EQUALS_EQUALS(Eigen::Index)
};
}

/* Stable names for the STL containers used across the selected API surface. */
%template(StringVector) std::vector<std::string>;
%template(StringSet) std::set<std::string>;
%template(StringDoubleMap) std::unordered_map<std::string, double>;
%template(StringBoolMap) std::unordered_map<std::string, bool>;
%template(StringStringMap) std::unordered_map<std::string, std::string>;
%template(StringVectorMap) std::unordered_map<std::string, std::vector<std::string>>;
%template(DoubleVector) std::vector<double>;
%template(IndexVector) std::vector<Eigen::Index>;

%extend std::vector<double>
{
  unsigned long long dataAddress() const
  {
    return static_cast<unsigned long long>(reinterpret_cast<std::uintptr_t>($self->data()));
  }
}

/* Tesseract's aligned aliases are deliberately opaque to SWIG. */
namespace tesseract
{
namespace common
{
class TransformMap {};
class VectorIsometry3d {};
class VectorVector2d {};
class VectorVector3d {};
class VectorVector4d {};
}
}

namespace tesseract
{
namespace kinematics
{
class IKSolutions {};
}
}
