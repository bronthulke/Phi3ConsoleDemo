using Microsoft.SemanticKernel;
using System.Text;
using Microsoft.SemanticKernel.ChatCompletion;
using Phi3SkConsoleApp.Plugins;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

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
        string prompt = $$"""
## Instructions
Provide the intent of the request using the following format:

{
    "intent": {intent}
}

Your entire response/output is going to consist of a single JSON object {}, and you will NOT wrap it within JSON MD markers

## Choices
You can choose between the following intents:

```json
["SendEmail", "Other"]
```

## User Input
The user input is:

```json
{
    "request": "{{request}}"
}
```

## Intent
""";

    KernelArguments arguments = new(new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" });

   // invoke the prompt to get back the intent of the request
    var promptResponse = await kernel.InvokePromptAsync<string>(prompt, arguments);

    // Just in case it's disobeyed our formatting instructions (I'm looking at you, LM Studio)
    // FFS even then it doesn't work properly - because sometimes they just add text at the end,
    // but don't include the markdown wrapper!!
    promptResponse = TrimMarkdown(promptResponse);

    var intentResponse = JsonSerializer.Deserialize<PromptIntentResponse>(promptResponse ?? "");

    if (intentResponse?.Intent == "SendEmail")
    {
        // I meawn sure, this is cheating and not using AI... but right now it's a simple way to test out the plugin itself
        // Ideally we would create loops to gather this information using the assistant to parse the information out of natural langauge
        Console.WriteLine("SendEmail");
        Console.Write("Recipient Emails: ");
        string recipientEmails = Console.ReadLine()!;
        Console.Write("Subject: ");
        string subject = Console.ReadLine()!;
        Console.Write("Body: ");
        string body = Console.ReadLine()!;

        await kernel.InvokeAsync("EmailSenderPlugin", "SendEmail", new() {
            { "recipientEmails", recipientEmails },
            { "subject", subject },
            { "body", body },
        });
    }
    // else if (intentResponse?.intent == "SendMessage")
    // {
    //     Console.WriteLine("SendMessage");
    // }
    // else if (intentResponse?.intent == "CompleteTask")
    // {
    //     Console.WriteLine("CompleteTask");
    // }
    // else if (intentResponse?.intent == "CreateDocument")
    // {
    //     Console.WriteLine("CreateDocument");
    // }
    else
    {
        // Something else, so let's pass it on to the chat completion service
        // Get the AI response streamed back to the console
        chat.AddUserMessage(request);
        stringBuilder.Clear();

        Console.WriteLine("Assistant: ");
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

}

string? TrimMarkdown(string? promptResponse)
{
    var result = promptResponse?.Trim();

    // based on experience, once we hit the closing markdown code block, ew can ignore the rest, which is usually
    // the AI's explanation of it's reasoning.
    if(result.StartsWith("```json"))
    {
        var indexOfClosingMarkdown = result.IndexOf("```", 7);
        result = result.Substring(7, indexOfClosingMarkdown - 7);
    }

    return result;
}

public class PromptIntentResponse
{
    [JsonPropertyName("intent")]
    public string Intent { get; set; }
}

#pragma warning restore SKEXP0010
