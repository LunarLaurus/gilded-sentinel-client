using System;
using System.Collections.Generic;
using LibreHardwareMonitor.Hardware;

public class HardwareMonitor
{
    private Computer computer;

    public HardwareMonitor()
    {
        computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true
        };
        computer.Open();
    }

    public int GetTotalCPUs()
    {
        int totalCPUs = 0;
        foreach (var hardware in computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Cpu)
            {
                totalCPUs++;
            }
        }
        return totalCPUs;
    }

    public Dictionary<string, Dictionary<string, float?>> GetCPUTemperatures()
    {
        Dictionary<string, Dictionary<string, float?>> cpuTemperatures = new Dictionary<string, Dictionary<string, float?>>();
        
        foreach (var hardware in computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Cpu)
            {
                Dictionary<string, float?> coreTemperatures = new Dictionary<string, float?>();
                
                // Update hardware to get the latest sensor data
                hardware.Update();
                
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        coreTemperatures[sensor.Name] = sensor.Value;
                    }
                }
                
                // Use hardware.Name or hardware.Identifier to distinguish between CPUs
                cpuTemperatures[hardware.Identifier.ToString()] = coreTemperatures;
            }
        }
        
        return cpuTemperatures;
    }

    public void Close()
    {
        computer.Close();
    }
}
