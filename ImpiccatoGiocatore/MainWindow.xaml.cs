﻿using System;
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

        //Invio del messaggio:
        private void btnInviaParola_Click(object sender, RoutedEventArgs e)
        {
            //Recuperato indirizzo ip destinatario e la sua porta.
            IPAddress destIpAddress = IPAddress.Parse(txtDestinationIp.Text);
            int destinationPortNumber = int.Parse(txtPort.Text);

            //Esecuzione metodo di invio del messaggio.
            SocketSend(destIpAddress, destinationPortNumber, $"- {txtUsername.Text}: {txtMessaggio.Text}");
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

        //Evento al click della creazione del canale di scolto
        private void btnCreaSocket_Click(object sender, RoutedEventArgs e)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(txtSourceIP.Text), int.Parse(txtPort.Text));
            Thread startListener = new Thread(new ParameterizedThreadStart(SocketReceive));
            startListener.Start(localEndPoint);
            btnCreaSocket.IsEnabled = false;
            txtPort.IsEnabled = false;
            txtSourceIP.IsEnabled = false;
        }

        //Controllo della casella di testo del messaggio.
        private void txtMessaggio_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtMessaggio.Text))
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
            txtUsername.IsEnabled = false;
            btnUsernameConfirm.IsEnabled = false;
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                btnUsernameConfirm.IsEnabled = true;
            }
            else
            {
                btnUsernameConfirm.IsEnabled = false;
            }
        }
    }
}
