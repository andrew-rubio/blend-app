using Blend.Api.Models;
using Blend.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blend.Api.Controllers;

[ApiController]
[Route("api/v1/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly ILogger<MediaController> _logger;

    public MediaController(IMediaService mediaService, ILogger<MediaController> logger)
    {
        _mediaService = mediaService;
        _logger = logger;
    }

    /// <summary>Generates a write-only SAS URL for direct client upload to blob storage.</summary>
    [HttpPost("upload-url")]
    [ProducesResponseType(typeof(MediaUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUploadUrl([FromBody] MediaUploadRequest request)
    {
        var correlationId = GetOrCreateCorrelationId();
        Response.Headers["X-Correlation-Id"] = correlationId;

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var response = await _mediaService.GetUploadUrlAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid upload request. CorrelationId={CorrelationId}", correlationId);
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Request");
        }
    }

    /// <summary>Confirms a client upload is complete and triggers image processing.</summary>
    [HttpPost("upload-complete")]
    [ProducesResponseType(typeof(UploadCompleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadComplete([FromBody] UploadCompleteRequest request)
    {
        var correlationId = GetOrCreateCorrelationId();
        Response.Headers["X-Correlation-Id"] = correlationId;

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var response = await _mediaService.CompleteUploadAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid upload-complete request. CorrelationId={CorrelationId}", correlationId);
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Request");
        }
    }

    private string GetOrCreateCorrelationId()
    {
        if (Request.Headers.TryGetValue("X-Correlation-Id", out var existing) && !string.IsNullOrWhiteSpace(existing))
            return existing.ToString();

        return Guid.NewGuid().ToString();
    }
}
