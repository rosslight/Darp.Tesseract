/* Typed containers required by environment commands. */
%template(DoublePair) std::pair<double, double>;
%template(StringDoublePairMap) std::unordered_map<std::string, std::pair<double, double>>;

namespace std {
template <typename T> class optional {
public:
  optional();
  optional(const T& value);
  bool has_value() const;
  T value() const;
  void reset();
};
}
%template(OptionalDouble) std::optional<double>;

/* C++ iterators and UUIDs are not part of the managed trajectory surface. */
%ignore tesseract::common::JointTrajectory::uuid;
%ignore tesseract::common::JointTrajectory::begin;
%ignore tesseract::common::JointTrajectory::end;
%ignore tesseract::common::JointTrajectory::rbegin;
%ignore tesseract::common::JointTrajectory::rend;
%ignore tesseract::common::JointTrajectory::cbegin;
%ignore tesseract::common::JointTrajectory::cend;
%ignore tesseract::common::JointTrajectory::crbegin;
%ignore tesseract::common::JointTrajectory::crend;
%ignore tesseract::common::JointTrajectory::data;
%ignore tesseract::common::JointTrajectory::insert;
%ignore tesseract::common::JointTrajectory::erase;
%ignore tesseract::common::JointTrajectory::push_back(const value_type&&);
%ignore tesseract::common::AllowedCollisionMatrix::AllowedCollisionMatrix(AllowedCollisionMatrix&&);
%ignore tesseract::common::AllowedCollisionMatrix::getAllAllowedCollisions;
%ignore tesseract::common::CollisionMarginPairData::CollisionMarginPairData(const PairsCollisionMarginData&);
%ignore tesseract::common::CollisionMarginPairData::getCollisionMargins;
%ignore tesseract::common::operator<<;

%ignore tesseract::common::AllowedCollisionMatrix::AllowedCollisionMatrix(const AllowedCollisionEntries&);
%include <tesseract/common/allowed_collision_matrix.h>
%include <tesseract/common/collision_margin_data.h>
