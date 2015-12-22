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
 */

using System;
using System.IO;

namespace HuaweiUpdateLibrary.Streams
{
    public class PartialStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly long _basePosition;
        private readonly long _size;
        
        public PartialStream(Stream baseStream, long size)
        {
            _baseStream = baseStream;
            _basePosition = baseStream.Position;
            
            if (_basePosition + _size > _baseStream.Length)
                throw new ArgumentOutOfRangeException("size", _basePosition + _size, "Invalid size");
            _size = size;
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset > _size || offset < 0)
                        throw new EndOfStreamException("Seek failed");
                    return _baseStream.Seek(_basePosition + offset, origin);

                case SeekOrigin.Current:
                    var currentPosition = _baseStream.Position - _basePosition;
                    if (currentPosition + offset > _size || currentPosition + offset < 0)
                        throw new EndOfStreamException("Seek failed");
                    return _baseStream.Seek(currentPosition + offset, origin);

                case SeekOrigin.End:
                    if (offset > 0)
                        throw new EndOfStreamException("Seek failed");
                    break;

            }

            return _baseStream.Seek(_basePosition + _size + offset, origin);
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var currentPosition = _baseStream.Position - _basePosition;
            if (currentPosition == _size)
                return 0;

            if (currentPosition + count > _size)
            {
                var rc = _size - currentPosition;
                count = (rc > int.MaxValue) ? int.MaxValue : (int) rc;
            }

            return _baseStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var currentPosition = _baseStream.Position - _basePosition;
            if (currentPosition + count > _size)
                throw new EndOfStreamException("Write failed");

            _baseStream.Write(buffer, offset, count);
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
            get { return _size; }
        }

        public override long Position
        {
            get { return _baseStream.Position - _basePosition; }
            set
            {
                if (value > _size || value < 0)
                    throw new EndOfStreamException("Position failed");
                _baseStream.Position = _basePosition + value;
            }
        }
    }
}

