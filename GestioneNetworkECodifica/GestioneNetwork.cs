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
        public static bool TCPSend(string hostname, int port, string mex)
        {
            try
            {
                TcpClient clientSender = new TcpClient(hostname, port);
                Byte[] data = Encoding.ASCII.GetBytes(mex);
                NetworkStream stream = clientSender.GetStream();
                stream.Write(data, 0, data.Length);
                stream.Close();
                clientSender.Close();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
            
        }

        //Ottiene l'IP della macchina corrente.
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

        //Verifica che una stringa sia un IP.
        public static bool ControllaTestoPerIP(string testo)
        {
            if (!string.IsNullOrWhiteSpace(testo))
            {
                if (IPAddress.TryParse(testo, out IPAddress address))
                {
                    return true;
                }
            }
            return false;
        }

        /*Questo metodo verifica se la porta data in input è tra quelle da escludere, in questo caso
         * il metodo ritorna false, altrimento esso ritorna true. La porta deve inoltre essere tra quelle
         * non registrate a IANA.
         */
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
