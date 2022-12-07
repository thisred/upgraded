public class RingBuffer : IBuffer
{
    private readonly int _capacity;

    /// <summary>
    /// 读总数
    /// </summary>
    private uint _readCount;

    /// <summary>
    /// 写总数
    /// </summary>
    private uint _writeCount;

    /// <summary>
    /// 构造函数，初始化缓冲区，设置缓冲区大小
    /// </summary>
    /// <param name="size">缓冲区大小，必须为2的幂</param>
    public RingBuffer(int size = 8192) : this(new byte[size])
    {
    }


    private RingBuffer(byte[] buffer)
    {
        if ((buffer.Length & (buffer.Length - 1)) != 0)
            throw new Exception("数组长度必须为2的幂");

        Array = buffer;
        Capacity = buffer.Length;
        _readCount = 0;
        _writeCount = 0;
    }

    /// <summary>
    /// 缓冲区大小,必须为2的幂
    /// </summary>
    public int Capacity
    {
        get => _capacity;
        private init
        {
            _capacity = value;
            _bufferSizeMask = value - 1;
        }
    }

    private readonly int _bufferSizeMask;

    public byte[] Array { get; }

    /// <summary>
    /// 写下标
    /// </summary>
    public int WriteIndex => (int)(_writeCount & (uint)_bufferSizeMask);

    /// <summary>
    /// 读下标
    /// </summary>
    public int ReadIndex => (int)(_readCount & (uint)_bufferSizeMask);

    /// <summary>
    /// 缓冲区未读数据长度
    /// </summary>
    public int DataLength => (int)(_writeCount - _readCount);

    /// <summary>
    /// 剩余空间
    /// </summary>
    public int RemainingLength => Capacity - DataLength;

    /// <summary>
    /// 向环形缓冲区中写给定数组
    /// </summary>
    /// <param name="srcBytes">要写入缓冲区的数组</param>
    /// <param name="srcOffset">给定数组的偏移量</param>
    /// <param name="count">写入长度</param>
    public void Write(byte[] srcBytes, int srcOffset, int count)
    {
        if (srcBytes == null || srcBytes.Length == 0) return;
        if (srcOffset > srcBytes.Length) return;


        // 剩余空间 = 数组长度 - 写下标
        var remainingLength = Capacity - WriteIndex;
        // 剩余空间 > 写入长度
        if (remainingLength > count)
        {
            System.Buffer.BlockCopy(srcBytes, srcOffset, Array, WriteIndex, count);
        }
        else
        {
            // 剩余空间 < 写入长度
            System.Buffer.BlockCopy(srcBytes, srcOffset, Array, WriteIndex, remainingLength);
            System.Buffer.BlockCopy(srcBytes, remainingLength, Array, 0, count - remainingLength);
        }

        _writeCount += (uint)count;
    }

    private uint _markedReadCount;

    public void MarkReadIndex()
    {
        _markedReadCount = _readCount;
    }

    public void ResetReadIndex()
    {
        _readCount = _markedReadCount;
    }

    private uint _markedWriteCount;

    public void MarkWriteIndex()
    {
        _markedWriteCount = _writeCount;
    }

    public void ResetWriteIndex()
    {
        _writeCount = _markedWriteCount;
    }


    public void SkipWriteBytes(int count)
    {
        if (count > RemainingLength)
        {
            throw new Exception("超过缓冲区大小了，未读取数据已经被复写了");
        }

        _writeCount += (uint)count;
    }

    public void SkipReadBytes(int count)
    {
        _readCount += (uint)count;
    }

    /// <summary>
    ///     读取数据到目标字节数组
    /// </summary>
    /// <param name="destination">目标字节数组</param>
    /// <param name="dstOffset">目标字节数组偏移量</param>
    /// <param name="count">读取长度</param>
    public void ReadBytes(byte[] destination, int dstOffset, int count)
    {
        if (destination == null) return;
        if (dstOffset > destination.Length) return;

        if (DataLength < count) return;
        var remainingLength = Capacity - ReadIndex;
        if (remainingLength > count)
        {
            System.Buffer.BlockCopy(Array, ReadIndex, destination, dstOffset, count);
        }
        else
        {
            System.Buffer.BlockCopy(Array, ReadIndex, destination, 0, remainingLength);
            System.Buffer.BlockCopy(Array, 0, destination, remainingLength, count - remainingLength);
        }

        _readCount += (uint)count;
    }


    /// <summary>
    /// 读取某一长度的数据
    /// </summary>
    /// <param name="count">数据长度</param>
    /// <returns></returns>
    public byte[] ReadBytes(int count)
    {
        var destination = new byte[count];
        ReadBytes(destination, 0, count);
        return destination;
    }

    public void WriteByte(byte value)
    {
        Array[WriteIndex] = value;
        _writeCount++;
    }

    public byte ReadByte()
    {
        var readByte = Array[ReadIndex];
        _readCount++;
        return readByte;
    }


    public void WriteInt(int value)
    {
        Array[WriteIndex] = (byte)(value >> 24);
        Array[(WriteIndex + 1) & _bufferSizeMask] = (byte)(value >> 16);
        Array[(WriteIndex + 2) & _bufferSizeMask] = (byte)(value >> 8);
        Array[(WriteIndex + 3) & _bufferSizeMask] = (byte)value;
        _writeCount += 4;
    }

    public int ReadInt()
    {
        var int32 = Array[(ReadIndex + 3) & _bufferSizeMask] |
                    (Array[(ReadIndex + 2) & _bufferSizeMask] << 8) |
                    (Array[(ReadIndex + 1) & _bufferSizeMask] << 16) |
                    (Array[(ReadIndex) & _bufferSizeMask] << 24);
        _readCount += 4;
        return int32;
    }


    public void WriteIntLE(int value)
    {
        Array[WriteIndex] = (byte)value;
        Array[(WriteIndex + 1) & _bufferSizeMask] = (byte)(value >> 8);
        Array[(WriteIndex + 2) & _bufferSizeMask] = (byte)(value >> 16);
        Array[(WriteIndex + 3) & _bufferSizeMask] = (byte)(value >> 24);
        _writeCount += 4;
    }

    public int ReadIntLE()
    {
        var intLe = Array[(ReadIndex) & _bufferSizeMask] |
                    (Array[(ReadIndex + 1) & _bufferSizeMask] << 8) |
                    (Array[(ReadIndex + 2) & _bufferSizeMask] << 16) |
                    (Array[(ReadIndex + 3) & _bufferSizeMask] << 24);
        _readCount += 4;
        return intLe;
    }


    public void WriteLong(long value)
    {
        Array[WriteIndex] = (byte)(value >> 56);
        Array[(WriteIndex + 1) & _bufferSizeMask] = (byte)(value >> 48);
        Array[(WriteIndex + 2) & _bufferSizeMask] = (byte)(value >> 40);
        Array[(WriteIndex + 3) & _bufferSizeMask] = (byte)(value >> 32);
        Array[(WriteIndex + 4) & _bufferSizeMask] = (byte)(value >> 24);
        Array[(WriteIndex + 5) & _bufferSizeMask] = (byte)(value >> 16);
        Array[(WriteIndex + 6) & _bufferSizeMask] = (byte)(value >> 8);
        Array[(WriteIndex + 7) & _bufferSizeMask] = (byte)(value);
        _writeCount += 8;
    }

    public long ReadLong()
    {
        var readLong =
            ((long)Array[(ReadIndex) & _bufferSizeMask] << 56) |
            ((long)Array[(ReadIndex + 1) & _bufferSizeMask] << 48) |
            ((long)Array[(ReadIndex + 2) & _bufferSizeMask] << 40) |
            ((long)Array[(ReadIndex + 3) & _bufferSizeMask] << 32) |
            ((long)Array[(ReadIndex + 4) & _bufferSizeMask] << 24) |
            ((long)Array[(ReadIndex + 5) & _bufferSizeMask] << 16) |
            ((long)Array[(ReadIndex + 6) & _bufferSizeMask] << 8) |
            ((long)Array[(ReadIndex + 7) & _bufferSizeMask]);
        _readCount += 8;
        return readLong;
    }


    public void WriteLongLE(long value)
    {
        Array[WriteIndex] = (byte)value;
        Array[(WriteIndex + 1) & _bufferSizeMask] = (byte)(value >> 8);
        Array[(WriteIndex + 2) & _bufferSizeMask] = (byte)(value >> 16);
        Array[(WriteIndex + 3) & _bufferSizeMask] = (byte)(value >> 24);
        Array[(WriteIndex + 4) & _bufferSizeMask] = (byte)(value >> 32);
        Array[(WriteIndex + 5) & _bufferSizeMask] = (byte)(value >> 40);
        Array[(WriteIndex + 6) & _bufferSizeMask] = (byte)(value >> 48);
        Array[(WriteIndex + 7) & _bufferSizeMask] = (byte)(value >> 56);
        _writeCount += 8;
    }

    public long ReadLongLE()
    {
        var readLongLe = ((long)Array[ReadIndex & _bufferSizeMask]) |
                         ((long)Array[(ReadIndex + 1) & _bufferSizeMask] << 8) |
                         ((long)Array[(ReadIndex + 2) & _bufferSizeMask] << 16) |
                         ((long)Array[(ReadIndex + 3) & _bufferSizeMask] << 24) |
                         ((long)Array[(ReadIndex + 4) & _bufferSizeMask] << 32) |
                         ((long)Array[(ReadIndex + 5) & _bufferSizeMask] << 40) |
                         ((long)Array[(ReadIndex + 6) & _bufferSizeMask] << 48) |
                         ((long)Array[(ReadIndex + 7) & _bufferSizeMask] << 56);
        _readCount += 8;
        return readLongLe;
    }
}