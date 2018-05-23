using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace filedownloader
{
    /// <summary>
    /// Classe que informa os eventos referentes ao download de arquivos
    /// </summary>
    public class FileGetterEventArgs : EventArgs
    {
        /// <summary>
        /// Obtém informações de gerenciamento de arquivos
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Message"></param>
        internal FileGetterEventArgs(FileGetterEnum Type, string Message, long FileSize, long FileDownloaded)
        {
            this.Type = Type;
            this.Message = Message;
            this.FileSize = FileSize;
            this.FileDownloaded = FileDownloaded;
        }

        public long FileSize { get; internal set; }
        public long FileDownloaded { get; internal set; }

        public FileGetterEnum Type { get; internal set; }
        /// <summary>
        /// Mensagem referente ao evento disparado, caso exista
        /// </summary>
        public string Message { get; internal set; }
    }
}
