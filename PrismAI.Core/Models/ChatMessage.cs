using Microsoft.SemanticKernel.ChatCompletion;

namespace PrismAI.Core.Models;

public class ChatMessage
{
    public string Id { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
public static class ChatMessageExtensions
{
    public static ChatHistory ToChatHistory(this List<ChatMessage> messages)
    {
        var chatHistory = new ChatHistory();
        foreach (var message in messages)
        {
            if (message.Role == "user")
            {
                chatHistory.AddUserMessage(message.Content);
            }
            else if (message.Role == "assistant")
            {
                chatHistory.AddAssistantMessage(message.Content);
            }
        }
        return chatHistory;
    }
    public static void UpsertAssistantMessage(this List<ChatMessage> messages, string token)
    {
        if (messages.Count > 0 && messages[^1].Role == "assistant")
        {
            messages[^1].Content += token;
        }
        else
        {
            messages.Add(new ChatMessage { Role = "assistant", Content = token, Timestamp = DateTime.UtcNow});
        }

    }
}