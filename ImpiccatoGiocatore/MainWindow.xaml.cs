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
        public MainWindow()
        {
            InitializeComponent();
        }

        //Invio del messaggio:
        private void btnInviaParola_Click(object sender, RoutedEventArgs e)
        {
            //Recuperato indirizzo ip destinatario e la sua porta.
            if (!GestioneNetwork.TCPSend(txtDestinationIp.Text, int.Parse(txtPort.Text), txtUsername.Text + ": ---> " + txtMessaggio.Text))
            {
                MessageBox.Show("Errore, nessun canale di ascolto disponibile.", "Connessione non riuscita", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            txtMessaggio.Text = string.Empty;
        }

        private async void TCPListen(object sourceEndPoint)
        {
            IPEndPoint ipAndPort = sourceEndPoint as IPEndPoint;
            TcpListener listener = new TcpListener(ipAndPort.Address, ipAndPort.Port);
            try
            {
                listener.Start();
            }
            catch (SocketException)
            {
                MessageBox.Show($"Errore, un client per l'impiccato è già in ascolto su questa macchina o il socket è già utilizzato, assicurarsi che il socket di ascolto --> {GestioneNetwork.OttieniIPLocale().ToString()}:61500 sia libero.\nL'applicazione verrà chiusa forzatamente dal sistema.","Errore TCP",MessageBoxButton.OK,MessageBoxImage.Error);
                Environment.Exit(0);
            }
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
                        datiRicevuti = Encoding.ASCII.GetString(bytesDati, 0, i);
                        datiRicevuti = datiRicevuti.ToLower();
                        if (datiRicevuti.EndsWith('?'))
                        {
                            datiRicevuti = datiRicevuti.Replace("?", "");
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                bkCounter.Text = datiRicevuti;
                            }));
                        }
                        else if (datiRicevuti.EndsWith('*'))
                        {
                            datiRicevuti = datiRicevuti.Replace("*", "");
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                lstParoleRicevute.Items.Add(datiRicevuti);
                            }));
                        }
                        else
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                bkParola.Text = datiRicevuti;
                            }));
                        }
                    }
                    stream.Close();
                    clientCollegato.Close();
                }
            });
        }

        //Controllo della casella di testo del messaggio.
        private void txtMessaggio_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool controllo = true;
            for (int i = 0; i < lstParoleRicevute.Items.Count; i++)
            {
                int index = lstParoleRicevute.Items[i].ToString().LastIndexOf(' ') + 1;
                if (lstParoleRicevute.Items[i].ToString().Substring(index) == txtMessaggio.Text || txtMessaggio.Text == "partreqmessage" || txtMessaggio.Text.Contains('*') || txtMessaggio.Text.Contains('?'))
                {
                    controllo = false;
                }
            }
            if (!string.IsNullOrWhiteSpace(txtMessaggio.Text) && controllo)
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

        private void TxtUsername_TextChanged(object sender, TextChangedEventArgs e)
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
            txtPort.Background = Brushes.LightGreen;
            Thread tcpListener = new(new ParameterizedThreadStart(TCPListen));
            tcpListener.Start(new IPEndPoint(GestioneNetwork.OttieniIPLocale(), 61500));
        }

        private void txtInfo_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool controllo2 = GestioneNetwork.ControllaTestoPerIP(txtDestinationIp.Text);
            bool controllo1 = GestioneNetwork.VerificaPorta(txtPort.Text, 61500, 61501, 61502);
            txtPort.Background = controllo1 ? Brushes.LightGreen : Brushes.LightCoral;
            txtDestinationIp.Background = controllo2 ? Brushes.LightGreen : Brushes.LightCoral;
            btnConfermaDestinazione.IsEnabled = controllo1 && controllo2;
        }

        private void btnConfermaDestinazione_Click(object sender, RoutedEventArgs e)
        {
            if (!GestioneNetwork.TCPSend(txtDestinationIp.Text, int.Parse(txtPort.Text), "partreqmessage"))
            {
                MessageBox.Show("Errore, nessun canale di ascolto disponibile.", "Connessione non riuscita", MessageBoxButton.OK, MessageBoxImage.Warning);
                btnConfermaDestinazione.IsEnabled = true;
                txtDestinationIp.IsEnabled = true;
                txtPort.IsEnabled = true;
            }
            else
            {
                btnConfermaDestinazione.IsEnabled = false;
                txtDestinationIp.IsEnabled = false;
                txtPort.IsEnabled = false;
                txtUsername.IsEnabled = true;
                btnUsernameConfirm.IsEnabled = true;
            }
        }

        private void bkCounter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bkCounter.Text != "v" && bkCounter.Text != "V")
            {
                imgImpiccato.Source = new BitmapImage(new Uri($"\\Immagini\\imp{bkCounter.Text}.png", UriKind.RelativeOrAbsolute));
                if (int.Parse(bkCounter.Text) <= 3)
                {
                    bkCounter.Foreground = Brushes.Red;
                    if (int.Parse(bkCounter.Text) == 0)
                    {
                        lstParoleRicevute.Items.Clear();
                        btnInviaParola.IsEnabled = false;
                        txtMessaggio.IsEnabled = false;
                        txtUsername.IsEnabled = true;
                        btnUsernameConfirm.IsEnabled = true;
                    }
                }
                else
                {
                    bkCounter.Foreground = Brushes.SpringGreen;
                }
            }
            else
            {
                lstParoleRicevute.Items.Clear();
                btnInviaParola.IsEnabled = false;
                txtMessaggio.IsEnabled = false;
                txtUsername.IsEnabled = true;
                btnUsernameConfirm.IsEnabled = true;
            }
        }
    }
}

