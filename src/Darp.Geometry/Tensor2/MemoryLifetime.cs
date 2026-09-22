using System.Buffers;
using System.Runtime.InteropServices;

namespace Darp.Geometry.Tensor2;

internal static class MemoryLifetime
{
    // Array-backed spans already retain the array. Native-backed spans need
    // their manager retained separately. Extracting it here keeps the memory's
    // owner reachable until the caller's finally block, without boxing a descriptor.
    internal static void KeepAlive(ReadOnlyMemory<double> memory)
    {
        MemoryMarshal.TryGetMemoryManager<double, MemoryManager<double>>(memory, out var manager);
        GC.KeepAlive(manager);
    }
}
