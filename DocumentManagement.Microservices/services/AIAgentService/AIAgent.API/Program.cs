using AIAgent.API.Middleware;
using AIAgent.Application.Behaviors;
using AIAgent.Application.Commands;
using AIAgent.Infrastructure.DependencyInjection;
using FluentValidation;
using MediatR;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog writes structured logs that can be collected by Splunk in AKS.
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options => builder.Configuration.Bind("Jwt", options));
builder.Services.AddAuthorization();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ProcessDocumentUploadedCommand>());
builder.Services.AddValidatorsFromAssemblyContaining<ProcessDocumentUploadedCommand>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddAiAgentInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
