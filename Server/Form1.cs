using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class Form1 : Form
    {
        Socket ns;
        IPEndPoint ep;
        BackgroundWorker worker = new BackgroundWorker();

        public Form1()
        {
            InitializeComponent();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(e.UserState != null)
                txtMessage.Text += e.UserState as string;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            DoWork();
        }

        void DoWork()
        {
            if (ns != null)
                return;

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            ep = new IPEndPoint(IPAddress.Parse(txtHost.Text), int.Parse(txtPort.Text));
            socket.Bind(ep);
            socket.Listen(10);

            try
            {
                ns = socket.Accept();

                if (ns.Connected)
                    worker.ReportProgress(-1, "Connected" + "\r\n");

                byte[] br = new byte[254];
                int bytes;

                do
                {
                    bytes = ns.Receive(br);
                    if (bytes > 0)
                    {
                        worker.ReportProgress(-1, "Receive : " + Encoding.ASCII.GetString(br, 0, bytes) + "\r\n");
                        ns.Send(br);
                        worker.ReportProgress(-1, "Respone : " + Encoding.ASCII.GetString(br, 0, bytes) + "\r\n");
                    }

                } while (true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            worker.RunWorkerAsync();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ns.Disconnect(true);

            worker.CancelAsync();
            if(worker.CancellationPending)
                worker.ReportProgress(-1, "Disconnected");

            worker.Dispose();
        }
    }
}
