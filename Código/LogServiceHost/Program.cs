using System;
using System.Configuration;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace LogServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var remotingTcpChannel = new TcpChannel(Int32.Parse(ConfigurationManager.AppSettings["LogServiceHostPort"]));

            ChannelServices.RegisterChannel(remotingTcpChannel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(LogService.LogService),
                ConfigurationManager.AppSettings["LogServiceHostName"],
                WellKnownObjectMode.SingleCall);

            Console.WriteLine("Server has started at: tcp://127.0.0.1:" +
                ConfigurationManager.AppSettings["LogServiceHostPort"] + "/" +
                ConfigurationManager.AppSettings["LogServiceHostName"]);
            Console.ReadLine();

            ChannelServices.UnregisterChannel(remotingTcpChannel);
        }
    }
}
