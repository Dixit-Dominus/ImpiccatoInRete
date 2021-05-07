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
        private List<IPAddress> DestinationIps { get; set; }

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
            startListening.Start(new IPEndPoint(IPAddress.Parse(txtSourceIp.Text), int.Parse(txtPort.Text)));
            ListaParole = new List<string>();
            DestinationIps = new List<IPAddress>();
            Reset();
            ParolaCorrente = GeneraParolaCasuale();
            ParolaCodificata = GestioneImpiccato.CodificaParola(ParolaCorrente);
            bkParola.Text = ParolaCodificata;
        }

        //Gestione della ricezione di dati da parte di comunicazioni TCP in entrata sulla porta 60000
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
                    string ip = string.Empty;
                    int index = clientCollegato.Client.RemoteEndPoint.ToString().IndexOf(':');
                    for (int j = 0; j < index; j++)
                    {
                        ip += clientCollegato.Client.RemoteEndPoint.ToString()[j];
                    }
                    IPAddress destinationIpAddress = IPAddress.Parse(ip);
                    //Controllo se chi comunica con il server ha già l'IP inserito nella lista per evitare ripetizioni.
                    bool controllo = true;
                    for (int k = 0; k < DestinationIps.Count; k++)
                    {
                        if (destinationIpAddress.Equals(DestinationIps[k]))
                        {
                            controllo = false;
                        }
                    }
                    if (controllo)
                    {
                        DestinationIps.Add(destinationIpAddress);
                    }
                    NetworkStream stream = clientCollegato.GetStream();
                    int i = 0;
                    while ((i = stream.Read(bytesDati, 0, bytesDati.Length)) != 0)
                    {
                        datiRicevuti = Encoding.ASCII.GetString(bytesDati, 0, i);
                        datiRicevuti = datiRicevuti.ToLower();
                        if (datiRicevuti != "partreqmessage")
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                lstParoleRicevute.Items.Add(datiRicevuti);
                                btnScorriParola.IsEnabled = true;
                                DistribuisciParolaAConnessi(datiRicevuti + "*",61500);
                                DistribuisciParolaAConnessi(bkCounter.Text + "?",61500);
                            }));
                        }
                        else
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (!GestioneNetwork.TCPSend(DestinationIps[DestinationIps.Count - 1].ToString(), 61500, bkParola.Text))
                                {
                                    MessageBox.Show($"Nessun canale di ascolto disponibile per il socket: {DestinationIps[DestinationIps.Count - 1]}:{txtPort.Text}.", "Connessione Non Riuscita", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                                else
                                {
                                    DistribuisciParolaAConnessi(bkCounter.Text, 61500);
                                }
                            }));
                        }
                    }
                    stream.Close();
                    clientCollegato.Close();
                }
            });
        }

        //Al click del bottone scorri parola, controllo correttezza parola ricevuta.
        private void btnScorriParola_Click(object sender, RoutedEventArgs e)
        {
            string parolaSelezionata = lstParoleRicevute.Items[SelectIndex].ToString();
            string parolaIntera = parolaSelezionata;
            lstParoleRicevute.Items[SelectIndex] = parolaSelezionata + "  V";
            int index = parolaSelezionata.LastIndexOf(' ') + 1;
            int indexPunti = parolaIntera.IndexOf(':');
            parolaSelezionata = parolaSelezionata.Substring(index);
            string username = string.Empty;
            for (int i = 0; i < indexPunti; i++)
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
                    bkParola.Text = $"- {username} - ha vinto! -> {ParolaCorrente}.";
                }
                else
                {
                    ErrorCounter--;
                    bkCounter.Text = ErrorCounter.ToString();
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
                            bkParola.Text = $"- {username} - ha vinto! -> {ParolaCorrente}.";
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
            if (lstParoleRicevute.Items.Count == SelectIndex)
            {
                btnScorriParola.IsEnabled = false;
            }
            else
            {
                btnScorriParola.IsEnabled = true;
            }
            DistribuisciParolaAConnessi(bkParola.Text,61500);
        }

        private void btnGeneraNuovaParola_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            btnMostraCorrente.IsEnabled = true;
            ParolaCorrente = GeneraParolaCasuale();
            ParolaCodificata = GestioneImpiccato.CodificaParola(ParolaCorrente);
            bkParola.Text = ParolaCodificata;
            MessageBox.Show(ParolaCorrente, "Parola generata:", MessageBoxButton.OK, MessageBoxImage.Information);
            DistribuisciParolaAConnessi(bkParola.Text,61500);
        }

        private void DistribuisciParolaAConnessi(string mex,int port)
        {
            for (int i = 0; i < DestinationIps.Count; i++)
            {
                if (!GestioneNetwork.TCPSend(DestinationIps[i].ToString(), port, mex))
                {
                    MessageBox.Show($"Nessun canale di ascolto disponibile per il socket: { DestinationIps[i]}:{ txtPort.Text}.", "Connessione Non Riuscita", MessageBoxButton.OK, MessageBoxImage.Warning);
                    DestinationIps.RemoveAt(i);
                }
            }
        }

        private void Reset()
        {
            ErrorCounter = 13;
            bkCounter.Text = ErrorCounter.ToString();
            SelectIndex = 0;
            lstParoleRicevute.Items.Clear();
            StreamReader leggiparole = new StreamReader("parole.txt");
            while (!leggiparole.EndOfStream)
            {
                ListaParole.Add(leggiparole.ReadLine());
            }
            leggiparole.Close();
        }

        private string GeneraParolaCasuale()
        {
            Random rnd = new Random();
            int nParolaRandom = rnd.Next(0, ListaParole.Count);
            return ListaParole[nParolaRandom];
        }

        private void bkCounter_TextChanged(object sender, TextChangedEventArgs e)
        {
            imgImpiccato.Source = new BitmapImage(new Uri($"\\Immagini\\imp{ErrorCounter}.png", UriKind.RelativeOrAbsolute));
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
            bool controllo1 = GestioneNetwork.VerificaPorta(txtPort.Text, 61500, 61501, 61502);
            bool controllo2 = GestioneNetwork.ControllaTestoPerIP(txtSourceIp.Text);
            txtPort.Background = controllo1 ? Brushes.LightGreen : Brushes.LightCoral;
            txtSourceIp.Background = controllo2 ? Brushes.LightGreen : Brushes.LightCoral;
            btnCreaSocket.IsEnabled = controllo2 && controllo1;
        }
    }
}
