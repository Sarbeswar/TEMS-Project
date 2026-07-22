using System.Text.Json;
using AIAgent.Application.Commands;
using AIAgent.Application.DTOs;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIAgent.Infrastructure.Kafka;

/// <summary>Background service that consumes DocumentUploaded events from IRMUpload via Kafka.</summary>
public sealed class DocumentUploadedKafkaConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DocumentUploadedKafkaConsumer> _logger;

    public DocumentUploadedKafkaConsumer(IConsumer<string, string> consumer, IServiceScopeFactory scopeFactory, ILogger<DocumentUploadedKafkaConsumer> logger)
    {
        _consumer = consumer;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>Continuously consumes upload events and delegates processing to scoped MediatR command handlers.</summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("irm.document.uploaded.v1");
        while (!stoppingToken.IsCancellationRequested)
        {
            var result = _consumer.Consume(stoppingToken);
            var message = JsonSerializer.Deserialize<DocumentUploadedEvent>(result.Message.Value);
            if (message is null) continue;

            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new ProcessDocumentUploadedCommand(message), stoppingToken);
            _consumer.Commit(result);
            _logger.LogInformation("Consumed DocumentUploaded for document {DocumentId}", message.DocumentId);
        }
    }
}
