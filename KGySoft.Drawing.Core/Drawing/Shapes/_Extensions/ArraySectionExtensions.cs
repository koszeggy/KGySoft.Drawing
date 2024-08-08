using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using KGySoft.Collections;

namespace KGySoft.Drawing.Shapes
{
    internal static class ArraySectionExtensions
    {
        //private sealed class ByteArrayMemoryManager<T> : MemoryManager<T>
        //    where T : unmanaged
        //{
        //    private ArraySection<byte> buffer;
        //    private GCHandle pinnedHandle;

        //    public ByteArrayMemoryManager(ArraySection<byte> buffer) => this.buffer = buffer;

        //    public override Span<T> GetSpan() => MemoryMarshal.Cast<byte, T>(buffer.AsSpan);

        //    public override unsafe MemoryHandle Pin(int elementIndex = 0)
        //    {
        //        if (!pinnedHandle.IsAllocated)
        //            pinnedHandle = GCHandle.Alloc(buffer.UnderlyingArray, GCHandleType.Pinned);
        //        byte* pResult = (byte*)pinnedHandle.AddrOfPinnedObject() + buffer.Offset + elementIndex * sizeof(T);

        //        // Not returning the GCHandle in the result because if it's copied, freeing it could be initiated multiple times.
        //        // Passing only this instance so Unpin will be called that releases handle only once.
        //        return new MemoryHandle(pResult, default, this);
        //    }

        //    public override void Unpin()
        //    {
        //        if (pinnedHandle.IsAllocated)
        //            pinnedHandle.Free();
        //    }

        //    protected override void Dispose(bool disposing)
        //    {
        //        // The original underlying buffer is released explicitly from outside so unpinning only if needed.
        //        Unpin();
        //    }
        //}

        //internal static unsafe Span<T> AllocateSpan<T>(this scoped ref ArraySection<byte> buffer, int elementCount)
        //    where T : unmanaged
        //{
        //    Span<byte> result = buffer.AsSpan.Slice(0, elementCount * sizeof(T));
        //    buffer = buffer.Slice(result.Length);
        //    return MemoryMarshal.Cast<byte, T>(result);
        //}

        //internal static unsafe Memory<T> AllocateMemory<T>(this ref ArraySection<byte> buffer, int elementCount)
        //    where T : unmanaged
        //{
        //    ArraySection<byte> result = buffer.Slice(0, elementCount * sizeof(T));
        //    buffer = buffer.Slice(result.Length);
        //    return new ByteArrayMemoryManager<T>(result).Memory;
        //}

        internal static unsafe CastArray<byte, T> Allocate<T>(this ref ArraySection<byte> buffer, int elementCount)
            where T : unmanaged
        {
            ArraySection<byte> result = buffer.Slice(0, elementCount * sizeof(T));
            buffer = buffer.Slice(result.Length);
            return result.Cast<byte, T>();
        }
    }
}
