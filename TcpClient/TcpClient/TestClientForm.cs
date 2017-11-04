using ServerBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcpClient
{
    public partial class TestClientForm : Form
    {
        private TcpTestClient _tcpTestClient;
        private Thread _logThread;
        public TestClientForm()
        {
            InitializeComponent();

            TestClientSocket.InitializePacketHandler();
            _tcpTestClient = new TcpTestClient();

            _logThread = new Thread(new ThreadStart(DisplayLog));
            _logThread.Start();
        }

        private void DisplayLog()
        {
            int MAX_LOG_COUNT = 30;
            while(true)
            {
                if(0 < Log.Count)
                {
                    string data = Log.GetLog();
                    if(data != string.Empty)
                    {
                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            listBox_log.Items.Add(data);
                        }));

                        if (MAX_LOG_COUNT < listBox_log.Items.Count)
                        {
                            int last = listBox_log.Items.Count - 1;
                            listBox_log.Items.RemoveAt(last);
                        }
                    }
                }

                Thread.Sleep(500);
            }
        }

        private void TestClientForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            string host = textBox_ip.Text;
            int port = 0;
            Int32.TryParse(textBox_port.Text, out port);

            if(0 < port)
            {
                _tcpTestClient.Connect(host, port);
            }
            else
            {
                Log.AddLog($"invliad server Port error - port : {port}");
            }
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            _tcpTestClient.Close();
            Log.AddLog("Disconnected");
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            string message = textBox_text.Text;
            TestClientPacket.SendEchoRequest(_tcpTestClient.Socket, message);
            Log.AddLog($"[SEND] {message}");
        }
    }
}
