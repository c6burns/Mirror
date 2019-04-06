namespace Mirror.Buffers
{
    public interface IBufferAllocator
    {
        IBuffer Acquire(ulong minSizeInBytes);
        IBuffer Reacquire(IBuffer buffer, ulong newMinSizeInBytes);
        void Release(IBuffer buffer);
    }
}