using System;
using System.IO;
using System.Windows;

using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;

namespace Testak2
{
    /// <summary>
    /// Logika interakcji dla klasy Updater.xaml
    /// </summary>
    public partial class Updater : Window
    {
        public string config_file = Directory.GetCurrentDirectory() + "\\config.ini";


        IFirebaseConfig config_firebase = new FirebaseConfig
        {
            AuthSecret = "rZXJ8IQrYyIHujqkoeGt8YKF23G1bqjSR98bBsiH",
            BasePath = "https://testak2-705bc-default-rtdb.europe-west1.firebasedatabase.app/"
        };

        IFirebaseClient client;


        public Updater()
        {
            InitializeComponent();
            if (FirebaseSetup()) {
                firebase_status.Text = "Firebase: Połączony";
                //setupTemplate();
                config_setup();
            }
        }
        async void config_setup() {
            Config config;
            if (File.Exists(config_file))
            {
                string read = File.ReadAllText(config_file);
                config = JsonConvert.DeserializeObject<Config>(read);
            }
            else {
                FirebaseResponse meta = await client.GetAsync("Aktualizacje/prod");
                config = meta.ResultAs<Config>();

                config.isDebug = false;

                string write = JsonConvert.SerializeObject(config);

                File.WriteAllText(config_file, write);
            }

        }
        bool FirebaseSetup()
        {
            client = new FireSharp.FirebaseClient(config_firebase);
            if (client != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        async void setupTemplate() {
            //dev
            Version version_dev = new Version(0, 1, 0, 0);
            Update update_dev = new Update(version_dev, "");
            SetResponse response_dev = await client.SetAsync("Aktualizacje/dev", update_dev);
            Console.WriteLine("Dodano dev do firebase: " + response_dev.StatusCode);

            //prod
            Version version = new Version(0, 1, 0, 0);
            Update update = new Update(version, "");
            SetResponse response = await client.SetAsync("Aktualizacje/prod", update);
            Console.WriteLine("Dodano dev do firebase: " + response.StatusCode);
        }
    }
    public class Update{
        public Update(Version wersja, string url_do_pobrania) {
            this.wersja = wersja;
            this.url = url_do_pobrania;
        }
        public Version wersja { get; set; } // budowa jest taka: major,minor,build,revision. Revision będzie tylko 0 lub wiecej, 0 oznacza wersję produkcyjną, powyżej oznacza rc tj. 0.0.0.2 => 0.0.0 - rc2
        public string url { set; get; } // domyślnie jest to na sztywno np. wickedlauncher.5v.pl/cos/ tutaj ta zmienna
    }

    public class Config {
        public Config(bool isDebug, Version wersja) {
            this.isDebug = isDebug;
            this.wersja = wersja;
        }

        public bool isDebug { get; set; }
        public Version wersja { get; set; }
    }
}
