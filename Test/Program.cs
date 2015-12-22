using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using HuaweiUpdateLibrary.Algorithms;
using HuaweiUpdateLibrary.Core;

namespace Test
{
    static class Program
    {
        public static void UpdateCrc16Test()
        {
            var crc = new UpdateCrc16();

            var stest = "test";
            var sum1 = BitConverter.ToUInt16(crc.ComputeHash(Encoding.ASCII.GetBytes(stest)), 0);

            var btest1 = Encoding.ASCII.GetBytes("te");
            var btest2 = Encoding.ASCII.GetBytes("st");
            crc.TransformBlock(btest1, 0, 2, btest1, 0);
            crc.TransformFinalBlock(btest2, 0, 2);

            var sum2 = BitConverter.ToUInt16(crc.Hash, 0);

            if (sum1 != 1928)
                throw new Exception("Sum1 mismatch");

            if (sum1 != sum2)
                throw new Exception("Sum2 mismatch");
        }


        static void Main(string[] args)
        {
            var u = UpdateFile.Create(@"c:\temp\test.app");
            var e = UpdateEntry.Create();
            e.FileSequence = 0xFF000000;
            e.HardwareId = "HW8x50";
            e.FileType = "IMAGE";
            u.Add(e, @"c:\temp\v1.14.zip");
            var v = UpdateFile.Open(@"c:\temp\test.app");
            v.Extract(v[0], "c:\\temp\\test.zip");
        }
    }
}
