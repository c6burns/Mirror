using System.Text;
using System.Runtime.CompilerServices;

//
// TODO: test actual effectiveness and assembly size change of aggressive inlining (JIT likely is already doing it)
//

namespace Mirror.Buffers
{
    public static class BufferUtil
    {
        const MethodImplOptions Inline = MethodImplOptions.AggressiveInlining;
        static Encoding _encoding = new UTF8Encoding(false);
        #region Min and Max: non-branching
        // from http://www.coranac.com/documents/bittrick/
        [MethodImpl(Inline)]
        public static byte Min(byte x, byte y) => (byte) (y + ((x - y) & (x - y) >> (sizeof(byte) * 8 - 1)));

        [MethodImpl(Inline)]
        public static byte Max(byte x, byte y) => (byte) (x - ((x - y) & (x - y) >> (sizeof(byte) * 8 - 1)));

        [MethodImpl(Inline)]
        public static ushort Min(ushort x, ushort y) => (ushort) (y + ((x - y) & (x - y) >> (sizeof(ushort) * 8 - 1)));

        [MethodImpl(Inline)]
        public static ushort Max(ushort x, ushort y) => (ushort) (x - ((x - y) & (x - y) >> (sizeof(ushort) * 8 - 1)));

        [MethodImpl(Inline)]
        public static uint Min(uint x, uint y) => (uint) (y + ((x - y) & (x - y) >> (sizeof(uint) * 8 - 1)));

        [MethodImpl(Inline)]
        public static uint Max(uint x, uint y) => (uint) (x - ((x - y) & (x - y) >> (sizeof(uint) * 8 - 1)));

        [MethodImpl(Inline)]
        public static ulong Min(ulong x, ulong y) => (ulong) (y + ((x - y) & (x - y) >> (sizeof(ulong) * 8 - 1)));

        [MethodImpl(Inline)]
        public static ulong Max(ulong x, ulong y) => (ulong) (x - ((x - y) & (x - y) >> (sizeof(ulong) * 8 - 1)));
        #endregion

        #region NextPow2: rounding up to closest power of two
        public static sbyte NextPow2(sbyte val) => (sbyte)NextPow2((byte)val);
        public static byte NextPow2(byte val)
        {
            val = Max(val, 1);
            val--;
            val |= (byte)(val >> 1);
            val |= (byte)(val >> 2);
            val |= (byte)(val >> 4);
            val++;
            return val;
        }

        public static short NextPow2(short val) => (short)NextPow2((ushort)val);
        public static ushort NextPow2(ushort val)
        {
            val = Max(val, 1);
            val--;
            val |= (ushort)(val >> 1);
            val |= (ushort)(val >> 2);
            val |= (ushort)(val >> 4);
            val |= (ushort)(val >> 8);
            val++;
            return val;
        }

        public static int NextPow2(int val) => (int)NextPow2((uint)val);
        public static uint NextPow2(uint val)
        {
            val = Max(val, 1);
            val--;
            val |= val >> 1;
            val |= val >> 2;
            val |= val >> 4;
            val |= val >> 8;
            val |= val >> 16;
            val++;
            return val;
        }

        public static long NextPow2(long val) => (long)NextPow2((ulong)val);
        public static ulong NextPow2(ulong val)
        {
            val = Max(val, 1);
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
        public static short SwapBytes(short input) => (short)SwapBytes((ushort)input);
        public static ushort SwapBytes(ushort input)
        {
            return (ushort)(((input & 0x00FFU) << 8) |
                            ((input & 0xFF00U) >> 8));
        }

        public static int SwapBytes(int input) => (int)SwapBytes((uint)input);
        public static uint SwapBytes(uint input)
        {
            return ((input & 0x000000FFU) << 24) |
                   ((input & 0x0000FF00U) << 8) |
                   ((input & 0x00FF0000U) >> 8) |
                   ((input & 0xFF000000U) >> 24);
        }

        public static long SwapBytes(long input) => (long)SwapBytes((ulong)input);
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

        #region UnsafeCopy#: raw pointer based binary copy for exact sizes from 1 - 8 bytes
        [MethodImpl(Inline)]
        public static unsafe void UnsafeCopy1(byte* pdst, byte* psrc)
        {
            *pdst = *psrc;
        }

        [MethodImpl(Inline)]
        public static unsafe void UnsafeCopy2(byte* pdst, byte* psrc)
        {
            *(ushort*)pdst = *(ushort*)psrc;
        }

        [MethodImpl(Inline)]
        public static unsafe void UnsafeCopy3(byte* pdst, byte* psrc)
        {
            *(ushort*)(pdst + 0) = *(ushort*)(psrc + 0);
            *(ushort*)(pdst + 1) = *(ushort*)(psrc + 1);
        }

        [MethodImpl(Inline)]
        public static unsafe void UnsafeCopy4(byte* pdst, byte* psrc)
        {
            *(uint*)pdst = *(uint*)psrc;
        }

        [MethodImpl(Inline)]
        public static unsafe void UnsafeCopy5(byte* pdst, byte* psrc)
        {
            *(uint*)(pdst + 0) = *(uint*)(psrc + 0);
            *(uint*)(pdst + 1) = *(uint*)(psrc + 1);
        }

        [MethodImpl(Inline)]
        public static unsafe void UnsafeCopy6(byte* pdst, byte* psrc)
        {
            *(uint*)(pdst + 0) = *(uint*)(psrc + 0);
            *(uint*)(pdst + 2) = *(uint*)(psrc + 2);
        }

        [MethodImpl(Inline)]
        public static unsafe void UnsafeCopy7(byte* pdst, byte* psrc)
        {
            *(uint*)(pdst + 0) = *(uint*)(psrc + 0);
            *(uint*)(pdst + 3) = *(uint*)(psrc + 3);
        }

        [MethodImpl(Inline)]
        public static unsafe void UnsafeCopy8(byte* pdst, byte* psrc)
        {
            *(ulong*)pdst = *(ulong*)psrc;
        }
        #endregion

        #region UnsafeWrite: unsafe binary writing using fixed pinning
        [MethodImpl(Inline)]
        public static uint UnsafeWrite(byte[] dst, ulong dstOffset, bool boolSrc) => UnsafeWrite(dst, dstOffset, (byte)(boolSrc ? 1 : 0));

        [MethodImpl(Inline)]
        public static uint UnsafeWrite(byte[] dst, ulong dstOffset, sbyte sbyteSrc) => UnsafeWrite(dst, dstOffset, (byte)sbyteSrc);
        [MethodImpl(Inline)]
        public static unsafe uint UnsafeWrite(byte[] dst, ulong dstOffset, byte byteSrc)
        {
            fixed (byte* pdst = &dst[dstOffset])
            {
                UnsafeCopy1(pdst, &byteSrc);
            }
            return sizeof(byte);
        }

        [MethodImpl(Inline)]
        public static uint UnsafeWrite(byte[] dst, ulong dstOffset, short shortSrc) => UnsafeWrite(dst, dstOffset, (ushort)shortSrc);
        [MethodImpl(Inline)]
        public static unsafe uint UnsafeWrite(byte[] dst, ulong dstOffset, ushort ushortSrc)
        {
            fixed (byte* pdst = &dst[dstOffset])
            {
                UnsafeCopy2(pdst, (byte*)&ushortSrc);
            }
            return sizeof(ushort);
        }

        [MethodImpl(Inline)]
        public static uint UnsafeWrite(byte[] dst, ulong dstOffset, int intSrc) => UnsafeWrite(dst, dstOffset, (uint)intSrc);
        [MethodImpl(Inline)]
        public static unsafe uint UnsafeWrite(byte[] dst, ulong dstOffset, uint uintSrc)
        {
            fixed (byte* pdst = &dst[dstOffset])
            {
                UnsafeCopy4(pdst, (byte*)&uintSrc);
            }
            return sizeof(uint);
        }

        [MethodImpl(Inline)]
        public static uint UnsafeWrite(byte[] dst, ulong dstOffset, long longSrc) => UnsafeWrite(dst, dstOffset, (ulong)longSrc);
        [MethodImpl(Inline)]
        public static unsafe uint UnsafeWrite(byte[] dst, ulong dstOffset, ulong ulongSrc)
        {
            fixed (byte* pdst = &dst[dstOffset])
            {
                UnsafeCopy8(pdst, (byte*)&ulongSrc);
            }
            return sizeof(ulong);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeWrite(byte[] dst, ulong dstOffset, float floatSrc)
        {
            fixed (byte* pdst = &dst[dstOffset])
            {
                UnsafeCopy4(pdst, (byte*)&floatSrc);
            }
            return sizeof(float);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeWrite(byte[] dst, ulong dstOffset, double doubleSrc)
        {
            fixed (byte* pdst = &dst[dstOffset])
            {
                UnsafeCopy8(pdst, (byte*)&doubleSrc);
            }
            return sizeof(double);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeWrite(byte[] dst, ulong dstOffset, string stringSrc)
        {
            uint written = 0;
            fixed (char* psrc = stringSrc)
            fixed (byte* pdst = &dst[dstOffset])
            {
                written = (uint)_encoding.GetBytes(psrc, stringSrc.Length, pdst, dst.Length - (int)dstOffset);
            }
            return written;
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeWrite(byte[] dst, ulong dstOffset, byte[] src, ulong srcOffset, int byteLength)
        {
            int longWriteLimit = (int)(byteLength & 0xfffffff8);
            fixed (byte* psrc = &src[srcOffset])
            fixed (byte* pdst = &dst[dstOffset])
            {
                // write anything over 8 bytes as a series of long*
                for (int cursor = 0; cursor < longWriteLimit; cursor += sizeof(long))
                {
                    UnsafeCopy8(pdst + cursor, psrc + cursor);
                }

                // write anything remaining under 8 bytes
                switch (byteLength & 0x00000007)
                {
                    case 0:
                        break;
                    case 1:
                        UnsafeCopy1(pdst + longWriteLimit, psrc + longWriteLimit);
                        break;
                    case 2:
                        UnsafeCopy2(pdst + longWriteLimit, psrc + longWriteLimit);
                        break;
                    case 3:
                        UnsafeCopy3(pdst + longWriteLimit, psrc + longWriteLimit);
                        break;
                    case 4:
                        UnsafeCopy4(pdst + longWriteLimit, psrc + longWriteLimit);
                        break;
                    case 5:
                        UnsafeCopy5(pdst + longWriteLimit, psrc + longWriteLimit);
                        break;
                    case 6:
                        UnsafeCopy6(pdst + longWriteLimit, psrc + longWriteLimit);
                        break;
                    case 7:
                        UnsafeCopy7(pdst + longWriteLimit, psrc + longWriteLimit);
                        break;
                }
            }
            return (uint)byteLength;
        }
        #endregion

        #region UnsafeRead: unsafe binary reading using fixed pinning
        [MethodImpl(Inline)]
        public static unsafe uint UnsafeRead(out bool boolDst, byte[] src, ulong srcOffset)
        {
            fixed (byte* psrc = &src[srcOffset])
            {
                boolDst = (*psrc == 0) ? false : true;
            }
            return sizeof(byte);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeRead(out sbyte sbyteDst, byte[] src, ulong srcOffset)
        {
            fixed (void* pdst = &sbyteDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                UnsafeCopy1((byte*)pdst, psrc);
            }
            return sizeof(sbyte);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeRead(out byte byteDst, byte[] src, ulong srcOffset)
        {
            fixed (byte* pdst = &byteDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                UnsafeCopy1(pdst, psrc);
            }
            return sizeof(byte);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeRead(out short shortDst, byte[] src, ulong srcOffset)
        {
            fixed (void* pdst = &shortDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                UnsafeCopy2((byte*)pdst, psrc);
            }
            return sizeof(short);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeRead(out ushort ushortDst, byte[] src, ulong srcOffset)
        {
            fixed (void* pdst = &ushortDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                UnsafeCopy2((byte*)pdst, psrc);
            }
            return sizeof(ushort);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeRead(out int intSrc, byte[] src, ulong srcOffset)
        {
            fixed (void* pdst = &intSrc)
            fixed (byte* psrc = &src[srcOffset])
            {
                UnsafeCopy4((byte*)pdst, psrc);
            }
            return sizeof(int);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeRead(out uint uintDst, byte[] src, ulong srcOffset)
        {
            fixed (void* pdst = &uintDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                UnsafeCopy4((byte*)pdst, psrc);
            }
            return sizeof(uint);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeRead(out long longDst, byte[] src, ulong srcOffset)
        {
            fixed (void* pdst = &longDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                UnsafeCopy8((byte*)pdst, psrc);
            }
            return sizeof(long);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeRead(out ulong ulongDst, byte[] src, ulong srcOffset)
        {
            fixed (void* pdst = &ulongDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                UnsafeCopy8((byte*)pdst, psrc);
            }
            return sizeof(ulong);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeRead(out float floatDst, byte[] src, ulong srcOffset)
        {
            fixed (void* pdst = &floatDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                UnsafeCopy4((byte*)pdst, psrc);
            }
            return sizeof(float);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeRead(out double doubleDst, byte[] src, ulong srcOffset)
        {
            fixed (void* pdst = &doubleDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                UnsafeCopy8((byte*)pdst, psrc);
            }
            return sizeof(double);
        }

        [MethodImpl(Inline)]
        public static unsafe uint UnsafeRead(out string stringDst, byte[] src, ulong srcOffset, int byteLength)
        {
            fixed (byte* psrc = &src[srcOffset])
            {
                stringDst = _encoding.GetString(psrc, byteLength);
            }
            return (uint)byteLength;
        }

        [MethodImpl(Inline)]
        public static unsafe void UnsafeRead(byte[] dst, ulong dstOffset, byte[] src, ulong srcOffset, int byteLength) => UnsafeWrite(src, srcOffset, dst, dstOffset, byteLength);
        #endregion

        [MethodImpl(Inline)]
        internal static uint StringByteCount(string stringSrc) => (uint) _encoding.GetByteCount(stringSrc);
    }
}
