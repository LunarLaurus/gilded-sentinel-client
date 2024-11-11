using System;
using System.Runtime.InteropServices;

public static class EnvironmentUtils
{

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
    public static int GetSystemArchitecture() => (int)RuntimeInformation.ProcessArchitecture;
    public static string GetSystemArchitectureName() => ArchitectureExtensions.ToString(RuntimeInformation.ProcessArchitecture);

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
