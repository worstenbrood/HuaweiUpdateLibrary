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
        public char[] HardwareId;
        public UInt32 FileSequence;
        public UInt32 FileSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string FileDate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string FileTime;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string FileType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] Blank1;
        public UInt16 HeaderChecksum;
        public UInt16 BlockSize;
        public UInt16 Blank2;
    }
}
