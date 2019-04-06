#define MIRROR_BUFFER_CHECK_BOUNDS
#define MIRROR_BUFFER_DYNAMIC_GROWTH

using System;
using System.Text;

namespace Mirror.Buffers
{
    internal sealed unsafe class Buffer : IBuffer
    {
        IBufferAllocator _allocator;
        byte[] _buffer;
        ulong _offset;
        ulong _position;
        ulong _length;

        internal ulong Capacity { get; private set; }

        public ulong Position
        {
            get
            {
                return _position;
            }
            set
            {
#if MIRROR_BUFFER_CHECK_BOUNDS
                CheckPosition(value);
#endif
                _position = value;
            }
        }

        internal Buffer()
        {
        }

        internal void Setup(IBufferAllocator allocator, byte[] buf, ulong offset, ulong capacity)
        {
            _allocator = allocator;
            _buffer = buf;
            _offset = offset;
            _position = 0;
            _length = 0;
            Capacity = capacity;
        }

#if MIRROR_BUFFER_CHECK_BOUNDS
        void CheckWrite(ulong addToPos)
        {
            ulong newPos = _position + addToPos;

            if (newPos > Capacity)
            {
#if MIRROR_BUFFER_DYNAMIC_GROWTH
                _allocator.Reacquire(this, Capacity << 1);
#else
                throw new ArgumentOutOfRangeException("buffer cursor position cannot be greater than buffer capacity");
#endif
            }
        }

        void CheckRead(ulong addToPos)
        {
            ulong newPos = _position + addToPos;

            if (newPos > _length)
            {
                throw new ArgumentOutOfRangeException("buffer cursor position cannot be greater than buffer length");
            }
        }

        void CheckPosition(ulong newPosition)
        {
            if (newPosition > _length)
            {
                throw new ArgumentOutOfRangeException("buffer cursor position cannot be greater than buffer length");
            }
        }
#endif

        void UpdateWrite(uint addToPos)
        {
            _position += addToPos;
            _length = BufferUtil.Max(_position, _length);
        }

        void UpdateRead(uint addToPos)
        {
            _position += addToPos;
        }

        unsafe void IBuffer.WriteByte(byte src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(sizeof(byte));
#endif
            UpdateWrite(BufferUtil.UnsafeWrite(_buffer, _offset + _position, src));
        }

        unsafe void IBuffer.WriteUShort(ushort src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(sizeof(ushort));
#endif
            UpdateWrite(BufferUtil.UnsafeWrite(_buffer, _offset + _position, src));
        }

        unsafe void IBuffer.WriteUInt(uint src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(sizeof(uint));
#endif
            UpdateWrite(BufferUtil.UnsafeWrite(_buffer, _offset + _position, src));
        }

        unsafe void IBuffer.WriteULong(ulong src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(sizeof(ulong));
#endif
            UpdateWrite(BufferUtil.UnsafeWrite(_buffer, _offset + _position, src));
        }

        unsafe void IBuffer.WriteFloat(float src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(sizeof(float));
#endif
            UpdateWrite(BufferUtil.UnsafeWrite(_buffer, _offset + _position, src));
        }

        unsafe void IBuffer.WriteDouble(double src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(sizeof(double));
#endif
            UpdateWrite(BufferUtil.UnsafeWrite(_buffer, _offset + _position, src));
        }

        unsafe void IBuffer.WriteString(string src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(BufferUtil.StringByteCount(src));
#endif
            UpdateWrite(BufferUtil.UnsafeWrite(_buffer, _offset + _position, src));
        }

        unsafe byte IBuffer.ReadByte()
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(sizeof(byte));
#endif
            UpdateRead(BufferUtil.UnsafeRead(out byte dst, _buffer, _offset + _position));
            return dst;
        }

        unsafe ushort IBuffer.ReadUShort()
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(sizeof(ushort));
#endif
            UpdateRead(BufferUtil.UnsafeRead(out ushort dst, _buffer, _offset + _position));
            return dst;
        }

        unsafe uint IBuffer.ReadUInt()
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(sizeof(uint));
#endif
            UpdateRead(BufferUtil.UnsafeRead(out uint dst, _buffer, _offset + _position));
            return dst;
        }

        unsafe ulong IBuffer.ReadULong()
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(sizeof(ulong));
#endif
            UpdateRead(BufferUtil.UnsafeRead(out ulong dst, _buffer, _offset + _position));
            return dst;
        }

        unsafe float IBuffer.ReadFloat()
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(sizeof(float));
#endif
            UpdateRead(BufferUtil.UnsafeRead(out float dst, _buffer, _offset + _position));
            return dst;
        }

        unsafe double IBuffer.ReadDouble()
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(sizeof(double));
#endif
            UpdateRead(BufferUtil.UnsafeRead(out double dst, _buffer, _offset + _position));
            return dst;
        }

        unsafe string IBuffer.ReadString(uint length)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(length);
#endif
            UpdateRead(BufferUtil.UnsafeRead(out string dst, _buffer, _offset + _position, (int)length));
            return dst;
        }
    }
}
