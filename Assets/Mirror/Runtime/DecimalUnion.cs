using System.Runtime.InteropServices;

namespace Mirror
{
	[StructLayout(LayoutKind.Explicit)]
	struct DecimalUnion
	{
		[FieldOffset(0)] decimal value;
		[FieldOffset(0)] uint flags;
		[FieldOffset(4)] uint hi;
		[FieldOffset(8)] uint lo;
		[FieldOffset(12)] uint mid;

		public DecimalUnion(NetworkReader reader)
		{
			value = default;
			lo = reader.ReadUInt32();
			mid = reader.ReadUInt32();
			hi = reader.ReadUInt32();
			flags = reader.ReadUInt32();
		}

		public DecimalUnion(decimal d)
		{
			flags = default;
			hi = default;
			lo = default;
			mid = default;
			value = d;
		}

		public void Write(NetworkWriter writer)
		{
			writer.Write(lo);
			writer.Write(mid);
			writer.Write(hi);
			writer.Write(flags);
		}

		public decimal Read() => value;
	}
}