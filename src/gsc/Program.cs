using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

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
        var ipmiAddress = configuration["General:ipmiAddress"];

        var isSystem64Bit = EnvironmentUtils.Is64BitOperatingSystem();
        var isProcess64Bit = EnvironmentUtils.Is64BitProcess();
        var systemArch = EnvironmentUtils.GetSystemArchitecture();
        var systemPlatform = EnvironmentUtils.GetPlatform();
        var systemStorage = EnvironmentUtils.GetDriveInfo();

        string hostname = NetworkUtils.GetHostName();

        long cycleCounter = 0;

        LogMessage($"Connecting {hostname} to {masterIp}:{masterPort} and polling data every {clientPollRate} seconds.");
        if (!NetworkUtils.IsZeroIPAddress(ipmiAddress))
        {
            LogMessage("Ipmi Address supplied: " + ipmiAddress);
        }

        var hardwareMonitor = new HardwareMonitor();
        string ClientModelName = hardwareMonitor.GetMotherboardName();
        var psuInfo = hardwareMonitor.GetPsuInfo();

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
                    SystemHostName = hostname,
                    CpuInfo = hardwareMonitor.GetCpuInfo(),
                    Gpus = hardwareMonitor.GetGpuInfo(),
                    Memory = hardwareMonitor.GetMemoryState(),
                    SystemModelName = ClientModelName,
                    IpmiAddress = ipmiAddress,
                    System64Bit = isSystem64Bit,
                    DotNetProcess64Bit = isProcess64Bit,
                    SystemArchitecture = systemArch,
                    SystemPlatform = systemPlatform,
                    SystemStorageInfo = systemStorage,
                    PowerSupplies = psuInfo,
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

}