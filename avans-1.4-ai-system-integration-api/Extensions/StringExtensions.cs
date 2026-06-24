namespace avans_1._4_ai_system_integration_api.Extensions;

public static class StringExtensions
{
    public static string ToTitleCaseWithUnderscores(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var chars = input.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            // eerste teken, of het teken direct na een underscore
            if (i == 0 || chars[i - 1] == '_')
                chars[i] = char.ToUpperInvariant(chars[i]);
        }

        return new string(chars);
    }
}