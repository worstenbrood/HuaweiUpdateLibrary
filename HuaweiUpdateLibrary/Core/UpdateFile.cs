using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace HuaweiUpdateLibrary.Core
{
    public class UpdateFile : IEnumerable<UpdateEntry>
    {
        private const long SkipBytes = 92;
        private readonly Int32 _uintSize = Marshal.SizeOf(typeof(UInt32));

        private UpdateFile(string fileName, bool checksum = true)
        {
            // Open stream
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Load entries
                LoadEntries(stream, checksum);
            }
        }

        private UpdateFile(Stream stream, bool checksum = true)
        {
            // Load entries
            LoadEntries(stream, checksum);
        }

        private void LoadEntries(Stream stream, bool checksum)
        {
            // Skip first 92 bytes
            stream.Seek(SkipBytes, SeekOrigin.Begin);

            // Read file
            while (stream.Position < stream.Length)
            {
                // Read entry
                var entry = UpdateEntry.Read(stream, checksum);

                // Add to list
                Entries.Add(entry);

                // Skip file data
                stream.Seek(entry.FileSize, SeekOrigin.Current);

                // Read remainder
                var remainder = _uintSize - (int)(stream.Position % _uintSize);
                if (remainder < _uintSize)
                    stream.Seek(remainder, SeekOrigin.Current);
            }
        }
       
        private List<UpdateEntry> _entries;

        private List<UpdateEntry> Entries
        {
            get { return _entries ?? (_entries = new List<UpdateEntry>()); }
        }

        /// <summary>
        /// Access <see cref="UpdateEntry"/> on index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns><see cref="UpdateEntry"/></returns>
        public UpdateEntry this[int index]
        {
            get { return Entries[index]; }
        }

        /// <summary>
        /// Returns number of <see cref="UpdateEntry"/>
        /// </summary>
        public int Count
        {
            get { return Entries.Count; }
        }
        
        /// <summary>
        /// Open an existing update file
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="checksum">Verify header checksum</param>
        /// <returns><see cref="UpdateFile"/></returns>
        public static UpdateFile Open(string fileName, bool checksum = true)
        {
            return new UpdateFile(fileName, checksum);
        }

        /// <summary>
        /// Open an existing update file
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> to read from</param>
        /// <param name="checksum">Verify header checksum</param>
        /// <returns><see cref="UpdateFile"/></returns>
        public static UpdateFile Open(Stream stream, bool checksum = true)
        {
            return new UpdateFile(stream, checksum);
        }
        
        /// <summary>
        /// Returns enumerator
        /// </summary>
        /// <returns><see cref="IEnumerator"/></returns>
        public IEnumerator<UpdateEntry> GetEnumerator()
        {
            return Entries.GetEnumerator();
        }

        /// <summary>
        /// Returns enumerator
        /// </summary>
        /// <returns><see cref="IEnumerator"/></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
