using System;
using System.Threading.Tasks;
using LagoVista.MQTT.Core;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Sockets;

namespace LagoVista.XPlat.iOS.Services
{
    public class MqttNetworkChannel : IMqttNetworkChannel
    {
        private string _remoteHostName;
        private IPAddress _remoteIpAddress;
        private int _remotePort;

        // socket for communication
        private Socket socket;
        // using SSL
        private bool _secure;


        // CA certificate
      //  private X509Certificate _caCert;

        /// <summary>
        /// Remote host name
        /// </summary>
        public string RemoteHostName { get { return this._remoteHostName; } }

        /// <summary>
        /// Remote IP address
        /// </summary>
        public IPAddress RemoteIpAddress { get { return this._remoteIpAddress; } }

        /// <summary>
        /// Remote port
        /// </summary>
        public int RemotePort { get { return this._remotePort; } }



#if SSL
        // SSL stream
        private SslStream sslStream;
        private NetworkStream netStream;
#endif

        /// <summary>
        /// Data available on the channel
        /// </summary>
        public bool DataAvailable
        {
            get
            {
#if SSL
                if (secure)
                    return this.netStream.DataAvailable;
                else
                    return (this.socket.Available > 0);
#else
                return (this.socket.Available > 0);
#endif
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="remoteHostName">Remote Host name</param>
        /// <param name="remotePort">Remote port</param>
        /// <param name="secure">Using SSL</param>
        public async Task InitAsync(String remoteHostName, int remotePort, bool secure)
        {
            // check if remoteHostName is a valid IP address and get it

            this._remoteHostName = remoteHostName;

            if (IPAddress.TryParse(remoteHostName, out IPAddress remoteIPAddress))
            {
                _remoteIpAddress = remoteIPAddress;
            }
            this._remotePort = remotePort;
            this._secure = secure;

            // in this case the parameter remoteHostName isn't a valid IP address
            if (_remoteIpAddress == null)
            {
                var hostEntry = await Dns.GetHostEntryAsync(_remoteHostName);
                if ((hostEntry != null) && (hostEntry.AddressList.Length > 0))
                {
                    // check for the first address not null
                    // it seems that with .Net Micro Framework, the IPV6 addresses aren't supported and return "null"
                    int i = 0;
                    while (hostEntry.AddressList[i] == null) i++;
                    _remoteIpAddress = hostEntry.AddressList[i];
                }
                else
                {
                    throw new Exception("No address found for the remote host name");
                }
            }
        }

        /// <summary>
        /// Connect to remote server
        /// </summary>
        public Task ConnectAsync()
        {
            this.socket = new Socket(this._remoteIpAddress.GetAddressFamily(), SocketType.Stream, ProtocolType.Tcp);
            // try connection to the broker
            this.socket.Connect(new IPEndPoint(this._remoteIpAddress, this._remotePort));

            return Task.FromResult(default(object));

#if SSL
            // secure channel requested
            if (secure)
            {
                // create SSL stream
                this.netStream = new NetworkStream(this.socket);
                this.sslStream = new SslStream(this.netStream, false, this.userCertificateValidationCallback, this.userCertificateSelectionCallback);

                // server authentication (SSL/TLS handshake)
                      this.sslStream.AuthenticateAsClient(
                        this.remoteHostName,
                        null,
                        SslProtocols.Tls,
                        false);
                
            }
#endif
        }

        /// <summary>
        /// Send data on the network channel
        /// </summary>
        /// <param name="buffer">Data buffer to send</param>
        /// <returns>Number of byte sent</returns>
        public int Send(byte[] buffer)
        {
#if SSL
            if (this.secure)
            {
                this.sslStream.Write(buffer, 0, buffer.Length);
                this.sslStream.Flush();
                return buffer.Length;
            }
            else
                return this.socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
#else
            return this.socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
#endif
        }

        /// <summary>
        /// Receive data from the network
        /// </summary>
        /// <param name="buffer">Data buffer for receiving data</param>
        /// <returns>Number of bytes received</returns>
        public int Receive(byte[] buffer)
        {
#if SSL
            if (this.secure)
            {
                // read all data needed (until fill buffer)
                int idx = 0, read = 0;
                while (idx < buffer.Length)
                {
                    // fixed scenario with socket closed gracefully by peer/broker and
                    // Read return 0. Avoid infinite loop.
                    read = this.sslStream.Read(buffer, idx, buffer.Length - idx);
                    if (read == 0)
                        return 0;
                    idx += read;
                }
                return buffer.Length;
            }
            else
            {
                // read all data needed (until fill buffer)
                int idx = 0, read = 0;
                while (idx < buffer.Length)
                {
                    // fixed scenario with socket closed gracefully by peer/broker and
                    // Read return 0. Avoid infinite loop.
                    read = this.socket.Receive(buffer, idx, buffer.Length - idx, SocketFlags.None);
                    if (read == 0)
                        return 0;
                    idx += read;
                }
                return buffer.Length;
            }
#else
            // read all data needed (until fill buffer)
            int idx = 0, read = 0;
            while (idx < buffer.Length)
            {
                // fixed scenario with socket closed gracefully by peer/broker and
                // Read return 0. Avoid infinite loop.
                read = this.socket.Receive(buffer, idx, buffer.Length - idx, SocketFlags.None);
                if (read == 0)
                    return 0;
                idx += read;
            }
            return buffer.Length;
#endif
        }

        /// <summary>
        /// Receive data from the network channel with a specified timeout
        /// </summary>
        /// <param name="buffer">Data buffer for receiving data</param>
        /// <param name="timeout">Timeout on receiving (in milliseconds)</param>
        /// <returns>Number of bytes received</returns>
        public int Receive(byte[] buffer, int timeout)
        {
            // check data availability (timeout is in microseconds)
            if (this.socket.Poll(timeout * 1000, SelectMode.SelectRead))
            {
                return this.Receive(buffer);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Close the network channel
        /// </summary>
        public void Close()
        {
#if SSL
            if (this.secure)
            {
                this.netStream.Close();
                this.sslStream.Close();
            }
            this.socket.Close();
#else
            this.socket.Dispose();
#endif
        }
    }

    /// <summary>
    /// IPAddress Utility class
    /// </summary>
    public static class IPAddressUtility
    {
        /// <summary>
        /// Return AddressFamily for the IP address
        /// </summary>
        /// <param name="ipAddress">IP address to check</param>
        /// <returns>Address family</returns>
        public static AddressFamily GetAddressFamily(this IPAddress ipAddress)
        {
            return ipAddress.AddressFamily;
        }
    }
}

