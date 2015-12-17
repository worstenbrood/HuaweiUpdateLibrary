using System;
using System.Runtime.InteropServices;

namespace HuaweiUpdateLibrary.Core
{
    // Credits: ZeBadger (zebadger@hotmail.com)
    // For some stuff ripped from split_updata.pl

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal struct FileHeader
    {
        public UInt32 HeaderId;
        public UInt32 HeaderSize;
        public UInt32 Unknown1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] HardwareId;
        public UInt32 FileSequence;
        public UInt32 FileSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] FileDate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] FileTime;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] FileType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Blank1;
        public UInt16 HeaderChecksum;
        public UInt16 BlockSize;
        public UInt16 Blank2;

        /// <summary>
        /// Return size of this struct
        /// </summary>
        public static readonly int Size = Marshal.SizeOf(typeof (FileHeader)); 
    }
}
