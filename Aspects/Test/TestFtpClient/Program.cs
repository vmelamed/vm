using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                var config = new TransferClientConfiguration
                {
                    EnableSsl = true,
                    UseBinary = true,
                    UserName  = "user",
                    Password  = "password",
                };

                config.Link = "ftp://localhost/outgoing";

                var site1 = new TransferClient(config);

                config.Link = "ftp://localhost/incoming";

                var site2 = new TransferClient(config);

                var files = new FtpParseMSDosListStreams()
                                    .Parse(site1.ListFiles())
                                    .Take(4)
                                    .ToList()
                                    ;
                var file1 = files[0];
                var file2 = files[1];
                var file3 = files[2];
                var file4 = files[3];

                // download to file from site1 to a local file and then upload it to site2
                using (var fileStream = new FileStream(file1.Name, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var outStream = site1.DownloadFile(file1.Name))
                    outStream.CopyTo(fileStream);

                using (var fileStream = new FileStream(file1.Name, FileMode.Open, FileAccess.Read, FileShare.None))
                    site2.UploadFile(fileStream, file1.Name);

                // ----------------------------------------------------------

                // download from site1 and then upload it to site2 w/o intermediate local file
                using (var outStream = site1.DownloadFile(file2.Name))
                    site2.UploadFile(outStream, file2.Name);

                // ----------------------------------------------------------

                // async download to file from site1 to a local file and then async upload it to site2
                using (var fileStream = new FileStream(file3.Name, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var outStream = site1.DownloadFileAsync(file3.Name).Result)
                    outStream.CopyTo(fileStream);

                using (var fileStream = new FileStream(file3.Name, FileMode.Open, FileAccess.Read, FileShare.None))
                    site2.UploadFileAsync(fileStream, file3.Name).GetAwaiter().GetResult();

                // ----------------------------------------------------------

                // download from site1 and then upload it to site2 w/o intermediate local file
                using (var outStream = site1.DownloadFileAsync(file4.Name).Result)
                    site2.UploadFileAsync(outStream, file4.Name).GetAwaiter().GetResult();
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
