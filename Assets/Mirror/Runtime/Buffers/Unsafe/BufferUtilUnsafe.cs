using System;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mirror.Buffers
{
    public static class BufferUtilUnsafe
    {
        const MethodImplOptions Inline = MethodImplOptions.AggressiveInlining;
        static Encoding _encoding = new UTF8Encoding(false, true);

        #region Copy#: raw pointer based binary copy for exact sizes from 1 - 8 bytes
        [MethodImpl(Inline)]
        public static unsafe void Copy1(byte* pdst, byte* psrc)
        {
            *pdst = *psrc;
        }

        [MethodImpl(Inline)]
        public static unsafe void Copy2(byte* pdst, byte* psrc)
        {
            *(ushort*)pdst = *(ushort*)psrc;
        }

        [MethodImpl(Inline)]
        public static unsafe void Copy3(byte* pdst, byte* psrc)
        {
            *(ushort*)(pdst + 0) = *(ushort*)(psrc + 0);
            *(ushort*)(pdst + 1) = *(ushort*)(psrc + 1);
        }

        [MethodImpl(Inline)]
        public static unsafe void Copy4(byte* pdst, byte* psrc)
        {
            *(uint*)pdst = *(uint*)psrc;
        }

        [MethodImpl(Inline)]
        public static unsafe void Copy5(byte* pdst, byte* psrc)
        {
            *(uint*)(pdst + 0) = *(uint*)(psrc + 0);
            *(uint*)(pdst + 1) = *(uint*)(psrc + 1);
        }

        [MethodImpl(Inline)]
        public static unsafe void Copy6(byte* pdst, byte* psrc)
        {
            *(uint*)(pdst + 0) = *(uint*)(psrc + 0);
            *(uint*)(pdst + 2) = *(uint*)(psrc + 2);
        }

        [MethodImpl(Inline)]
        public static unsafe void Copy7(byte* pdst, byte* psrc)
        {
            *(uint*)(pdst + 0) = *(uint*)(psrc + 0);
            *(uint*)(pdst + 3) = *(uint*)(psrc + 3);
        }

        [MethodImpl(Inline)]
        public static unsafe void Copy8(byte* pdst, byte* psrc)
        {
            *(ulong*)pdst = *(ulong*)psrc;
        }
        #endregion

        #region Write: unsafe binary writing using fixed pinning
        [MethodImpl(Inline)]
        public static uint Write(byte[] dst, int dstOffset, bool boolSrc) => Write(dst, dstOffset, (byte)(boolSrc ? 1 : 0));

        [MethodImpl(Inline)]
        public static uint Write(byte[] dst, int dstOffset, sbyte sbyteSrc) => Write(dst, dstOffset, (byte)sbyteSrc);
        [MethodImpl(Inline)]
        public static unsafe uint Write(byte[] dst, int dstOffset, byte byteSrc)
        {
            fixed (byte* pdst = &dst[dstOffset])
            {
                Copy1(pdst, &byteSrc);
            }
            return sizeof(byte);
        }

        [MethodImpl(Inline)]
        public static uint Write(byte[] dst, int dstOffset, short shortSrc) => Write(dst, dstOffset, (ushort)shortSrc);
        [MethodImpl(Inline)]
        public static unsafe uint Write(byte[] dst, int dstOffset, ushort ushortSrc)
        {
            fixed (byte* pdst = &dst[dstOffset])
            {
                Copy2(pdst, (byte*)&ushortSrc);
            }
            return sizeof(ushort);
        }

        [MethodImpl(Inline)]
        public static uint Write(byte[] dst, int dstOffset, int intSrc) => Write(dst, dstOffset, (uint)intSrc);
        [MethodImpl(Inline)]
        public static unsafe uint Write(byte[] dst, int dstOffset, uint uintSrc)
        {
            fixed (byte* pdst = &dst[dstOffset])
            {
                Copy4(pdst, (byte*)&uintSrc);
            }
            return sizeof(uint);
        }

        [MethodImpl(Inline)]
        public static uint Write(byte[] dst, int dstOffset, long longSrc) => Write(dst, dstOffset, (ulong)longSrc);
        [MethodImpl(Inline)]
        public static unsafe uint Write(byte[] dst, int dstOffset, ulong ulongSrc)
        {
            fixed (byte* pdst = &dst[dstOffset])
            {
                Copy8(pdst, (byte*)&ulongSrc);
            }
            return sizeof(ulong);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Write(byte[] dst, int dstOffset, float floatSrc)
        {
            fixed (byte* pdst = &dst[dstOffset])
            {
                Copy4(pdst, (byte*)&floatSrc);
            }
            return sizeof(float);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Write(byte[] dst, int dstOffset, double doubleSrc)
        {
            fixed (byte* pdst = &dst[dstOffset])
            {
                Copy8(pdst, (byte*)&doubleSrc);
            }
            return sizeof(double);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Write(byte[] dst, int dstOffset, decimal decimalSrc)
        {
            fixed (byte* pdst = &dst[dstOffset])
            {
                Copy4(pdst + 12, (byte*)&decimalSrc + 0);
                Copy4(pdst + 8, (byte*)&decimalSrc + 4);
                Copy8(pdst + 0, (byte*)&decimalSrc + 8);
            }
            return sizeof(decimal);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Write(byte[] dst, int dstOffset, string stringSrc)
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
        public static unsafe int Write(Span<byte> dst, int dstOffset, string stringSrc)
        {
            int written = 0;
            fixed (char* psrc = stringSrc)
            fixed (byte* pdst = &MemoryMarshal.GetReference(dst.Slice(dstOffset)))
            {
                written = _encoding.GetBytes(psrc, stringSrc.Length, pdst, dst.Length - (int)dstOffset);
            }
            return written;
        }

        [MethodImpl(Inline)]
        public static unsafe ulong Write(byte[] dst, int dstOffset, byte[] src, int srcOffset, ulong byteLength)
        {
            ulong longWriteLimit = byteLength & ~7ul;
            fixed (byte* psrc = src)
            fixed (byte* pdst = dst)
            {
                // write anything over 8 bytes as a series of long*
                for (ulong cursor = 0; cursor < longWriteLimit; cursor += sizeof(long))
                {
                    Copy8(pdst + dstOffset + cursor, psrc + srcOffset + cursor);
                }

                // write anything remaining under 8 bytes
                switch (byteLength & 7ul)
                {
                    case 0:
                        break;
                    case 1:
                        Copy1(pdst + dstOffset + longWriteLimit, psrc + srcOffset + longWriteLimit);
                        break;
                    case 2:
                        Copy2(pdst + dstOffset + longWriteLimit, psrc + srcOffset + longWriteLimit);
                        break;
                    case 3:
                        Copy3(pdst + dstOffset + longWriteLimit, psrc + srcOffset + longWriteLimit);
                        break;
                    case 4:
                        Copy4(pdst + dstOffset + longWriteLimit, psrc + srcOffset + longWriteLimit);
                        break;
                    case 5:
                        Copy5(pdst + dstOffset + longWriteLimit, psrc + srcOffset + longWriteLimit);
                        break;
                    case 6:
                        Copy6(pdst + dstOffset + longWriteLimit, psrc + srcOffset + longWriteLimit);
                        break;
                    case 7:
                        Copy7(pdst + dstOffset + longWriteLimit, psrc + srcOffset + longWriteLimit);
                        break;
                }
            }
            return byteLength;
        }
        #endregion

        #region Read: unsafe binary reading using fixed pinning
        [MethodImpl(Inline)]
        public static unsafe uint Read(out bool boolDst, byte[] src, int srcOffset)
        {
            fixed (byte* psrc = &src[srcOffset])
            {
                boolDst = (*psrc == 0) ? false : true;
            }
            return sizeof(byte);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Read(out sbyte sbyteDst, byte[] src, int srcOffset)
        {
            fixed (void* pdst = &sbyteDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                Copy1((byte*)pdst, psrc);
            }
            return sizeof(sbyte);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Read(out byte byteDst, byte[] src, int srcOffset)
        {
            fixed (byte* pdst = &byteDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                Copy1(pdst, psrc);
            }
            return sizeof(byte);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Read(out short shortDst, byte[] src, int srcOffset)
        {
            fixed (void* pdst = &shortDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                Copy2((byte*)pdst, psrc);
            }
            return sizeof(short);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Read(out ushort ushortDst, byte[] src, int srcOffset)
        {
            fixed (void* pdst = &ushortDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                Copy2((byte*)pdst, psrc);
            }
            return sizeof(ushort);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Read(out int intSrc, byte[] src, int srcOffset)
        {
            fixed (void* pdst = &intSrc)
            fixed (byte* psrc = &src[srcOffset])
            {
                Copy4((byte*)pdst, psrc);
            }
            return sizeof(int);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Read(out uint uintDst, byte[] src, int srcOffset)
        {
            fixed (void* pdst = &uintDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                Copy4((byte*)pdst, psrc);
            }
            return sizeof(uint);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Read(out long longDst, byte[] src, int srcOffset)
        {
            fixed (void* pdst = &longDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                Copy8((byte*)pdst, psrc);
            }
            return sizeof(long);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Read(out ulong ulongDst, byte[] src, int srcOffset)
        {
            fixed (void* pdst = &ulongDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                Copy8((byte*)pdst, psrc);
            }
            return sizeof(ulong);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Read(out float floatDst, byte[] src, int srcOffset)
        {
            fixed (void* pdst = &floatDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                Copy4((byte*)pdst, psrc);
            }
            return sizeof(float);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Read(out double doubleDst, byte[] src, int srcOffset)
        {
            fixed (void* pdst = &doubleDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                Copy8((byte*)pdst, psrc);
            }
            return sizeof(double);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Read(out decimal decimalDst, byte[] src, int srcOffset)
        {
            fixed (void* pdst = &decimalDst)
            fixed (byte* psrc = &src[srcOffset])
            {
                Copy4((byte*)pdst + 0, psrc + 12);
                Copy4((byte*)pdst + 4, psrc + 8);
                Copy8((byte*)pdst + 8, psrc + 0);
            }
            return sizeof(decimal);
        }

        [MethodImpl(Inline)]
        public static unsafe uint Read(out string stringDst, byte[] src, int srcOffset, int byteLength)
        {
            fixed (byte* psrc = &src[srcOffset])
            {
                stringDst = _encoding.GetString(psrc, byteLength);
            }
            return (uint)byteLength;
        }

        [MethodImpl(Inline)]
        public static unsafe int Read(out string stringDst, Span<byte> src, int srcOffset, int byteLength)
        {
            fixed (byte* psrc = &MemoryMarshal.GetReference(src.Slice(srcOffset)))
            {
                stringDst = _encoding.GetString(psrc, byteLength);
            }
            return byteLength;
        }

        [MethodImpl(Inline)]
        public static unsafe ulong Read(byte[] dst, int dstOffset, byte[] src, int srcOffset, ulong byteLength) => Write(src, srcOffset, dst, dstOffset, byteLength);
        #endregion
    }
}
