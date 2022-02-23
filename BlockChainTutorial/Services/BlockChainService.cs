using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BlockChainTutorial.Extensions;
using BlockChainTutorial.Models;

namespace BlockChainTutorial.Services;

public class BlockChainService : IBlockChainService
{    
    private readonly ILogger<BlockChainService> logger;
    private readonly IHttpService httpService;
    
    public Guid NodeId { get; private set; } 

    public BlockChain BlockChain { get; private set; }
    
    public BlockChainService(ILogger<BlockChainService> logger, IHttpService httpService, BlockChain blockChain) 
    {
        this.BlockChain = blockChain;
        // Add the genesis block
        this.logger = logger;
        this.httpService = httpService;

        this.NodeId = Guid.NewGuid();
        this.BlockChain.Chain.Add(new Block(0, DateTime.UtcNow, new List<Transaction>(), 0, "1"));
    }

    public List<Block> GetChain() => this.BlockChain.Chain;

    public async Task<Block> CreateNewBlockAsync(int proof, string previousHash)
    {
        var previousIndex = 0;
        var now = DateTime.UtcNow;

        if (this.BlockChain.Chain.Count > 0)
        {
            previousIndex = this.BlockChain.Chain.Count() - 1;
        }

        var block = new Block(this.BlockChain.Chain.Count() + 1,
            now, 
            this.BlockChain.CurrentTransactions, 
            0,
            previousHash ?? await HashBlockAsync(this.BlockChain.Chain.ElementAt(previousIndex)));
        
        this.BlockChain.CurrentTransactions = new List<Transaction>();
        this.BlockChain.Chain.Add(block);
        
        return block;
    }

    public Block? LastBlock()
    {
        if (this.BlockChain.Chain.Count == 0)
        {
            return null;
        }

        return this.BlockChain.Chain.Last();
    }

    public int CreateNewTransaction(Transaction transaction)
    {
        this.BlockChain.CurrentTransactions.Add(transaction);

        if (this.BlockChain.Chain.Count == 1)
        {
            return 1;
        } 
        else {
            return this.BlockChain.Chain.Last().Index + 1;
        }
    }

    public async Task<int> ProofOfWorkAsync(int lastProof)
    {
        var proof = 0;
        while(!await this.ValidProofAsync(lastProof, proof)) {
            proof += 1;
        }

        return proof;
    }

    public async Task<bool> ValidProofAsync(int lastProof = 0, int proof = 0)
    {        
        logger.LogInformation($"Proof: {proof} Last Proof: {lastProof}");
        var base64Str = $"{proof.ToString().ToBase64String()}{lastProof.ToString().ToBase64String()}";
        
        using var sha256 = SHA256.Create();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(base64Str));
        var hash = await sha256.ComputeHashAsync(stream);

        if (hash != null)
        {
            var hashStr = Convert.ToBase64String(hash);
            logger.LogInformation($"Guess: {base64Str}");
            logger.LogInformation($"Hash: {hashStr}");
            return hashStr.StartsWith("0000");            
        }
        
        return false;
    }

    public async Task<string> HashBlockAsync(Block block)
    {
        var json = JsonSerializer.Serialize(block);

        using var sha256 = SHA256.Create();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var hash = await sha256.ComputeHashAsync(stream);

        return Convert.ToBase64String(hash);
    }

    public void RegisterNodes(List<string> nodes)
    {
        foreach (var node in nodes)
        {
            var url = new Uri(node);
            this.BlockChain.Nodes.Add(url); 
        }
    }

    public async Task<bool> ValidChainAsync(List<Block> chain)
    {
        var idx = 1;
        var lastBlock = chain.First();

        while (idx < chain.Count)
        {
            var block = chain.ElementAt(idx);

            if (block.PreviousHash != await HashBlockAsync(block))
            {
                return false;
            }

            if (!await ValidProofAsync(lastBlock.Proof, block.Proof))
            {
                return false;
            }

            lastBlock = block;
            idx += 1;
        }

        return true;
    }

    public async Task ResolveConflictsAsync()
    {
        int maxLength = this.BlockChain.Chain.Count;

        foreach (var node in this.BlockChain.Nodes)
        {
            if (node != null)
            {
                var newChain = await httpService.GetBlocksAsync(node);
                
                if (newChain != null && await ValidChainAsync(newChain) && newChain.Count() > maxLength)
                {
                    this.BlockChain.Chain = newChain;
                    maxLength = newChain.Count;
                }                
            }
            
        }
    }

    public async Task<Block> MineAsync()
    {
        var lastProof = 0;
        var lastBlock = LastBlock();

        if (lastBlock != null)
        {
            lastProof = lastBlock.Proof;
        }

        var proof = await ProofOfWorkAsync(lastProof);
        var index = CreateNewTransaction(new Transaction("0", NodeId.ToString(), 1));            
        var previousHash = await HashBlockAsync(lastBlock ?? this.BlockChain.Chain.First());
        var block = await CreateNewBlockAsync(proof, previousHash);

        return block;
    }
}