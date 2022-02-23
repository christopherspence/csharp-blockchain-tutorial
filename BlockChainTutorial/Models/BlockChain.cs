namespace BlockChainTutorial.Models;

public class BlockChain
{
    public List<Block> Chain { get; set; }
    public List<Uri> Nodes { get; private set; }
    public List<Transaction> CurrentTransactions { get; set; }

    public BlockChain()
    {
        Chain = new List<Block>();
        Nodes = new List<Uri>();
        CurrentTransactions = new List<Transaction>();
    }
}