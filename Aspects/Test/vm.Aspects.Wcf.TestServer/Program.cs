using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using vm.Aspects.Facilities;
using vm.Aspects.Facilities.Diagnostics;
using vm.Aspects.Security;
using vm.Aspects.Wcf.Bindings;
using vm.Aspects.Wcf.Services;

namespace vm.Aspects.Wcf.TestServer
{
    public class AddressBinding
    {
        public string Address { get; set; }
        public Func<Binding> BindingFactory { get; set; }
    }

    public class PatternAddressesAndBindings
    {
        public string PatternName { get; set; }
        public AddressBinding[] AddressesAndBindings { get; set; }
    }

    class Program
    {
        static AddressBinding[] addressesAndBindings1 =
        {
            new AddressBinding { Address = "http://localhost:1480/basicHttp.svc",    BindingFactory = () => new BasicHttpBinding() },
            new AddressBinding { Address = "https://localhost:14443/basicHttps.svc", BindingFactory = () => new BasicHttpsBinding() },
            new AddressBinding { Address = "https://localhost:14444/netHttps.svc",   BindingFactory = () => new NetHttpsBinding() },
            new AddressBinding { Address = "http://localhost:1481/netHttp.svc",      BindingFactory = () => new NetHttpBinding() },
            new AddressBinding { Address = "http://localhost:1483/wsHttp.svc",       BindingFactory = () => new WSHttpBinding() },
            new AddressBinding { Address = "net.pipe://localhost/net.pipe.svc",      BindingFactory = () => new NetNamedPipeBinding() },
            new AddressBinding { Address = "net.tcp://localhost:14808/net.tcp.svc",  BindingFactory = () => new NetTcpBinding() },
            new AddressBinding { Address = "http://localhost:1482/webHttp",          BindingFactory = () => new WebHttpBinding() },
        };

        static AddressBinding[] addressesAndBindings2 =
        {
            new AddressBinding { Address = "https://localhost:14443/basicHttp.svc",  BindingFactory = () => new BasicHttpBinding() },
            new AddressBinding { Address = "https://localhost:14444/basicHttps.svc", BindingFactory = () => new BasicHttpsBinding() },
            new AddressBinding { Address = "https://localhost:14445/netHttps.svc",   BindingFactory = () => new NetHttpsBinding() },
            new AddressBinding { Address = "https://localhost:14446/netHttp.svc",    BindingFactory = () => new NetHttpBinding() },
            new AddressBinding { Address = "https://localhost:14447/webHttp",        BindingFactory = () => new WebHttpBinding() },
            new AddressBinding { Address = "https://localhost:14448/wsHttp.svc",     BindingFactory = () => new WSHttpBinding() },
            new AddressBinding { Address = "net.pipe://localhost/net.pipe.svc",      BindingFactory = () => new NetNamedPipeBinding() },
            new AddressBinding { Address = "net.tcp://localhost:14808/net.tcp.svc",  BindingFactory = () => new NetTcpBinding() },
        };

        static AddressBinding[] addressesAndBindings3 =
        {
            new AddressBinding { Address = "http://localhost:1483/wsHttp.svc",       BindingFactory = () => new WSHttpBinding() },
            new AddressBinding { Address = "net.tcp://localhost:14808/net.tcp.svc",  BindingFactory = () => new NetTcpBinding() },
        };

        static AddressBinding[] addressesAndBindings4 =
        {
            new AddressBinding { Address = "http://localhost:1480/basicHttp.svc",    BindingFactory = () => new BasicHttpBinding() },
            new AddressBinding { Address = "http://localhost:1483/wsHttp.svc",       BindingFactory = () => new WSHttpBinding() },
            new AddressBinding { Address = "net.tcp://localhost:14808/net.tcp.svc",  BindingFactory = () => new NetTcpBinding() },
        };

        static PatternAddressesAndBindings[] patternsAddressesAndBindings =
        {
            new PatternAddressesAndBindings { PatternName = RequestResponseNoSecurityConfigurator.PatternName,                                  AddressesAndBindings = addressesAndBindings1 },
            //new PatternAddressesAndBindings { PatternName = RequestResponseConfigurator.PatternName,                                            AddressesAndBindings = addressesAndBindings2 },
            //new PatternAddressesAndBindings { PatternName = RequestResponseTransportConfigurator.PatternName,                                   AddressesAndBindings = addressesAndBindings2 },
            //new PatternAddressesAndBindings { PatternName = RequestResponseTransportClientWindowsAuthenticationConfigurator.PatternName,        AddressesAndBindings = addressesAndBindings2 },
            //new PatternAddressesAndBindings { PatternName = RequestResponseTransportClientCertificateAuthenticationConfigurator.PatternName,    AddressesAndBindings = addressesAndBindings2 },
            //new PatternAddressesAndBindings { PatternName = RequestResponseMessageConfigurator.PatternName,                                     AddressesAndBindings = addressesAndBindings3 },
            //new PatternAddressesAndBindings { PatternName = RequestResponseMessageClientWindowsAuthenticationConfigurator.PatternName,          AddressesAndBindings = addressesAndBindings3 },
            //new PatternAddressesAndBindings { PatternName = RequestResponseMessageClientCertificateAuthenticationConfigurator.PatternName,      AddressesAndBindings = addressesAndBindings4 },
        };

        static void Main(string[] args)
        {
            var ai = new TelemetryClient();

            TelemetryConfiguration.Active.InstrumentationKey = "1ea142b2-1d97-453e-a0f4-32b15523dd7d";

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                foreach (var pattern in patternsAddressesAndBindings)
                    using (var host = new RequestResponseServiceHostFactory(pattern.AddressesAndBindings, pattern.PatternName).CreateHost())
                    {
                        try
                        {
                            if (new[]
                                {
                                    RequestResponseTransportConfigurator.PatternName,
                                    RequestResponseMessageConfigurator.PatternName,

                                    RequestResponseTransportClientCertificateAuthenticationConfigurator.PatternName,
                                    RequestResponseMessageClientCertificateAuthenticationConfigurator.PatternName,

                                    RequestResponseTransportClientWindowsAuthenticationConfigurator.PatternName,
                                    RequestResponseMessageClientWindowsAuthenticationConfigurator.PatternName,
                                }.Contains(pattern.PatternName))
                                host.SetServiceCredentials(
                                        CertificateFactory.GetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindByThumbprint, "351ae52cb2c3ac15ec12a3bdce838554fc63da95"));

                            host.Open();

                            Console.WriteLine($"\nA service host for {host.Description.ServiceType.Name} service is initializing for pattern {pattern.PatternName}:");
                            foreach (var ep in host.Description.Endpoints)
                                Console.WriteLine("    {0}", ep.ListenUri.AbsoluteUri.ToString());

                            foreach (var ab in pattern.AddressesAndBindings)
                                try
                                {
                                    var binding = ab.BindingFactory();
                                    var client = new RequestResponseClient(binding, ab.Address, ServiceIdentity.None, "", pattern.PatternName);

                                    using (client)
                                    {
                                        var s = client.GetStrings(2);
                                        Console.WriteLine($"{ab.Address} => successfully got {s.Count()} strings.");
                                    }
                                }
                                catch (Exception x)
                                {
                                    Facility.LogWriter.ExceptionError(x);
                                    VmAspectsEventSource.Log.Exception(x);
                                    ai.TrackException(x);

                                    Console.WriteLine($"{ab.Address} => failed:");
                                    Console.WriteLine(x.DumpString());
                                    Debug.WriteLine(x.DumpString());
                                }

                            host.Close();
                        }
                        catch (Exception x)
                        {
                            Facility.LogWriter.ExceptionError(x);
                            VmAspectsEventSource.Log.Exception(x);
                            ai.TrackException(x);

                            host.Abort();
                            Console.WriteLine($"The host with pattern {pattern.PatternName} => failed:");
                            Console.WriteLine(x.DumpString());
                            Debug.WriteLine(x.DumpString());
                        }

                        ai.Flush();
                        Task.Delay(1000).Wait();

                        Console.Write("Press any key to continue...");
                        Console.ReadKey(false);
                        Console.WriteLine();
                        Console.WriteLine();
                    }
            }
            catch (Exception x)
            {
                Facility.LogWriter.ExceptionError(x);
                VmAspectsEventSource.Log.Exception(x);
                ai.TrackException(x);

                Console.WriteLine(x.DumpString());
                Debug.WriteLine(x.DumpString());

                Console.Write("Press any key to finish...");
                Console.ReadKey(false);
            }

            ai.Flush();
            Task.Delay(1000).Wait();
        }
    }
}
