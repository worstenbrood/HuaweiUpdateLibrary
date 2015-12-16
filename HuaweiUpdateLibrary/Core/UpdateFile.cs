using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace HuaweiUpdateLibrary.Core
{
    public class UpdateFile : IEnumerable<UpdateEntry>
    {
        private enum Mode
        {
            Open,
            Create
        }
        
        private const long SkipBytes = 92;
        private string _fileName;
        
        private UpdateFile(string fileName, Mode mode, bool checksum = true)
        {
            _fileName = fileName;

            // Only load if opened
            if (mode != Mode.Open) 
                return;

            // Open stream
            using (var stream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Load entries
                LoadEntries(stream, checksum);
            }
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
                var remainder = Utilities.UintSize - (int)(stream.Position % Utilities.UintSize);
                if (remainder < Utilities.UintSize)
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
            return new UpdateFile(fileName, Mode.Open, checksum);
        }

        /// <summary>
        /// Create an <see cref="UpdateFile"/>
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <returns><see cref="UpdateFile"/></returns>
        public static UpdateFile Create(string fileName)
        {
            return new UpdateFile(fileName, Mode.Create);
        }

        /// <summary>
        /// Extract <see cref="UpdateEntry"/>
        /// </summary>
        /// <param name="index"><see cref="UpdateEntry"/> index</param>
        /// <param name="output">Output file</param>
        /// <param name="checksum">Verify checksum</param>
        public void Extract(int index, string output, bool checksum = true)
        {
            // Extract entry
            Extract(Entries[index], output, checksum);
        }

        /// <summary>
        /// Extract <see cref="UpdateEntry"/>
        /// </summary>
        /// <param name="entry"><see cref="UpdateEntry"/></param>
        /// <param name="output">Output file</param>
        /// <param name="checksum">Verify checksum</param>
        public void Extract(UpdateEntry entry, string output, bool checksum = true)
        {
            // Extract entry
            entry.Extract(_fileName, output, checksum);
        }

        /// <summary>
        /// Extract <see cref="UpdateEntry"/>
        /// </summary>
        /// <param name="index"><see cref="UpdateEntry"/> index</param>
        /// <param name="output">Output file</param>
        /// <param name="checksum">Verify checksum</param>
        public void Extract(int index, Stream output, bool checksum = true)
        {
            // Extract entry
            Extract(Entries[index], output, checksum);
        }

        /// <summary>
        /// Extract <see cref="UpdateEntry"/>
        /// </summary>
        /// <param name="entry"><see cref="UpdateEntry"/></param>
        /// <param name="output">Output file</param>
        /// <param name="checksum">Verify checksum</param>
        public void Extract(UpdateEntry entry, Stream output, bool checksum = true)
        {
            // Extract entry
            entry.Extract(_fileName, output, checksum);
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
