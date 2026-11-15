

using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Agents.AI;
using OpenAI.Responses;

#pragma warning disable OPENAI001

Console.OutputEncoding = System.Text.Encoding.UTF8; // Ensure proper encoding for console output, especially for special characters.

var configuration = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"))
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var model = configuration["OpenAI:Model"] ?? throw new InvalidOperationException("Set OpenAI:Model in appsettings.json or your environment.");
var endpoint = configuration["OpenAI:Endpoint"] ?? throw new InvalidOperationException("Set OpenAI:Endpoint in appsettings.json or your environment.");
var apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("Set OpenAI:ApiKey in appsettings.json or your environment.");

Console.WriteLine($"Model: {model}");
Console.WriteLine($"Endpoint: {endpoint}");

string prompt =
    """
    You are an AI assistant that can give tips about a map. 
    The map is provided as an image as part of the user prompt.
    You need to analyze the image and give information about concerts and venues that happen in this area in the upcoming 3 months.
    Provide information about the concerts in the chat and add location details so I can set pins on this map with an image editing model.
    """;

string imagePath = File.Exists(Path.Combine(AppContext.BaseDirectory, "SanDiego-Area.png"))
    ? Path.Combine(AppContext.BaseDirectory, "SanDiego-Area.png")
    : "SanDiego-Area.png";

byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);

var openAIClient = new OpenAI.OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAI.OpenAIClientOptions
    {
        Endpoint = new Uri(new Uri(endpoint), "openai/v1/")
    });

AIAgent agent = openAIClient.GetResponsesClient().AsAIAgent(
    options: new ChatClientAgentOptions
    {
        Name = "GloboTicketAssistant",
        ChatOptions = new ChatOptions
        {
            Instructions = prompt
        }
    },
    model: model);

var message = new ChatMessage(ChatRole.User,
[
    new TextContent("Here is the image of the San Diego area:"),
    new DataContent(imageBytes, "image/png")
]);

var result = await agent.RunAsync(message);
Console.WriteLine(result);

#pragma warning restore OPENAI001