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