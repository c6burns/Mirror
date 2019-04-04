#define MIRROR_BUFFER_CHECK_BOUNDS
#define MIRROR_BUFFER_DYNAMIC_GROWTH

using System;
using System.Text;

namespace Mirror.Buffers
{
    public interface IBuffer
    {
        
    }

    internal unsafe class Buffer : IBuffer
    {
        private IBufferAllocator _bufferAllocator;
        private byte[] _buffer;
        private int _offset;
        private int _position;
        private int _length;
        private int _capacity;

        //public int Position { get { return _position; } set { writer.BaseStream.Position = value; } }

        internal Buffer()
        {
        }

        internal void Setup(byte[] buf, int offset, int capacity)
        {

        }

        private void CheckPosition(int addToPos)
        {
            int newPos = _position + addToPos;
            if (newPos < 0)
            {
                throw new ArgumentOutOfRangeException("buffer cursor position cannot be negative");
            }

            if (newPos >= _capacity)
            {
#if MIRROR_BUFFER_DYNAMIC_GROWTH

                BufferManager.ReacquireBuffer(this, _capacity << 1);
#else
                throw new ArgumentOutOfRangeException("buffer cursor position cannot be greater than buffer capacity");
#endif
            }
        }

        private void UpdatePosition(int addToPos)
        {
            _position += addToPos;
            if (_position > _length)
            {
                _length = _position;
            }
        }

        private void Write(bool src) => Write((byte)(src ? 1 : 0));

        private void Write(sbyte src) => Write((byte)src);
        private unsafe void Write(byte src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckPosition(BufferConstants.SizeOfByte);
#endif
            fixed (byte* dst = &_buffer[_offset + _position])
            {
                *dst = src;
            }
            UpdatePosition(BufferConstants.SizeOfByte);
        }

        private void Write(ushort src) => Write((short)src);
        private unsafe void Write(short src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckPosition(BufferConstants.SizeOfShort);
#endif
            fixed (byte* dst = &_buffer[_offset + _position])
            {
                *(short*)dst = src;
            }
            UpdatePosition(BufferConstants.SizeOfShort);
        }

        private void Write(uint src) => Write((int)src);
        private unsafe void Write(int src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckPosition(BufferConstants.SizeOfInt);
#endif
            fixed (byte* dst = &_buffer[_offset + _position])
            {
                *(int*)dst = src;
            }
            UpdatePosition(BufferConstants.SizeOfInt);
        }

        private void Write(ulong src) => Write((long)src);
        private unsafe void Write(long src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckPosition(BufferConstants.SizeOfLong);
#endif
            fixed (byte* dst = &_buffer[_offset + _position])
            {
                *(long*)dst = src;
            }
            UpdatePosition(BufferConstants.SizeOfLong);
        }

        private unsafe void Write(float src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckPosition(BufferConstants.SizeOfFloat);
#endif
            fixed (byte* dst = &_buffer[_offset + _position])
            {
                *(float*)dst = src;
            }
            UpdatePosition(BufferConstants.SizeOfFloat);
        }

        private unsafe void Write(double src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckPosition(BufferConstants.SizeOfDouble);
#endif
            fixed (byte* dst = &_buffer[_offset + _position])
            {
                *(double*)dst = src;
            }
            UpdatePosition(BufferConstants.SizeOfDouble);
        }

        public unsafe void Write(string src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckPosition(Encoding.UTF8.GetByteCount(src));
#endif
            int written;

            fixed (char* s = src)
            fixed (byte* dst = &_buffer[_offset + _position])
            {
                written = Encoding.UTF8.GetBytes(s, src.Length, dst, _capacity - _position);
            }
            UpdatePosition(written);
        }
    }
}