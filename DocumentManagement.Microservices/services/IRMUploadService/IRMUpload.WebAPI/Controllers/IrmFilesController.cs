using IRMUpload.Application.Commands;
using IRMUpload.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace IRMUpload.WebAPI.Controllers;

[ApiController]
[Route("api/irm/files")]
public class IrmFilesController : ControllerBase
{
    private readonly IrmFileCommandHandlers _commands;
    private readonly GetIrmFileByIdQueryHandler _queries;

    public IrmFilesController(IrmFileCommandHandlers commands, GetIrmFileByIdQueryHandler queries)
    {
        _commands = commands;
        _queries = queries;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIrmFileRequest request, CancellationToken cancellationToken)
    {
        var fileId = request.FileId == Guid.Empty ? Guid.NewGuid() : request.FileId;
        var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString("N");

        await _commands.Handle(new CreateIrmFileCommand(fileId, request.FileName, request.StoragePath, request.RequestedBy, correlationId), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { fileId }, new { fileId, correlationId });
    }

    [HttpGet("{fileId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid fileId, CancellationToken cancellationToken)
    {
        var result = await _queries.Handle(new GetIrmFileByIdQuery(fileId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{fileId:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid fileId, [FromBody] UpdateIrmFileRequest request, CancellationToken cancellationToken)
    {
        var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
        await _commands.Handle(new UpdateIrmFileCommand(fileId, request.FileName, request.RequestedBy, correlationId), cancellationToken);
        return Accepted(new { fileId, correlationId });
    }

    [HttpDelete("{fileId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid fileId, [FromBody] DeleteIrmFileRequest request, CancellationToken cancellationToken)
    {
        var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
        await _commands.Handle(new DeleteIrmFileCommand(fileId, request.RequestedBy, correlationId), cancellationToken);
        return Accepted(new { fileId, correlationId });
    }
}

public record CreateIrmFileRequest(Guid FileId, string FileName, string StoragePath, string RequestedBy);
public record UpdateIrmFileRequest(string FileName, string RequestedBy);
public record DeleteIrmFileRequest(string RequestedBy);
