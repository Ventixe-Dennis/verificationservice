using System.ComponentModel.DataAnnotations;

namespace Presentation.Models;

public class VerifyCodeRequest
{
    [Required]
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
}
