using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace GestioneNetworkECodifica
{
    public class GestioneNetwork
    {
        //Metodi utili per la gestione delle funzionalità di rete dell'applicativo in cui vengono utilizzati.

        /// <summary>
        /// Metodo che instaura una connessione per poter inviare dei dati su un canale TCP.
        /// </summary>
        /// <param name="hostname">L'hostname o l'IP del server a cui andranno inviati i dati.</param>
        /// <param name="port">Porta di destinazione tramite la quale andranno inviati i dati.</param>
        /// <param name="mex">Messaggio che andrà inviato nel canale.</param>
        /// <returns>Ritorna true se la comunicazione avviene senza errori, in contrario ritorna false.</returns>
        public static bool TCPSend(string hostname, int port, string mex)
        {
            try
            {
                //Instanziamento della classe TcpClient con un IP e una porta di destinazione dati come parametri.
                TcpClient clientSender = new(hostname, port);
                //Array di bytes che andranno inviati sul canale scelto, la stringa posta come parametro viene convertita.
                Byte[] data = Encoding.ASCII.GetBytes(mex);
                //Ottenimento dello stream del client.
                NetworkStream stream = clientSender.GetStream();
                //Invio dei dati nello stream.
                stream.Write(data, 0, data.Length);
                //Abbattimento.
                stream.Close();
                clientSender.Close();
                //Riscontro positivo nell'uso del metodo.
                return true;
            }
            //Se ci sono problemi nella comunicazione viene ritornato un riscontro negativo.
            catch (SocketException)
            {
                return false;
            }
        }

        /// <summary>
        /// Metodo che ritorna l'indirizzo IP della macchina su cui viene eseguito l'applicativo.
        /// </summary>
        /// <returns>Ritorna l'indirizzo IP della macchina.</returns>
        public static IPAddress OttieniIPLocale()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return IPAddress.Parse(localIP);
        }

        /// <summary>
        /// Metodo che verifica se una stringa è convertibile in un indirizzo IP.
        /// </summary>
        /// <param name="stringaIp">Stringa da verificare.</param>
        /// <returns>true se la stringa e convertibile, false altrimenti</returns>
        public static bool ControllaTestoPerIP(string stringaIp)
        {
            if (!string.IsNullOrWhiteSpace(stringaIp))
            {
                if (IPAddress.TryParse(stringaIp, out _))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Metodo che verifica se una stringa è una porta che può essere utilizzata (Tra le porte dinamiche/private)
        /// </summary>
        /// <param name="porta">Stringa da controllare (Porta)</param>
        /// <param name="porteDaEscludere">Eventuali porte da ritenere non utilizzabili.</param>
        /// <returns>true se la stringa è una porta utilizzabile, false altrimenti.</returns>
        public static bool VerificaPorta(string porta, params int[] porteDaEscludere)
        {
            int portaConvertita;
            if (!string.IsNullOrWhiteSpace(porta))
            {
                if (int.TryParse(porta, out portaConvertita))
                {
                    if (portaConvertita > 49151 && portaConvertita <= 65535)
                    {
                        foreach (int p in porteDaEscludere)
                        {
                            if (portaConvertita == p)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
