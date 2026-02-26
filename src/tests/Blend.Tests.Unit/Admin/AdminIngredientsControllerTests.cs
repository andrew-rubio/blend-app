using System.Security.Claims;
using Blend.Api.Controllers.Admin;
using Blend.Api.Models.Admin;
using Blend.Api.Services;
using Blend.Domain.Entities;
using Blend.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Blend.Tests.Unit.Admin;

/// <summary>
/// Unit tests for AdminIngredientsController actions.
/// </summary>
public class AdminIngredientsControllerTests
{
    private readonly Mock<IRepository<IngredientSubmission>> _submissionsRepoMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<IKnowledgeBaseService> _kbServiceMock = new();
    private readonly AdminIngredientsController _controller;

    public AdminIngredientsControllerTests()
    {
        _controller = new AdminIngredientsController(
            _submissionsRepoMock.Object,
            _notificationServiceMock.Object,
            _kbServiceMock.Object,
            NullLogger<AdminIngredientsController>.Instance);

        SetAdminUser();
    }

    [Fact]
    public async Task GetSubmissions_ReturnsOk_WithAllSubmissions()
    {
        var paged = new PagedResult<IngredientSubmission>
        {
            Items = new List<IngredientSubmission>
            {
                new() { Id = "s1", IngredientName = "Turmeric", SubmissionStatus = IngredientSubmissionStatus.Pending }
            }
        };
        _submissionsRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<string>(),
                It.IsAny<PaginationOptions?>(),
                It.IsAny<IDictionary<string, object>?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _controller.GetSubmissions();

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PagedIngredientSubmissionsResponse>(ok.Value);
        Assert.Single(response.Items);
    }

    [Fact]
    public async Task GetSubmissions_ReturnsBadRequest_ForInvalidStatus()
    {
        var result = await _controller.GetSubmissions(status: "invalid");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetSubmissions_FiltersBy_ValidStatus()
    {
        _submissionsRepoMock
            .Setup(r => r.QueryAsync(
                It.Is<string>(q => q.Contains("@status")),
                It.IsAny<PaginationOptions?>(),
                It.IsAny<IDictionary<string, object>?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<IngredientSubmission>());

        var result = await _controller.GetSubmissions(status: "pending");

        Assert.IsType<OkObjectResult>(result);
        _submissionsRepoMock.Verify(
            r => r.QueryAsync(
                It.Is<string>(q => q.Contains("@status")),
                It.IsAny<PaginationOptions?>(),
                It.IsAny<IDictionary<string, object>?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ApproveSubmission_ReturnsNotFound_WhenMissing()
    {
        _submissionsRepoMock
            .Setup(r => r.GetByIdAsync("missing", "ingredient-submission", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngredientSubmission?)null);

        var result = await _controller.ApproveSubmission("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ApproveSubmission_ReturnsConflict_WhenAlreadyReviewed()
    {
        var submission = new IngredientSubmission
        {
            Id = "s1",
            IngredientName = "Turmeric",
            SubmissionStatus = IngredientSubmissionStatus.Approved
        };
        _submissionsRepoMock
            .Setup(r => r.GetByIdAsync("s1", "ingredient-submission", It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        var result = await _controller.ApproveSubmission("s1");

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task ApproveSubmission_SetsApprovedStatus_AndNotifiesUser()
    {
        var submission = new IngredientSubmission
        {
            Id = "s1",
            IngredientName = "Turmeric",
            SubmittedByUserId = "user-1",
            SubmissionStatus = IngredientSubmissionStatus.Pending
        };
        _submissionsRepoMock
            .Setup(r => r.GetByIdAsync("s1", "ingredient-submission", It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);
        _submissionsRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<IngredientSubmission>(), "ingredient-submission", It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        var result = await _controller.ApproveSubmission("s1");

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(IngredientSubmissionStatus.Approved, submission.SubmissionStatus);
        _kbServiceMock.Verify(
            k => k.AddIngredientAsync("Turmeric", It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _notificationServiceMock.Verify(
            n => n.SendIngredientApprovedAsync("user-1", "Turmeric", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RejectSubmission_SetsRejectedStatus_WithReason_AndNotifiesUser()
    {
        var submission = new IngredientSubmission
        {
            Id = "s2",
            IngredientName = "Widget",
            SubmittedByUserId = "user-2",
            SubmissionStatus = IngredientSubmissionStatus.Pending
        };
        _submissionsRepoMock
            .Setup(r => r.GetByIdAsync("s2", "ingredient-submission", It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);
        _submissionsRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<IngredientSubmission>(), "ingredient-submission", It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        var request = new RejectSubmissionRequest { Reason = "Not a real ingredient." };
        var result = await _controller.RejectSubmission("s2", request);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(IngredientSubmissionStatus.Rejected, submission.SubmissionStatus);
        Assert.Equal("Not a real ingredient.", submission.RejectionReason);
        _notificationServiceMock.Verify(
            n => n.SendIngredientRejectedAsync("user-2", "Widget", "Not a real ingredient.", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RejectSubmission_ReturnsConflict_WhenAlreadyReviewed()
    {
        var submission = new IngredientSubmission
        {
            Id = "s3",
            IngredientName = "Test",
            SubmissionStatus = IngredientSubmissionStatus.Rejected
        };
        _submissionsRepoMock
            .Setup(r => r.GetByIdAsync("s3", "ingredient-submission", It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        var result = await _controller.RejectSubmission("s3", new RejectSubmissionRequest());

        Assert.IsType<ConflictObjectResult>(result);
    }

    private void SetAdminUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "admin-1"), new Claim(ClaimTypes.Role, "admin") },
            "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }
}
