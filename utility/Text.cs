
using System.Text.RegularExpressions;

static class TextUtil
{

    static readonly Regex NumericSupplantRegex = new Regex(@"{(?<index>\d+)}");
    public static string Supplant<T>(this string source, T[] values)
    {
        return NumericSupplantRegex.Replace(source, match =>
             values[int.Parse(match.Groups["index"].Value)].ToString());
    }

}