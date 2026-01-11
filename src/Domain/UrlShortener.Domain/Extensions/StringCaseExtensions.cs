namespace UrlShortener.Domain.Extensions;

public static class StringCaseExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var result = new System.Text.StringBuilder();
        result.Append(char.ToLowerInvariant(input[0]));
        for (int i = 1; i < input.Length; i++)
        {
            char c = input[i];
            if (char.IsUpper(c))
            {
                if (char.IsLower(input[i - 1]) || (i < input.Length - 1 && char.IsLower(input[i + 1])))
                    result.Append('_');
                result.Append(char.ToLowerInvariant(c));
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
}