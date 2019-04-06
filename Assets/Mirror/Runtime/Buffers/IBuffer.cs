namespace Mirror.Buffers
{
    public interface IBuffer
    {
        void WriteByte(byte src);
        void WriteUShort(ushort src);
        void WriteUInt(uint src);
        void WriteULong(ulong src);
        void WriteFloat(float src);
        void WriteDouble(double src);
        void WriteString(string src);
        byte ReadByte();
        ushort ReadUShort();
        uint ReadUInt();
        ulong ReadULong();
        float ReadFloat();
        double ReadDouble();
        string ReadString(uint length);
    }
}