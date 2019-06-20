using System;
using System.IO;

namespace Ready.Framework.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ReadFully(this Stream stream, long initialLength)
        {
            stream.Seek(0, SeekOrigin.Begin);
            if (initialLength < 1)
                initialLength = 32768;

            var buffer = new byte[initialLength];
            var read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                if (read == buffer.Length)
                {
                    var nextByte = stream.ReadByte();

                    if (nextByte == -1) return buffer;

                    var newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }

            var ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
    }
}