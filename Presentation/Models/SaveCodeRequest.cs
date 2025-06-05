namespace Presentation.Models;

public class SaveCodeRequest
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
    public TimeSpan ValidFor { get; set; }
}
