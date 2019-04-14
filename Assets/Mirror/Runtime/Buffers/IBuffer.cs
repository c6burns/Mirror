namespace Mirror.Buffers
{
    public interface IBuffer
    {
        int Position { get; set; }
        int Length { get; set; }
        void WriteByte(byte src);
        void WriteUShort(ushort src);
        void WriteUInt(uint src);
        void WriteULong(ulong src);
        void WriteFloat(float src);
        void WriteDouble(double src);
        void WriteBytes(byte[] data, int offset, int length);
        void WriteString(string src);
        byte ReadByte();
        ushort ReadUShort();
        uint ReadUInt();
        ulong ReadULong();
        float ReadFloat();
        double ReadDouble();
        int ReadBytes(byte[] data, int offset, int length);
        string ReadString(int length);
    }

    public interface IBufferSink
    {
        void Sink(IBuffer buffer);
    }

    public interface IBufferSource
    {
        IBuffer Source();
    }
}
