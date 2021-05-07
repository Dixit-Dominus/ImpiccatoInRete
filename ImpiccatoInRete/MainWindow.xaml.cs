using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using GestioneNetworkECodifica;

namespace ImpiccatoInRete
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Parola generata casualmente per il gioco.
        private string ParolaCorrente { get; set; }
        private List<string> ListaParole { get; set; }
        private int SelectIndex { get; set; }
        private string ParolaCodificata { get; set; }
        private int ErrorCounter { get; set; }
        private int DataDestinationPortAddress { get; set; }
        private IPAddress DestinationIp { get; set; }

        //Inizzializzazione componenti della finestra,
        public MainWindow()
        {
            InitializeComponent();
        }
        //Metodo di avvio della finestra.
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtPort.Text = "60000";
            txtSourceIp.Text = GestioneNetwork.OttieniIPLocale().ToString();
            Thread startListening = new Thread(new ParameterizedThreadStart(TCPListen));
            startListening.Start(new IPEndPoint(IPAddress.Parse(txtSourceIp.Text),int.Parse(txtPort.Text)));
            SelectIndex = 0;
            DataDestinationPortAddress = 61500;
            ErrorCounter = 10;
            ListaParole = new List<string>();
            StreamReader leggiparole = new StreamReader("parole.txt");
            while (!leggiparole.EndOfStream)
            {
                ListaParole.Add( leggiparole.ReadLine());
            }
            leggiparole.Close();
        }

        private async void TCPListen(object sourceEndPoint)
        {
            IPEndPoint ipAndPort = sourceEndPoint as IPEndPoint;
            TcpListener listener = new TcpListener(ipAndPort.Address, ipAndPort.Port);
            listener.Start();
            Byte[] bytesDati = new byte[256];

            await Task.Run(() =>
            {
                while (true)
                {
                    string datiRicevuti = string.Empty;
                    TcpClient clientCollegato = listener.AcceptTcpClient();
                    NetworkStream stream = clientCollegato.GetStream();
                    int i = 0;
                    while ((i = stream.Read(bytesDati, 0, bytesDati.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        datiRicevuti = System.Text.Encoding.ASCII.GetString(bytesDati, 0, i);
                        datiRicevuti = datiRicevuti.ToLower();
                    }
                }
            });
        }

        //Al click del bottone scorri parola, controllo correttezza parola ricevuta.
        private void btnScorriParola_Click(object sender, RoutedEventArgs e)
        {
            string parolaSelezionata = lstParoleRicevute.Items[SelectIndex].ToString();
            string parolaIntera = parolaSelezionata;
            lstParoleRicevute.Items[SelectIndex] = parolaSelezionata + "  V";
            int index = parolaSelezionata.IndexOf(':') + 1;
            parolaSelezionata = parolaSelezionata.Substring(index + 1);
            string username = string.Empty;
            for (int i = 0; i < index - 1; i++)
            {
                username += parolaIntera[i];
            }

            //Index di selezione della parola ricevuta.
            SelectIndex++;

            if (parolaSelezionata.Length != 1)
            {
                if (parolaSelezionata == ParolaCorrente)
                {
                    ParolaCodificata = ParolaCorrente;
                    bkParola.FontSize = 15;
                    bkParola.Text = $"Il giocatore {username} - ha vinto! La parola era {ParolaCorrente}.";
                }
                else
                {
                    ErrorCounter--;
                }
            }
            else
            {
                bool ok = false;
                char carattere = char.Parse(parolaSelezionata);
                for (int i = 0; i < ParolaCodificata.Length; i++)
                {
                    if (ParolaCorrente[i] == carattere)
                    {
                        char[] chars = ParolaCodificata.ToCharArray();
                        chars[i] = carattere;
                        ParolaCodificata = new string(chars);
                        bkParola.Text = ParolaCodificata;
                        if (ParolaCodificata == ParolaCorrente)
                        {
                            bkParola.FontSize = 15;
                            bkParola.Text = $"Il giocatore {username} - ha vinto! La parola era {ParolaCorrente}.";
                            lstParoleRicevute.Items.Clear();
                        }
                        ok = true;
                    }
                }
                if (!ok)
                {
                    ErrorCounter--;
                    bkCounter.Text = ErrorCounter.ToString();
                }
            }


            //Recuperato indirizzo IP destinatario.
            DestinationIp = IPAddress.Parse(txtDestIp.Text);

            //Esecuzione metodo di invio del messaggio.
            DataDestinationPortAddress = 61501;
            if (lstParoleRicevute.Items.Count == SelectIndex)
            {
                btnScorriParola.IsEnabled = false;
            }
            else
            {
                btnScorriParola.IsEnabled = true;
            }
        }

        private void btnGeneraNuovaParola_Click(object sender, RoutedEventArgs e)
        {
            bkCounter.Text = "10";
            btnMostraCorrente.IsEnabled = true;
            ErrorCounter = 10;
            SelectIndex = 0;
            lstParoleRicevute.Items.Clear();
            Random rnd = new Random();
            int nParolaRandom = rnd.Next(0,ListaParole.Count);
            ParolaCorrente = ListaParole[nParolaRandom];
            ParolaCodificata = GestioneImpiccato.CodificaParola(ParolaCorrente);
            bkParola.Text = ParolaCodificata;
            MessageBox.Show(ParolaCorrente, "Parola generata:", MessageBoxButton.OK, MessageBoxImage.Information);
            DestinationIp = IPAddress.Parse(txtDestIp.Text);
            DataDestinationPortAddress = 61500;
        }

        private void bkCounter_TextChanged(object sender, TextChangedEventArgs e)
        {
            imgImpiccato.Source = new BitmapImage(new Uri($"\\Immagini\\imp{ErrorCounter}.png",UriKind.RelativeOrAbsolute));
            if (int.Parse(bkCounter.Text) <= 3)
            {
                bkCounter.Foreground = Brushes.Red;
                if (int.Parse(bkCounter.Text) == 0)
                {
                    lstParoleRicevute.Items.Clear();
                }
            }
            else
            {
                bkCounter.Foreground = Brushes.SpringGreen;
            }
        }

        private void btnMostraCorrente_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(ParolaCorrente, "Parola generata:", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void txtSoPo_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool controllo1 = GestioneNetwork.VerificaPorta(txtPort.Text, 61500, 61501,61502);
            bool controllo2 = GestioneNetwork.ControllaTestoPerIP(txtSourceIp.Text);
            txtPort.Background = controllo1 ? Brushes.LightGreen : Brushes.LightCoral;
            txtSourceIp.Background = controllo2 ? Brushes.LightGreen : Brushes.LightCoral;
            btnCreaSocket.IsEnabled = controllo2 && controllo1;
        }

        private void txtDestIp_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool controllo = GestioneNetwork.ControllaTestoPerIP(txtDestIp.Text);
            txtDestIp.Background =  controllo ? Brushes.LightGreen : Brushes.LightCoral;
            btnGeneraNuovaParola.IsEnabled = controllo;
        }
    }
}
