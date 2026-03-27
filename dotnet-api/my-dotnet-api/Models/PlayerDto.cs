namespace MyDotNetApi.Models;

public class PlayerDto
{
    /// <summary>
    /// Player identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Player display name
    /// </summary>
    public string Name { get; set; } = null!;
}
