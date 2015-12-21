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
        private readonly string _fileName;

        public override string ToString()
        {
            return _fileName;
        }

        private UpdateFile(string fileName, Mode mode, bool checksum = true)
        {
            // Store filename
            _fileName = fileName;

            switch (mode)
            {
                case Mode.Open:
                {
                    // Load entries
                    LoadEntries(checksum);
                    break;
                }

                case Mode.Create:
                {
                    // Create file
                    CreateFile();
                    break;
                }
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

        private void LoadEntries(bool checksum)
        {
            using (var stream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Skip first 92 bytes
                stream.Seek(SkipBytes, SeekOrigin.Begin);

                // Read file
                while (stream.Position < stream.Length)
                {
                    // Read entry
                    var entry = UpdateEntry.Open(stream, checksum);

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
        }

        private void CreateFile()
        {
            using (var stream = new FileStream(_fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var buffer = new byte[SkipBytes];

                // Write SkipBytes
                stream.Write(buffer, 0, buffer.Length);
            }
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
        /// Add <see cref="UpdateEntry"/>
        /// </summary>
        /// <param name="entry"><see cref="UpdateEntry"/></param>
        /// <param name="stream"><see cref="Stream"/> to input data</param>
        public void Add(UpdateEntry entry, Stream stream)
        {
            // Set size
            entry.FileSize = (uint) stream.Length;
            
            // Calculate checksum table size
            var checksumTableSize = entry.FileSize / entry.BlockSize;
            if (entry.FileSize % entry.BlockSize != 0) 
                checksumTableSize++;

            // Allocate checksum table
            entry.CheckSumTable = new ushort[checksumTableSize];

            // Set headersize
            entry.HeaderSize = (uint) (FileHeader.Size + (checksumTableSize*Utilities.UshortSize));

            using (var output = new FileStream(_fileName, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                // Compute header checksum
                entry.ComputeHeaderChecksum();

                // Get header
                var header = entry.GetHeader();

                // Write header
                output.Write(header, 0, header.Length);

                // Skip checksum table
                output.Seek(checksumTableSize * Utilities.UshortSize, SeekOrigin.Current);

                // Set offset
                entry.DataOffset = output.Position;

                // Read data
                var buffer = new byte[entry.BlockSize];
                var blockNumber = 0;
                int size;

                // Calculate checksum
                while ((size = stream.Read(buffer, 0, entry.BlockSize)) > 0)
                {
                    // Calculate checksum
                    entry.CheckSumTable[blockNumber] = Utilities.Crc.ComputeSum(buffer, 0, size);

                    // Write data
                    output.Write(buffer, 0, size);

                    // Increase blocknumber
                    blockNumber++;
                }

                // Jump back 
                output.Seek(-(stream.Length + (checksumTableSize * Utilities.UshortSize)), SeekOrigin.Current);

                // Write checksum table
                var writer = new BinaryWriter(output);

                // Write
                for (var count = 0; count < entry.CheckSumTable.Length; count++) writer.Write(entry.CheckSumTable[count]);

                // Write remainder
                var remainder = Utilities.UintSize - (int)(writer.BaseStream.Position % Utilities.UintSize);
                if (remainder < Utilities.UintSize)
                {
                    // Write remainder bytes
                    writer.Write(new byte[remainder]);
                }
            }

            // Add entry
            Entries.Add(entry);
        }

        /// <summary>
        /// Add <see cref="UpdateEntry"/>
        /// </summary>
        /// <param name="entry"><see cref="UpdateEntry"/></param>
        /// <param name="fileName">File to add</param>
        public void Add(UpdateEntry entry, string fileName)
        {
            using (var input = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Add(entry, input);
            }
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
