using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Utilities
{
    internal class CompressionUtilities
    {
        public static byte[] CompressWithDeflate(byte[] data)
        {
            var output = new MemoryStream();
            using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                stream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] DecompressWithDeflate(byte[] data)
        {
            var input = new MemoryStream(data);
            var output = new MemoryStream();
            using (var stream = new DeflateStream(input, CompressionMode.Decompress))
            {
                stream.CopyTo(output);
            }
            return output.ToArray();
        }
    }
}
