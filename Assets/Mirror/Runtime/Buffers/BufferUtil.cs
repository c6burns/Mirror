using System;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mirror.Buffers
{
    public static class BufferUtil
    {
        const MethodImplOptions Inline = MethodImplOptions.AggressiveInlining;
        static Encoding _encoding = new UTF8Encoding(false, true);

        [MethodImpl(Inline)]
        internal static int StringByteCount(string stringSrc) => _encoding.GetByteCount(stringSrc);

        #region Min and Max: inlined
        [MethodImpl(Inline)]
        public static byte Min(byte x, byte y) => x < y ? x : y;
        [MethodImpl(Inline)]
        public static sbyte Min(sbyte x, sbyte y) => (sbyte)Min((byte)x, (byte)y);

        [MethodImpl(Inline)]
        public static byte Max(byte x, byte y) => x > y ? x : y;
        [MethodImpl(Inline)]
        public static sbyte Max(sbyte x, sbyte y) => (sbyte)Max((byte)x, (byte)y);

        [MethodImpl(Inline)]
        public static ushort Min(ushort x, ushort y) => x < y ? x : y;
        [MethodImpl(Inline)]
        public static short Min(short x, short y) => (short)Min((ushort)x, (ushort)y);

        [MethodImpl(Inline)]
        public static ushort Max(ushort x, ushort y) => x > y ? x : y;
        [MethodImpl(Inline)]
        public static short Max(short x, short y) => (short)Max((ushort)x, (ushort)y);

        [MethodImpl(Inline)]
        public static uint Min(uint x, uint y) => x < y ? x : y;
        [MethodImpl(Inline)]
        public static int Min(int x, int y) => (int)Min((uint)x, (uint)y);

        [MethodImpl(Inline)]
        public static uint Max(uint x, uint y) => x > y ? x : y;
        [MethodImpl(Inline)]
        public static int Max(int x, int y) => (int)Max((uint)x, (uint)y);

        [MethodImpl(Inline)]
        public static ulong Min(ulong x, ulong y) => x < y ? x : y;
        [MethodImpl(Inline)]
        public static long Min(long x, long y) => (long)Min((ulong)x, (ulong)y);

        [MethodImpl(Inline)]
        public static ulong Max(ulong x, ulong y) => x > y ? x : y;
        [MethodImpl(Inline)]
        public static long Max(long x, long y) => (long)Max((ulong)x, (ulong)y);
        #endregion

        #region NextPow2: rounding up to closest power of two
        [MethodImpl(Inline)]
        public static sbyte NextPow2(sbyte val) => (sbyte)NextPow2((byte)val);
        [MethodImpl(Inline)]
        public static byte NextPow2(byte val)
        {
            val = Max(val, (byte)1);
            val--;
            val |= (byte)(val >> 1);
            val |= (byte)(val >> 2);
            val |= (byte)(val >> 4);
            val++;
            return val;
        }

        [MethodImpl(Inline)]
        public static short NextPow2(short val) => (short)NextPow2((ushort)val);
        [MethodImpl(Inline)]
        public static ushort NextPow2(ushort val)
        {
            val = Max(val, (ushort)1);
            val--;
            val |= (ushort)(val >> 1);
            val |= (ushort)(val >> 2);
            val |= (ushort)(val >> 4);
            val |= (ushort)(val >> 8);
            val++;
            return val;
        }

        [MethodImpl(Inline)]
        public static int NextPow2(int val) => (int)NextPow2((uint)val);
        [MethodImpl(Inline)]
        public static uint NextPow2(uint val)
        {
            val = Max(val, 1U);
            val--;
            val |= val >> 1;
            val |= val >> 2;
            val |= val >> 4;
            val |= val >> 8;
            val |= val >> 16;
            val++;
            return val;
        }

        [MethodImpl(Inline)]
        public static long NextPow2(long val) => (long)NextPow2((ulong)val);
        [MethodImpl(Inline)]
        public static ulong NextPow2(ulong val)
        {
            val = Max(val, 1UL);
            val--;
            val |= val >> 1;
            val |= val >> 2;
            val |= val >> 4;
            val |= val >> 8;
            val |= val >> 16;
            val |= val >> 32;
            val++;
            return val;
        }
        #endregion

        #region SwapBytes: endian swapping
        [MethodImpl(Inline)]
        public static ushort SwapBytes(ushort input)
        {
            return (ushort)(((input & 0x00FFU) << 8) |
                            ((input & 0xFF00U) >> 8));
        }

        [MethodImpl(Inline)]
        public static uint SwapBytes(uint input)
        {
            return ((input & 0x000000FFU) << 24) |
                   ((input & 0x0000FF00U) << 8) |
                   ((input & 0x00FF0000U) >> 8) |
                   ((input & 0xFF000000U) >> 24);
        }

        [MethodImpl(Inline)]
        public static ulong SwapBytes(ulong input)
        {
            return ((input & 0x00000000000000FFUL) << 56) |
                   ((input & 0x000000000000FF00UL) << 40) |
                   ((input & 0x0000000000FF0000UL) << 24) |
                   ((input & 0x00000000FF000000UL) << 8) |
                   ((input & 0x000000FF00000000UL) >> 8) |
                   ((input & 0x0000FF0000000000UL) >> 24) |
                   ((input & 0x00FF000000000000UL) >> 40) |
                   ((input & 0xFF00000000000000UL) >> 56);
        }
        #endregion

        #region Write: safe binary writing using Span
        [MethodImpl(Inline)]
        public static int Write<T>(Span<byte> dst, int dstOffset, T src) where T : struct
        {
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(dst), (IntPtr)dstOffset), src);
            //Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(dst), src);
            return Unsafe.SizeOf<T>();
        }

        [MethodImpl(Inline)]
        public static int WriteBool(Span<byte> dst, int dstOffset, bool src) => Write(dst, dstOffset, (byte)(src ? 1 : 0));

        [MethodImpl(Inline)]
        public static int WriteSByte(Span<byte> dst, int dstOffset, sbyte src) => Write(dst, dstOffset, src);
        [MethodImpl(Inline)]
        public static int WriteByte(Span<byte> dst, int dstOffset, byte src) => Write(dst, dstOffset, src);

        [MethodImpl(Inline)]
        public static int WriteShort(Span<byte> dst, int dstOffset, short src) => Write(dst, dstOffset, src);
        [MethodImpl(Inline)]
        public static int WriteUShort(Span<byte> dst, int dstOffset, ushort src) => Write(dst, dstOffset, src);

        [MethodImpl(Inline)]
        public static int WriteInt(Span<byte> dst, int dstOffset, int src) => Write(dst, dstOffset, src);
        [MethodImpl(Inline)]
        public static int WriteUInt(Span<byte> dst, int dstOffset, uint src) => Write(dst, dstOffset, src);

        [MethodImpl(Inline)]
        public static int WriteLong(Span<byte> dst, int dstOffset, long src) => Write(dst, dstOffset, src);
        [MethodImpl(Inline)]
        public static int WriteULong(Span<byte> dst, int dstOffset, ulong src) => Write(dst, dstOffset, src);

        [MethodImpl(Inline)]
        public static int WriteFloat(Span<byte> dst, int dstOffset, float src) => Write(dst, dstOffset, src);

        [MethodImpl(Inline)]
        public static int WriteDouble(Span<byte> dst, int dstOffset, double src) => Write(dst, dstOffset, src);

        [MethodImpl(Inline)]
        public static int WriteDecimal(Span<byte> dst, int dstOffset, decimal src) => Write(dst, dstOffset, src);

        [MethodImpl(Inline)]
        public static int WriteBytes(Span<byte> dst, int dstOffset, ReadOnlySpan<byte> src, int srcOffset, int byteLength)
        {
            if (src.Slice(srcOffset, byteLength).TryCopyTo(dst))
            {
                return byteLength;
            }
            return 0;
        }

        //public static int WriteString(Span<byte> dstSpan, int dstOffset, string src, int srcOffset, int srcLength)
        //{
        //    //ref byte dst = ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(dstSpan), (IntPtr)dstOffset);
        //    //return _encoding.GetBytes(psrc, srcLength, Unsafe.AsPointer(dst), dst.Length - dstOffset);

        //}
        #endregion

        #region Read: safe binary reading using span
        [MethodImpl(Inline)]
        public static int Read<T>(out T dst, Span<byte> src, int srcOffset) where T : struct
        {
            dst = Unsafe.ReadUnaligned<T>(ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(src), (IntPtr)srcOffset));
            //dst = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(src));
            return Unsafe.SizeOf<T>();
        }

        [MethodImpl(Inline)]
        public static int ReadBool(out bool dst, Span<byte> src, int srcOffset) => Read(out dst, src, srcOffset);

        [MethodImpl(Inline)]
        public static int ReadSByte(out sbyte dst, Span<byte> src, int srcOffset) => Read(out dst, src, srcOffset);
        [MethodImpl(Inline)]
        public static int ReadByte(out byte dst, Span<byte> src, int srcOffset) => Read(out dst, src, srcOffset);

        [MethodImpl(Inline)]
        public static int ReadShort(out short dst, Span<byte> src, int srcOffset) => Read(out dst, src, srcOffset);
        [MethodImpl(Inline)]
        public static int ReadUShort(out ushort dst, Span<byte> src, int srcOffset) => Read(out dst, src, srcOffset);

        [MethodImpl(Inline)]
        public static int ReadInt(out int dst, Span<byte> src, int srcOffset) => Read(out dst, src, srcOffset);
        [MethodImpl(Inline)]
        public static int ReadUInt(out uint dst, Span<byte> src, int srcOffset) => Read(out dst, src, srcOffset);

        [MethodImpl(Inline)]
        public static int ReadLong(out long dst, Span<byte> src, int srcOffset) => Read(out dst, src, srcOffset);
        [MethodImpl(Inline)]
        public static int ReadULong(out ulong dst, Span<byte> src, int srcOffset) => Read(out dst, src, srcOffset);

        [MethodImpl(Inline)]
        public static int ReadFloat(out float dst, Span<byte> src, int srcOffset) => Read(out dst, src, srcOffset);

        [MethodImpl(Inline)]
        public static int ReadDouble(out double dst, Span<byte> src, int srcOffset) => Read(out dst, src, srcOffset);

        [MethodImpl(Inline)]
        public static int ReadDecimal(out decimal dst, Span<byte> src, int srcOffset) => Read(out dst, src, srcOffset);

        [MethodImpl(Inline)]
        public static int ReadBytes(Span<byte> dst, int dstOffset, Span<byte> src, int srcOffset, int byteLength) => WriteBytes(src, srcOffset, dst, dstOffset, byteLength);
        #endregion
        
        #region UTF8: custom encode / decode ... experimental
        const char UTF8UnknownChar = '?';
        static readonly int UTF8MaxBytesPerChar = Encoding.UTF8.GetMaxByteCount(1);

        [MethodImpl(Inline)]
        internal static int MaxBytesUTF8(string stringSrc)
        {
            return UTF8MaxBytesPerChar * stringSrc.Length;
        }

        [MethodImpl(Inline)]
        public static int CodePointUTF16(char high, char low)
        {
            // See RFC 2781, Section 2.2
            // http://www.faqs.org/rfcs/rfc2781.html
            int h = (high & 0x3FF) << 10;
            int l = low & 0x3FF;
            return (h | l) + 0x10000;
        }

        [MethodImpl(Inline)]
        public static int WriteUTF8(Span<byte> dstSpan, int dstOffset, string srcStr) => WriteUTF8(dstSpan, dstOffset, srcStr, 0, srcStr.Length);
        [MethodImpl(Inline)]
        public static int WriteUTF8(Span<byte> dstSpan, int dstOffset, string srcStr, int srcOffset, int srcLen)
        {
            int dstIdx = 0;
            ref byte dst = ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(dstSpan), (IntPtr)dstOffset);
            ref char src = ref MemoryMarshal.GetReference(srcStr.AsSpan(srcOffset, srcLen));
            for (int i = 0; i < srcLen; i++)
            {
                char c = Unsafe.ReadUnaligned<char>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref src, i)));
                if (c < 0x80)
                {
                    Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref dst, (IntPtr)dstIdx++), (byte)c);
                }
                else if (c < 0x800)
                {
                    Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref dst, (IntPtr)dstIdx++), (byte)(0xc0 | (c >> 6)));
                    Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref dst, (IntPtr)dstIdx++), (byte)(0x80 | (c & 0x3f)));
                }
                else if (char.IsSurrogate(c))
                {
                    if (!char.IsHighSurrogate(c))
                    {
                        throw new System.Text.DecoderFallbackException("missing expected high surrogate");
                        //dst[dstIndex++] = (byte)UTF8UnknownChar;
                        //continue;
                    }

                    if (i + 1 >= srcLen)
                    {
                        throw new System.Text.DecoderFallbackException("string ended on partial surrogate");
                        //dst[dstIndex++] = (byte)UTF8UnknownChar;
                    }

                    char c2 = Unsafe.ReadUnaligned<char>(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref src, ++i)));
                    if (!char.IsLowSurrogate(c2))
                    {
                        throw new System.Text.DecoderFallbackException("missing expected low surrogate");
                        //dst[dstIndex++] = (byte)UTF8UnknownChar;
                        //dst[dstIndex++] = char.IsHighSurrogate(c2) ? (byte)UTF8UnknownChar : (byte)c2;
                        //continue;
                    }

                    int codePoint = CodePointUTF16(c, c2);
                    // See http://www.unicode.org/versions/Unicode7.0.0/ch03.pdf#G2630.
                    Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref dst, (IntPtr)dstIdx++), (byte)(0xf0 | (codePoint >> 18)));
                    Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref dst, (IntPtr)dstIdx++), (byte)(0x80 | ((codePoint >> 12) & 0x3f)));
                    Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref dst, (IntPtr)dstIdx++), (byte)(0x80 | ((codePoint >> 6) & 0x3f)));
                    Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref dst, (IntPtr)dstIdx++), (byte)(0x80 | (codePoint & 0x3f)));
                }
                else
                {
                    Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref dst, (IntPtr)dstIdx++), (byte)(0xe0 | (c >> 12)));
                    Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref dst, (IntPtr)dstIdx++), (byte)(0x80 | ((c >> 6) & 0x3f)));
                    Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref dst, (IntPtr)dstIdx++), (byte)(0x80 | (c & 0x3f)));
                }
            }

            return dstIdx - dstOffset;
        }
        #endregion

    }
}
