/**
 * C#-portable overlay for the upstream tesseract_common SWIG module.
 *
 * The canonical upstream interface is:
 *   native/tesseract_python/tesseract_python/swig/tesseract_common_python.i
 *
 * That interface targets Python and imports NumPy/Python-only typemaps. This
 * overlay deliberately reuses its Timer declaration and adds the adjacent,
 * dependency-free Stopwatch API. generate_bindings.ps1 verifies the reused
 * declaration still exists upstream before invoking SWIG.
 */
%module TesseractCommon

%{
#include <tesseract/common/timer.h>
#include <tesseract/common/stopwatch.h>
%}

/*
 * SWIG does not parse the C++17 nested-namespace spelling used by the current
 * headers (`namespace tesseract::common`). Spell the same declarations using
 * the equivalent traditional syntax for SWIG while the generated C++ wrapper
 * compiles against the real headers included above. The callback-based
 * Timer.start overload is deferred until a safe managed callback lifetime API
 * is designed; stop and ownership are already useful as an ABI smoke test.
 */
namespace tesseract
{
namespace common
{
class Timer
{
public:
  Timer();
  ~Timer();
  void stop();
};

class Stopwatch
{
public:
  Stopwatch();
  void start();
  void stop();
  double elapsedMilliseconds() const;
  double elapsedSeconds() const;
};
}
}
