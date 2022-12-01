using System;
using System.IO;

namespace Streams.Compression
{
    public class CustomCompressionStream : Stream
    {
        private readonly bool _read;
        private readonly Stream _baseStream;
        private int _counter;
        private static int _valueByteStock;
        private static int _repeatByteStock;

        public CustomCompressionStream(Stream baseStream, bool read)
        {
            _read = read;
            _baseStream = baseStream;
            _valueByteStock = -1;
            _repeatByteStock = -1;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var countBytes = AddReadedBytesToBuffer(buffer);
            for (_counter = countBytes; _counter < count + offset;)
            {
                var valueByte = _baseStream.ReadByte();
                var repetitionsByte = _baseStream.ReadByte();
                if (repetitionsByte == -1)
                    return valueByte == -1 ? _counter : throw new InvalidOperationException();
                for (var i = 0; i < repetitionsByte; i++)
                {
                    if (_counter + offset == buffer.Length)
                    {
                        _valueByteStock = valueByte;
                        _repeatByteStock = repetitionsByte - i;
                        if (offset > 0) buffer[buffer.Length - 1] = 0;
                        return _counter - offset;
                    }
                    buffer[_counter++ + offset] = (byte) valueByte;
                }
            }
            return _counter;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (var i = offset; i < count; i++)
            {
                byte countRepeatByte = 1;
                while (IsByteRepeats(buffer, i, countRepeatByte))
                {
                    countRepeatByte++;
                    i++;
                }
                _baseStream.WriteByte(buffer[i]);
                _baseStream.WriteByte(ReturnRepeatsCount(buffer, offset, i, countRepeatByte));
            }
        }

        private static byte ReturnRepeatsCount(byte[] buffer, int offset, int i, byte countRepeatByte)
        {
            return i == buffer.Length - 1 ? (byte) (countRepeatByte - offset) : countRepeatByte;
        }

        private static bool IsByteRepeats(byte[] buffer, int i, byte countRepeatByte)
        {
            return i < buffer.Length - 1 && buffer[i] == buffer[i + 1] && countRepeatByte < byte.MaxValue;
        }

        private int AddReadedBytesToBuffer(byte[] buffer)
        {
            int subCounter;
            for (subCounter = 0; subCounter < _repeatByteStock; subCounter++)
                buffer[subCounter] = (byte) _valueByteStock;
            _valueByteStock = -1;
            _repeatByteStock = -1;
            return subCounter;
        }

        public override bool CanRead => _read;
        public override bool CanSeek => false;
        public override bool CanWrite => !_read;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => _baseStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}