using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Net.Sockets;
using GestioneNetworkECodifica;

namespace ImpiccatoGiocatore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool abilitazioneMessaggio;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void SocketReceive(object socketSource)
        {
            //Definizione dell'endPoint di ascolto
            IPEndPoint listenerEndPoint = socketSource as IPEndPoint;
            //Creazione socket di ascolto
            Socket socket = new Socket(listenerEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(listenerEndPoint);
            switch (listenerEndPoint.Port)
            {
                case 61500:
                    {
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
                                    abilitazioneMessaggio = true;
                                    messaggio = string.Empty;
                                    //Otteniamo il numero di caratteri ricevuto.
                                    nCaratteriRicevuti = socket.Receive(byteRicevuti, byteRicevuti.Length, 0);
                                    //Trasformiamo il numero ricevuto nella codifica ascii e otteniamo quindi un messaggio di stringa.
                                    messaggio += Encoding.ASCII.GetString(byteRicevuti, 0, nCaratteriRicevuti);
                                    this.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        bkParola.Text = messaggio;
                                    }));
                                }
                            }
                        });
                        break;
                    }
                case 61501:
                    {
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
                        break;
                    }
                case 61502:
                    {
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
                                        bkCounter.Text = messaggio;
                                    }));
                                }
                            }
                        });
                        break;
                    }
            }
        }

        //Invio del messaggio:
        private void btnInviaParola_Click(object sender, RoutedEventArgs e)
        {
            txtMessaggio.Text = string.Empty;
            //Recuperato indirizzo ip destinatario e la sua porta.
            IPAddress destIpAddress = IPAddress.Parse(txtDestinationIp.Text);
            int destinationPortNumber = int.Parse(txtPort.Text);

            //Esecuzione metodo di invio del messaggio.
            SocketSend(destIpAddress, destinationPortNumber, $"- {txtUsername.Text}: {txtMessaggio.Text.ToLower()}");
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
            socket.SendTo(byteInviati, destinationEndPoint);
        }

        //Controllo della casella di testo del messaggio.
        private void txtMessaggio_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool controllo = true;
            for (int i = 0; i < lstParoleRicevute.Items.Count; i++)
            {
                int index = lstParoleRicevute.Items[i].ToString().IndexOf(':') + 1;
                if (lstParoleRicevute.Items[i].ToString().Substring(index + 1) == txtMessaggio.Text)
                {
                    controllo = false;
                }
            }
            if (!string.IsNullOrWhiteSpace(txtMessaggio.Text) && abilitazioneMessaggio && controllo)
            {
                btnInviaParola.IsEnabled = true;
            }
            else
            {
                btnInviaParola.IsEnabled = false;
            }
        }

        private void btnUsernameConfirm_Click(object sender, RoutedEventArgs e)
        {
            txtUsername.Text = txtUsername.Text.Trim();
            txtUsername.IsEnabled = false;
            btnUsernameConfirm.IsEnabled = false;
            txtMessaggio.IsEnabled = true;
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtUsername.Text) && !txtUsername.Text.Contains(":")) 
            {
                btnUsernameConfirm.IsEnabled = true;
                txtUsername.Background = Brushes.LightGreen;
            }
            else
            {
                btnUsernameConfirm.IsEnabled = false;
                txtUsername.Background = Brushes.LightCoral;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUsername.Background = Brushes.LightCoral;
            txtPort.Text = "60000";
            Thread startDataListener = new Thread(new ParameterizedThreadStart(SocketReceive));
            IPAddress destIP = GestioneNetwork.OttieniIPLocale();
            IPEndPoint data2EndPoint = new IPEndPoint(destIP, 61500);
            startDataListener.Start(data2EndPoint);
            Thread startSecondDataListener = new Thread(new ParameterizedThreadStart(SocketReceive));
            destIP = GestioneNetwork.OttieniIPLocale();
            data2EndPoint = new IPEndPoint(destIP, 61501);
            startSecondDataListener.Start(data2EndPoint);
            Thread startThirdDataListener = new Thread(new ParameterizedThreadStart(SocketReceive));
            destIP = GestioneNetwork.OttieniIPLocale();
            data2EndPoint = new IPEndPoint(destIP, 61502);
            startThirdDataListener.Start(data2EndPoint);
            txtPort.Background = Brushes.LightGreen;
            abilitazioneMessaggio = false;
        }

        private void txtInfo_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool controllo2 = GestioneNetwork.ControllaTestoPerIP(txtDestinationIp.Text);
            bool controllo1 = GestioneNetwork.VerificaPorta(txtPort.Text, 61500, 61501,61502);
            txtPort.Background = controllo1 ? Brushes.LightGreen : Brushes.LightCoral;
            txtDestinationIp.Background = controllo2 ? Brushes.LightGreen : Brushes.LightCoral;
            btnConfermaDestinazione.IsEnabled = controllo1 && controllo2;
        }

        private void btnConfermaDestinazione_Click(object sender, RoutedEventArgs e)
        {
            txtDestinationIp.IsEnabled = false;
            txtPort.IsEnabled = false;
            txtUsername.IsEnabled = true;
            btnConfermaDestinazione.IsEnabled = false;
            btnUsernameConfirm.IsEnabled = true;
        }
    }
}

