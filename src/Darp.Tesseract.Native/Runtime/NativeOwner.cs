using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Darp.Tesseract.Native;

internal sealed class NativeOwner : SafeHandleZeroOrMinusOneIsInvalid
{
    internal NativeOwner(IntPtr pointer) : base(true) => SetHandle(pointer);
    internal HandleRef Handle => new(this, handle);
    internal IntPtr Take()
    {
        var result = handle;
        SetHandleAsInvalid();
        return result;
    }
    protected override bool ReleaseHandle()
    {
        // Cleanup must not consume a pending exception from the caller's native
        // operation (notably inside generated constructor helpers).
        TesseractNativePINVOKE.DarpGeometryInterop_release(new HandleRef(this, handle));
        return true;
    }
}
