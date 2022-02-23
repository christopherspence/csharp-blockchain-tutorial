namespace BlockChainTutorial.Models;

public class Block
{
    public int Index { get; private set; }
    public DateTime TimeStamp { get; private set; }
    public int Proof { get; private set; }
    public string PreviousHash { get; private set; }
    public List<Transaction> Transactions { get; private set; }

    public Block(int index, DateTime timeStamp, List<Transaction> transactions, int proof, string previousHash)
    {
        Index = index;
        TimeStamp = timeStamp;
        Transactions = transactions;
        Proof = proof;
        PreviousHash = previousHash;
    }
}