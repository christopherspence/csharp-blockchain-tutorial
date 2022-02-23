using System.Text;

namespace BlockChainTutorial.Extensions;

public static class StringExtensions
{
    public static string ToBase64String(this string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        return Convert.ToBase64String(bytes);
    }
}
