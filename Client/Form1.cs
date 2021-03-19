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

namespace Client
{
    public partial class Form1 : Form
    {
        Socket socket;
        BackgroundWorker worker = new BackgroundWorker();
        public Form1()
        {
            InitializeComponent();
            
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string str = e.UserState as string;
            if(string.IsNullOrEmpty(str))
                txtReceive.Text += str + "\n";
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Start();
        }

        void Start()
        {
            try
            {
                byte[] bytesReceived = new byte[1024];

                using (socket = ConnectSocket())
                {

                    if (socket.Connected)
                    {
                        int bytes;
                        do
                        {
                            bytes = socket.Receive(bytesReceived);
                            worker.ReportProgress(-1,"Respone : " + Encoding.ASCII.GetString(bytesReceived, 0, bytes) + "\n");
                        } while (bytes > 0);
                    }
                    else MessageBox.Show("Cannot Connected");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        Socket ConnectSocket(string server = "127.0.0.1", int port = 80)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(txtHost.Text??server), int.Parse(txtPort.Text));
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(endPoint);

            if (socket.Connected)
                return socket;

            return null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(socket == null && !worker.IsBusy)
                worker.RunWorkerAsync();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (socket != null)
                {
                    socket.Send(Encoding.ASCII.GetBytes(txtSend.Text));
                    txtSend.Text = "";
                }
                else MessageBox.Show("Cannot Connect");
            }
            catch { }
        }
    }
}
