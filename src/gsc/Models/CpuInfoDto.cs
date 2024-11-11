public record CpuInfoDto
{
    public Dictionary<int, CpuDataDto> CpuDataBySocket { get; set; } = new();
    public int CpuCount { get; set; }
    public int CpuCoreCount { get; set; }

}