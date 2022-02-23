using BlockChainTutorial.Models;

namespace BlockChainTutorial.Services;

public interface IBlockChainService
{
    Task<Block> CreateNewBlockAsync(int proof, string previousHash);

    Block? LastBlock();

    int CreateNewTransaction(Transaction transaction);

    List<Block> GetChain();

    Task<int> ProofOfWorkAsync(int lastProof);

    Task<bool> ValidProofAsync(int lastProof = 0, int proof = 0);

    Task<string> HashBlockAsync(Block block);

    void RegisterNodes(List<string> nodes);

    Task<bool> ValidChainAsync(List<Block> chain);

    Task ResolveConflictsAsync();

    Task<Block> MineAsync();
}