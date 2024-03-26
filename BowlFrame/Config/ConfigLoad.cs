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
        private static readonly ConcurrentDictionary<string, (byte[], string?)> files = new();

        public static bool IsHaveFile(string name) => files.ContainsKey(name);

        public static async Task<bool> AddFileFromPath(string name, string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                filePath = Path.GetFullPath(filePath);
                if (!Path.Exists(filePath))
                    return false;
                byte[] bytes = Encoding.UTF8.GetBytes(await File.ReadAllTextAsync(filePath, cancellationToken));

                return AddFile(name, bytes, filePath);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return false;
            }
        }

        public static bool AddFile(string name, string content, string? filePath = null)
        {
            return AddFile(name, Encoding.UTF8.GetBytes(content), filePath);
        }

        public static bool AddFile(string name, byte[] content, string? filePath = null)
        {
            return files.TryAdd(name, (content, filePath));
        }

        public static bool RemoveFile(string name)
        {
            return files.TryRemove(name, out _);
        }

        public static byte[]? GetFileBytes(string name)
        {
            files.TryGetValue(name, out (byte[], string?) value);

            return value.Item1;
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

        public async static Task<bool> Reload()
        {
            foreach (KeyValuePair<string, (byte[], string?)> value in files)
            {
                if (value.Value.Item2 is not null)
                {
                    if (!Path.Exists(value.Value.Item2))
                        continue;

                    RemoveFile(value.Key);

                    if (await AddFileFromPath(value.Key, value.Value.Item2) != true)
                        return false;
                }
            }
            return true;
        }
    }
}