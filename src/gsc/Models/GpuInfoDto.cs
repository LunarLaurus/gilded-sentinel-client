    public record GpuInfoDto
    {
        public string Name { get; set; }
        public Dictionary<string, float> Temperatures { get; set; } = new();
        public Dictionary<string, float> Load { get; set; } = new();
        public Dictionary<string, float> ClockSpeeds { get; set; } = new();
        public Dictionary<string, float> MemoryUsage { get; set; } = new();
    }