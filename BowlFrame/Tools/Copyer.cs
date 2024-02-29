namespace BowlFrame.Tools
{
    internal static class Copyer
    {
        public static async Task<HttpRequestMessage> CopyHttpRequestMessage(HttpRequestMessage original)
        {
            HttpRequestMessage clone = new(original.Method, original.RequestUri)
            {
                Version = original.Version,
                VersionPolicy = original.VersionPolicy
            };

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