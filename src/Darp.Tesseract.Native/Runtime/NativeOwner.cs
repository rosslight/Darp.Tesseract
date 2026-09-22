using System.Buffers;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Darp.Tesseract.Native;

internal sealed class NativeOwner : SafeHandleZeroOrMinusOneIsInvalid
{
    internal NativeOwner(IntPtr pointer) : base(true) => SetHandle(pointer);
    internal HandleRef Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsClosed, this);
            return new(this, handle);
        }
    }
    internal NativeLease Borrow() => new(this);
    internal IntPtr Take()
    {
        ObjectDisposedException.ThrowIf(IsClosed, this);
        var result = handle;
        SetHandleAsInvalid();
        return result;
    }
    protected override bool ReleaseHandle()
    {
        // Cleanup must not consume a pending exception from a generated constructor.
        TesseractNativePINVOKE.DarpGeometryInterop_release(new HandleRef(this, handle));
        return true;
    }
}

/// <summary>Keeps a native handle alive across explicit owner disposal.</summary>
internal sealed class NativeLease : IDisposable, IPinnable
{
    private NativeOwner? _owner;
    internal HandleRef Handle { get; }
    internal NativeLease(NativeOwner owner)
    {
        bool retained = false;
        try
        {
            owner.DangerousAddRef(ref retained);
            _owner = owner;
            Handle = new HandleRef(this, owner.DangerousGetHandle());
        }
        catch
        {
            if (retained) owner.DangerousRelease();
            throw;
        }
    }
    ~NativeLease() => Release();
    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }
    private void Release() => Interlocked.Exchange(ref _owner, null)?.DangerousRelease();
    public void Unpin() => Dispose();
    public MemoryHandle Pin(int elementIndex) => throw new NotSupportedException();
}
