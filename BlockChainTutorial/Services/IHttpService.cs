using BlockChainTutorial.Models;

namespace BlockChainTutorial.Services;

public interface IHttpService
{
    Task<List<Block>?> GetBlocksAsync(Uri address);
}