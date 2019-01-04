using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Victim
{
    /// <summary>
    /// This class represent a victim
    /// </summary>
    public class Victim
    {
        /// <summary>
        /// fields of a victim
        /// </summary>
        private int m_tcpPort = 0;
        private List<DateTime> m_times = new List<DateTime>();
        private string m_passowrd;

        /// <summary>
        /// Method to start run the victim
        /// </summary>
        public void start()
        {
            setPort();
            getRandomPasword();
            Console.WriteLine("----------------my ip: " + GetLocalIPAddress().ToString());         //JUST FOR OUR TEST
            Console.WriteLine("Server listening on port " + m_tcpPort + ", password is " + m_passowrd);
            StartTCPClient();
        }

        /// <summary>
        /// Method to set random port to victim
        /// </summary>
        private void setPort()
        {
            Random r = new Random();
            int port = r.Next(1000, 7000);
            m_tcpPort = Int16.Parse(port + "");
        }

        /// <summary>
        /// Meethod to get random password from the user
        /// </summary>
        private void getRandomPasword()
        {
            Console.WriteLine("Enter a six lower letters password!");
            m_passowrd = Console.ReadLine();
            while (m_passowrd.Length != 6 || !m_passowrd.All(char.IsLetter) || !m_passowrd.All(char.IsLower))
            {
                Console.WriteLine("Invalid Password. Enter a six lower letters password!");
                m_passowrd = Console.ReadLine();
            }
        }

        /// <summary>
        /// Method to start tcp client
        /// </summary>
        public void StartTCPClient()
        {
            byte[] bytes = new Byte[1024];
            IPAddress LoaclIPAddress = GetLocalIPAddress();
            IPEndPoint groupEP = new IPEndPoint(LoaclIPAddress, m_tcpPort);
            Socket listener = new Socket(LoaclIPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            int _maximum = 10, bytesRec;  //The maximum length of the pending connections queue
            string data = "";
            Socket handler;
            try
            {
                listener.Bind(groupEP);
                listener.Listen(_maximum);
                while (true)
                {
                    handler = listener.Accept();
                    handler.Send(Encoding.ASCII.GetBytes("Please enter your password\r\n"));
                    bytesRec = handler.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (!data.Equals(m_passowrd))
                    {
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                    else
                    {
                        handler.Send(Encoding.ASCII.GetBytes("Access granted\r\n"));
                        DateTime localDate = DateTime.Now;
                        m_times.Add(localDate);
                        if (CheckConnections(localDate))
                        {
                            bytesRec = handler.Receive(bytes);
                            data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                            Console.WriteLine(data);
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                        }
                    }
                }

            }
            catch
            {
                Console.WriteLine("failed connection with bot");
            }
        }

        /// <summary>
        /// Method to check all previous connections
        /// </summary>
        /// <param name="currentDate">cuurent time</param>
        /// <returns>if there were 10 connection in the last 1 sec</returns>
        public bool CheckConnections(DateTime currentDate) 
        {
            int counter = 0;
            TimeSpan difference;
            foreach (DateTime d in m_times)
            {
                difference = currentDate - d;
                if (difference.Milliseconds < 1000 && difference.Hours == 0 && difference.Minutes == 0)
                {
                    counter++;
                }
                else
                {
                    m_times.Remove(d);  
                }
                if (counter > 9) 
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Method to get the local ip address
        /// </summary>
        /// <returns></returns>
        public IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }


    }
}
