using AIChatApp.Components;
using AIChatApp.Model;
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Add support for a local configuration file, which doesn't get committed to source control
builder.Configuration.Sources.Insert(0, new JsonConfigurationSource { Path = "appsettings.Local.json", Optional = true });

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

// Configure AI related features
builder.Services.AddKernel();

var aiHost = builder.Configuration["AIHost"];
if (String.IsNullOrEmpty(aiHost))
{
    aiHost = "OpenAI";
}

switch (aiHost) {
    case "github":
        #pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        builder.Services.AddAzureAIInferenceChatCompletion(
            modelId: builder.Configuration["GITHUB_MODEL_NAME"],
            builder.Configuration["GITHUB_TOKEN"],
            new Uri("https://models.inference.ai.azure.com"));
        break;
    case "azureAIModelCatalog":
        builder.Services.AddAzureAIInferenceChatCompletion(
            builder.Configuration["AZURE_MODEL_NAME"],
            builder.Configuration["AZURE_INFERENCE_KEY"],
            new Uri(builder.Configuration["AZURE_MODEL_ENDPOINT"]!));
        break;
    case "local":
        builder.Services.AddOllamaChatCompletion(
            modelId: builder.Configuration["LOCAL_MODEL_NAME"],
            endpoint: new Uri(builder.Configuration["LOCAL_ENDPOINT"]!));
        break;
    default:
        builder.Services.AddAzureOpenAIChatCompletion(builder.Configuration["AZURE_OPENAI_DEPLOYMENT"]!,
            builder.Configuration["AZURE_OPENAI_ENDPOINT"]!,
            "357517bc2dd6453eaf6393996c912433");
        break;
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();