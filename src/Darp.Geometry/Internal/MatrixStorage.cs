namespace Darp.Geometry;

// Views and access leases keep this shared storage reachable; there are no per-view references to count.
internal sealed class MatrixStorage(Memory<double> memory, IDisposable? owner)
{
    internal readonly Memory<double> Memory = memory;
    private readonly StorageOwner? _owner = owner is null ? null : new(owner);

    // Only transferred ownership needs finalization. Ordinary managed matrices have no finalizer.
    private sealed class StorageOwner(IDisposable owner)
    {
        ~StorageOwner()
        {
            try { owner.Dispose(); }
            catch { } // Finalizers cannot propagate exceptions from an external owner.
        }
    }
}
