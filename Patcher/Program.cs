using System;
using System.IO;
using System.IO.Compression;

using System.Threading.Tasks;

using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;


using Firebase.Storage;
using Firebase.Auth;

using Newtonsoft.Json;

namespace Patcher
{
    class Program
    {


        static string update_cfg = Directory.GetCurrentDirectory() + "\\update.ini";
        static string update_kat = Directory.GetCurrentDirectory() + "\\update";

        static string zipFile = Directory.GetCurrentDirectory() + "\\main.pak";


        static Update up = new Update(new Version(0, 0, 0, 0), "");

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
            IFirebaseConfig config_firebase = new FireSharp.Config.FirebaseConfig
            {
                AuthSecret = "rZXJ8IQrYyIHujqkoeGt8YKF23G1bqjSR98bBsiH",
                BasePath = "https://testak2-705bc-default-rtdb.europe-west1.firebasedatabase.app/"
            };

            IFirebaseClient client;

            client = new FireSharp.FirebaseClient(config_firebase);
            if (client != null)
            {
                Console.WriteLine("Firebase: Connected");
                await Task.Run(() => uploader());

                Console.WriteLine("-------------------------------------");
                Console.WriteLine("----------------DEBUG----------------");
                Console.WriteLine($"Url: {up.url} werjsa {up.wersja.ToString()}");
                Console.WriteLine("-------------------------------------");
                FirebaseResponse response = await client.UpdateAsync("Aktualizacje/prod", up);
                Console.WriteLine("Zaktualizowano dane w bazie!");
            }
            else
            {
                Console.WriteLine("Firebase: Unable to connect");
            }
        }   
        static async Task uploader() {
            
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Testak 2.0 Uploader Software");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Aby zaktualizować pliki na serwerze musisz mieć utworzony katalog 'update' i plik update.ini, jeśli ich nie ma zostaną utworzone automatycznie. Do folderu 'update' przenieś wszystkie pliki które mają być zawarte w aktualizacji.");
            if (!File.Exists(update_cfg)) {
                up.wersja = new Version(0, 0, 0, 0);
                string result = JsonConvert.SerializeObject(up);
                File.WriteAllText(update_cfg, result);
            }
            else{
                string stream = File.ReadAllText(update_cfg);
                up = JsonConvert.DeserializeObject<Update>(stream);
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
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Upload");
            await Task.Run(() => upload());          
            Console.WriteLine("---------------------------------");
        }
        static async Task compression() {
            Console.WriteLine("Compress: TEGO");
            Task.Run(() => ZipFile.CreateFromDirectory(update_kat, zipFile)).GetAwaiter().GetResult(); 
            Console.WriteLine("Compress: Tamtego");
        }
        static async Task upload() {
            Console.WriteLine("Open?");
            var stream = File.Open(Directory.GetCurrentDirectory() + "/main.pak", FileMode.Open);
            Console.WriteLine("Open");
            var auth = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig("AIzaSyB_7MzUXDD6-3g6h6Dvbfwuadt_rFrXZCE"));
            var a = await auth.SignInWithEmailAndPasswordAsync("user@uploader.net", "asdf1234");
            Console.WriteLine("Login");
            var task = new FirebaseStorage(
                "testak2-705bc.appspot.com",
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true,
                })
            .Child("testak")
            .Child("full")
            .Child("main.pak")
            .PutAsync(stream);

            task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Postęp uploadu: {e.Percentage}/100 %");
            var downloadUrl = await task;
            up.url = downloadUrl;
            Console.WriteLine("Plik dostępny pod adresem: " + up.url + " rozpoczynam aktualizację bazy.");            
        }
    }
    public class Update
    {
        public Update(Version wersja, string url_do_pobrania,bool isDev = false)
        {
            this.wersja = wersja;
            this.url = url_do_pobrania;
            this.isDev = isDev;
        }
        public Version wersja { get; set; } // budowa jest taka: major,minor,build,revision. Revision będzie tylko 0 lub wiecej, 0 oznacza wersję produkcyjną, powyżej oznacza rc tj. 0.0.0.2 => 0.0.0 - rc2
        public string url { set; get; } // domyślnie jest to na sztywno 

        public bool isDev { set; get; }
    }
}
