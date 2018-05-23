using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace filedownloader
{
    /// <summary>
    /// Classe de rede e conexão com internet
    /// </summary>
    public class Ethernet
    {
        internal Ethernet()
        {
        }

        /// <summary>
        /// Verifica se a internet está disponivel para conexão
        /// </summary>
        /// <returns>Verdadeiro se estiver disponivel</returns>
        public bool isInternetAvailable()
        {
            return this.canReachHost("http://clients3.google.com/generate_204");
        }

        /// <summary>
        /// Verifica se o endereço da internet pode ser alcançado pelo método.
        /// </summary>
        /// <param name="sInternetAddress">Endereço de internet</param>
        /// <returns></returns>
        public bool canReachHost(string sInternetAddress)
        {
            try
            {
                using (WebClient wc = new WebClient())
                using (Stream st = wc.OpenRead(sInternetAddress))
                {
                    st.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Verifica se o micro possui rede disponivel
        /// </summary>
        /// <returns></returns>
        public bool hasNetworkAvailable()
        {
            try
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
            catch
            {
                return false;
            }
        }
    }
}
