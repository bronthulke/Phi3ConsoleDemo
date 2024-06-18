using Microsoft.SemanticKernel;
using System.Text;
using Microsoft.SemanticKernel.ChatCompletion;
using Phi3SkConsoleApp.Plugins;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Configuration;

#pragma warning disable SKEXP0010

ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
IConfiguration configuration = configurationBuilder.AddUserSecrets<Program>().Build();

// Initialize the Semantic kernel
var kernelBuilder = Kernel.CreateBuilder();

// With Azure Open AI
var modelId = "gpt-35-turbo-16k";
var apiKey = configuration["AzureOpenAiAPIKey"];
var endpoint = "https://learnaibron.openai.azure.com/";
kernelBuilder.AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);

// Add the plugins
kernelBuilder.Plugins.AddFromType<AuthorEmailPlanner>();
kernelBuilder.Plugins.AddFromType<EmailSenderPlugin>();

var kernel = kernelBuilder.Build();

// Create a new chat
var ai = kernel.GetRequiredService<IChatCompletionService>();

ChatHistory chat = new("""
You are a friendly assistant who likes to follow the rules. You will complete required steps
and request approval before taking any consequential actions. If the user doesn't provide
enough information for you to complete a task, you will keep asking questions until you have
enough information to complete the task. You always try to respond with with suggestions that
are less than 500 characters in length.
""");

StringBuilder stringBuilder = new();

// User question & answer loop
while (true)
{
    stringBuilder.Clear();

    Console.WriteLine();
    Console.WriteLine("Your request: ");
    string request = Console.ReadLine()!;

    // Enable auto function calling
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    // Something else, so let's pass it on to the chat completion service
    // Get the AI response streamed back to the console
    chat.AddUserMessage(request);
    stringBuilder.Clear();

    Console.WriteLine("Assistant: ");
    await foreach (var message in ai.GetStreamingChatMessageContentsAsync(
      chat,
      kernel: kernel,
      executionSettings: openAIPromptExecutionSettings))
    {
        Console.Write(message);
        stringBuilder.Append(message.Content);
    }
    chat.AddAssistantMessage(stringBuilder.ToString());

    Console.WriteLine();
}

#pragma warning restore SKEXP0010
