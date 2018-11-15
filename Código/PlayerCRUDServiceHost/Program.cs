using PlayerCRUDServiceInterfaces;
using System;
using System.Configuration;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace PlayerCRUDServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var remotingTcpChannel = new TcpChannel(Int32.Parse(ConfigurationManager.AppSettings["PlayerCRUDServiceHostPort"]));

            ChannelServices.RegisterChannel(remotingTcpChannel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(PlayerCRUDService.PlayerCRUDService),
                ConfigurationManager.AppSettings["PlayerCRUDServiceHostName"],
                WellKnownObjectMode.SingleCall);

            Console.WriteLine("Server has started at: tcp://127.0.0.1:"+
                ConfigurationManager.AppSettings["PlayerCRUDServiceHostPort"] + "/" +
                ConfigurationManager.AppSettings["PlayerCRUDServiceHostName"]);

            LoadTestData();

            Console.ReadLine();

            ChannelServices.UnregisterChannel(remotingTcpChannel);
        }

        public static void LoadTestData()
        {
            var players = (IPlayerCRUDService)Activator.GetObject(
                                        typeof(IPlayerCRUDService),
                                        "tcp://" + ConfigurationManager.AppSettings["PlayerCRUDServiceHostIP"] +
                                        ":" + ConfigurationManager.AppSettings["PlayerCRUDServiceHostPort"]
                                        + "/" + ConfigurationManager.AppSettings["PlayerCRUDServiceHostName"]);

            players.Add(new Player() { Nickname = "1", Avatar = "default" });
            players.Add(new Player() { Nickname = "2", Avatar = "default" });
            players.Add(new Player() { Nickname = "3", Avatar = "default" });
            players.Add(new Player() { Nickname = "4", Avatar = "default" });
            players.Add(new Player() { Nickname = "5", Avatar = "default" });
            players.Add(new Player() { Nickname = "6", Avatar = "default" });
            players.Add(new Player() { Nickname = "7", Avatar = "default" });
            players.Add(new Player() { Nickname = "8", Avatar = "default" });

        }
    }
}
