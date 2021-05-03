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

        //Inizzializzazione componenti della finestra,
        public MainWindow()
        {
            InitializeComponent();
        }
        //Metodo di avvio della finestra.
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtPort.Text = "60000";
            txtSourceIp.Text = GestioneNetwork.OttieniIPLocale();
            SelectIndex = 0;
            ErrorCounter = 0;
            ListaParole = new List<string>();
            StreamReader leggiparole = new StreamReader("parole.txt");
            while (!leggiparole.EndOfStream)
            {
                ListaParole.Add( leggiparole.ReadLine());
            }
            leggiparole.Close();
        }

        //Metodo di invio del messaggio
        private void SocketSend(IPAddress destinationIp, int destinationPort, string message)
        {
            //Codifica in byte del messaggio da inviare
            Byte[] byteInviati = Encoding.ASCII.GetBytes(message);

            //Creazione socket di invio
            Socket socket = new Socket(destinationIp.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            //Creazione invio socket di ascolto
            IPEndPoint destinationEndPoint = new IPEndPoint(destinationIp, destinationPort);

            //Invio dei dati.
            socket.EnableBroadcast = true;
            socket.SendTo(byteInviati, destinationEndPoint);
        }
        //Metodo di ricezione dei messaggi, attivazione del canale di ascolto.
        private async void SocketReceive(object socketSource)
        {
            //Definizione dell'endPoint di ascolto
            IPEndPoint listenerEndPoint = socketSource as IPEndPoint;

            //Creazione socket di ascolto
            Socket socket = new Socket(listenerEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(listenerEndPoint);

            //Messaggio grande massimo 256 caratteri.
            Byte[] byteRicevuti = new byte[256];
            string messaggio = string.Empty;
            int nCaratteriRicevuti;

            //Istruzione che non va a bloccare la task della finesta (Non si blocca l'interfaccia)
            await Task.Run(() =>
            {
                //Ascolto continuo
                while (true)
                {
                    //Se c'è qualcosa di ricevuto nel socket.
                    if (socket.Available > 0)
                    {
                        messaggio = string.Empty;
                        //Otteniamo il numero di caratteri ricevuto.
                        nCaratteriRicevuti = socket.Receive(byteRicevuti, byteRicevuti.Length, 0);
                        //Trasformiamo il numero ricevuto nella codifica ascii e otteniamo quindi un messaggio di stringa.
                        messaggio += Encoding.ASCII.GetString(byteRicevuti, 0, nCaratteriRicevuti);
                        //Viene aggiornata l'interfaccia grafica in modo asincrono.
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            lstParoleRicevute.Items.Add(messaggio);
                        }));
                    }
                }
            });
        }
        //Metodo di creazione del socket di ascolto.
        private void btnCreaSocket_Click(object sender, RoutedEventArgs e)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(txtSourceIp.Text), int.Parse(txtPort.Text));
            Thread startListener = new Thread(new ParameterizedThreadStart(SocketReceive));
            startListener.Start(localEndPoint);
            btnCreaSocket.IsEnabled = false;
            txtPort.IsEnabled = false;
            txtSourceIp.IsEnabled = false;
        }
        //Al click del bottone scorri parola, controllo correttezza parola ricevuta.
        private void btnScorriParola_Click(object sender, RoutedEventArgs e)
        {
            string parolaSelezionata = lstParoleRicevute.Items[SelectIndex].ToString();
            int index = parolaSelezionata.IndexOf(':') + 1;
            parolaSelezionata = parolaSelezionata.Substring(index + 1);
            MessageBox.Show(parolaSelezionata);

            //Index di selezione della parola ricevuta.
            SelectIndex++;

            if (parolaSelezionata.Length != 1)
            {
                if (parolaSelezionata == ParolaCorrente)
                {
                    bkParola.Text = ParolaCorrente;
                }
                else
                {
                    ErrorCounter++;
                }
            }
            else
            {
                char carattere = char.Parse(parolaSelezionata);
                for (int i = 0; i < ParolaCodificata.Length; i++)
                {
                    if (ParolaCorrente[i] == carattere)
                    {
                        char[] chars = ParolaCodificata.ToCharArray();
                        chars[i] = carattere;
                        ParolaCodificata = new string(chars);
                    }
                }
            }
           

            //Recuperato indirizzo ip destinatario e la sua porta.
            IPAddress broadcastAddress = IPAddress.Parse(txtDestIp.Text);
            int destinationPortNumber = 61500;

            //Esecuzione metodo di invio del messaggio.
            SocketSend(broadcastAddress, destinationPortNumber, ParolaCodificata);
            bkParola.Text = ParolaCodificata;
        }

        private void btnGeneraNuovaParola_Click(object sender, RoutedEventArgs e)
        {
            ErrorCounter = 0;
            SelectIndex = 0;
            lstParoleRicevute.Items.Clear();
            Random rnd = new Random();
            int nParolaRandom = rnd.Next(0,ListaParole.Count);
            ParolaCorrente = ListaParole[nParolaRandom];
            ParolaCodificata = GestioneImpiccato.CodificaParola(ParolaCorrente);
            bkParola.Text = ParolaCodificata;
            MessageBox.Show(ParolaCorrente);
        }

    }
}
