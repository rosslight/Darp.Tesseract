using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Darp.Tesseract.Native;

/// <summary>Immutable native map. Retrieved elements are managed copies independent of this container.</summary>
public abstract class NativeMap<T> : IReadOnlyDictionary<string, T>, IDisposable
{
    private readonly NativeOwner _owner;
    private readonly int _kind;
    private readonly Func<IntPtr, T> _wrap;
    internal NativeOwner Owner => _owner;
    private protected NativeMap(IntPtr pointer, int kind, Func<IntPtr, T> wrap)
    { _owner = new NativeOwner(pointer); _kind = kind; _wrap = wrap; }
    private protected NativeMap(int kind, Func<IntPtr, T> wrap)
        : this(DarpGeometryInterop.create(kind), kind, wrap) { }
    private protected NativeMap(int kind, Func<IntPtr, T> wrap, IReadOnlyDictionary<string, T> values, Func<T, TensorArgument> argument) : this(kind, wrap)
    {
        try
        {
            foreach (var pair in values)
            {
                using var input = argument(pair.Value);
                DarpGeometryInterop.add(_owner.Handle, kind, pair.Key, input.Handle);
            }
        }
        catch { _owner.Dispose(); throw; }
    }
    public int Count
    {
        get
        {
            using var lease = _owner.Borrow();
            return DarpGeometryInterop.count(lease.Handle, _kind);
        }
    }
    public T this[string key]
    {
        get
        {
            using var lease = _owner.Borrow();
            if (!DarpGeometryInterop.contains(lease.Handle, _kind, key)) throw new KeyNotFoundException(key);
            return _wrap(DarpGeometryInterop.element(lease.Handle, _kind, 0, key));
        }
    }
    public bool ContainsKey(string key)
    {
        using var lease = _owner.Borrow();
        return DarpGeometryInterop.contains(lease.Handle, _kind, key);
    }
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
    {
        using var lease = _owner.Borrow();
        if (!DarpGeometryInterop.contains(lease.Handle, _kind, key)) { value = default; return false; }
        value = _wrap(DarpGeometryInterop.element(lease.Handle, _kind, 0, key));
        return true;
    }
    public IEnumerable<string> Keys
    {
        get
        {
            using var lease = _owner.Borrow();
            using var keys = DarpGeometryInterop.keys(lease.Handle, _kind);
            foreach (string key in keys) yield return key;
        }
    }
    public IEnumerable<T> Values { get { foreach (var pair in this) yield return pair.Value; } }
    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
    {
        using var lease = _owner.Borrow();
        using var keys = DarpGeometryInterop.keys(lease.Handle, _kind);
        foreach (string key in keys)
            yield return new(key, _wrap(DarpGeometryInterop.element(lease.Handle, _kind, 0, key)));
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Dispose() => _owner.Dispose();
}

/// <summary>Immutable native sequence. Retrieved elements are managed copies independent of this container.</summary>
public abstract class NativeList<T> : IReadOnlyList<T>, IDisposable
{
    private readonly NativeOwner _owner;
    private readonly int _kind;
    private readonly Func<IntPtr, T> _wrap;
    internal NativeOwner Owner => _owner;
    private protected NativeList(IntPtr pointer, int kind, Func<IntPtr, T> wrap)
    { _owner = new NativeOwner(pointer); _kind = kind; _wrap = wrap; }
    private protected NativeList(int kind, Func<IntPtr, T> wrap)
        : this(DarpGeometryInterop.create(kind), kind, wrap) { }
    private protected NativeList(int kind, Func<IntPtr, T> wrap, IReadOnlyList<T> values, Func<T, TensorArgument> argument) : this(kind, wrap)
    {
        try
        {
            foreach (var value in values)
            {
                using var input = argument(value);
                DarpGeometryInterop.add(_owner.Handle, kind, "", input.Handle);
            }
        }
        catch { _owner.Dispose(); throw; }
    }
    public int Count
    {
        get
        {
            using var lease = _owner.Borrow();
            return DarpGeometryInterop.count(lease.Handle, _kind);
        }
    }
    public T this[int index]
    {
        get
        {
            using var lease = _owner.Borrow();
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, DarpGeometryInterop.count(lease.Handle, _kind));
            return _wrap(DarpGeometryInterop.element(lease.Handle, _kind, index, ""));
        }
    }
    public IEnumerator<T> GetEnumerator()
    {
        using var lease = _owner.Borrow();
        int count = DarpGeometryInterop.count(lease.Handle, _kind);
        for (int i = 0; i < count; i++) yield return _wrap(DarpGeometryInterop.element(lease.Handle, _kind, i, ""));
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Dispose() => _owner.Dispose();
}
