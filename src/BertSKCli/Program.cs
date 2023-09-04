// See https://aka.ms/new-console-template for more information

using BertCppLib;
using BertCppLib.SemanticKernel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Memory;
string MemoryCollectionName = "SKGitHub";
Console.Write("Please input your model path: ");
var modelPath = Console.ReadLine();

BertCppInterop.bert_params_parse(new[] { "-t", $"8", "-p", "Can I build a chat with SK?", "-m", modelPath }, out var bparams);
var embed = new BertEmbeddingGeneration(modelPath, bparams);

var kernel = Kernel.Builder
    .WithAIService<ITextEmbeddingGeneration>("local-llama-embed", embed, true)
    .WithMemoryStorage(new VolatileMemoryStore())
    .Build();
    
var data = new Dictionary<string, string>
    {
        ["https://github.com/microsoft/semantic-kernel/blob/main/README.md"]
            = "README: Installation, getting started, and how to contribute",
        ["https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks/02-running-prompts-from-file.ipynb"]
            = "Jupyter notebook describing how to pass prompts from a file to a semantic skill or function",
        ["https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks//00-getting-started.ipynb"]
            = "Jupyter notebook describing how to get started with the Semantic Kernel",
        ["https://github.com/microsoft/semantic-kernel/tree/main/samples/skills/ChatSkill/ChatGPT"]
            = "Sample demonstrating how to create a chat skill interfacing with ChatGPT",
        ["https://github.com/microsoft/semantic-kernel/blob/main/dotnet/src/SemanticKernel/Memory/VolatileMemoryStore.cs"]
            = "C# class that defines a volatile embedding store",
        ["https://github.com/microsoft/semantic-kernel/blob/main/samples/dotnet/KernelHttpServer/README.md"]
            = "README: How to set up a Semantic Kernel Service API using Azure Function Runtime v4",
        ["https://github.com/microsoft/semantic-kernel/blob/main/samples/apps/chat-summary-webapp-react/README.md"]
            = "README: README associated with a sample chat summary react-based webapp",
    };
    
Console.WriteLine("\nAdding some GitHub file URLs and their descriptions to the semantic memory.");
var i2 = 0;
foreach (var entry in data)
{
    var result = await kernel.Memory.SaveReferenceAsync(
        collection: MemoryCollectionName,
        externalSourceName: "GitHub",
        externalId: entry.Key,
        description: entry.Value,
        text: entry.Value);

    Console.WriteLine($"#{++i2} saved.");
    Console.WriteLine(result);
}

Console.WriteLine("\n----------------------");

Console.WriteLine("\nQuery: " + bparams.prompt + "\n");

var memories = kernel.Memory.SearchAsync(MemoryCollectionName, bparams.prompt, limit: 10, minRelevanceScore: 0.5);

int i = 0;
await foreach (MemoryQueryResult memory in memories)
{
    Console.WriteLine($"Result {++i}:");
    Console.WriteLine("  URL:     : " + memory.Metadata.Id);
    Console.WriteLine("  Title    : " + memory.Metadata.Description);
    Console.WriteLine("  Relevance: " + memory.Relevance);
    Console.WriteLine();
}

Console.WriteLine("----------------------");