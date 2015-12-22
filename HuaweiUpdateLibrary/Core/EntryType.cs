using System;

namespace HuaweiUpdateLibrary.Core
{
    [Flags] 
    internal enum EntryType
    {
        Checksum,
        Signature,
        Normal
    }
}
