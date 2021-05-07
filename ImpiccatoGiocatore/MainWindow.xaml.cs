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

        //Invio del messaggio:
        private void btnInviaParola_Click(object sender, RoutedEventArgs e)
        {
            txtMessaggio.Text = string.Empty;
            //Recuperato indirizzo ip destinatario e la sua porta.
            IPAddress destIpAddress = IPAddress.Parse(txtDestinationIp.Text);
            int destinationPortNumber = int.Parse(txtPort.Text);

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

