namespace BlockChainTutorial.Models;

public class Transaction
{
    public string Sender { get; private set; }
    public string Recipient { get; private set; }
    public int Amount { get; private set; }

    public Transaction(string sender, string recipient, int amount)
    {
        Sender = sender;
        Recipient = recipient;
        Amount = amount;
    }
}