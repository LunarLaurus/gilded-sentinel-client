public record SystemInfoDTO
{
    public int SystemArchitecture { get; set; }
    public string SystemPlatform { get; set; }
    public string SystemHostName { get; set; }
    public string SystemModelName { get; set; }
    public bool System64Bit { get; set; }
    public bool DotNetProcess64Bit { get; set; }
    public long SystemUptime { get; set; }
    public List<StorageDriveDto> SystemStorageInfo { get; set; }
    public CpuInfoDto CpuInfo { get; set; }
    public List<GpuInfoDto> Gpus { get; set; }
    public MemoryStateDto Memory { get; set; }
    public List<PsuInfoDto> PowerSupplies { get; set; }
    public string IpmiAddress { get; set; }

}