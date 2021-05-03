using System;
using System.Net;
using System.Net.Sockets;

namespace GestioneNetworkECodifica
{
    public class GestioneImpiccato
    {
        //Codifica le parole per il gioco dell'impiccato.
        public static string CodificaParola(string parolaNonModificata)
        {
            char[] vocali = new char[] { 'a', 'e', 'i', 'o', 'u' };
            string parolaModificata = parolaNonModificata;
            foreach (char vocale in vocali)
            {
                parolaModificata = parolaModificata.Replace(vocale, '+');
            }
            for (int i = 0; i < parolaModificata.Length; i++)
            {
                if (parolaModificata[i] != '+')
                {
                    char[] chars = parolaModificata.ToCharArray();
                    chars[i] = '-';
                    parolaModificata = new string(chars);
                }
            }
            return parolaModificata;
        }
    }
}
