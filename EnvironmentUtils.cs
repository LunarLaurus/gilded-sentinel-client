using System;
using System.Runtime.InteropServices;

public static class EnvironmentUtils
{

    public record StorageDriveDto
    {
        public string Name { get; set; }
        public DriveType DriveType { get; set; }
        public string DriveFormat { get; set; }
        public long TotalSize { get; set; }
        public long AvailableFreeSpace { get; set; }
        public string VolumeLabel { get; set; }

        // Constructor for convenience
        public StorageDriveDto(string name, DriveType driveType, string driveFormat, long totalSize, long availableFreeSpace, string volumeLabel)
        {
            Name = name;
            DriveType = driveType;
            DriveFormat = driveFormat;
            TotalSize = totalSize;
            AvailableFreeSpace = availableFreeSpace;
            VolumeLabel = volumeLabel;
        }

        // Method to return a human-readable format of the drive
        public override string ToString()
        {
            return $"Drive: {Name}\n" +
                    $"  Type: {DriveType}\n" +
                    $"  Format: {DriveFormat}\n" +
                    $"  Total Size: {FormatBytes(TotalSize)}\n" +
                    $"  Available Free Space: {FormatBytes(AvailableFreeSpace)}\n" +
                    $"  Volume Label: {VolumeLabel}\n";
        }

        // Utility method to format byte size into a human-readable format
        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1L << 30)
                return $"{bytes / (1L << 30)} GB";
            if (bytes >= 1L << 20)
                return $"{bytes / (1L << 20)} MB";
            if (bytes >= 1L << 10)
                return $"{bytes / (1L << 10)} KB";
            return $"{bytes} Bytes";
        }
    }

    public static class ArchitectureExtensions
    {
        public static string ToString(Architecture architecture) => Enum.GetName(typeof(Architecture), architecture);

        public static Architecture? FromString(string name)
        {
            return Enum.TryParse(name, true, out Architecture result) ? result : (Architecture?)null;
        }
    }

    public static class DriveTypeExtensions
    {
        public static string ToString(DriveType driveType) => Enum.GetName(typeof(DriveType), driveType);
        
        public static DriveType? FromString(string name)
        {
            return Enum.TryParse(name, true, out DriveType result) ? result : (DriveType?)null;
        }
    }

    // Get the architecture of the operating system.
    public static string GetSystemArchitecture() => ArchitectureExtensions.ToString(RuntimeInformation.ProcessArchitecture);

    // Get the current system user's name.
    public static string GetCurrentUserName() => Environment.UserName;

    // Get the machine name.
    public static string GetMachineName() => Environment.MachineName;

    // Get the number of processor cores.
    public static int GetProcessorCount() => Environment.ProcessorCount;

    // Get the current OS version and description.
    public static string GetOSVersion() => $"OS: {Environment.OSVersion}, {RuntimeInformation.OSDescription}";

    // Get the framework version.
    public static string GetFrameworkVersion() => RuntimeInformation.FrameworkDescription;

    // Get the platform (Windows, Linux, macOS).
    public static string GetPlatform()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macOS" :
                    "Unknown";
    }

    // Get the current version of the CLR being used.
    public static string GetClrVersion() => Environment.Version.ToString();

    // Is the process running in 64-bit mode?
    public static bool Is64BitProcess() => Environment.Is64BitProcess;

    // Is the OS 64-bit?
    public static bool Is64BitOperatingSystem() => Environment.Is64BitOperatingSystem;

    // Get the system directory.
    public static string GetSystemDirectory() => Environment.SystemDirectory;

    // Get the current directory of the application.
    public static string GetCurrentDirectory() => Environment.CurrentDirectory;

    // Get environment variables (can return all environment variables, or one by key).
    public static string GetEnvironmentVariable(string key) => Environment.GetEnvironmentVariable(key);

    // Get the system uptime in milliseconds.
    public static long GetRawUptime() => Environment.TickCount64;

    // Get system uptime as a TimeSpan.
    public static TimeSpan GetUptime() => TimeSpan.FromMilliseconds(GetRawUptime());

    public static List<StorageDriveDto> GetDriveInfo()
    {
        var storageDrives = new List<StorageDriveDto>();

        // Get all drives on the system
        var drives = DriveInfo.GetDrives();

        foreach (var drive in drives)
        {
            // Check if the drive is ready (e.g., it exists and can be accessed)
            if (drive.IsReady)
            {
                // Create a StorageDriveDto object for each ready drive and add it to the list
                var storageDriveDto = new StorageDriveDto(
                    drive.Name,
                    drive.DriveType,
                    drive.DriveFormat,
                    drive.TotalSize,
                    drive.AvailableFreeSpace,
                    drive.VolumeLabel
                );

                storageDrives.Add(storageDriveDto);
            }
        }

        return storageDrives;
    }
}
