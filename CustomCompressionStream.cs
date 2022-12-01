using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streams.Compression
{
    public class CustomCompressionStream : Stream
    {
        public CustomCompressionStream(Stream baseStream, bool read)
        {
            this.read = read;  // Используйте этот флаг, чтобы понимать:
                               // ваш стрим открыт в режиме чтения или в режиме записи.
                               // Не нужно поддерживать и чтение и запись одновременно.
            this.baseStream = baseStream;
        }
    }
}
