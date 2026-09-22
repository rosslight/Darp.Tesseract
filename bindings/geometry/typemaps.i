/* Geometry mappings are selected by native type/qualifier, never method name. */
%{
#include "../geometry/runtime.h"
%}

%ignore darp_geometry::Value;
namespace darp_geometry { struct Value {}; }
%typemap(ctype) darp_geometry::Value* "void *"
%typemap(imtype, out="global::System.IntPtr") darp_geometry::Value* "global::System.Runtime.InteropServices.HandleRef"
%typemap(cstype, out="global::System.IntPtr") darp_geometry::Value* "global::System.Runtime.InteropServices.HandleRef"
%typemap(csin) darp_geometry::Value* "$csinput"
%typemap(csout, excode=SWIGEXCODE) darp_geometry::Value* { var result = $imcall;$excode return result; }
%typemap(in) darp_geometry::Value* { $1 = static_cast<darp_geometry::Value*>($input); }
%typemap(out) darp_geometry::Value* { $result = $1; }

%define DARP_GEOMETRY_CATCH
  catch (const std::exception& error) {
    SWIG_CSharpSetPendingExceptionArgument(SWIG_CSharpArgumentException, error.what(), "");
    return $null;
  }
%enddef

%define DARP_TENSOR(TYPE, MANAGED, WRAP, MUTABLE, TAKE)
%ignore TYPE;
%typemap(ctype) TYPE, const TYPE&, TYPE* "void *"
%typemap(imtype, out="global::System.IntPtr") TYPE, const TYPE&, TYPE* "global::System.Runtime.InteropServices.HandleRef"
%typemap(cstype) TYPE, const TYPE&, TYPE* "global::Darp.Geometry.Tensor2.MANAGED"
%typemap(csin,
  pre="    using (var $csinput_arg = new TensorArgument($csinput.AsReadOnlyMatrix())) {",
  terminator="    }",
  cshin="$csinput") TYPE, const TYPE&, TYPE* "$csinput_arg.Handle"
%typemap(csout, excode=SWIGEXCODE) TYPE, const TYPE&, TYPE* {
    var result = $imcall;$excode
    return TensorResult.WRAP(result);
  }
%typemap(csvarout, excode=SWIGEXCODE2) TYPE, const TYPE&, TYPE* %{
    get {
      var result = $imcall;$excode
      return TensorResult.WRAP(result);
    }
%}
%typemap(csvarin, excode=SWIGEXCODE2) TYPE, const TYPE&, TYPE* %{
    set {
      using (var value_arg = new TensorArgument(value.AsReadOnlyMatrix())) {
        $imcall;$excode
      }
    }
%}
%typemap(in, canthrow=1) TYPE {
  try { $1 = darp_geometry::read<TYPE>(static_cast<darp_geometry::Value*>($input)); }
  DARP_GEOMETRY_CATCH
}
%typemap(in, canthrow=1) const TYPE& (TYPE local) {
  try { local = darp_geometry::read<TYPE>(static_cast<darp_geometry::Value*>($input)); $1 = &local; }
  DARP_GEOMETRY_CATCH
}
%typemap(in, canthrow=1) TYPE* (TYPE local) {
  try { local = darp_geometry::read<TYPE>(static_cast<darp_geometry::Value*>($input)); $1 = &local; }
  DARP_GEOMETRY_CATCH
}
%typemap(out, canthrow=1) TYPE {
  try { $result = new darp_geometry::Value(darp_geometry::owned_tensor<TYPE>(SWIG_STD_MOVE(*(&$1)))); }
  DARP_GEOMETRY_CATCH
}
// References and properties get stable snapshots: const does not imply that
// the original owner cannot mutate, resize or explicitly dispose its storage.
%typemap(out, canthrow=1) const TYPE&, TYPE* {
  try { $result = new darp_geometry::Value(darp_geometry::owned_tensor<TYPE>(*$1)); }
  DARP_GEOMETRY_CATCH
}

%typemap(ctype) TYPE& "void *"
%typemap(imtype) TYPE& "global::System.Runtime.InteropServices.HandleRef"
%typemap(cstype) TYPE& "ref global::Darp.Geometry.Tensor2.MUTABLE"
%typemap(csin,
  pre="    using (var $csinput_arg = new TensorArgument($csinput.AsReadOnlyMatrix())) {",
  post="      if ($csinput_arg.HasOutput) $csinput = $csinput_arg.TAKE();",
  terminator="    }",
  cshin="ref $csinput") TYPE& "$csinput_arg.Handle"
%typemap(in, canthrow=1) TYPE& (TYPE local) {
  try { local = darp_geometry::read<TYPE>(static_cast<darp_geometry::Value*>($input)); $1 = &local; }
  DARP_GEOMETRY_CATCH
}
%typemap(argout, canthrow=1) TYPE& {
  try { *static_cast<darp_geometry::Value*>($input) = darp_geometry::owned_tensor<TYPE>(std::move(*$1)); }
  DARP_GEOMETRY_CATCH
}
%enddef

%define DARP_TENSOR_REF(TYPE, MANAGED, MUTABLE)
%typemap(ctype) const Eigen::Ref<const TYPE>& "void *"
%typemap(imtype) const Eigen::Ref<const TYPE>& "global::System.Runtime.InteropServices.HandleRef"
%typemap(cstype) const Eigen::Ref<const TYPE>& "global::Darp.Geometry.Tensor2.MANAGED"
%typemap(csin,
  pre="    using (var $csinput_arg = new TensorArgument($csinput.AsReadOnlyMatrix())) {",
  terminator="    }",
  cshin="$csinput") const Eigen::Ref<const TYPE>& "$csinput_arg.Handle"
%typemap(in, canthrow=1) const Eigen::Ref<const TYPE>& (std::optional<darp_geometry::ConstRef<TYPE>> local) {
  try { local.emplace(static_cast<darp_geometry::Value*>($input)); $1 = &local->ref.value(); }
  DARP_GEOMETRY_CATCH
}
%typemap(ctype) Eigen::Ref<TYPE> "void *"
%typemap(imtype) Eigen::Ref<TYPE> "global::System.Runtime.InteropServices.HandleRef"
%typemap(cstype) Eigen::Ref<TYPE> "global::Darp.Geometry.Tensor2.MUTABLE"
%typemap(csin,
  pre="    using (var $csinput_arg = new TensorArgument($csinput.AsReadOnlyMatrix())) {",
  post="      if ($csinput_arg.HasOutput) $csinput_arg.CopyBack($csinput.AsMatrix());",
  terminator="    }",
  cshin="$csinput") Eigen::Ref<TYPE> "$csinput_arg.Handle"
%typemap(in, canthrow=1) Eigen::Ref<TYPE> (TYPE local, std::optional<Eigen::Ref<TYPE>> ref) {
  try { local = darp_geometry::read<TYPE>(static_cast<darp_geometry::Value*>($input)); ref.emplace(local); $1 = *ref; }
  DARP_GEOMETRY_CATCH
}
%typemap(argout, canthrow=1) Eigen::Ref<TYPE> {
  try { *static_cast<darp_geometry::Value*>($input) = darp_geometry::owned_tensor<TYPE>(TYPE(*(&$1))); }
  DARP_GEOMETRY_CATCH
}
%enddef

%define DARP_CONTAINER(TYPE, MANAGED)
%typemap(ctype) TYPE, const TYPE&, TYPE* "void *"
%typemap(imtype, out="global::System.IntPtr") TYPE, const TYPE&, TYPE* "global::System.Runtime.InteropServices.HandleRef"
%typemap(cstype) TYPE, const TYPE&, TYPE* "MANAGED"
%typemap(csin) TYPE, const TYPE&, TYPE* "$csinput.Handle"
%typemap(in, canthrow=1) TYPE {
  try { $1 = darp_geometry::container<TYPE>(static_cast<darp_geometry::Value*>($input)); }
  DARP_GEOMETRY_CATCH
}
%typemap(in, canthrow=1) const TYPE&, TYPE* {
  try { $1 = &darp_geometry::container<TYPE>(static_cast<darp_geometry::Value*>($input)); }
  DARP_GEOMETRY_CATCH
}
%typemap(out, canthrow=1) TYPE {
  try { $result = new darp_geometry::Value(darp_geometry::owned_container<TYPE>(SWIG_STD_MOVE(*(&$1)))); }
  DARP_GEOMETRY_CATCH
}
%typemap(out, canthrow=1) const TYPE&, TYPE* {
  try { $result = new darp_geometry::Value(darp_geometry::owned_container<TYPE>(*$1)); }
  DARP_GEOMETRY_CATCH
}
%typemap(csout, excode=SWIGEXCODE) TYPE, const TYPE&, TYPE* {
    var result = $imcall;$excode
    return new MANAGED(result);
  }
%typemap(csvarout, excode=SWIGEXCODE2) TYPE, const TYPE&, TYPE* %{
    get { var result = $imcall;$excode return new MANAGED(result); }
%}
%typemap(csvarin, excode=SWIGEXCODE2) TYPE, const TYPE&, TYPE* %{
    set { $imcall;$excode }
%}
%typemap(ctype) TYPE& "void *"
%typemap(imtype) TYPE& "global::System.Runtime.InteropServices.HandleRef"
%typemap(cstype) TYPE& "ref MANAGED"
%typemap(csin,
  pre="    using (var $csinput_arg = new ContainerArgument($csinput.Handle)) {",
  post="      if ($csinput_arg.HasOutput) $csinput = new MANAGED($csinput_arg.Take());",
  terminator="    }",
  cshin="ref $csinput") TYPE& "$csinput_arg.Handle"
%typemap(in, canthrow=1) TYPE& (TYPE local) {
  try { local = darp_geometry::container<TYPE>(static_cast<darp_geometry::Value*>($input)); $1 = &local; }
  DARP_GEOMETRY_CATCH
}
%typemap(argout, canthrow=1) TYPE& {
  try { *static_cast<darp_geometry::Value*>($input) = darp_geometry::owned_container<TYPE>(std::move(*$1)); }
  DARP_GEOMETRY_CATCH
}
%enddef

/* Frozen collections use the shared managed owner instead of SWIG's disposable proxy. */
%define DARP_CONTAINER_PROXY(TYPE, MANAGED, BASE, ELEMENT, KIND, WRAP)
%nodefaultctor TYPE;
%nodefaultdtor TYPE;
%typemap(csclassmodifiers) TYPE "public sealed class";
%typemap(csbase) TYPE "BASE<global::Darp.Geometry.Tensor2.ELEMENT>";
%typemap(csinterfaces) TYPE "";
%typemap(csdispose) TYPE "";
%typemap(csdisposing) TYPE "";
%typemap(csbody) TYPE %{
  public MANAGED() : base(KIND, TensorResult.WRAP) { }
  internal MANAGED(global::System.IntPtr handle) : base(handle, KIND, TensorResult.WRAP) { }
%}
%enddef

%define DARP_MAP_PROXY(TYPE, MANAGED, ELEMENT, KIND, WRAP)
DARP_CONTAINER_PROXY(TYPE, MANAGED, NativeMap, ELEMENT, KIND, WRAP)
%typemap(cscode) TYPE %{
  public MANAGED(global::System.Collections.Generic.IReadOnlyDictionary<string, global::Darp.Geometry.Tensor2.ELEMENT> values)
    : base(KIND, TensorResult.WRAP, values) { }
%}
%enddef

%define DARP_LIST_PROXY(TYPE, MANAGED, ELEMENT, KIND, WRAP)
DARP_CONTAINER_PROXY(TYPE, MANAGED, NativeList, ELEMENT, KIND, WRAP)
%typemap(cscode) TYPE %{
  public MANAGED(global::System.Collections.Generic.IReadOnlyList<global::Darp.Geometry.Tensor2.ELEMENT> values)
    : base(KIND, TensorResult.WRAP, values) { }
%}
%enddef
