using AIAgent.Application.Abstractions;
using AIAgent.Domain.Repositories;
using AIAgent.Infrastructure.BlobStorage;
using AIAgent.Infrastructure.Configuration;
using AIAgent.Infrastructure.DataLookup;
using AIAgent.Infrastructure.Decision;
using AIAgent.Infrastructure.Kafka;
using AIAgent.Infrastructure.LLM;
using AIAgent.Infrastructure.OCR;
using AIAgent.Infrastructure.Persistence;
using AIAgent.Infrastructure.Prompts;
using AIAgent.Infrastructure.Risk;
using Azure.Storage.Blobs;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIAgent.Infrastructure.DependencyInjection;

/// <summary>Registers infrastructure adapters and keeps Program.cs small and readable.</summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>Adds SQL Server, Kafka, Blob Storage, AI, DataLookup, and business service implementations.</summary>
    public static IServiceCollection AddAiAgentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("AiAgent").Get<AiAgentOptions>() ?? new AiAgentOptions();
        services.Configure<AiAgentOptions>(configuration.GetSection("AiAgent"));

        services.AddDbContext<AiAgentDbContext>(db => db.UseSqlServer(options.SqlConnectionString));
        services.AddSingleton(new BlobServiceClient(options.BlobConnectionString));
        services.AddSingleton<IProducer<string, string>>(_ => new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = options.KafkaBootstrapServers }).Build());
        services.AddSingleton<IConsumer<string, string>>(_ => new ConsumerBuilder<string, string>(new ConsumerConfig { BootstrapServers = options.KafkaBootstrapServers, GroupId = "ai-agent-service", EnableAutoCommit = false, AutoOffsetReset = AutoOffsetReset.Earliest }).Build());

        services.AddScoped<IAiDocumentProcessingJobRepository, AiDocumentProcessingJobRepository>();
        services.AddScoped<IBlobStorageClient, AzureBlobStorageClient>();
        services.AddScoped<IOcrService, AzureOcrService>();
        services.AddScoped<IPromptBuilder, PromptBuilder>();
        services.AddScoped<ILlmService, AzureOpenAiLlmService>();
        services.AddScoped<IMetadataValidator, MetadataValidator>();
        services.AddScoped<IRiskAnalyzer, RiskAnalyzer>();
        services.AddScoped<IDecisionEngine, DecisionEngine>();
        services.AddScoped<IAiEventPublisher, AiKafkaEventPublisher>();
        services.AddHttpClient<IDataLookupClient, DataLookupClient>(client => client.BaseAddress = new Uri(options.DataLookupBaseUrl));
        services.AddHostedService<DocumentUploadedKafkaConsumer>();

        return services;
    }
}
