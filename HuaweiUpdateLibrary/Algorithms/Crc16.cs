using System;
using System.Security.Cryptography;

namespace HuaweiUpdateLibrary.Algorithms
{
    public class Crc16 : HashAlgorithm
    {
        private readonly ushort[] _table = new ushort[256];
        private const ushort InitialSum = 0xFFFF;
        private const ushort Polynomial = 0x8408;
        private const ushort XorValue = 0xFFFF;

        public override void Initialize()
        {
            for (ushort i = 0; i < _table.Length; ++i)
            {
                ushort value = 0;
                var temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ Polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                _table[i] = value;
            }
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            var sum = InitialSum;
            var i = ibStart;
            var size = (cbSize - ibStart) * 8;

            while (size >= 8)
            {
                var v = array[i++];
                sum = (ushort)((_table[(v ^ sum) & 0xff] ^ (sum >> 8)) & 0xffff);
                size -= 8;
            }

            if (size == 0)
            {
                HashValue = new [] {(byte) ((sum ^ XorValue) & 0xffff)};
                HashSizeValue = 1;
                return;
            }

            for (var n = array[i] << 8; ; n >>= 1)
            {
                if (size == 0) break;
                size -= 1;
                var flag = ((sum ^ n) & 1) == 0;
                sum >>= 1;
                if (flag) sum ^= Polynomial;
            }

            HashValue = new[] { (byte)((sum ^ XorValue) & 0xffff) };
            HashSizeValue = 1;
        }

        protected override byte[] HashFinal()
        {
            return HashValue;
        }
    }
}
