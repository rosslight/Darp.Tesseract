%typemap(cscode) std::vector<double> %{
  /// <summary>Views the native vector storage. The span is invalidated by mutation or disposal.</summary>
  public unsafe global::System.Span<double> AsSpan() =>
    new global::System.Span<double>((void*)dataAddress(), checked((int)Count));
%}

%typemap(cscode) tesseract::kinematics::IKSolutions %{
  /// <summary>Views one native IK solution without copying it.</summary>
  public unsafe global::System.ReadOnlySpan<double> GetSolutionSpan(int index) =>
    new global::System.ReadOnlySpan<double>((void*)solutionDataAddress(index), solutionSize(index));
%}

/* Stable names for the STL containers used across the selected API surface. */
%template(StringVector) std::vector<std::string>;
%template(StringSet) std::set<std::string>;
%template(StringDoubleMap) std::unordered_map<std::string, double>;
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
}
}

%extend tesseract::common::TransformMap
{
  TransformMap()
  {
    return new tesseract::common::TransformMap();
  }

  int size() const
  {
    return static_cast<int>($self->size());
  }

  bool contains(const std::string& key) const
  {
    return $self->find(key) != $self->end();
  }

  void set(const std::string& key, const Eigen::Isometry3d& value)
  {
    $self->insert_or_assign(key, value);
  }

  Eigen::Isometry3d get(const std::string& key) const
  {
    return $self->at(key);
  }

  void clear()
  {
    $self->clear();
  }
}

%extend tesseract::common::VectorIsometry3d
{
  int size() const { return static_cast<int>($self->size()); }

  Eigen::Isometry3d get(int index) const
  {
    if (index < 0 || index >= static_cast<int>($self->size()))
      throw std::out_of_range("Transform index is out of range.");
    return (*$self)[static_cast<std::size_t>(index)];
  }
}

/*
 * IKSolutions owns the outer vector and every Eigen row. GetSolutionSpan views
 * the existing native row directly, avoiding a second native allocation/copy.
 */
namespace tesseract
{
namespace kinematics
{
class IKSolutions {};
}
}

%extend tesseract::kinematics::IKSolutions
{
  int size() const { return static_cast<int>($self->size()); }

  int solutionSize(int index) const
  {
    if (index < 0 || index >= static_cast<int>($self->size()))
      throw std::out_of_range("IK solution index is out of range.");
    return static_cast<int>((*$self)[static_cast<std::size_t>(index)].size());
  }

  unsigned long long solutionDataAddress(int index) const
  {
    if (index < 0 || index >= static_cast<int>($self->size()))
      throw std::out_of_range("IK solution index is out of range.");
    return static_cast<unsigned long long>(reinterpret_cast<std::uintptr_t>(
      (*$self)[static_cast<std::size_t>(index)].data()));
  }
}
