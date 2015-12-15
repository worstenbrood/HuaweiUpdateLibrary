using System;
using System.Security.Cryptography;

namespace HuaweiUpdateLibrary.Algorithms
{
    public class UpdateCrc16 : HashAlgorithm
    {
        private readonly ushort[] _table = new ushort[256];
        private readonly ushort _initialSum;
        private readonly ushort _polynomial = 0x8408;
        private readonly ushort _xorValue = 0xFFFF;

        private void InitializeTable()
        {
            for (ushort i = 0; i < _table.Length; ++i)
            {
                ushort value = 0;
                var temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ _polynomial);
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
        
        public UpdateCrc16(ushort initialSum = 0xFFFF, ushort polynomial = 0x8408, ushort xorValue = 0xFFFF)
        {
            _initialSum = initialSum;
            _polynomial = polynomial;
            _xorValue = xorValue;
            
            // Init table
            InitializeTable();

            // Initialize sum
            HashValue = BitConverter.GetBytes(_initialSum);
        }

        public override void Initialize()
        {
            HashValue = BitConverter.GetBytes(_initialSum);
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            var sum = BitConverter.ToUInt16(HashValue, 0);
            var i = ibStart;
            var size = (cbSize - ibStart) * 8;

            while (size >= 8)
            {
                var v = array[i++];
                sum = (ushort)((_table[(v ^ sum) & 0xFF] ^ (sum >> 8)) & 0xFFFF);
                size -= 8;
            }

            if (size != 0)
            {
                for (var n = array[i] << 8;; n >>= 1)
                {
                    if (size == 0) break;
                    size -= 1;
                    var flag = ((sum ^ n) & 1) == 0;
                    sum >>= 1;
                    if (flag) sum ^= _polynomial;
                }
            }

            HashValue = BitConverter.GetBytes(sum);
            HashSizeValue = HashValue.Length;
        }

        protected override byte[] HashFinal()
        {
            var result = BitConverter.GetBytes((ushort)((BitConverter.ToUInt16(HashValue, 0) ^ _xorValue) & 0xFFFF));

            // Reinit
            HashValue = BitConverter.GetBytes(_initialSum);

            return result;
        }
    }
}
