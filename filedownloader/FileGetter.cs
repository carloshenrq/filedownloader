using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace filedownloader
{
    /// <summary>
    /// Classe para obter os arquivos a serem baixados
    /// </summary>
    public class FileGetter
    {
        /// <summary>
        /// Faz o download de arquivos e permite continuar o download.
        /// </summary>
        /// <param name="sInternetHost"></param>
        /// <param name="sFile"></param>
        public FileGetter(string sInternetHost, string sFile)
            : this(sInternetHost, sFile, String.Empty)
        {
        }

        /// <summary>
        /// Faz o download de um arquivo e define uma pasta para salvar
        /// </summary>
        /// <param name="sInternetHost"></param>
        /// <param name="sFile"></param>
        /// <param name="sDownloadPath"></param>
        public FileGetter(string sInternetHost, string sFile, string sDownloadPath)
            : this(sInternetHost, sFile, sDownloadPath, null)
        {
        }

        /// <summary>
        /// Faz o download de um arquivo, define a pasta para salvar e um servidor proxy
        /// </summary>
        /// <param name="sInternetHost"></param>
        /// <param name="sFile"></param>
        /// <param name="sDownloadPath"></param>
        /// <param name="proxy"></param>
        public FileGetter(string sInternetHost, string sFile, string sDownloadPath, WebProxy proxy)
        {
            this.sInternetHost = sInternetHost;
            this.sFile = sFile;
            this.sDownloadPath = sDownloadPath;
            this.webProxy = proxy;
            this.ethernet = new Ethernet();
        }

        /// <summary>
        /// Inicia o download do arquivo de forma asyncrona
        /// </summary>
        public void start()
        {
            if (!this.ethernet.canReachHost(this.sInternetHost))
                throw new Exception("Não foi possível se conectar ao host solicitado.");

            // Caminho onde será salvo o arquivo...
            this.pathFile = Path.Combine(this.sDownloadPath, this.sFile);

            // Tamanho do arquivo que será baixado.
            this.fileTotalSize = this.fileDownloadedSize = 0L;

            // Se o arquivo existir, irá tentar continuar o download de onde parou...
            if (File.Exists(this.pathFile))
            {
                FileInfo fiPath = new FileInfo(this.pathFile);
                this.fileDownloadedSize = fiPath.Length;
            }

            // Faz a chamada informando que a requisição para download do arquivo está sendo criada
            this.OnEventHandlerCall(FileGetterEnum.CREATING_REQUEST);

            // Inicia os procedimentos para download do arquivo...
            HttpWebRequest webRequest = WebRequest.CreateHttp(this.sInternetHost + "/" + this.sFile);

            // Caso exista proxy atribui o proxy
            if (this.webProxy != null)
            {
                this.OnEventHandlerCall(FileGetterEnum.APPENDING_PROXY);
                webRequest.Proxy = this.webProxy;
            }

            this.OnEventHandlerCall(FileGetterEnum.STARTING_REQUEST);

            // Inicia a solicitação para obter resposta referente ao arquivo informado...
            webRequest.BeginGetResponse(this.WR_beginGetResponse, webRequest);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="WR_Response"></param>
        private void WR_beginGetResponse(IAsyncResult WR_Response)
        {
            FileGetterStreamRequest fgsr = new FileGetterStreamRequest();
            fgsr.httpWebRequest = (HttpWebRequest)WR_Response.AsyncState;

            try
            {
                fgsr.webResponse = fgsr.httpWebRequest.EndGetResponse(WR_Response);

                // Tamanho total do arquivo a ser baixado
                this.fileTotalSize = fgsr.webResponse.ContentLength;

                // Define o tamanho total do arquivo baixado
                // Caso seja completado o download
                if (this.fileDownloadedSize >= this.fileTotalSize)
                {
                    fgsr.webResponse.Close();

                    this.OnEventHandlerCall(FileGetterEnum.REQUEST_DOWNLOAD_FINISHED, string.Empty, this.fileTotalSize, this.fileDownloadedSize);
                    return;
                }

                // Informa que stá calculando o tamanho do arquivo para iniciar o download...
                this.OnEventHandlerCall(FileGetterEnum.REQUEST_COMPUTING_SIZE, string.Empty, this.fileTotalSize, 0);

                // Verifica é possível continuar um download pausado, se não for possível, então
                // Zera o tamanho da requisição
                bool bSupportContinue = String.Compare(fgsr.webResponse.Headers["Accept-Ranges"], "bytes") == 0;

                // Zera o que foi obtido do download do arquivo
                if (!bSupportContinue)
                {
                    File.Delete(this.pathFile);
                    this.fileDownloadedSize = 0;
                }

                // Informa que o download está sendo iniciado e também a posição inicial do download...
                this.OnEventHandlerCall(FileGetterEnum.REQUEST_STARTING_DOWNLOAD, string.Empty, this.fileTotalSize, this.fileDownloadedSize);

                // Obtém o stream de conexão com o arquivo que sera baixado.
                fgsr.webStreamRead = fgsr.webResponse.GetResponseStream();
                // Obtém o stream de escrita do arquivo.
                fgsr.fileStreamWrite = new FileStream(this.pathFile, FileMode.OpenOrCreate);

                // Inicializa os bytes para leitura...
                this.bTmpRead = new byte[512];

                // Inicia a leitura dos dados
                fgsr.webStreamRead.BeginRead(this.bTmpRead, 0, this.bTmpRead.Length, this.WSR_BeginReadStream, fgsr);
            }
            catch
            {
                File.Delete(this.pathFile);
                fgsr.httpWebRequest.AddRange(0);
                fgsr.httpWebRequest.BeginGetResponse(this.WR_beginGetResponse, fgsr.httpWebRequest);
            }
        }

        // Inicia a leitura dos dados para escrever em disco...
        private void WSR_BeginReadStream(IAsyncResult WSR_ReadStream)
        {
            FileGetterStreamRequest fgsr = (FileGetterStreamRequest)WSR_ReadStream.AsyncState;
            int bytesRead = fgsr.webStreamRead.EndRead(WSR_ReadStream);

            // Escreve a quantidade de bytes lidos no arquivo
            fgsr.fileStreamWrite.Write(this.bTmpRead, 0, bytesRead);
            fgsr.fileStreamWrite.Flush();

            // Marca o total que foi escrito do arquivo em disco
            this.fileDownloadedSize += bytesRead;

            // Limpa o array lido desde o inicio
            Array.Clear(this.bTmpRead, 0, this.bTmpRead.Length);

            // Define o tamanho total do arquivo baixado
            // Caso seja completado o download
            if (this.fileDownloadedSize >= this.fileTotalSize)
            {
                fgsr.webStreamRead.Close();
                fgsr.webResponse.Close();
                fgsr.fileStreamWrite.Close();

                this.OnEventHandlerCall(FileGetterEnum.REQUEST_DOWNLOAD_FINISHED, string.Empty, this.fileTotalSize, this.fileDownloadedSize);
                return;
            }

            // Informa que o arquivo está sendo baixado...
            this.OnEventHandlerCall(FileGetterEnum.REQUEST_DOWNLOADING, string.Empty, this.fileTotalSize, this.fileDownloadedSize);

            // Solicita uma nova leitura de dados referente ao arquivo que está sendo baixado.
            fgsr.webStreamRead.BeginRead(this.bTmpRead, 0, this.bTmpRead.Length, this.WSR_BeginReadStream, fgsr);
        }

        /// <summary>
        /// Classe interna de informação para obtenção dos dados para download do arquivo
        /// </summary>
        internal class FileGetterStreamRequest
        {
            public HttpWebRequest httpWebRequest { get; set; }
            public WebResponse webResponse { get; set; }
            public Stream webStreamRead { get; set; }
            public Stream fileStreamWrite { get; set; }
        }

        /// <summary>
        /// Quando realiza a chamada do gerenciador de eventos...
        /// </summary>
        /// <param name="e"></param>
        private void OnEventHandlerCall(FileGetterEnum Type)
        {
            this.OnEventHandlerCall(Type, string.Empty);
        }

        /// <summary>
        /// Quando realiza a chamada do gerenciador de eventos...
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Message"></param>
        private void OnEventHandlerCall(FileGetterEnum Type, string Message)
        {
            this.OnEventHandlerCall(Type, Message, 0, 0);
        }

        /// <summary>
        /// Quando realiza a chamada do gerenciador de eventos...
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Message"></param>
        /// <param name="FileSize"></param>
        /// <param name="FileDownloaded"></param>
        private void OnEventHandlerCall(FileGetterEnum Type, string Message, long FileSize, long FileDownloaded)
        {
            if (this.EventHandler != null)
                this.EventHandler(new FileGetterEventArgs(Type, Message, FileSize, FileDownloaded));
        }

        /// <summary>
        /// Define o gerenciador de eventos para o FileGetter
        /// </summary>
        public event FileGetterEventHandler EventHandler;

        /// <summary>
        /// Bytes temporarios que serão lidos para serem escritos
        /// </summary>
        private byte[] bTmpRead { get; set; }

        /// <summary>
        /// Caminho do arquivo que será salvo.
        /// </summary>
        private string pathFile { get; set; }

        /// <summary>
        /// Tamanho total do arquivo a ser baixado
        /// </summary>
        private long fileTotalSize { get; set; }

        /// <summary>
        /// Total do arquivo que já foi baixado...
        /// </summary>
        private long fileDownloadedSize { get; set; }

        /// <summary>
        /// Objeto de verificação de informações de internet
        /// </summary>
        private Ethernet ethernet { get; set; }

        /// <summary>
        /// Endereço do host na internet que será usado para fazer o download do arquivo
        /// </summary>
        private string sInternetHost { get; set; }

        /// <summary>
        /// Arquivo que será usado para fazer o download 
        /// </summary>
        private string sFile { get; set; }

        /// <summary>
        /// Pasta de download os arquivos
        /// </summary>
        private string sDownloadPath { get; set; }

        /// <summary>
        /// Caso seja definido algum proxy
        /// </summary>
        private WebProxy webProxy { get; set; }
    }

    /// <summary>
    /// Delegate para o gerenciamento do download do arquivo...
    /// </summary>
    /// <param name="e"></param>
    public delegate void FileGetterEventHandler(FileGetterEventArgs e);
}
