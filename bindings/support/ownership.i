/* Convert upstream unique_ptr results into the same generated shared_ptr
 * proxy used for the corresponding class. This transfers ownership once and
 * avoids exposing SWIGTYPE placeholders or adding forwarding entry points. */
%define DARP_UNIQUE_PTR_TO_SHARED(TYPE)
%typemap(ctype) std::unique_ptr<TYPE> "void *"
%typemap(imtype, out="global::System.IntPtr") std::unique_ptr<TYPE> "global::System.Runtime.InteropServices.HandleRef"
%typemap(cstype) std::unique_ptr<TYPE> "$typemap(cstype, TYPE)"
%typemap(csout, excode=SWIGEXCODE) std::unique_ptr<TYPE> {
    global::System.IntPtr cPtr = $imcall;
    $typemap(cstype, TYPE) ret = (cPtr == global::System.IntPtr.Zero)
      ? null
      : new $typemap(cstype, TYPE)(cPtr, true);$excode
    return ret;
  }
%typemap(out) std::unique_ptr<TYPE>
{
  std::unique_ptr<TYPE> swig_result = std::move($1);
  $result = swig_result
    ? new std::shared_ptr<TYPE>(std::move(swig_result))
    : nullptr;
}
%enddef

/* Materialize a move-only value result directly into the generated shared_ptr
 * proxy. SwigValueWrapper overloads operator& to expose its stored T; moving
 * that T avoids MSVC selecting the wrapped type's deleted copy constructor. */
%define DARP_MOVE_ONLY_VALUE_TO_SHARED(TYPE)
%typemap(out) TYPE
{
  $result = new std::shared_ptr<TYPE>(new TYPE(SWIG_STD_MOVE(*(&$1))));
}
%enddef
