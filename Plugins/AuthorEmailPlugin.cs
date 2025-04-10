using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.ComponentModel;

namespace Phi3SkConsoleApp.Plugins;

public class AuthorEmailPlugin
{
    private readonly IConfiguration _configuration;

    public AuthorEmailPlugin(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [KernelFunction("author_email_generate_required_steps")]
    [Description("Get the required steps required to author an email.")]
    [return: Description("The list of steps needed to author an email")]
    public async Task<string> GenerateRequiredStepsAsync(
        Kernel kernel,
        [Description("A 2-3 sentence description of what the email should be about")] string topic,
        [Description("A description of the recipients")] string recipients
    )
    {
        // Prompt the LLM to generate a list of steps to complete the task
        var result = await kernel.InvokePromptAsync($"""
        I'm going to write an email to {recipients} about {topic} on behalf of a user.
        Before I do that, can you succinctly recommend the top 3 steps I should take in a numbered list?
        I want to make sure I don't forget anything that would help my user's email sound more professional.
        """, new() {
            { "topic", topic },
            { "recipients", recipients }
        });

        // Return the plan back to the agent
        return result.ToString();
    }

    [KernelFunction("send_email")]
    [Description(@"You are a plugin responsible for sending emails. Do not assume or create email addresses. If an email address is not explicitly provided,
    ask for it. For example: 'What is the email address for Jody?'. You will actually send the email in the end, so you will be careful not to have any placeholders
    still in the body or subject when this is called such as '[Your name]'. This plugin sends an email to specified recipients. If no email address is provided, or if the address seems
    incomplete or invalid, ask for clarification. Ensure the email address is formatted correctly before proceeding.")]
    [return: Description("The status of the email sent")]
    public async Task<object> SendEmailAsync(
        Kernel kernel,
        [Description("Semicolon delimitated list of emails of the recipients (required)")] string recipientEmails,
        [Description("The subject that will be used for the email (required)")] string subject,
        [Description("The body that is used as the main message, in plain text, that will be used for the email (required)")] string body
    )
    {
        var recipients = recipientEmails.Split(';');
        if (recipients.Any(e => string.IsNullOrWhiteSpace(e)))
        {
            return "No recipient email provided. Please specify the recipient's email address.";
        }
        else if (recipients.Any(e => e.EndsWith("@example.com")))
        {
            return "The email address appears to be a placeholder (e.g., 'example.com'). Please provide a valid email.";
        }

        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
        {
            return "The email subject or body is empty. Please provide a subject and body for the email.";
        }

        // Get the SendGrid API key from the configuration
        string apiKey = _configuration["SendGridApiKey"] ?? string.Empty;

        // Create a SendGrid client
        var client = new SendGridClient(apiKey);

        // Create the email message
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(_configuration["FromEmail"], _configuration["FromName"]),
            Subject = subject,
            PlainTextContent = body,
            HtmlContent = $"<p>{body}</p>"
        };

        // Add the recipients
        foreach (var recipient in recipients)
        {
            msg.AddTo(new EmailAddress(recipient));
        }

        // Send the email
        var response = await client.SendEmailAsync(msg);

        // Check the response status
        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
        {
            Console.WriteLine($"Email sent to [{String.Join(", ", recipientEmails)}]! Woot woot!");

            return new { status = "success" };
        }
        else
        {
            var responseBody = await response.Body.ReadAsStringAsync(); // <-- read the response body here

            Console.WriteLine("Error sending email! - " + responseBody);

            return new { status = "error", message = responseBody };
        }
    }
}
