using System;
using System.Text;
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
            var sum1 = crc.ComputeSum(Encoding.ASCII.GetBytes(stest));

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
            u.Add(e, @"c:\temp\v1.14.zip");
            var crc = UpdateEntry.Create();
            u.AddChecksum(crc);
            u = UpdateFile.Open(@"c:\temp\test.app");
            u.Remove(0);
            u = UpdateFile.Open(@"c:\temp\test.app");
        }
    }
}
