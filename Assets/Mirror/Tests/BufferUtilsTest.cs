using System;
using Mirror.Buffers;
using NUnit.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class BufferUtilsTest
    {
        [Test]
        public void TestMinByte()
        {
            byte[] bytes = new byte[]{0,1,2,127,128,129,253,254,255};
            foreach (byte a in bytes)
                foreach (byte b in bytes)
                    Assert.That(BufferUtil.Min(a, b), Is.EqualTo(Math.Min(a, b)));
        }

        [Test]
        public void TestMaxByte()
        {
            byte[] bytes = new byte[]{0,1,2,127,128,129,253,254,255};
            foreach (byte a in bytes)
                foreach (byte b in bytes)
                    Assert.That(BufferUtil.Max(a, b), Is.EqualTo(Math.Max(a, b)));
        }

        [Test]
        public void TestMinUShort()
        {
            ushort[] ushorts = new ushort[]{0,1,2,127,128,129,253,254,255,256,257,65533,65534,65535};
            foreach (ushort a in ushorts)
                foreach (ushort b in ushorts)
                    Assert.That(BufferUtil.Min(a, b), Is.EqualTo(Math.Min(a, b)));
        }

        [Test]
        public void TestMaxUShort()
        {
            ushort[] ushorts = new ushort[]{0,1,2,127,128,129,253,254,255,256,257,65533,65534,65535};
            foreach (ushort a in ushorts)
                foreach (ushort b in ushorts)
                    Assert.That(BufferUtil.Max(a, b), Is.EqualTo(Math.Max(a, b)));
        }

        [Test]
        public void TestMinUInt()
        {
            uint[] uints = new uint[]{0,1,2,127,128,129,253,254,255,256,257,65533,65534,65535,65536,65537,~2u,~1u,~0u};
            foreach (uint a in uints)
                foreach (uint b in uints)
                    Assert.That(BufferUtil.Min(a, b), Is.EqualTo(Math.Min(a, b)));
        }

        [Test]
        public void TestMaxUInt()
        {
            uint[] uints = new uint[]{0,1,2,127,128,129,253,254,255,256,257,65533,65534,65535,65536,65537,~2u,~1u,~0u};
            foreach (uint a in uints)
                foreach (uint b in uints)
                    Assert.That(BufferUtil.Max(a, b), Is.EqualTo(Math.Max(a, b)));
        }

        [Test]
        public void TestMinULong()
        {
            ulong[] ulongs = new ulong[]{0,1,2,127,128,129,253,254,255,256,257,65533,65534,65535,65536,65537,~2u,~1u,~0u,~2ul,~1ul,~0ul};
            foreach (ulong a in ulongs)
                foreach (ulong b in ulongs)
                    Assert.That(BufferUtil.Min(a, b), Is.EqualTo(Math.Min(a, b)));
        }

        [Test]
        public void TestMaxULong()
        {
            ulong[] ulongs = new ulong[]{0,1,2,127,128,129,253,254,255,256,257,65533,65534,65535,65536,65537,~2u,~1u,~0u,~2ul,~1ul,~0ul};
            foreach (ulong a in ulongs)
                foreach (ulong b in ulongs)
                    Assert.That(BufferUtil.Max(a, b), Is.EqualTo(Math.Max(a, b)));
        }

        [Test]
        public void TestNextPowerOfTwoByte()
        {
            byte[] input  = new byte[]{0,1,2,5, 9,127,128,129,253,254,255};
            byte[] expect = new byte[]{1,1,2,8,16,128,128,  0,  0,  0,  0};
            for (int i = 0; i < input.Length; i++)
                Assert.That(BufferUtil.NextPow2(input[i]), Is.EqualTo(expect[i]));
        }

        [Test]
        public void TestNextPowerOfTwoUShort()
        {
            ushort[] input  = new ushort[]{0,1,2,5, 9,127,128,129,253,254,255,1000,4000,60000};
            ushort[] expect = new ushort[]{1,1,2,8,16,128,128,256,256,256,256,1024,4096,    0};
            for (int i = 0; i < input.Length; i++)
                Assert.That(BufferUtil.NextPow2(input[i]), Is.EqualTo(expect[i]));
        }

        [Test]
        public void TestNextPowerOfTwoUInt()
        {
            uint[] input  = new uint[]{0,1,2,5, 9,127,128,129,253,254,255,1000,4000,60000,~1u,~0u};
            uint[] expect = new uint[]{1,1,2,8,16,128,128,256,256,256,256,1024,4096,65536,  0,  0};
            for (int i = 0; i < input.Length; i++)
                Assert.That(BufferUtil.NextPow2(input[i]), Is.EqualTo(expect[i]));
        }

        [Test]
        public void TestNextPowerOfTwoULong()
        {
            ulong[] input  = new ulong[]{0,1,2,5, 9,127,128,129,253,254,255,1000,4000,60000,    ~1u,    ~0u,~1ul,~0ul};
            ulong[] expect = new ulong[]{1,1,2,8,16,128,128,256,256,256,256,1024,4096,65536,~0u+1ul,~0u+1ul,   0,   0};
            for (int i = 0; i < input.Length; i++)
                Assert.That(BufferUtil.NextPow2(input[i]), Is.EqualTo(expect[i]));
        }

        [Test]
        public void TestSwapBytesUShort()
        {
            ushort[] input  = new ushort[]{0x0000,0x0001,0x0002,0x0ab0,0xab0a,0xf0e0,0xffff,0xabcd};
            ushort[] expect = new ushort[]{0x0000,0x0100,0x0200,0xb00a,0x0aab,0xe0f0,0xffff,0xcdab};
            for (int i = 0; i < input.Length; i++)
                Assert.That(BufferUtil.SwapBytes(input[i]), Is.EqualTo(expect[i]));
            for (int i = 0; i < input.Length; i++)
                Assert.That(BufferUtil.SwapBytes(expect[i]), Is.EqualTo(input[i]));
        }

        [Test]
        public void TestSwapBytesUInt()
        {
            uint[] input  = new uint[]{0x00000000,0x00000001,0x00001a02,0x000a00b0,0x00ab0a00,0xf00000e0,0xffffffff,0x89abcdef};
            uint[] expect = new uint[]{0x00000000,0x01000000,0x021a0000,0xb0000a00,0x000aab00,0xe00000f0,0xffffffff,0xefcdab89};
            for (int i = 0; i < input.Length; i++)
                Assert.That(BufferUtil.SwapBytes(input[i]), Is.EqualTo(expect[i]));
            for (int i = 0; i < input.Length; i++)
                Assert.That(BufferUtil.SwapBytes(expect[i]), Is.EqualTo(input[i]));
        }

        [Test]
        public void TestSwapBytesULong()
        {
            ulong[] input  = new ulong[]{0,0x0000000000000001,0x0000000000001a02,0xffffffffffffffff,0x1234567890abcdef};
            ulong[] expect = new ulong[]{0,0x0100000000000000,0x021a000000000000,0xffffffffffffffff,0xefcdab9078563412};
            for (int i = 0; i < input.Length; i++)
                Assert.That(BufferUtil.SwapBytes(input[i]), Is.EqualTo(expect[i]));
            for (int i = 0; i < input.Length; i++)
                Assert.That(BufferUtil.SwapBytes(expect[i]), Is.EqualTo(input[i]));
        }

        [Test]
        public void TestUnsafeWriteArray()
        {
            byte[] src;
            byte[] dst;
            byte[] exp;
            src = new byte[10] { 0,1,2,3,4,5,6,7,8,9 };
            dst = new byte[10];
            exp = new byte[10] { 0,1,2,3,4,5,6,7,8,9 };
            BufferUtilUnsafe.Write(dst, 0, src, 0, 10);
            Assert.That(dst, Is.EqualTo(exp));

            dst = new byte[5];
            exp = new byte[5] { 0,1,2,3,4 };
            BufferUtilUnsafe.Write(dst, 0, src, 0, 5);
            Assert.That(dst, Is.EqualTo(exp));

            dst = new byte[10];
            exp = new byte[10] { 0,0,0,0,0,0,1,2,3,4 };
            BufferUtilUnsafe.Write(dst, 5, src, 0, 5);
            Assert.That(dst, Is.EqualTo(exp));

            dst = new byte[5];
            exp = new byte[5] { 5,6,7,8,9 };
            BufferUtilUnsafe.Write(dst, 0, src, 5, 5);
            Assert.That(dst, Is.EqualTo(exp));
            return;
        }

        [Test]
        public void TestReadAndWriteBool()
        {
            bool[] inputs = new bool[]{true, false};
            foreach (bool input in inputs)
            {
                for (int i = 0; i < 10; i++)
                {
                    byte[] src = new byte[128];
                    BufferUtilUnsafe.Write(src, i, input);
                    BufferUtilUnsafe.Read(out bool output, src, i);
                    Assert.That(output, Is.EqualTo(input));
                }
            }
        }

        [Test]
        public void TestReadAndWriteSByte()
        {
            sbyte[] inputs = new sbyte[]{0,1,2,3,-1,-2,-3,126,127,-127,-128};
            foreach (sbyte input in inputs)
            {
                for (int i = 0; i < 10; i++)
                {
                    byte[] src = new byte[128];
                    BufferUtilUnsafe.Write(src, i, input);
                    BufferUtilUnsafe.Read(out sbyte output, src, i);
                    Assert.That(output, Is.EqualTo(input));
                }
            }
        }

        [Test]
        public void TestReadAndWriteByte()
        {
            byte[] inputs = new byte[]{0,1,2,3,253,254,255};
            foreach (byte input in inputs)
            {
                for (int i = 0; i < 10; i++)
                {
                    byte[] src = new byte[128];
                    BufferUtilUnsafe.Write(src, i, input);
                    BufferUtilUnsafe.Read(out byte output, src, i);
                    Assert.That(output, Is.EqualTo(input));
                }
            }
        }

        [Test]
        public void TestReadAndWriteShort()
        {
            short[] inputs = new short[]{0,1,2,-1,-2,257,-257,short.MaxValue,short.MinValue};
            foreach (short input in inputs)
            {
                for (int i = 0; i < 10; i++)
                {
                    byte[] src = new byte[128];
                    BufferUtilUnsafe.Write(src, i, input);
                    BufferUtilUnsafe.Read(out short output, src, i);
                    Assert.That(output, Is.EqualTo(input));
                }
            }
        }

        [Test]
        public void TestReadAndWriteUShort()
        {
            ushort[] inputs = new ushort[]{0,1,2,3,257,0xFFFE,0xFFFF,0xabcd};
            foreach (ushort input in inputs)
            {
                for (int i = 0; i < 10; i++)
                {
                    byte[] src = new byte[128];
                    BufferUtilUnsafe.Write(src, i, input);
                    BufferUtilUnsafe.Read(out ushort output, src, i);
                    Assert.That(output, Is.EqualTo(input));
                }
            }
        }

        [Test]
        public void TestReadAndWriteInt()
        {
            int[] inputs = new int[]{0,1,2,-1,-2,257,-257,123456789,-123456789};
            foreach (int input in inputs)
            {
                for (int i = 0; i < 10; i++)
                {
                    byte[] src = new byte[128];
                    BufferUtilUnsafe.Write(src, i, input);
                    BufferUtilUnsafe.Read(out int output, src, i);
                    Assert.That(output, Is.EqualTo(input));
                }
            }
        }

        [Test]
        public void TestReadAndWriteUInt()
        {
            uint[] inputs = new uint[]{0,1,2,257,123456789,~0u,0xC0FFEE};
            foreach (uint input in inputs)
            {
                for (int i = 0; i < 10; i++)
                {
                    byte[] src = new byte[128];
                    BufferUtilUnsafe.Write(src, i, input);
                    BufferUtilUnsafe.Read(out uint output, src, i);
                    Assert.That(output, Is.EqualTo(input));
                }
            }
        }

        [Test]
        public void TestReadAndWriteLong()
        {
            long[] inputs = new long[]{0,1,2,-1,-2,257,-1233456789,123456789,0xDEAD_C0FFEE_FEEL};
            foreach (long input in inputs)
            {
                for (int i = 0; i < 10; i++)
                {
                    byte[] src = new byte[128];
                    BufferUtilUnsafe.Write(src, i, input);
                    BufferUtilUnsafe.Read(out long output, src, i);
                    Assert.That(output, Is.EqualTo(input));
                }
            }
        }

        [Test]
        public void TestReadAndWriteULong()
        {
            ulong[] inputs = new ulong[]{0,1,2,257,~0ul,123456789,0xDEAD_C0FFEE_FEEL};
            foreach (ulong input in inputs)
            {
                for (int i = 0; i < 10; i++)
                {
                    byte[] src = new byte[128];
                    BufferUtilUnsafe.Write(src, i, input);
                    BufferUtilUnsafe.Read(out ulong output, src, i);
                    Assert.That(output, Is.EqualTo(input));
                }
            }
        }

        [Test]
        public void TestReadAndWriteFloat()
        {
            float[] inputs = new float[]{0f,0.1f,1f,(float)Math.PI,float.PositiveInfinity,float.NaN,1.0f/3.0f};
            foreach (float input in inputs)
            {
                for (int i = 0; i < 10; i++)
                {
                    byte[] src = new byte[128];
                    BufferUtilUnsafe.Write(src, i, input);
                    BufferUtilUnsafe.Read(out float output, src, i);
                    Assert.That(output, Is.EqualTo(input));
                }
            }
        }

        [Test]
        public void TestReadAndWriteDouble()
        {
            double[] inputs = new double[]{0d,0.1,Math.PI,Math.E,double.PositiveInfinity,double.NaN,1.0/3.0};
            foreach (double input in inputs)
            {
                for (int i = 0; i < 10; i++)
                {
                    byte[] src = new byte[128];
                    BufferUtilUnsafe.Write(src, i, input);
                    BufferUtilUnsafe.Read(out double output, src, i);
                    Assert.That(output, Is.EqualTo(input));
                }
            }
        }

        [Test]
        public void TestStrings()
        {
            string[] strings = new string[]{"","\0","abc","aBcD\n\r\t1","ß≈£∆Î·ç∂™€µ±—Ø∏","\uFFFF","˜˘¯ªº","єאקɭ๏๔є","𝕖𝕩𝕡𝕝𝕠𝕕𝕖"};
            foreach (string input in strings)
            {
                for (int i = 0; i < 10; i++)
                {
                    byte[] buffer = new byte[1024];
                    uint written = BufferUtilUnsafe.Write(buffer, i, input);
                    BufferUtilUnsafe.Read(out string output, buffer, i, (int) written);
                    Assert.That(output, Is.EqualTo(input));
                }
            }
        }
    }
}