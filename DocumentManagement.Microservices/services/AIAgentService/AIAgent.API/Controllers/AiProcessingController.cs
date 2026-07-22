using AIAgent.Application.DTOs;
using AIAgent.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIAgent.API.Controllers;

/// <summary>HTTP API for ICMPAPI/dashboard to query AI processing status.</summary>
[ApiController]
[Authorize]
[Route("api/ai-processing")]
public sealed class AiProcessingController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiProcessingController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns current AI processing status for a document.</summary>
    [HttpGet("{documentId:guid}")]
    [ProducesResponseType(typeof(AiProcessingJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDocumentId(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAiProcessingJobQuery(documentId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
