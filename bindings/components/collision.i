/* Keep this first collision slice to operations with stable generated shapes. */
%ignore tesseract::collision::DiscreteContactManager::addCollisionObject;
%ignore tesseract::collision::DiscreteContactManager::getCollisionObjectGeometries;
%ignore tesseract::collision::DiscreteContactManager::getCollisionObjectGeometriesTransforms;
%ignore tesseract::collision::DiscreteContactManager::setCollisionMarginData;
%ignore tesseract::collision::DiscreteContactManager::getCollisionMarginData;
%ignore tesseract::collision::DiscreteContactManager::setCollisionMarginPairData;
%ignore tesseract::collision::DiscreteContactManager::setContactAllowedValidator;
%ignore tesseract::collision::DiscreteContactManager::getContactAllowedValidator;
%ignore tesseract::collision::DiscreteContactManager::contactTest;
%ignore tesseract::collision::DiscreteContactManager::applyContactManagerConfig;
%ignore tesseract::collision::ContinuousContactManager::addCollisionObject;
%ignore tesseract::collision::ContinuousContactManager::getCollisionObjectGeometries;
%ignore tesseract::collision::ContinuousContactManager::getCollisionObjectGeometriesTransforms;
%ignore tesseract::collision::ContinuousContactManager::setCollisionMarginData;
%ignore tesseract::collision::ContinuousContactManager::getCollisionMarginData;
%ignore tesseract::collision::ContinuousContactManager::setCollisionMarginPairData;
%ignore tesseract::collision::ContinuousContactManager::setContactAllowedValidator;
%ignore tesseract::collision::ContinuousContactManager::getContactAllowedValidator;
%ignore tesseract::collision::ContinuousContactManager::contactTest;
%ignore tesseract::collision::ContinuousContactManager::applyContactManagerConfig;

%include <tesseract/collision/discrete_contact_manager.h>
%include <tesseract/collision/continuous_contact_manager.h>
