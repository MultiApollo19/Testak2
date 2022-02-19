using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Net;


using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace Testak2
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class Testak_main : Window
    {
        public ListaTestow listaTestow;

        bool isAdmin = false;

        int wybranyTest = 0;

        //Debug
        bool isDebug = true;
        string adminPsswd;


        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "rZXJ8IQrYyIHujqkoeGt8YKF23G1bqjSR98bBsiH",
            BasePath = "https://testak2-705bc-default-rtdb.europe-west1.firebasedatabase.app/"
        };

        IFirebaseClient client;

        public Testak_main()
        {
            InitializeComponent();

            if (FirebaseSetup())
            {
                
                FireBaseDownload();
                //blankTest();
                //admin psswd Debug
                // Firebase_admin.Content += adminPsswd;
            }
            
            
        }
        async void FireBaseDownload()
        {
            FirebaseResponse response = await client.GetAsync("admin"); //Admin
            if (response != null)
            {
                adminPsswd = response.ResultAs<string>();
                Console.WriteLine(adminPsswd);

                FirebaseResponse response1 = await client.GetAsync("Testy");
                listaTestow = response1.ResultAs<ListaTestow>();

                Console.WriteLine($"Liczba testów na serwerze: {listaTestow.ileTestow}");
            }
            
        }
        void setup_admin()
        {
            kod.Visibility = Visibility.Hidden;
            Admin.Visibility = Visibility.Visible;
            //blankTest();
            //Firebase_admin.Visibility = Visibility.Visible;
            for (int i = 0; i < listaTestow.ileTestow; i++)
            {
                string tmp =  listaTestow.testy[i].Tytul;
                admin_lista_testow.Items.Add(tmp);               
                Console.WriteLine($"Dodano do listy: {listaTestow.testy[i].Tytul}");
                
            }
            admin_tytul.Text = listaTestow.testy[wybranyTest].Tytul;
            admin_kod.Text = listaTestow.testy[wybranyTest].Kod;
            admin_haslo.Text = listaTestow.testy[wybranyTest].Haslo;
            admin_lista_testow.ScrollIntoView(admin_lista_testow.Items[admin_lista_testow.Items.Count - 1]);

            isAdmin = true;
        }
        bool FirebaseSetup()
        {
            client = new FireSharp.FirebaseClient(config);
            if (client != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void msg_box(string text)
        {
            MessageBox.Show(text, "ZŁY KOD");
        }

        private void kod_ok_Click(object sender, RoutedEventArgs e)
        {
            string tmp_input = kod_input.Text;

            if (tmp_input == "KOD") msg_box("Wprowadź pan kod kurła");
            if (tmp_input == adminPsswd) setup_admin();
        }

        

        async void blankTest()
        {
            List<Pytanie> pytania = new List<Pytanie>();

            pytania.Add(new Pytanie("Jak to jest być skrybą? Dobrze?", "Moim zdaniem to nie ma tak że dobrze...", "Na niewtajemniczonych robi to spore wrazenie", "Kalafiorowa", "Conajmniej -700", 1));
            pytania.Add(new Pytanie("Co sprawiło, że Michałek oślepł?", "Kapitan Bomba", "Hawajska", "Guma", "JJ Torpeda", 3));

            List<Test> TestLista = new List<Test>();
            TestLista.Add(new Test("Przykładowy test","Wiesiek", "Mianowicie",pytania.Count,pytania));
            TestLista.Add(new Test("Przykładowy test 2", "Amsterix", "Axterixm", pytania.Count, pytania));
            Console.WriteLine($"Liczba pytan: {pytania.Count}");

            ListaTestow listaTestow = new ListaTestow(TestLista.Count,TestLista);


            SetResponse response = await client.SetAsync("Testy", listaTestow);
            Console.WriteLine("Dodano dane do firebase: " + response.StatusCode);
        }

        private void admin_lista_testow_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string test = admin_lista_testow.SelectedItem.ToString();
            Console.WriteLine(test);
        }
    }
    public class ListaTestow {

        public ListaTestow(int ile, List<Test> testy) {
            this.testy = testy;
            this.ileTestow = ile;
        }

        public int ileTestow { set; get; }
        public List<Test> testy { set; get; }
    }

    public class Test
    {

        public Test(string tytul,string kod, string haslo,int ilePytan, List<Pytanie> pytania)
        {
            this.Tytul = tytul;
            this.Kod = kod;
            this.Haslo = haslo;
            this.Pytania = pytania;
            this.ilePytan = ilePytan;
        }
        public string Tytul { set; get; }
        public string Kod { set; get; }
        public int ilePytan { set; get; }
        public string Haslo { set; get; }
        public List<Pytanie> Pytania = new List<Pytanie>();
        
    }
    public class Pytanie
    {
        public Pytanie(string tresc, string odpowiedz1, string odpowiedz2, string odpowiedz3, string odpowiedz4, int poprawnaOdpowiedz)
        {
            //Console.WriteLine($"Treść:{tresc} odp1: {odpowiedz1} odp2: {odpowiedz2} odp3: {odpowiedz3} odp4: {odpowiedz4} odp prawidlowa: {poprawnaOdpowiedz}");
            this.Tresc = tresc;
            odpowiedzi.Add(odpowiedz1);
            odpowiedzi.Add(odpowiedz2);
            odpowiedzi.Add(odpowiedz3);
            odpowiedzi.Add(odpowiedz4);
            this.PoprawnaOdpowiedz = poprawnaOdpowiedz - 1;
        }
        public string Tresc { set; get; }
        public List<string> odpowiedzi = new List<string>();
        public int PoprawnaOdpowiedz { set; get; }
    }

}
