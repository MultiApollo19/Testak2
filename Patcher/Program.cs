using System;
using System.IO;
using System.IO.Compression;

using System.Threading.Tasks;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using FluentFTP;
using Newtonsoft.Json;

namespace Patcher
{
    class Program
    {


        static string update_cfg = Directory.GetCurrentDirectory() + "\\update.ini";
        static string update_kat = Directory.GetCurrentDirectory() + "\\update";

        static string zipFile = Directory.GetCurrentDirectory() + "\\temp.pak";

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                checkFirebase();
            }).GetAwaiter().GetResult();


            Console.ReadLine();
        }
        static async Task checkFirebase()
        {
            IFirebaseConfig config_firebase = new FirebaseConfig
            {
                AuthSecret = "rZXJ8IQrYyIHujqkoeGt8YKF23G1bqjSR98bBsiH",
                BasePath = "https://testak2-705bc-default-rtdb.europe-west1.firebasedatabase.app/"
            };

            IFirebaseClient client;

            client = new FireSharp.FirebaseClient(config_firebase);
            if (client != null)
            {
                Console.WriteLine("Firebase: Connected");
                await Task.Run(() => checkFTP());
            }
            else
            {
                Console.WriteLine("Firebase: Unable to connect");
            }
        }
        static async Task checkFTP() {
            FtpClient client = new FtpClient("multiapollo19.prv.pl", "multiapollo19@prv.pl", "testaczegg");
            await client.AutoConnectAsync();

            if (client.IsConnected)
            {
                Console.WriteLine("FTP: Connected");
                
                await Task.Run(() => uploader());

                Console.WriteLine("FTP: Wrzucam");
                await client.UploadFileAsync(zipFile, "/Testag/prod/temp.pak", FtpRemoteExists.Overwrite,true);

            }
            else {
                Console.WriteLine("Firebase: Unable to connect: "+client.LastReply);
            }

            //await client.DisconnectAsync();
        }
        static async Task uploader() {
            Update up = new Update(new Version(0,0,0,0),"");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Testak 2.0 Uploader Software");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Aby zaktualizować pliki na serwerze musisz mieć utworzony katalog 'update' i plik update.ini, jeśli ich nie ma zostaną utworzone automatycznie. Do folderu 'update' przenieś wszystkie pliki które mają być zawarte w aktualizacji.");
            if (!File.Exists(update_cfg)) {
                up.wersja = new Version(0, 0, 0, 0);
                string result = JsonConvert.SerializeObject(up);
                File.WriteAllText(update_cfg, result);
            }
            if (!Directory.Exists(update_kat)) {
                Directory.CreateDirectory(update_kat);
            }
            if (File.Exists(zipFile)) {
                File.Delete(zipFile);
            }
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Kompresja plików...");
            await Task.Run(() => compression());
            Console.WriteLine("Ukończono");
        }
        static async Task compression() {
            Console.WriteLine("Compress: TEGO");
            Task.Run(() => ZipFile.CreateFromDirectory(update_kat, zipFile));
            Console.WriteLine("Compress: Tamtego");
        }
    }
    public class Update
    {
        public Update(Version wersja, string url_do_pobrania)
        {
            this.wersja = wersja;
            this.url = url_do_pobrania;
        }
        public Version wersja { get; set; } // budowa jest taka: major,minor,build,revision. Revision będzie tylko 0 lub wiecej, 0 oznacza wersję produkcyjną, powyżej oznacza rc tj. 0.0.0.2 => 0.0.0 - rc2
        public string url { set; get; } // domyślnie jest to na sztywno np. wickedlauncher.5v.pl/cos/ tutaj ta zmienna
    }
}
