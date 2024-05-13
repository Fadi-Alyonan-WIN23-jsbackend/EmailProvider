namespace EmailProvider.Models;

public class EmailRequest
{
    public string to { get; set; } = null!;
    public string subject { get; set; } = null!;
    public string HTMLbody { get; set; } = null!;
    public string Text { get; set; } = null!;
}
