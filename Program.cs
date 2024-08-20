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

[JsonSerializable(typeof(SystemInfoDTO))]
internal partial class SystemInfoDTOJsonContext : JsonSerializerContext
{
}

public class SystemInfoDTO
{
    public String ClientName { get; set; }
    public String ModelName { get; set; }
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
                var systemInfo = new SystemInfoDTO
                {
                    ClientName = ClientHostName,
                    CpuInfo = hardwareMonitor.GetCPUInfo(),
                    Gpus = hardwareMonitor.GetGPUInfo(),
                    Memory = hardwareMonitor.GetMemoryState(),
                    ModelName = ClientModelName,
                    IloAddress = iloAddress
                };

                var jsonData = JsonSerializer.Serialize(systemInfo, SystemInfoDTOJsonContext.Default.SystemInfoDTO);

                using var client = new TcpClient(masterIp, masterPort);
                using var stream = client.GetStream();
                using var writer = new StreamWriter(stream);
                using var reader = new StreamReader(stream);

                await writer.WriteLineAsync(jsonData);
                await writer.FlushAsync();

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
                LogMessage("Error: " + ex.Message);
            }

            await Task.Delay(clientPollRate * 1000);
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