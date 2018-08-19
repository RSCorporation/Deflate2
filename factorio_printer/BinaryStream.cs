using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace FactorioPrinter
{
    class BinaryStream : Stream
    {
        List<byte> data;
        public byte[] Data { get { return data.ToArray(); } }

        bool __canRead;
        public override bool CanRead { get { return __canRead; } }

        bool __canWrite;
        public override bool CanWrite { get { return __canWrite; } }

        public override bool CanSeek { get { return true; } }

        public override bool CanTimeout { get { return false; } }

        public override long Length 
        {
            get
            {
                if(!CanSeek)
                    throw new NotSupportedException("A class derived from Stream does not support seeking."); 
                return data.Count; 
            }
        }

        long __position;
        public override long Position 
        {
            get
            {
                return __position;
            }
            set
            {
                if(value >= data.Count)
                    throw new IOException("Position is out of stream");
                __position = value;
            }
        }

        public override int ReadTimeout { get { throw new InvalidOperationException(); } }
        public override int WriteTimeout { get { throw new InvalidOperationException(); } }

        public override void Flush()
        {
            data.Clear();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch(origin)
            {
                case SeekOrigin.Begin: Position = offset; break;
                case SeekOrigin.Current: Position += offset; break;
                case SeekOrigin.End: Position = data.Count - offset - 1; break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(!__canRead)
                throw new NotSupportedException();
            if(offset >= data.Count) throw new IOException();
            int reallength = Math.Min(count, data.Count - offset);
            buffer = data.GetRange(offset, reallength).ToArray();
            return reallength;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if(!__canWrite)
                throw new NotSupportedException();
            if(offset > data.Count)
                data.AddRange(new ConstRange<byte>(0,offset - data.Count));
            if(offset == data.Count)    
                data.AddRange(buffer.Take(count));
            data.InsertRange(offset, buffer.Take(count));
        }

        public BinaryStream(byte[] data, StreamMode mode)
        {
            switch(mode)
            {
                case StreamMode.Read:
                    __canRead = true;
                    __canWrite = false;
                    this.data = new List<byte>(data);
                    break;
                case StreamMode.Write:
                    __canRead = false;
                    __canWrite = true;
                    this.data = new List<byte>();
                    break;
                case StreamMode.ReadWrite:
                    __canRead = true;
                    __canWrite = true;
                    this.data = new List<byte>(data);
                    break;
            }
        }
    }
    public enum StreamMode
    {
        Read,
        Write,
        ReadWrite
    }
}