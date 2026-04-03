using System.ComponentModel.DataAnnotations;

namespace Ceng382_25_26_202211035.Models;

public record Thing(
    [Range(1, 10)] int? Id,
    [Required] string? Color,
    [EmailAddress] string? Email
);