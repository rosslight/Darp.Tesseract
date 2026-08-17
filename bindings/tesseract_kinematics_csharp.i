/**
 * Direct C# bindings for the dependency-coherent analytical IK slice of
 * Tesseract Kinematics.
 *
 * The generated proxies below own the real Eigen and Tesseract C++ objects.
 * %extend only supplies collection-style access where SWIG cannot parse Eigen
 * aliases; it does not introduce a parallel native or managed object model.
 */
%module TesseractKinematics

%include <std_string.i>
%include <std_vector.i>

%exception
{
  try
  {
    $action
  }
  catch (const std::invalid_argument& exception)
  {
    SWIG_CSharpSetPendingExceptionArgument(SWIG_CSharpArgumentException, exception.what(), "");
    return $null;
  }
  catch (const std::out_of_range& exception)
  {
    SWIG_CSharpSetPendingException(SWIG_CSharpIndexOutOfRangeException, exception.what());
    return $null;
  }
  catch (const std::exception& exception)
  {
    SWIG_CSharpSetPendingException(SWIG_CSharpApplicationException, exception.what());
    return $null;
  }
}

%{
#include <stdexcept>
#include <Eigen/Core>
#include <Eigen/Geometry>
#include <opw_kinematics/opw_parameters.h>
#include <tesseract/common/eigen_types.h>
#include <tesseract/kinematics/inverse_kinematics.h>
#include <tesseract/kinematics/opw/opw_inv_kin.h>
%}

namespace std
{
%template(StringVector) vector<string>;
}

namespace Eigen
{
class VectorXd
{
};

class Isometry3d
{
};
}

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
    Eigen::Quaterniond quaternion(w, x, y, z);
    if (quaternion.norm() < 1e-12)
      throw std::invalid_argument("The orientation quaternion must not be zero.");
    $self->linear() = quaternion.normalized().toRotationMatrix();
  }

  double translationX() const { return $self->translation().x(); }
  double translationY() const { return $self->translation().y(); }
  double translationZ() const { return $self->translation().z(); }
}

namespace tesseract
{
namespace common
{
class TransformMap
{
};
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

  void set(const std::string& link_name, const Eigen::Isometry3d& pose)
  {
    $self->insert_or_assign(link_name, pose);
  }
}

namespace tesseract
{
namespace kinematics
{
class IKSolutions
{
};
}
}

%extend tesseract::kinematics::IKSolutions
{
  int size() const
  {
    return static_cast<int>($self->size());
  }

  Eigen::VectorXd get(int index) const
  {
    if (index < 0 || index >= static_cast<int>($self->size()))
      throw std::out_of_range("IK solution index is out of range.");
    return (*$self)[static_cast<std::size_t>(index)];
  }
}

namespace opw_kinematics
{
template <typename T>
struct Parameters
{
  Parameters();
  T a1;
  T a2;
  T b;
  T c1;
  T c2;
  T c3;
  T c4;
};

%extend Parameters<double>
{
  void setOffset(int index, double value)
  {
    if (index < 0 || index >= 6)
      throw std::out_of_range("OPW offset index is out of range.");
    $self->offsets[static_cast<std::size_t>(index)] = value;
  }

  double getOffset(int index) const
  {
    if (index < 0 || index >= 6)
      throw std::out_of_range("OPW offset index is out of range.");
    return $self->offsets[static_cast<std::size_t>(index)];
  }

  void setSignCorrection(int index, int value)
  {
    if (index < 0 || index >= 6)
      throw std::out_of_range("OPW sign-correction index is out of range.");
    if (value != -1 && value != 1)
      throw std::invalid_argument("An OPW sign correction must be either -1 or 1.");
    $self->sign_corrections[static_cast<std::size_t>(index)] = static_cast<signed char>(value);
  }

  int getSignCorrection(int index) const
  {
    if (index < 0 || index >= 6)
      throw std::out_of_range("OPW sign-correction index is out of range.");
    return static_cast<int>($self->sign_corrections[static_cast<std::size_t>(index)]);
  }
}

%template(OPWParameters) Parameters<double>;
}

namespace tesseract
{
namespace kinematics
{
%nodefaultctor InverseKinematics;
class InverseKinematics
{
public:
  virtual ~InverseKinematics();
  IKSolutions calcInvKin(const tesseract::common::TransformMap& tip_link_poses,
                         const Eigen::VectorXd& seed) const;
  std::vector<std::string> getJointNames() const;
  long long numJoints() const;
  std::string getBaseLinkName() const;
  std::string getWorkingFrame() const;
  std::vector<std::string> getTipLinkNames() const;
  std::string getSolverName() const;
};

class OPWInvKin : public InverseKinematics
{
public:
  OPWInvKin(opw_kinematics::Parameters<double> parameters,
            std::string base_link_name,
            std::string tip_link_name,
            std::vector<std::string> joint_names,
            std::string solver_name = "OPWInvKin");
};
}
}
