using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using TalkLibrary;


namespace Talk
{

    public partial class TalkServer : Form
    {
        private delegate void updateUICallback(string text, string type);

        private UnicodeEncoding encoder;

        private TcpListener tcplistener;
        private Thread listenThread;
        private Talkmessagehandler talkmessagehandler;

        public TalkServer()
        {
            talkmessagehandler = new Talkmessagehandler();
            encoder = new UnicodeEncoding();
            InitializeComponent();
        }

        public void TalkServer_Load(object sender , EventArgs e)
        {
        }


        //功能 : 持續監聽新使用者的連線 , 並且為其創建一個獨立的thread
        private void listenProc()
        {
            tcplistener.Start();

            while (true)
            {
                TcpClient tcpclient = tcplistener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(clientProc));
                clientThread.Start(tcpclient);
            }

        }
        private void clientProc(object client)
        {
            //獲取網路資訊
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            Talkconnection connection = new Talkconnection(tcpClient);

            //建立使用者資訊
            byte[] nickname = new byte[4096];
            clientStream.Read(nickname, 0, nickname.Length);
            connection.ClientUser.Username = encoder.GetString(nickname);

            //新增到訊息處理器           
            talkmessagehandler.addconnection(connection);
            connection.messageComing(new TalkmessageEventArgs("", connection.ClientUser.Username , "USER", connection));

            updateUI("[", "Monitor");
            updateUI(connection.ClientUser.Username, "Monitor");
            updateUI(" is online.]\n", "Monitor");
            updateUI(connection.ClientUser.Username , "List");



            //從client不斷接收訊息
            while (true)
            {
                byte[] message = new byte[4096];
                int bytesRead = 0;

                try
                {
                    //從client讀入訊息
                    bytesRead = clientStream.Read(message, 0, message.Length);
                    
                    //觸發messageEvent (通知talkmessagehandler 廣播訊息)
                    TalkmessageEventArgs e = new TalkmessageEventArgs(Encoding.Unicode.GetString(message), connection.ClientUser.Username, "TEXT", connection);
                    connection.messageComing(e);
                    //更新ServerUI
                    updateUI(e.Message , "Monitor");
                }
                catch
                {
                    //在Server monitor上顯示錯誤訊息
                    
                }
                if (bytesRead == 0)
                    break;
            }
            talkmessagehandler.removeconnection(connection);
            connection.messageComing(new TalkmessageEventArgs("", connection.ClientUser.Username, "USER", connection));

            updateUI(connection.ClientUser.Username , "List-r");
            updateUI("[", "Monitor");
            updateUI(connection.ClientUser.Username , "Monitor");
            updateUI(" is offline.]\n" , "Monitor");

            tcpClient.Close();
        }

        //功能 : 更新server端的UI
        public void updateUI(string text, string type)
        {
            if (this.InvokeRequired)
            {
                updateUICallback updateUICallback = new updateUICallback(updateUI);
                this.Invoke(updateUICallback, text, type);
            }
            else
            {
                if(type.Equals("Monitor"))
                    monitor.AppendText(text);
                if(type.Equals("List"))
                    list.Items.Add(text);
                if (type.Equals("List-r"))
                    list.Items.Remove(text);
            }
        }
        //Host
        private void button1_Click(object sender, EventArgs e)
        {
            stat.Text = "Listening";
            button1.Enabled = false;
            tcplistener = new TcpListener(TalkServerSettings.IP, TalkServerSettings.PORT);
            listenThread = new Thread(new ThreadStart(listenProc));
            listenThread.Start();
        }

        private void domainUpDown1_SelectedItemChanged(object sender, EventArgs e)
        {

        }
    }
}
