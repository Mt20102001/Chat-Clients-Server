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
        bool isConnected;
        public Client()
        {
            InitializeComponent();
            timer1.Tick += timer1_Tick;
            timer1.Interval = 1000;
            timer1.Enabled = true;
            CheckForIllegalCrossThreadCalls = false;
            Connect();
        }

        private void ButtonSend_Click(object sender, EventArgs e)
        {
            if (server != null)
            {
                for(int i = 1; i < clientList.Count; i++)
                {
                    SendServer(clientList[i]);
                }
            }
            else
            {
                SendClient();
            }
            AddMessage(txbMessage.Text);
            txbMessage.Clear();
        }

        //IP
        IPEndPoint IP;
        Socket client, server;
        List<Socket> clientList;

        void CreateServer()
        {
            clientList = new List<Socket>();
            IP = new IPEndPoint(IPAddress.Any, 9999);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            server.Bind(IP);

            Thread Listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        clientList.Add(client);

                        Thread receive = new Thread(ReceiveServer);
                        receive.IsBackground = true;
                        receive.Start(client);
                    }
                }
                catch
                {
                    IP = new IPEndPoint(IPAddress.Any, 9999);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
            });
            Listen.IsBackground = true;
            Listen.Start();
        }

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
                //MessageBox.Show("Không thể kết nối tói server!\nTiến hành tạo mới server", "Có sự cố", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                isConnected = false;
                client.Close();
                CreateServer();
                return;
            }
            Thread listen = new Thread(ReceiveClient);
            listen.IsBackground = true;
            listen.Start();
            isConnected = true;
        }

        //Dong chat
        void CloseWin()
        {
            if (server != null)
            {
                server.Close();
            }
            else
            {
                client.Close();
            }
        }

        //Gui tin nhan khi la Client
        void SendClient()
        {
            if (txbMessage.Text != string.Empty)
                client.Send(Serialize(txbMessage.Text));
            //MessageBox.Show("gửi từ client", "thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //Gui tin nhan khi la Server
        void SendServer(Socket client)
        {
            if (client != null && txbMessage.Text != string.Empty)
                client.Send(Serialize(txbMessage.Text));
            //MessageBox.Show("gửi từ server", "thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //Nhan tin nhan khi la Client
        void ReceiveClient()
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

        //Nhan tin nhan khi la Server
        void ReceiveServer(object obj)
        {
            Socket client = obj as Socket;
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    client.Receive(data);

                    string message = (string)Deserialize(data);

                    foreach (Socket item in clientList)
                    {
                        if (item != null && item != client)
                            item.Send(Serialize(message));
                    }
                    AddMessage(message);
                }
            }
            catch
            {
                clientList.Remove(client);
                client.Close();
            }
        }

        //add tin nhan vao khung chat
        void AddMessage(string s)
        {
            txbView.Items.Add(new ListViewItem() { Text = DateTime.Now.ToString("[hh:mm:ss]: ") + s + "\n" });
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

        int i = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            i++;
            label1.Text = i.ToString();
            try
            {
                bool part1 = client.Poll(1000, SelectMode.SelectRead);
                bool part2 = (client.Available == 0);
                if (part1 && part2)
                    isConnected = false;
                else
                    isConnected = true;
            }
            catch
            {
                isConnected = false;
            }
            if (!isConnected)
            {
                Connect();
            }

        }

        //Dong form
        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseWin();
        }
    }
}
