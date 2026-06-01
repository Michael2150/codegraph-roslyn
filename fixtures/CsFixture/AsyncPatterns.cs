using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Fixtures
{
    // Covers: interface with async method signatures
    public interface IDataFetcher
    {
        Task<string>   FetchAsync(string url);
        Task<string[]> FetchManyAsync(IEnumerable<string> urls);
    }

    // Covers: async Task<T> implementation, await expression, method-to-method call
    public class HttpDataFetcher : IDataFetcher
    {
        public async Task<string> FetchAsync(string url)
        {
            await Task.Delay(1);
            return $"response from {url}";
        }

        public async Task<string[]> FetchManyAsync(IEnumerable<string> urls)
        {
            var tasks = urls.Select(u => FetchAsync(u));
            return await Task.WhenAll(tasks);
        }
    }

    // Covers: async Task<int>, await on injected interface, async void (fire-and-forget),
    //         sync method calling async via .GetAwaiter().GetResult()
    public class UrlProcessor : IDataFetcher
    {
        private readonly IDataFetcher _inner;

        public UrlProcessor(IDataFetcher inner)
        {
            _inner = inner;
        }

        public async Task<string> FetchAsync(string url) =>
            await _inner.FetchAsync(url);

        public async Task<string[]> FetchManyAsync(IEnumerable<string> urls) =>
            await _inner.FetchManyAsync(urls);

        public async Task<int> ProcessUrlAsync(string url)
        {
            var content = await FetchAsync(url);
            return content.Length;
        }

        // Covers: async void method (isAsync = true)
        public async void FireAndForget(string url)
        {
            await ProcessUrlAsync(url);
        }

        public int GetLengthSync(string url) =>
            ProcessUrlAsync(url).GetAwaiter().GetResult();
    }
}
