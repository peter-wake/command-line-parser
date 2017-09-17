using System;
using System.Text.RegularExpressions;

namespace CommandLineTests
{
    internal class PortAddress
    {
        public string Address { get; }
        public int Port { get; }

        public PortAddress(string address, int port)
        {
            var addressPattern = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");

            if (!addressPattern.IsMatch(address))
            {
                throw new ArgumentException("Bad address");
            }

            Address = address;
            Port = port;
        }

        public override string ToString()
        {
            return 0 == Port ? Address : $"{Address}:{Port}";
        }
    }
}
