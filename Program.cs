using Microsoft.SemanticKernel;
using System.Text;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Plugins.Core;
using Phi3SkConsoleApp.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.DependencyInjection;
// using Microsoft.SemanticKernel.Connectors.Ollama;

ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
IConfiguration configuration = configurationBuilder
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

// Initialize the Semantic kernel
var kernelBuilder = Kernel.CreateBuilder();

kernelBuilder.Services.AddSingleton(configuration);

// With Azure Open AI
var modelId = "gpt-4o";
var apiKey = configuration["AzureOpenAiAPIKey"];
var endpoint = "https://ai-hubskdemo659887657047.openai.azure.com/";
kernelBuilder.AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);

// With LM Studio
// string modelId = "phi3";
// string apiKey = null;
// string endpoint = "http://localhost:1234";
// kernelBuilder.AddOpenAIChatCompletion(modelId, new Uri(endpoint), apiKey);

// With Ollama
// string modelId = "phi4-mini";
// string endpoint = "http://localhost:11434";
// kernelBuilder.AddOllamaChatCompletion(modelId, new Uri(endpoint));

// Add the plugins
kernelBuilder.Plugins
    .AddFromType<AuthorEmailPlugin>()
    .AddFromType<PersonalDetailsPlugin>()
    .AddFromType<PublicHolidaysPlugin>()
    .AddFromType<DateHelpers>()
    .AddFromType<TimePlugin>();
    // .AddFromType<MyTimePlugin>()
    // .AddFromObject(new MyLightPlugin(turnedOn: true))
    // .AddFromObject(new MyAlarmPlugin("11"));

var kernel = kernelBuilder.Build();

// Create a new chat
var ai = kernel.GetRequiredService<IChatCompletionService>();

ChatHistory chat = new("""
You are a friendly assistant responsible for helping with tasks as a personal assistant, like sending emails. You will complete required steps and request approval before taking any consequential actions.

If the user doesn't provide enough information to complete a task, you must ask follow-up questions until you have all necessary details. You are especially careful with emails:
1. Do not fabricate email addresses. 
2. If no email is provided, ask for clarification. For example: "What is Jody's email address?"
3. Do not proceed with incomplete or placeholder content in the email subject, body, or recipient field.

Always keep responses concise, under 200 characters where possible, as the user may not have much time.
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
    OpenAIPromptExecutionSettings settings = new()
    {
        // ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    };

    // var settings = new OllamaPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

    // Something else, so let's pass it on to the chat completion service
    // Get the AI response streamed back to the console
    chat.AddUserMessage(request);
    stringBuilder.Clear();

    Console.WriteLine("Assistant: ");
    await foreach (var message in ai.GetStreamingChatMessageContentsAsync(
      chat,
      kernel: kernel,
      executionSettings: settings))
    {
        Console.Write(message);
        stringBuilder.Append(message.Content);
    }

    // ChatMessageContent chatResult = await ai.GetChatMessageContentAsync(chat, settings, kernel);
    // Console.Write($"\n>>> Result: {chatResult}\n\n> ");

    chat.AddAssistantMessage(stringBuilder.ToString());

    Console.WriteLine();
}

