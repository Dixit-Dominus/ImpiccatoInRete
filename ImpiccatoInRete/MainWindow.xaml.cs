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

namespace ImpiccatoInRete
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Parola generata casualmente per il gioco.
        private string ParolaCorrente { get; set; }
        //Lista delle parole ricevute dal socket di ascolto.
        private List<string> listaParole { get; set; }

        //Metodo di aggiornamento della lista di parole.
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        //Metodo di invio del messaggio
        private void SocketSend(IPAddress destinationIp, int destinationPort, string message)
        {
            //Codifica in byte del messaggio da inviare
            Byte[] byteInviati = Encoding.ASCII.GetBytes(message.ToCharArray());

            //Creazione socket di invio
            Socket socket = new Socket(destinationIp.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            //Creazione invio socket di ascolto
            IPEndPoint destinationEndPoint = new IPEndPoint(destinationIp, destinationPort);

            //Invio dei dati.
            socket.EnableBroadcast = true;
            socket.SendTo(byteInviati, destinationEndPoint);
        }

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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnCreaSocket_Click(object sender, RoutedEventArgs e)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.234"), int.Parse(txtPort.Text));
            Thread startListener = new Thread(new ParameterizedThreadStart(SocketReceive));
            startListener.Start(localEndPoint);
            btnCreaSocket.IsEnabled = false;
            txtPort.IsEnabled = false;
            txtSourceIp.IsEnabled = false;
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            //Recuperato indirizzo ip destinatario e la sua porta.
            IPAddress broadcastAddress = IPAddress.Parse("192.168.1.234");
            int destinationPortNumber = int.Parse(txtPort.Text);

            //Esecuzione metodo di invio del messaggio.
            SocketSend(broadcastAddress, destinationPortNumber, txtTest.Text);
        }
    }
}
