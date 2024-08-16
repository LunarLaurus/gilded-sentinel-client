# Gilded Sentinel Client

The client is a lightweight C# application designed to monitor system hardware (CPU, GPU, memory, etc.) and send data to a central server for real-time analysis and monitoring. It utilizes the LibreHardwareMonitor library to gather hardware metrics and transmit them to the [Gilded Sentinel backend](https://github.com/LunarLaurus/gilded-sentinel-backend). This data is then accessible via the [Gilded Sentinel Web-UI](https://github.com/LunarLaurus/gilded-sentinel).

## Features

- **Real-Time Monitoring:** Collects real-time CPU temperatures and other hardware metrics.
- **Configurable Polling:** Allows configuration of the data collection frequency.
- **Data Transmission:** Sends collected data to the central server over TCP for analysis.

## Project Structure

```plaintext
Gilded-Sentinel-Client/
├── HardwareMonitor.cs          # Handles hardware monitoring using LibreHardwareMonitor
├── Program.cs                  # Main entry point; handles configuration, data collection, and transmission
├── app.manifest.xml            # Application manifest, setting execution level
├── appsettings.json            # Configuration file for server IP, port, and polling rate
├── Gilded-Sentinel-Client.csproj  # C# project file with dependencies
└── Gilded-Sentinel-Client.sln  # Visual Studio solution file
```

## Installation
### Prerequisites
.NET 8.0 SDK or later
Visual Studio 2022 or later (optional for development)
### Steps
Clone the repository:

```bash
git clone https://github.com/your-repo/gilded-sentinel-client.git
cd gilded-sentinel-client
```

Build the project:

```bash
dotnet build
```
Publish the project as a single executable:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```
## Configuration
The client is configured via the appsettings.json file. This file contains the server IP, port, and polling interval:

```json
{
    "General": {
        "ip": "192.168.0.10",
        "iloAddress": "0.0.0.0",
        "port": "32500",
        "pollInSeconds": "3"
    }
}
```
- ip: The IP address of the central server to which the client will send data.  
- iloAddress: The IP address of the clients iLO interface if available, else leave as `0.0.0.0`.
- port: The port on the server that listens for incoming data.  
- pollInSeconds: The interval (in seconds) between data collection.

## Usage
Modify the appsettings.json file to point to your server's IP address and port.

Run the client:

```bash
dotnet run
```
The client will start collecting hardware data and sending it to the specified server at the configured polling rate.

## Logging
The client logs messages with timestamps to the console. These logs include connection status, data sent, and any errors encountered during operation.

## Dependencies
- LibreHardwareMonitor: Library for accessing hardware information such as CPU, GPU, and memory metrics.  
- .NET 8.0 SDK: Required for building and running the application.  
- Microsoft.Extensions.Configuration: Used for configuration management.  
