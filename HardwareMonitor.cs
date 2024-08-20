using System;
using System.Collections.Generic;
using LibreHardwareMonitor.Hardware;

public class HardwareMonitor
{
    private readonly Computer _computer;

    public HardwareMonitor()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true
        };
        _computer.Open();
    }

    // CPU Info Class
    public class CPUInfoDto
    {
        public Dictionary<int, CPUData> CpuDataBySocket { get; set; } = new();
        public int CpuCount { get; set; }
        public int CpuCoreCount { get; set; }
        public class CPUData
        {
            public Dictionary<string, float?> Temperatures { get; set; } = new();
            public Dictionary<string, float?> Load { get; set; } = new();
            public Dictionary<string, float?> ClockSpeeds { get; set; } = new();
        }
    }

    // Memory State Class
    public class MemoryStateDto
    {
        public float? Used { get; set; }
        public float? Available { get; set; }
        public float? Load { get; set; }
    }

    // GPU Info Class
    public class GPUInfoDto
    {
        public string Name { get; set; }
        public Dictionary<string, float?> Temperatures { get; set; } = new();
        public Dictionary<string, float?> Load { get; set; } = new();
        public Dictionary<string, float?> ClockSpeeds { get; set; } = new();
        public Dictionary<string, float?> MemoryUsage { get; set; } = new();
    }

    // Get CPU Data
    public CPUInfoDto GetCPUInfo()
    {
        var cpuInfo = new CPUInfoDto();
        int cpuCountLocal = 0;
        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Cpu)
            {
                hardware.Update();
                var socketName = hardware.Identifier.ToString();

                if (!cpuInfo.CpuDataBySocket.ContainsKey(cpuCountLocal))
                {
                    cpuInfo.CpuDataBySocket[cpuCountLocal] = new CPUInfoDto.CPUData();
                }

                foreach (var sensor in hardware.Sensors)
                {
                    var cpuData = cpuInfo.CpuDataBySocket[cpuCountLocal];
                    switch (sensor.SensorType)
                    {
                        case SensorType.Temperature:
                            cpuData.Temperatures[sensor.Name] = sensor.Value;
                            break;
                        case SensorType.Load:
                            cpuData.Load[sensor.Name] = sensor.Value;
                            break;
                        case SensorType.Clock:
                            cpuData.ClockSpeeds[sensor.Name] = sensor.Value;
                            break;
                    }
                }
                cpuCountLocal++;
            }
        }
        cpuInfo.CpuCount = cpuCountLocal;
        cpuInfo.CpuCoreCount = GetCpuCoreCount();
        return cpuInfo;
    }

    public int cpuCoreCount = -1;
    public int GetCpuCoreCount()
    {
        if (cpuCoreCount > 0){
            return cpuCoreCount;
        }
        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Cpu)
            {
                hardware.Update();

                // Count cores based on sensors named "CPU Core #X"
                cpuCoreCount = hardware.Sensors
                    .Where(sensor => sensor.SensorType == SensorType.Temperature && sensor.Name.StartsWith("CPU Core #"))
                    .Count();

                // Since all CPUs have the same core count, we can break after the first CPU
                break;
            }
        }
        return cpuCoreCount;
    }


    // Get Memory Data
    public MemoryStateDto GetMemoryState()
    {
        var memoryState = new MemoryStateDto();

        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Memory)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    switch (sensor.SensorType)
                    {
                        case SensorType.Data:
                            if (sensor.Name.Contains("Used"))
                                memoryState.Used = sensor.Value;
                            else if (sensor.Name.Contains("Available"))
                                memoryState.Available = sensor.Value;
                            break;
                        case SensorType.Load:
                            memoryState.Load = sensor.Value;
                            break;
                    }
                }
            }
        }

        return memoryState;
    }

    // Get GPU Data
    public List<GPUInfoDto> GetGPUInfo()
    {
        var gpuInfoList = new List<GPUInfoDto>();

        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuIntel)
            {
                var gpuInfo = new GPUInfoDto
                {
                    Name = hardware.Name
                };

                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    switch (sensor.SensorType)
                    {
                        case SensorType.Temperature:
                            gpuInfo.Temperatures[sensor.Name] = sensor.Value;
                            break;
                        case SensorType.Load:
                            gpuInfo.Load[sensor.Name] = sensor.Value;
                            break;
                        case SensorType.Clock:
                            gpuInfo.ClockSpeeds[sensor.Name] = sensor.Value;
                            break;
                        case SensorType.SmallData:
                            gpuInfo.MemoryUsage[sensor.Name] = sensor.Value;
                            break;
                    }
                }

                gpuInfoList.Add(gpuInfo);
            }
        }

        return gpuInfoList;
    }

    // Get Motherboard Name
    public string GetMotherboardName()
    {
        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Motherboard)
            {
                hardware.Update();
                return hardware.Name;
            }
        }
        return "Unknown";
    }

    public void Close()
    {
        _computer.Close();
    }
}
