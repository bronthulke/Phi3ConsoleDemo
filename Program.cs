using Microsoft.SemanticKernel;
using System.Text;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0010

// Initialize the Semantic kernel
var kernelBuilder = Kernel.CreateBuilder();

// With Ollama
// string modelId = "phi3";
// string apiKey = null;
// string endpoint = "http://localhost:11434";

// With LM Studio
string modelId = "phi3";
string apiKey = null;
string endpoint = "http://localhost:1234";

// We use Semantic Kernel OpenAI API
kernelBuilder.AddOpenAIChatCompletion(modelId, new Uri(endpoint), apiKey);

var kernel = kernelBuilder.Build();

// Create a new chat
var ai = kernel.GetRequiredService<IChatCompletionService>();

ChatHistory chat = new("You are an AI assistant that helps people find information.");

StringBuilder stringBuilder = new();

// User question & answer loop
while (true)
{
    Console.Write("Question: ");
    chat.AddUserMessage(Console.ReadLine()!);
    
    stringBuilder.Clear();

    Console.Write("Assistant: ");
    await foreach (var message in ai.GetStreamingChatMessageContentsAsync(
        chat,
        kernel: kernel))
    {
        Console.Write(message);
        stringBuilder.Append(message.Content);
    }
    chat.AddAssistantMessage(stringBuilder.ToString());

    Console.WriteLine();
}

#pragma warning restore SKEXP0010
