using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Config
{
    public static class ConfigLoad
    {
        private static readonly ConcurrentDictionary<string, byte[]> files = new();

        public static async Task<bool> AddFileFromPath(string name, string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                filePath = Path.GetFullPath(filePath);
                if (!Path.Exists(filePath))
                    return false;
                byte[] bytes = Encoding.UTF8.GetBytes(await File.ReadAllTextAsync(filePath, cancellationToken));

                return AddFile(name, bytes);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }
        }

        public static bool AddFile(string name, string content)
        {
            return AddFile(name, Encoding.UTF8.GetBytes(content));
        }

        public static bool AddFile(string name, byte[] content)
        {
            return files.TryAdd(name, content);
        }

        public static bool RemoveFile(string name)
        {
            return files.TryRemove(name, out _);
        }

        public static byte[]? GetFileBytes(string name)
        {
            files.TryGetValue(name, out byte[]? bytes);

            return bytes;
        }

        public static string? GetFileString(string name)
        {
            byte[]? bytes;

            bytes = GetFileBytes(name);

            if (bytes is null)
                return null;
            else
                try
                {
                    return Encoding.UTF8.GetString(bytes);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    return null;
                }
        }
    }
}