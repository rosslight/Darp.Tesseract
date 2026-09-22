using System.Collections;
using System.Runtime.InteropServices;
using Darp.Geometry.Tensor2;

namespace Darp.Tesseract.Native;

/// <summary>Shared implementation for generated immutable native map bindings.</summary>
public abstract class NativeMap<T> : IReadOnlyDictionary<string, T> where T : struct, IReadOnlyMatrixD
{
    private readonly NativeOwner _owner;
    private readonly int _kind;
    private readonly Func<IntPtr, T> _wrap;
    internal HandleRef Handle => _owner.Handle;
    private protected NativeMap(IntPtr pointer, int kind, Func<IntPtr, T> wrap)
    { _owner = new NativeOwner(pointer); _kind = kind; _wrap = wrap; }
    private protected NativeMap(int kind, Func<IntPtr, T> wrap)
        : this(DarpGeometryInterop.create(kind), kind, wrap) { }
    private protected NativeMap(int kind, Func<IntPtr, T> wrap, IReadOnlyDictionary<string, T> values) : this(kind, wrap)
    {
        try
        {
            foreach (var pair in values)
            {
                using var input = new TensorArgument(pair.Value.AsReadOnlyMatrix());
                DarpGeometryInterop.add(Handle, kind, pair.Key, input.Handle);
            }
        }
        catch { _owner.Dispose(); throw; }
    }
    public int Count => DarpGeometryInterop.count(Handle, _kind);
    public T this[string key]
    {
        get
        {
            if (!ContainsKey(key)) throw new KeyNotFoundException(key);
            return _wrap(DarpGeometryInterop.element(Handle, _kind, 0, key));
        }
    }
    public bool ContainsKey(string key) => DarpGeometryInterop.contains(Handle, _kind, key);
    public bool TryGetValue(string key, out T value)
    {
        if (!ContainsKey(key)) { value = default; return false; }
        value = this[key]; return true;
    }
    public IEnumerable<string> Keys
    {
        get
        {
            using var keys = DarpGeometryInterop.keys(Handle, _kind);
            foreach (string key in keys) yield return key;
        }
    }
    public IEnumerable<T> Values { get { foreach (string key in Keys) yield return this[key]; } }
    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
    { foreach (string key in Keys) yield return new(key, this[key]); }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Shared implementation for generated immutable native sequence bindings.</summary>
public abstract class NativeList<T> : IReadOnlyList<T> where T : struct, IReadOnlyMatrixD
{
    private readonly NativeOwner _owner;
    private readonly int _kind;
    private readonly Func<IntPtr, T> _wrap;
    internal HandleRef Handle => _owner.Handle;
    private protected NativeList(IntPtr pointer, int kind, Func<IntPtr, T> wrap)
    { _owner = new NativeOwner(pointer); _kind = kind; _wrap = wrap; }
    private protected NativeList(int kind, Func<IntPtr, T> wrap)
        : this(DarpGeometryInterop.create(kind), kind, wrap) { }
    private protected NativeList(int kind, Func<IntPtr, T> wrap, IReadOnlyList<T> values) : this(kind, wrap)
    {
        try
        {
            foreach (var value in values)
            {
                using var input = new TensorArgument(value.AsReadOnlyMatrix());
                DarpGeometryInterop.add(Handle, kind, "", input.Handle);
            }
        }
        catch { _owner.Dispose(); throw; }
    }
    public int Count => DarpGeometryInterop.count(Handle, _kind);
    public T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
            return _wrap(DarpGeometryInterop.element(Handle, _kind, index, ""));
        }
    }
    public IEnumerator<T> GetEnumerator() { for (int i = 0; i < Count; i++) yield return this[i]; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
