%ignore tesseract::collision::ContactResultValidator;
%ignore tesseract::collision::ContactTestData;
%ignore tesseract::collision::ContactResult::operator==;
%ignore tesseract::collision::ContactResult::operator!=;
%ignore tesseract::collision::ContactResultMap::addContactResult;
%ignore tesseract::collision::ContactResultMap::setContactResult;
%ignore tesseract::collision::ContactResultMap::addInterpolatedCollisionResults;
%ignore tesseract::collision::ContactResultMap::flattenMoveResults;
%ignore tesseract::collision::ContactResultMap::flattenCopyResults;
%ignore tesseract::collision::ContactResultMap::flattenWrapperResults;
%ignore tesseract::collision::ContactResultMap::filter;
%ignore tesseract::collision::ContactResultMap::getContainer;
%ignore tesseract::collision::ContactResultMap::begin;
%ignore tesseract::collision::ContactResultMap::end;
%ignore tesseract::collision::ContactResultMap::cbegin;
%ignore tesseract::collision::ContactResultMap::cend;
%ignore tesseract::collision::ContactResultMap::at;
%ignore tesseract::collision::ContactResultMap::find;
%ignore tesseract::collision::ContactResultMap::operator==;
%ignore tesseract::collision::ContactResultMap::operator!=;
%ignore tesseract::collision::ContactTrajectorySubstepResults::worstCollision;
%ignore tesseract::collision::ContactTrajectoryStepResults::worstCollision;
%ignore tesseract::collision::ContactTrajectoryResults::worstCollision;
%ignore tesseract::collision::ContactTrajectoryResults::trajectoryCollisionResultsTable;
%ignore tesseract::collision::ContactTrajectoryResults::collisionFrequencyPerLink;
%ignore tesseract::collision::ContactTrajectoryResults::condensedSummary;
%ignore tesseract::collision::ContactRequest::is_valid;
%ignore tesseract::collision::ContactRequest::operator==;
%ignore tesseract::collision::ContactRequest::operator!=;
%ignore tesseract::collision::ContactManagerConfig::operator==;
%ignore tesseract::collision::ContactManagerConfig::operator!=;
%ignore tesseract::collision::CollisionCheckConfig::operator==;
%ignore tesseract::collision::CollisionCheckConfig::operator!=;

namespace tesseract::collision
{
struct ContactResult;
class ContactResultMap;
struct ContactTrajectorySubstepResults;
struct ContactTrajectoryStepResults;
struct ContactTrajectoryResults;
}

%template(IntArray2) std::array<int, 2>;
%template(StringArray2) std::array<std::string, 2>;
%template(DoubleArray2) std::array<double, 2>;
%template(Vector3dArray2) std::array<Eigen::Vector3d, 2>;
%template(Isometry3dArray2) std::array<Eigen::Isometry3d, 2>;
%template(ContactResultVector) std::vector<tesseract::collision::ContactResult>;
%template(ContactResultMapVector) std::vector<tesseract::collision::ContactResultMap>;
%template(ContactTrajectorySubstepResultsVector) std::vector<tesseract::collision::ContactTrajectorySubstepResults>;
%template(ContactTrajectoryStepResultsVector) std::vector<tesseract::collision::ContactTrajectoryStepResults>;

%include <tesseract/collision/types.h>
%template(ContinuousCollisionTypeArray2) std::array<tesseract::collision::ContinuousCollisionType, 2>;

%inline %{
namespace darp_tesseract_bindings
{
std::vector<tesseract::collision::ContactResult>
flattenContactResults(const tesseract::collision::ContactResultMap& results)
{
  tesseract::collision::ContactResultVector aligned_results;
  results.flattenCopyResults(aligned_results);
  return { aligned_results.begin(), aligned_results.end() };
}

std::string trajectoryCollisionResultsTable(const tesseract::collision::ContactTrajectoryResults& results)
{
  return results.trajectoryCollisionResultsTable().str();
}

std::string collisionFrequencyPerLink(const tesseract::collision::ContactTrajectoryResults& results)
{
  return results.collisionFrequencyPerLink().str();
}

std::string condensedCollisionSummary(const tesseract::collision::ContactTrajectoryResults& results)
{
  return results.condensedSummary().str();
}
}
%}

%ignore tesseract::collision::DiscreteContactManager::addCollisionObject;
%ignore tesseract::collision::DiscreteContactManager::getCollisionObjectGeometries;
%ignore tesseract::collision::DiscreteContactManager::getCollisionObjectGeometriesTransforms;
%ignore tesseract::collision::DiscreteContactManager::setCollisionMarginData;
%ignore tesseract::collision::DiscreteContactManager::getCollisionMarginData;
%ignore tesseract::collision::DiscreteContactManager::setCollisionMarginPairData;
%ignore tesseract::collision::DiscreteContactManager::setContactAllowedValidator;
%ignore tesseract::collision::DiscreteContactManager::getContactAllowedValidator;
%ignore tesseract::collision::ContinuousContactManager::addCollisionObject;
%ignore tesseract::collision::ContinuousContactManager::getCollisionObjectGeometries;
%ignore tesseract::collision::ContinuousContactManager::getCollisionObjectGeometriesTransforms;
%ignore tesseract::collision::ContinuousContactManager::setCollisionMarginData;
%ignore tesseract::collision::ContinuousContactManager::getCollisionMarginData;
%ignore tesseract::collision::ContinuousContactManager::setCollisionMarginPairData;
%ignore tesseract::collision::ContinuousContactManager::setContactAllowedValidator;
%ignore tesseract::collision::ContinuousContactManager::getContactAllowedValidator;

%include <tesseract/collision/discrete_contact_manager.h>
%include <tesseract/collision/continuous_contact_manager.h>
