#define MIRROR_BUFFER_PEDANTIC_ALLOCATOR

using System.Collections.Generic;
using System.Buffers;

namespace Mirror.Buffers
{
    public sealed class BufferAllocator : IBufferAllocator
    {
        Stack<Buffer> _bufferPool = new Stack<Buffer>();
        ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
        IBuffer IBufferAllocator.Acquire(ulong minSizeInBytes)
        {
            Buffer buffer;
            if (_bufferPool.Count > 0)
            {
                buffer = _bufferPool.Pop();
            }
            else
            {
                buffer = new Buffer();
            }

            byte[] bytes = _arrayPool.Rent((int) minSizeInBytes);
            buffer.Setup(this, bytes, 0, (ulong) bytes.Length);

            return buffer;
        }

        IBuffer IBufferAllocator.Reacquire(IBuffer ibuffer, ulong newMinSizeInBytes)
        {
            if (ibuffer is Buffer buffer)
            {
#if MIRROR_BUFFER_PEDANTIC_ALLOCATOR
                if (_bufferPool.Contains(buffer))
                {
                    throw new System.ArgumentException("Do not Reacquire buffers which have been Released.", ibuffer.ToString());
                }
#endif
                // one of two options here:
                // 1) rent new array from ArrayPool, copy from old, release old
                // 2) buffer segments / system.io.pipelines magic
                // for now option 1)
                if (newMinSizeInBytes < buffer.Capacity) return buffer;

                byte[] bytes = _arrayPool.Rent((int) newMinSizeInBytes);
                buffer.Setup(this, bytes, 0, (ulong) bytes.Length);

                return ibuffer;
            }
            else
            {
                throw new System.ArgumentException("Do not Reacquire buffers Acquired from a different Allocator!", ibuffer.ToString());
            }
        }

        void IBufferAllocator.Release(IBuffer ibuffer)
        {
            if (ibuffer is Buffer buffer)
            {
#if MIRROR_BUFFER_PEDANTIC_ALLOCATOR
                if (_bufferPool.Contains(buffer))
                {
                    throw new System.ArgumentException("Do not Release buffers twice.", ibuffer.ToString());
                }
#endif
                _bufferPool.Push(buffer);
            }
            else
            {
                throw new System.ArgumentException("Do not Release buffers Acquired from a different Allocator!", ibuffer.ToString());
            }
        }
    }
}
