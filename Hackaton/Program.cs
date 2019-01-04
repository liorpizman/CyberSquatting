using CyberSquatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberSquatting
{
    class Program
    {
        static void Main(string[] args)
        {
            cyberSquatting server = new cyberSquatting();
            Console.WriteLine("----------------CyberSquatting's window");
            server.StartUDP();
        }
    }
}
