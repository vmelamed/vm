using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using vm.Aspects;
using vm.Aspects.FtpTransfer;

namespace TestFtpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var site = new TransferSite
                {
                    Link                  = "ftp://ftp.testalmond.net",
                    EnableSsl             = true,
                    UseBinary             = true,
                    TransmissionDirection = TransmissionDirection.Inbound,
                    ClientCertificate = new CertificateData
                    {
                        StoreLocation = StoreLocation.LocalMachine,
                        StoreName     = StoreName.My,
                        FindBy        = X509FindType.FindByThumbprint,
                        FindByValue   = "8114d495c16247dac51c586af91eb0d42c2d1ccf",
                    },
                    Credentials = new FtpUserCredentials
                    {
                        UserName = "val",
                        Password = "Almond123"
                    },
                };

                using (var rdr = new StreamReader(site.ListFiles()))
                    Console.WriteLine(rdr.ReadToEnd());
            }
            catch (Exception x)
            {
                var dump = x.DumpString();

                Console.WriteLine(dump);
                Debug.WriteLine(dump);
            }

            Console.WriteLine("Press any key to finish...");
            Console.ReadKey(true);
        }
    }
}
