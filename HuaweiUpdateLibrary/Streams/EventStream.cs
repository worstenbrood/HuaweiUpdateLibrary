using System;
using System.IO;

namespace HuaweiUpdateLibrary.Streams
{
    public class EventStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly object _param;

        public class ReadEventArgs : EventArgs
        {
            public object Param;
            public int Result;
            public byte []Buffer;
        }

        public class WriteEventArgs : EventArgs
        {
            public object Param;
        }

        public delegate void OnReadEventHandler(object sender, ReadEventArgs e);

        public OnReadEventHandler OnReadEvent;

        public delegate void OnWriteEventHandler(object sender, WriteEventArgs e);

        public OnWriteEventHandler OnWriteEvent;

        public EventStream(Stream baseStream, object param = null)
        {
            _baseStream = baseStream;
            _param = param;
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var result = _baseStream.Read(buffer, offset, count);
            if (result != 0 && OnReadEvent != null) OnReadEvent(this, new ReadEventArgs { Buffer = buffer, Result = result, Param = _param });
            return result;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer, offset, count);
            if (OnWriteEvent != null) OnWriteEvent(this, new WriteEventArgs { Param = _param });
        }

        public override bool CanRead
        {
            get { return _baseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _baseStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _baseStream.CanWrite; }
        }

        public override long Length
        {
            get { return _baseStream.Length; }
        }

        public override long Position
        {
            get { return _baseStream.Position; }
            set { _baseStream.Position = value; }
        }
    }
}
