public interface IBuffer
{
    /// <summary>
    /// 字节数组
    /// </summary>
    byte[] Array { get; }
    /// <summary>
    /// 容量
    /// </summary>
    int Capacity { get; }
    /// <summary>
    /// 读下标
    /// </summary>
    int ReadIndex { get; }
    /// <summary>
    /// 写下标
    /// </summary>
    int WriteIndex { get; }
    /// <summary>
    /// 未读数据长度
    /// </summary>
    int DataLength { get; }
    /// <summary>
    /// 剩余可写长度
    /// </summary>
    int RemainingLength { get; }
    byte ReadByte();
    void WriteByte(byte value);
    int ReadInt();
    int ReadIntLE();
    void WriteInt(int value);
    void WriteIntLE(int value);
    long ReadLong();
    long ReadLongLE();
    void WriteLong(long value);
    void WriteLongLE(long value);
    byte[] ReadBytes(int length);
    void ReadBytes(byte[] destination, int dstOffset, int count);
    void Write(byte[] array, int srcOffset, int length);
    /// <summary>
    /// 标记读下标
    /// </summary>
    void MarkReadIndex();
    /// <summary>
    /// 重置读下标到之前的标记
    /// </summary>
    void ResetReadIndex();
    /// <summary>
    /// 标记写下标
    /// </summary>
    void MarkWriteIndex();
    /// <summary>
    /// 重置写下标到之前的标记
    /// </summary>
    void ResetWriteIndex();
    /// <summary>
    /// 加读下标
    /// </summary>
    /// <param name="count"></param>
    void SkipWriteBytes(int count);
    /// <summary>
    /// 加写下标
    /// </summary>
    /// <param name="count"></param>
    void SkipReadBytes(int count);
}