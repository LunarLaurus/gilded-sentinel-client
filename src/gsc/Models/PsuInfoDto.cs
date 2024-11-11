public class PsuInfoDto
{
    public string Name { get; set; }
    public Dictionary<string, float> Power { get; set; } = new Dictionary<string, float>();
    public Dictionary<string, float> Voltage { get; set; } = new Dictionary<string, float>();
}
