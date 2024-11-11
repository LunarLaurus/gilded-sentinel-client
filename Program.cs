using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.Extensions.Configuration;
using static EnvironmentUtils;

[JsonSerializable(typeof(SystemInfoDTO))]
internal partial class SystemInfoDTOJsonContext : JsonSerializerContext
{
}

public record SystemInfoDTO
{
    public string SystemArchitecture { get; set; }
    public string SystemPlatform { get; set; }
    public string SystemHostName { get; set; }
    public string SystemModelName { get; set; }
    public bool System64Bit { get; set; }
    public bool DotNetProcess64Bit { get; set; }
    public long SystemUptime { get; set; }
    public List<StorageDriveDto> SystemStorageInfo { get; set; }
    public HardwareMonitor.CPUInfoDto CpuInfo { get; set; }
    public List<HardwareMonitor.GPUInfoDto> Gpus { get; set; }
    public HardwareMonitor.MemoryStateDto Memory { get; set; }
    public string IloAddress { get; set; }

}

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var masterIp = configuration["General:ip"];
        var masterPort = int.Parse(configuration["General:port"]);
        var clientPollRate = int.Parse(configuration["General:pollInSeconds"]);
        var iloAddress = configuration["General:iloAddress"];

        var isSystem64Bit = EnvironmentUtils.Is64BitOperatingSystem();
        var isProcess64Bit = EnvironmentUtils.Is64BitProcess();
        var systemArch = EnvironmentUtils.GetSystemArchitecture();
        var systemPlatform = EnvironmentUtils.GetPlatform();
        var systemStorage = EnvironmentUtils.GetDriveInfo();

        long cycleCounter = 0;

        LogMessage($"Connecting {GetHostName()} to {masterIp}:{masterPort} and polling data every {clientPollRate} seconds.");
        if (!IsZeroIPAddress(iloAddress))
        {
            LogMessage("Ilo Address supplied: " + iloAddress);
        }

        var hardwareMonitor = new HardwareMonitor();
        String ClientHostName = GetHostName();
        String ClientModelName = hardwareMonitor.GetMotherboardName();

        while (true)
        {
            try
            {
                cycleCounter++;
                if (cycleCounter % 10 == 0)
                {
                    systemStorage = EnvironmentUtils.GetDriveInfo();
                }

                var systemInfo = new SystemInfoDTO
                {
                    SystemHostName = ClientHostName,
                    CpuInfo = hardwareMonitor.GetCPUInfo(),
                    Gpus = hardwareMonitor.GetGPUInfo(),
                    Memory = hardwareMonitor.GetMemoryState(),
                    SystemModelName = ClientModelName,
                    IloAddress = iloAddress,
                    System64Bit = isSystem64Bit,
                    DotNetProcess64Bit = isProcess64Bit,
                    SystemArchitecture = systemArch,
                    SystemPlatform = systemPlatform,
                    SystemStorageInfo = systemStorage,
                    SystemUptime = EnvironmentUtils.GetRawUptime(),
                };
                LogMessage(systemInfo.ToString());
                await SendSystemInfoAsync(masterIp, masterPort, systemInfo);

            }
            catch (Exception ex)
            {
                LogMessage("Error: " + ex.Message);
            }

            await Task.Delay(clientPollRate * 1000);
        }
    }

        public static async Task SendSystemInfoAsync(string masterIp, int masterPort, object systemInfo)
    {
        // Serialize the system info object to JSON using a specific JsonContext (if needed)
        var jsonData = JsonSerializer.Serialize(systemInfo, SystemInfoDTOJsonContext.Default.SystemInfoDTO);

        try
        {
            // Establish a TCP connection to the master server
            using var client = new TcpClient(masterIp, masterPort);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream);
            using var reader = new StreamReader(stream);

            // Send the serialized data to the master server
            await writer.WriteLineAsync(jsonData);
            await writer.FlushAsync();

            // Read the server's response
            var responseData = await reader.ReadLineAsync();
            if (responseData != null && responseData.Equals("OK-200", StringComparison.OrdinalIgnoreCase))
            {
                LogMessage("Received response: " + responseData);
            }
            else
            {
                LogMessage("ERROR-404: " + responseData);
            }
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during the process
            LogMessage("Exception: " + ex.Message);
        }
    }

    static void LogMessage(string message)
    {
        var timestamp = DateTime.Now.ToString("dd-MM HH:mm:ss");
        Console.WriteLine($"[{timestamp}] {message}");
    }

    static bool IsZeroIPAddress(string ipAddress)
    {
        if (IPAddress.TryParse(ipAddress, out IPAddress parsedAddress))
        {
            // Compare the parsed IP address to the IPAddress representation of "0.0.0.0"
            return parsedAddress.Equals(IPAddress.Any);
        }

        // Return false if the input string is not a valid IP address
        return false;
    }

    public static string GetHostName()
    {
        string[] hostNameMethods =
        {
            Environment.MachineName,
            Dns.GetHostName(),
            Dns.GetHostEntry(Dns.GetHostName()).HostName
        };

        foreach (var hostName in hostNameMethods)
        {
            if (!string.IsNullOrEmpty(hostName))
            {
                return hostName;
            }
        }

        return "UnknownHost";
    }
}