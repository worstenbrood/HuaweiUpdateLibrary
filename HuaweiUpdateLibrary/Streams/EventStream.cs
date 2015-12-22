/*
 *  Copyright 2015 worstenbrood
 *  
 *  This file is part of HuaweiUpdateLibrary.
 *  
 *  HuaweiUpdateLibrary is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as 
 *  published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *  
 *  HuaweiUpdateLibrary is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of 
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *  You should have received a copy of the GNU General Public License along with HuaweiUpdateLibrary. 
 *  If not, see http://www.gnu.org/licenses/.
 *  
 */

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
