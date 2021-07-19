using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;


namespace Client
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            Connect();
        }

        private void ButtonSend_Click(object sender, EventArgs e)
        {
            Send();
            AddMessage(txbMessage.Text);
        }

        //IP
        IPEndPoint IP;
        Socket client;

        //Tao ket noi
        void Connect()
        {
            IP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            try
            {
                client.Connect(IP);
            }
            catch
            {
                MessageBox.Show("Không thể kết nối tói server!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();
        }

        //Dong chat
        void CloseWin()
        {
            client.Close();
        }

        //Gui tin nhan
        void Send()
        {
            if(txbMessage.Text != string.Empty)
                client.Send(Serialize(txbMessage.Text));
        }

        //Nhan tin nhan
        void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    client.Receive(data);

                    string message = (string)Deserialize(data);
                    AddMessage(message);
                }
            }
            catch
            {
                CloseWin();
            }
        }

        //add tin nhan vao khung chat
        void AddMessage(string s)
        {
            txbView.Items.Add(new ListViewItem() { Text = DateTime.Now.ToString("[hh:mm:ss]: ") + s + "\n"});
            txbMessage.Clear();
        }

        //Xu li phan manh

        byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, obj);

            return stream.ToArray();
        }

        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();

            return formatter.Deserialize(stream);
        }

        //Dong form
        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseWin();
        }
    }
}
