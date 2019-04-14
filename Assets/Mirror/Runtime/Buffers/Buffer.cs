#define MIRROR_BUFFER_CHECK_BOUNDS
#define MIRROR_BUFFER_DYNAMIC_GROWTH
#define MIRROR_BUFFER_WARN_ALL
#define MIRROR_BUFFER_ZERO_ON_RESIZE

using System;
using System.Buffers;
using UnityEngine;

namespace Mirror.Buffers
{
    internal sealed class Buffer : IBuffer
    {
#if MIRROR_BUFFER_DYNAMIC_GROWTH
        BufferAllocator _allocator;
#endif

        IMemoryOwner<byte> _ownedMem;
        Memory<byte> _buffer => _ownedMem.Memory;
        int _position;
        int _length;
        int _capacity;

        internal int Capacity => _capacity;

        int IBuffer.Position
        {
            get
            {
                return _position;
            }
            set
            {
                CheckCapacity(value);
                _position = value;
                _length = BufferUtil.Max(_position, _length);
            }
        }

        int IBuffer.Length
        {
            get
            {
                return _length;
            }
            set
            {
                CheckCapacity(value);
#if MIRROR_BUFFER_ZERO_ON_RESIZE
                if (_length > value)
                {
#if MIRROR_BUFFER_WARN_ALL
                    Debug.LogWarning("Downsizing Buffer from " + _length + " to " + value);
#endif
                    _buffer.Slice(value, (_length - value)).Span.Fill(0);
                }
#endif
                _length = value;
                _position = BufferUtil.Min(_position, _length);
            }
        }

        internal Buffer()
        {
        }

        internal IMemoryOwner<byte> Setup(BufferAllocator allocator, IMemoryOwner<byte> memOwner)
        {
#if MIRROR_BUFFER_DYNAMIC_GROWTH
            _allocator = allocator;
#endif
            IMemoryOwner<byte> retMem = _ownedMem;
            if (_ownedMem != null && memOwner != null)
            {
                _buffer.CopyTo(memOwner.Memory);
            }
            else
            {
                _position = 0;
                _length = 0;
            }
            _ownedMem = memOwner;
            _capacity = memOwner.Memory.Length;

            return retMem;
        }

        void CheckCapacity(int minimum)
        {
            if (minimum > _capacity)
            {
#if MIRROR_BUFFER_DYNAMIC_GROWTH
                minimum = BufferUtil.NextPow2(minimum);
#if MIRROR_BUFFER_WARN_ALL
                Debug.LogWarning("Upsizing Buffer from " + _capacity + " to " + minimum);
#endif
                _allocator.Reacquire(this, minimum);
#else
                throw new ArgumentException("buffer dynamic growth is disabled");
#endif
            }
        }

#if MIRROR_BUFFER_CHECK_BOUNDS
        void CheckWrite(int addToPos)
        {
            int newPos = _position + addToPos;

            if (newPos > _length)
            {
                _length = newPos;
                CheckCapacity(newPos);
            }
        }

        void CheckRead(int addToPos)
        {
            int newPos = _position + addToPos;

            if (newPos > _length)
            {
                throw new System.IO.EndOfStreamException("buffer cursor position cannot be greater than buffer length");
            }
        }
#endif

        void UpdateWrite(int addToPos)
        {
            _position += addToPos;
            _length = BufferUtil.Max(_position, _length);
        }

        void UpdateRead(int addToPos)
        {
            _position += addToPos;
        }

        void IBuffer.WriteByte(byte src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(sizeof(byte));
#endif
            UpdateWrite(BufferUtil.WriteByte(_buffer.Span, _position, src));
        }

        void IBuffer.WriteUShort(ushort src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(sizeof(ushort));
#endif
            UpdateWrite(BufferUtil.WriteUShort(_buffer.Span, _position, src));
        }

        void IBuffer.WriteUInt(uint src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(sizeof(uint));
#endif
            UpdateWrite(BufferUtil.WriteUInt(_buffer.Span, _position, src));
        }

        void IBuffer.WriteULong(ulong src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(sizeof(ulong));
#endif
            UpdateWrite(BufferUtil.WriteULong(_buffer.Span, _position, src));
        }

        void IBuffer.WriteFloat(float src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(sizeof(float));
#endif
            UpdateWrite(BufferUtil.WriteFloat(_buffer.Span, _position, src));
        }

        void IBuffer.WriteDouble(double src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(sizeof(double));
#endif
            UpdateWrite(BufferUtil.WriteDouble(_buffer.Span, _position, src));
        }

        void IBuffer.WriteBytes(byte[] data, int offset, int length)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(length);
#endif
            UpdateRead(BufferUtil.WriteBytes(_buffer.Span, _position, data, offset, length));
        }

        void IBuffer.WriteString(string src)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckWrite(BufferUtil.StringByteCount(src));
#endif
            UpdateWrite(BufferUtilUnsafe.Write(_buffer.Span, _position, src));
        }

        byte IBuffer.ReadByte()
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(sizeof(byte));
#endif
            UpdateRead(BufferUtil.ReadByte(out byte dst, _buffer.Span, _position));
            return dst;
        }

        ushort IBuffer.ReadUShort()
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(sizeof(ushort));
#endif
            UpdateRead(BufferUtil.ReadUShort(out ushort dst, _buffer.Span, _position));
            return dst;
        }

        uint IBuffer.ReadUInt()
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(sizeof(uint));
#endif
            UpdateRead(BufferUtil.ReadUInt(out uint dst, _buffer.Span, _position));
            return dst;
        }

        ulong IBuffer.ReadULong()
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(sizeof(ulong));
#endif
            UpdateRead(BufferUtil.ReadULong(out ulong dst, _buffer.Span, _position));
            return dst;
        }

        float IBuffer.ReadFloat()
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(sizeof(float));
#endif
            UpdateRead(BufferUtil.ReadFloat(out float dst, _buffer.Span, _position));
            return dst;
        }

        double IBuffer.ReadDouble()
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(sizeof(double));
#endif
            UpdateRead(BufferUtil.ReadDouble(out double dst, _buffer.Span, _position));
            return dst;
        }

        int IBuffer.ReadBytes(byte[] data, int offset, int length)
        {
            length = BufferUtil.Min(length, _length - _position);
            UpdateRead(BufferUtil.ReadBytes(_buffer.Span, _position, data, offset, length));
            return length;
        }

        string IBuffer.ReadString(int length)
        {
#if MIRROR_BUFFER_CHECK_BOUNDS
            CheckRead(length);
#endif
            UpdateRead(BufferUtilUnsafe.Read(out string dst, _buffer.Span, _position, length));
            return dst;
        }
    }
}
