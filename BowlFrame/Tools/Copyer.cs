using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BowlFrame.Tools
{
    internal static class Copyer
    {
        public static async Task<HttpRequestMessage> CopyHttpRequestMessage(HttpRequestMessage original)
        {
            HttpRequestMessage clone = new HttpRequestMessage(original.Method, original.RequestUri);
            clone.Version = original.Version;
            clone.VersionPolicy = original.VersionPolicy;
            foreach (var header in original.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            if (original.Content != null)
            {
                await original.Content.LoadIntoBufferAsync();
                var ms = new MemoryStream();
                await original.Content.CopyToAsync(ms);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);
                foreach (var header in original.Content.Headers)
                    clone.Content.Headers.Add(header.Key, header.Value);
            }
            return clone;
        }
    }
}