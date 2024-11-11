
using System.Net;

public static class NetworkUtils {

    public static bool IsZeroIPAddress(string ipAddress)
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