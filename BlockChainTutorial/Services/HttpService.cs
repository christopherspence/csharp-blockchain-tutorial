using BlockChainTutorial.Models;

namespace BlockChainTutorial.Services;

public class HttpService : IHttpService
{
    private readonly HttpClient client = new HttpClient();

    public async Task<List<Block>?> GetBlocksAsync(Uri address)
    {
        var result = await client.GetAsync($"{address.AbsolutePath}/chain");
        
        if (!result.IsSuccessStatusCode)
        {
            throw new ApplicationException($"Error getting data from {address.AbsolutePath}");
        }

        return await result.Content.ReadFromJsonAsync<List<Block>>();
    }
}