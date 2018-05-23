using filedownloader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test_FileDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.pgbDownload.Maximum = 1;
            this.pgbDownload.Value = 0;
            this.btnStart.Enabled = false;

            this.iniciarDownload();

        }

        private BackgroundWorker bw;

        private void iniciarDownload()
        {
            FileGetter fg = new FileGetter("http://patch.carloshenrq.com.br", "patch.zip");
            fg.EventHandler += fg_EventHandler;
            fg.start();
        }

        void fg_EventHandler(FileGetterEventArgs e)
        {
            switch (e.Type)
            {
                case FileGetterEnum.REQUEST_STARTING_DOWNLOAD:
                    this.Invoke((MethodInvoker)delegate
                    {
                        this.pgbDownload.Value = 0;
                        this.pgbDownload.Maximum = (int)e.FileSize;
                    });
                    break;
                case FileGetterEnum.REQUEST_DOWNLOADING:
                    this.Invoke((MethodInvoker)delegate
                    {
                        this.pgbDownload.Value = (int)e.FileDownloaded;
                    });
                    break;
                case FileGetterEnum.REQUEST_DOWNLOAD_FINISHED:
                    this.Invoke((MethodInvoker)delegate
                    {
                        this.btnStart.Enabled = true;
                    });
                    MessageBox.Show("DOWNLOAD COMPLETO!!!");
                    break;
                default:
                    break;
            }
        }
    }
}
