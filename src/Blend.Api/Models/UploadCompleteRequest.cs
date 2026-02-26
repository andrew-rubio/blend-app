using System.ComponentModel.DataAnnotations;

namespace Blend.Api.Models;

public class UploadCompleteRequest
{
    [Required]
    public string BlobPath { get; set; } = "";

    [Required]
    public string EntityType { get; set; } = "";

    [Required]
    public Guid EntityId { get; set; }
}
