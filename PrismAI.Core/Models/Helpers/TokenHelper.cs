using Tiktoken;

namespace PrismAI.Core.Models.Helpers;

public static class TokenHelper
{
    private static Encoder _encoding = ModelToEncoder.For("gpt-4o");
    public static int GetTokens(string text)
    {
        return _encoding.CountTokens(text);
    }
}