using HuaweiUpdateLibrary.Core;

namespace Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            const string file = @"C:\Temp\update.app";
            var updateFile = UpdateFile.Open(file);
            updateFile.Extract(0, @"c:\temp\" + updateFile[0].FileType + ".img");
        }
    }
}
