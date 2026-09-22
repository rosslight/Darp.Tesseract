#pragma once
#include <memory>
#include <optional>
#include <type_traits>
#include <limits>
#include <stdexcept>

namespace darp_geometry
{
// One retained allocation and an optional strided coefficient view. Container
// element views share the allocation, not the lifetime of a managed proxy.
struct Value
{
  std::shared_ptr<void> owner;
  double* data = nullptr;
  int rows = 0, columns = 0, row_stride = 1, column_stride = 1;
  bool changed = false;
};

template <typename T> decltype(auto) coefficients(T& value) { return (value); }
inline auto& coefficients(Eigen::Isometry3d& value) { return value.matrix(); }
inline auto& coefficients(Eigen::Quaterniond& value) { return value.coeffs(); }

template <typename T> Value view(std::shared_ptr<void> owner, T& value)
{
  auto& matrix = coefficients(value);
  if (matrix.rows() > std::numeric_limits<int>::max() || matrix.cols() > std::numeric_limits<int>::max())
    throw std::overflow_error("Geometry dimensions exceed Int32.");
  static_assert(!std::decay_t<decltype(matrix)>::IsRowMajor, "Native geometry is column-major.");
  return {std::move(owner), matrix.data(), static_cast<int>(matrix.rows()), static_cast<int>(matrix.cols()),
          1, std::max(1, static_cast<int>(matrix.rows())), true};
}

template <typename T> Value owned_tensor(T value)
{
  auto owner = std::make_shared<T>(std::move(value));
  return view(owner, *owner);
}
template <typename T> Value owned_container(T value)
{
  return {std::make_shared<T>(std::move(value)), nullptr, 0, 0, 1, 1, true};
}
template <typename T> T& container(Value* value)
{
  if (!value || !value->owner) throw std::invalid_argument("Missing native container owner.");
  return *static_cast<T*>(value->owner.get());
}

template <typename T> void validate(const Value* value)
{
  using Matrix = std::decay_t<decltype(coefficients(std::declval<T&>()))>;
  if (!value || value->rows < 0 || value->columns < 0 || value->row_stride <= 0 || value->column_stride <= 0 ||
      (value->rows != 0 && value->columns != 0 && !value->data))
    throw std::invalid_argument("Invalid geometry buffer.");
  if ((Matrix::RowsAtCompileTime != Eigen::Dynamic && value->rows != Matrix::RowsAtCompileTime) ||
      (Matrix::ColsAtCompileTime != Eigen::Dynamic && value->columns != Matrix::ColsAtCompileTime))
    throw std::invalid_argument("Geometry shape does not match the native type.");
}
template <typename T> T read(const Value* value)
{
  validate<T>(value);
  T result;
  auto& matrix = coefficients(result);
  using Matrix = std::decay_t<decltype(matrix)>;
  Eigen::Map<const Matrix, Eigen::Unaligned, Eigen::Stride<Eigen::Dynamic, Eigen::Dynamic>> source(
    value->data, value->rows, value->columns, {value->column_stride, value->row_stride});
  matrix = source;
  return result;
}

// Default Eigen::Ref accepts inner-contiguous column-major storage, including
// padded columns. Other input layouts get an explicit local packing copy.
template <typename T> struct ConstRef
{
  std::optional<T> packed;
  using Map = Eigen::Map<const T, Eigen::Unaligned, Eigen::OuterStride<Eigen::Dynamic>>;
  std::optional<Map> mapped;
  std::optional<Eigen::Ref<const T>> ref;
  explicit ConstRef(const Value* value)
  {
    validate<T>(value);
    if (value->row_stride == 1)
    {
      mapped.emplace(value->data, value->rows, value->columns, Eigen::OuterStride<Eigen::Dynamic>(value->column_stride));
      ref.emplace(*mapped);
    }
    else
    {
      packed.emplace(read<T>(value));
      ref.emplace(*packed);
    }
  }
};
}
