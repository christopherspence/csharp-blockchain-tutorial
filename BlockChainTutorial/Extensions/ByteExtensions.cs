namespace BlockChainTutorial.Extensions;

public static class ByteExtensions
{
    public static string Print(this byte[] array)
    {
        var str = string.Empty;
        for (var i = 0; i < array.Length; i++) 
        {
            str += $"{array[i]:X2}";
            if ((i % 4) == 3) {
                str += " ";
            }
        }
        return str;
    }
}