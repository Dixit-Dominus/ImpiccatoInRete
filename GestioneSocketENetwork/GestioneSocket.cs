using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GestioneSocketENetwork
{
    //Classe che gestisce il socket di ascolto e invio dei dati in UDP
    internal class GestioneSocket
    {
        //Array di parole in continuo aggiornamento con il canale di ascolto.
        internal static List<string> listaParole { get; private set; }
        /*INVIO*/
        //Metodo di creazione del canale di invio.
        internal static void CreaSender(string destinationIp, int destinationPort, string messaggio)
        {
            CreaSocketDiInvio(IPAddress.Parse(destinationIp), destinationPort, messaggio);
        }
        //Metodo di crazione del socket di invio.
        private static void CreaSocketDiInvio(IPAddress destinationIpAddress, int destinationPortNumber, string messaggio)
        {
            //Array di byte da inviare come datagram (Contengono una parola / lettera)
            Byte[] arrayDiByteDaInviare = Encoding.ASCII.GetBytes(messaggio);
            //Definizione del socket di invio.
            Socket senderSocket = new Socket(destinationIpAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            //Invio del dato al destinatario.
            senderSocket.SendTo(arrayDiByteDaInviare, new IPEndPoint(destinationIpAddress, destinationPortNumber));
        }

        /* ASCOLTO */
        //Metodo di creazione del canale di ascolto.
        internal static void CreaListener(string sourceIp, int sourcePort)
        {
            listaParole = new List<string>();
            IPEndPoint sourceEndPoint = new IPEndPoint(IPAddress.Parse(sourceIp), sourcePort);
            Thread listener = new Thread(new ParameterizedThreadStart(CreaSocketDiAscolto));
            listener.Start(sourceEndPoint);
        }
        //Metodo di creazione del socket di ascolto da usare in modo asincrono in un thread
        private static Socket CreaSocketDiAscolto(object sourceEndPoint)
        {
            //Endpoint di origine passato come oggetto con downcasting
            IPEndPoint listenerEndPoint = sourceEndPoint as IPEndPoint;
            //Creazione socket di ascolto
            Socket listenerSocket = new Socket(listenerEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            //Connessione del socket di ascolto all'endpoint di origine.
            listenerSocket.Bind(listenerEndPoint);
            return listenerSocket;
        }
    }
}
