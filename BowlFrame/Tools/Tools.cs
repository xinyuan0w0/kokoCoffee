using Dm.net.buffer;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace BowlFrame.Tools
{
    internal static class Tools
    {
        public static (string, string) GetMimeType(byte[] bytes)
        {
            string hex = Convert.ToHexString(bytes);
            return GetMimeType(hex);
        }

        public static (string, string) GetMimeType(Stream stream)
        {
            using BinaryReader reader = new(stream);
            byte[] header = reader.ReadBytes(32);
            string hex = Convert.ToHexString(header);
            return GetMimeType(hex);
        }

        private static readonly Dictionary<string, (string, string)> MimeTypeMappings = new()
        {
            { "89504E", ("image/png", "png") }, // PNG
            { "FFD8FF", ("image/jpeg", "jpg") }, // JPEG
            { "49492A", ("image/tiff", "tiff") }, // TIFF
            { "4D4D00", ("image/tiff", "tiff") }, // TIFF
            { "4D4D2A", ("image/tiff", "tiff") }, // TIFF
            { "47494638", ("image/gif", "gif") }, // GIF
            { "424D", ("image/bmp", "bmp") } // BMP
        };

        public static (string, string) GetMimeType(string hex)
        {
            foreach (KeyValuePair<string, (string, string)> mime in MimeTypeMappings)
                if (hex.StartsWith(mime.Key))
                    return mime.Value;

            return ("application/octet-stream", "bytes");
        }

        public static string GetMD5Hex(byte[] fileBytes) => Convert.ToHexString(MD5.HashData(fileBytes));

        public static async Task<string> GetMD5Hex(Stream fileStream)
        {
            using MD5 md5 = MD5.Create();
            byte[] result = await md5.ComputeHashAsync(fileStream);
            return Convert.ToHexString(MD5.HashData(result));
        }
    }
}