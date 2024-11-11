using LibreHardwareMonitor.Hardware;

public interface IHardwareMonitor
{
    CpuInfoDto GetCpuInfo();
    MemoryStateDto GetMemoryState();
    List<GpuInfoDto> GetGpuInfo();
    string GetMotherboardName();
}

public class HardwareMonitor : IHardwareMonitor
{
    private readonly Computer _computer;

    public HardwareMonitor()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsPsuEnabled = true
        };
        _computer.Open();
    }

    #region CPU Information

    public CpuInfoDto GetCpuInfo()
    {
        var cpuInfo = new CpuInfoDto();
        int cpuCount = 0;

        foreach (var hardware in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Cpu))
        {
            hardware.Update();

            if (!cpuInfo.CpuDataBySocket.ContainsKey(cpuCount))
            {
                cpuInfo.CpuDataBySocket[cpuCount] = new CpuDataDto();
            }

            foreach (var sensor in hardware.Sensors)
            {
                var cpuData = cpuInfo.CpuDataBySocket[cpuCount];
                ProcessSensorData(sensor, cpuData);
            }

            cpuCount++;
        }

        cpuInfo.CpuCount = cpuCount;
        cpuInfo.CpuCoreCount = GetCpuCoreCount();
        return cpuInfo;
    }

    public int GetCpuCoreCount()
    {
        foreach (var hardware in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Cpu))
        {
            hardware.Update();
            var coreCount = hardware.Sensors.Count(s => s.SensorType == SensorType.Temperature && s.Name.StartsWith("CPU Core #"));
            return coreCount;
        }
        return 0;
    }

    #endregion

    #region Memory Information

    public MemoryStateDto GetMemoryState()
    {
        var memoryState = new MemoryStateDto();

        foreach (var hardware in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Memory))
        {
            hardware.Update();

            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.SensorType == SensorType.Data)
                {
                    if (sensor.Name.Contains("Used"))
                        memoryState.Used = sensor.Value.GetValueOrDefault();
                    else if (sensor.Name.Contains("Available"))
                        memoryState.Available = sensor.Value.GetValueOrDefault();
                }
                else if (sensor.SensorType == SensorType.Load)
                {
                    memoryState.Load = sensor.Value.GetValueOrDefault();
                }
            }
        }

        return memoryState;
    }

    #endregion

    #region GPU Information

    public List<GpuInfoDto> GetGpuInfo()
    {
        var gpuInfoList = new List<GpuInfoDto>();

        foreach (var hardware in _computer.Hardware.Where(h => h.HardwareType == HardwareType.GpuAmd ||
                                                                h.HardwareType == HardwareType.GpuNvidia ||
                                                                h.HardwareType == HardwareType.GpuIntel))
        {
            var gpuInfo = new GpuInfoDto { Name = hardware.Name };
            hardware.Update();

            foreach (var sensor in hardware.Sensors)
            {
                ProcessSensorData(sensor, gpuInfo);
            }

            gpuInfoList.Add(gpuInfo);
        }

        return gpuInfoList;
    }

    #endregion

    #region Motherboard Information

    public string GetMotherboardName()
    {
        var motherboard = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Motherboard);
        motherboard?.Update();
        return motherboard?.Name ?? "Unknown";
    }

    #endregion

    #region PSU Data

    // Method to Get PSU Data
    public List<PsuInfoDto> GetPsuInfo()
    {
        var psuInfoList = new List<PsuInfoDto>();

        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Psu)
            {
                var psuInfo = new PsuInfoDto
                {
                    Name = hardware.Name
                };

                hardware.Update();

                foreach (var sensor in hardware.Sensors)
                {
                    // Extract Power and Voltage info
                    switch (sensor.SensorType)
                    {
                        case SensorType.Power:
                            psuInfo.Power[sensor.Name] = sensor.Value.GetValueOrDefault();
                            break;
                        case SensorType.Voltage:
                            psuInfo.Voltage[sensor.Name] = sensor.Value.GetValueOrDefault();
                            break;
                    }
                }

                psuInfoList.Add(psuInfo);
            }
        }

        return psuInfoList;
    }

    #endregion

    #region Helper Methods

    private static void ProcessSensorData(ISensor sensor, CpuDataDto cpuData)
    {
        switch (sensor.SensorType)
        {
            case SensorType.Temperature:
                cpuData.Temperatures[sensor.Name] = sensor.Value.GetValueOrDefault();
                break;
            case SensorType.Load:
                cpuData.Load[sensor.Name] = sensor.Value.GetValueOrDefault();
                break;
            case SensorType.Clock:
                cpuData.ClockSpeeds[sensor.Name] = sensor.Value.GetValueOrDefault();
                break;
        }
    }

    private static void ProcessSensorData(ISensor sensor, GpuInfoDto gpuInfo)
    {
        switch (sensor.SensorType)
        {
            case SensorType.Temperature:
                gpuInfo.Temperatures[sensor.Name] = sensor.Value.GetValueOrDefault();
                break;
            case SensorType.Load:
                gpuInfo.Load[sensor.Name] = sensor.Value.GetValueOrDefault();
                break;
            case SensorType.Clock:
                gpuInfo.ClockSpeeds[sensor.Name] = sensor.Value.GetValueOrDefault();
                break;
            case SensorType.SmallData:
                gpuInfo.MemoryUsage[sensor.Name] = sensor.Value.GetValueOrDefault();
                break;
        }
    }

    #endregion

    public void Close()
    {
        _computer.Close();
    }
}
