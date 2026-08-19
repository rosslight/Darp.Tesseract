/*
 * Small, type-driven Eigen surface used by every wrapped Tesseract module.
 * Eigen is template-heavy and intentionally not parsed by SWIG. These proxy
 * declarations describe only the stable value types selected for the managed
 * API. The generated wrapper always compiles against the real Eigen types.
 */
/* Proxy augmentation typemaps must precede the class declarations. */
%typemap(cscode) Eigen::Vector2d %{
  /// <summary>Views the native Eigen storage. The span is valid until this object is disposed.</summary>
  public unsafe global::System.Span<double> AsSpan() =>
    new global::System.Span<double>((void*)dataAddress(), 2);
%}

%typemap(cscode) Eigen::Vector3d %{
  /// <summary>Views the native Eigen storage. The span is valid until this object is disposed.</summary>
  public unsafe global::System.Span<double> AsSpan() =>
    new global::System.Span<double>((void*)dataAddress(), 3);
%}

%typemap(cscode) Eigen::Vector4d %{
  /// <summary>Views the native Eigen storage. The span is valid until this object is disposed.</summary>
  public unsafe global::System.Span<double> AsSpan() =>
    new global::System.Span<double>((void*)dataAddress(), 4);
%}

%typemap(cscode) Eigen::VectorXd %{
  /// <summary>Views the native Eigen storage. The span is valid until this object is disposed or resized.</summary>
  public unsafe global::System.Span<double> AsSpan() =>
    new global::System.Span<double>((void*)dataAddress(), size());
%}

%typemap(cscode) Eigen::MatrixXd %{
  /// <summary>Views Eigen's native column-major storage. The span is valid until disposal or resize.</summary>
  public unsafe global::System.Span<double> AsSpan() =>
    new global::System.Span<double>((void*)dataAddress(), checked(rows() * columns()));
%}

namespace Eigen
{
typedef long long Index;
class Vector2d {};
class Vector3d {};
class Vector4d {};
class VectorXd {};
class MatrixXd {};
class MatrixX2d {};
class Quaterniond {};
class Isometry3d {};
}

/* Eigen::Ref does not own storage. Accept the corresponding generated Eigen
 * owner and construct the lightweight Ref in automatic wrapper storage. */
%define DARP_CONST_EIGEN_REF(TYPE)
%typemap(ctype) const Eigen::Ref<const Eigen::TYPE>& "void *"
%typemap(imtype) const Eigen::Ref<const Eigen::TYPE>& "global::System.Runtime.InteropServices.HandleRef"
%typemap(cstype) const Eigen::Ref<const Eigen::TYPE>& "TYPE"
%typemap(csin) const Eigen::Ref<const Eigen::TYPE>& "TYPE.getCPtr($csinput)"
%typemap(in) const Eigen::Ref<const Eigen::TYPE>&
  (std::optional<Eigen::Ref<const Eigen::TYPE>> ref)
{
  if ($input == nullptr)
  {
    SWIG_CSharpSetPendingExceptionArgument(
      SWIG_CSharpArgumentNullException, "Eigen input cannot be null.", nullptr);
    return $null;
  }
  ref.emplace(*reinterpret_cast<Eigen::TYPE*>($input));
  $1 = &ref.value();
}
%enddef

%define DARP_MUTABLE_EIGEN_REF(TYPE)
%typemap(ctype) Eigen::Ref<Eigen::TYPE> "void *"
%typemap(imtype) Eigen::Ref<Eigen::TYPE> "global::System.Runtime.InteropServices.HandleRef"
%typemap(cstype) Eigen::Ref<Eigen::TYPE> "TYPE"
%typemap(csin) Eigen::Ref<Eigen::TYPE> "TYPE.getCPtr($csinput)"
%typemap(in) Eigen::Ref<Eigen::TYPE>
  (std::optional<Eigen::Ref<Eigen::TYPE>> ref)
{
  if ($input == nullptr)
  {
    SWIG_CSharpSetPendingExceptionArgument(
      SWIG_CSharpArgumentNullException, "Eigen output cannot be null.", nullptr);
    return $null;
  }
  ref.emplace(*reinterpret_cast<Eigen::TYPE*>($input));
  $1 = ref.value();
}
%enddef

DARP_CONST_EIGEN_REF(VectorXd)
DARP_CONST_EIGEN_REF(MatrixXd)
DARP_MUTABLE_EIGEN_REF(VectorXd)
DARP_MUTABLE_EIGEN_REF(MatrixXd)

%define DARP_FIXED_EIGEN_VECTOR(TYPE, SIZE)
%extend Eigen::TYPE
{
  TYPE()
  {
    return new Eigen::TYPE(Eigen::TYPE::Zero());
  }

  int size() const
  {
    (void)$self;
    return SIZE;
  }

  double get(int index) const
  {
    if (index < 0 || index >= SIZE)
      throw std::out_of_range("Eigen vector index is out of range.");
    return (*$self)[index];
  }

  void set(int index, double value)
  {
    if (index < 0 || index >= SIZE)
      throw std::out_of_range("Eigen vector index is out of range.");
    (*$self)[index] = value;
  }

  unsigned long long dataAddress() const
  {
    return static_cast<unsigned long long>(reinterpret_cast<std::uintptr_t>($self->data()));
  }
}
%enddef

DARP_FIXED_EIGEN_VECTOR(Vector2d, 2)
DARP_FIXED_EIGEN_VECTOR(Vector3d, 3)
DARP_FIXED_EIGEN_VECTOR(Vector4d, 4)

%extend Eigen::VectorXd
{
  VectorXd(int size)
  {
    if (size < 0)
      throw std::invalid_argument("Vector size cannot be negative.");
    return new Eigen::VectorXd(size);
  }

  int size() const
  {
    if ($self->size() > std::numeric_limits<int>::max())
      throw std::overflow_error("Vector length cannot be represented by System.Int32.");
    return static_cast<int>($self->size());
  }

  double get(int index) const
  {
    if (index < 0 || index >= $self->size())
      throw std::out_of_range("Vector index is out of range.");
    return (*$self)[index];
  }

  void set(int index, double value)
  {
    if (index < 0 || index >= $self->size())
      throw std::out_of_range("Vector index is out of range.");
    (*$self)[index] = value;
  }

  unsigned long long dataAddress() const
  {
    return static_cast<unsigned long long>(reinterpret_cast<std::uintptr_t>($self->data()));
  }
}

%extend Eigen::MatrixXd
{
  MatrixXd(int rows, int columns)
  {
    if (rows < 0 || columns < 0)
      throw std::invalid_argument("Matrix dimensions cannot be negative.");
    return new Eigen::MatrixXd(rows, columns);
  }

  int rows() const { return static_cast<int>($self->rows()); }
  int columns() const { return static_cast<int>($self->cols()); }

  double get(int row, int column) const
  {
    if (row < 0 || row >= $self->rows() || column < 0 || column >= $self->cols())
      throw std::out_of_range("Matrix index is out of range.");
    return (*$self)(row, column);
  }

  void set(int row, int column, double value)
  {
    if (row < 0 || row >= $self->rows() || column < 0 || column >= $self->cols())
      throw std::out_of_range("Matrix index is out of range.");
    (*$self)(row, column) = value;
  }

  unsigned long long dataAddress() const
  {
    return static_cast<unsigned long long>(reinterpret_cast<std::uintptr_t>($self->data()));
  }
}

%extend Eigen::Quaterniond
{
  Quaterniond(double x, double y, double z, double w)
  {
    Eigen::Quaterniond value(w, x, y, z);
    if (value.norm() < 1e-12)
      throw std::invalid_argument("The quaternion must not be zero.");
    return new Eigen::Quaterniond(value.normalized());
  }

  double x() const { return $self->x(); }
  double y() const { return $self->y(); }
  double z() const { return $self->z(); }
  double w() const { return $self->w(); }
}

%extend Eigen::Isometry3d
{
  Isometry3d()
  {
    return new Eigen::Isometry3d(Eigen::Isometry3d::Identity());
  }

  void setTranslation(double x, double y, double z)
  {
    $self->translation() = Eigen::Vector3d(x, y, z);
  }

  void setQuaternion(double x, double y, double z, double w)
  {
    Eigen::Quaterniond value(w, x, y, z);
    if (value.norm() < 1e-12)
      throw std::invalid_argument("The orientation quaternion must not be zero.");
    $self->linear() = value.normalized().toRotationMatrix();
  }

  double translationX() const { return $self->translation().x(); }
  double translationY() const { return $self->translation().y(); }
  double translationZ() const { return $self->translation().z(); }
  double quaternionX() const { return Eigen::Quaterniond($self->linear()).x(); }
  double quaternionY() const { return Eigen::Quaterniond($self->linear()).y(); }
  double quaternionZ() const { return Eigen::Quaterniond($self->linear()).z(); }
  double quaternionW() const { return Eigen::Quaterniond($self->linear()).w(); }
}
