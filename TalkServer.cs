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
using Talk;


namespace Talk
{

    public partial class TalkServer : Form
    {
        private UnicodeEncoding encoder = new UnicodeEncoding();
        private TcpListener tcplistener;
        private IPAddress localIP = IPAddress.Parse("127.0.0.1");
        private int port = 16000;
        private Thread listenThread;
        private List<User> userlist = new List<User>();
        private List<Talkconnection> connectionList = new List<Talkconnection>();

        private delegate void updateUICallback(String text, TextBox ctrl);
        private delegate void updataeStatCallback();

        public TalkServer()
        {

            InitializeComponent();
        }
        //Function : update message monitor
        private void updateUI(String text, TextBox ctrl)
        {
            if (this.InvokeRequired)
            {
                updateUICallback UIcallback = new updateUICallback(updateUI);
                this.Invoke(UIcallback , text , ctrl);
            }
            else
            {
                ctrl.AppendText(text);

            }

        }
        //Function : update connection list according to userlist
        private void updateStat()
        {
            if (this.InvokeRequired)
            {
                updataeStatCallback statCallback = new updataeStatCallback(updateStat);
                this.Invoke(statCallback);
            }
            else
            {
               
                connlist.Items.Clear();
                foreach (Talkconnection connection in connectionList)
                {
                    Stream statBroadcast = connection.Client.GetStream();
                    var bin = new BinaryFormatter();
                    bin.Serialize(statBroadcast , userlist);
                    connlist.Items.Add(connection.ClientUser.Username);
                }
                   
            }
        }
        //Function : continueously listening client connection
        private void listenProc()
        {
            //Start listening
            tcplistener.Start();
            //Listening loop
            while (true)
            {
                TcpClient tcpclient = tcplistener.AcceptTcpClient();
                //Got a tcpclient
                //Start a clientProc
                Thread clientThread = new Thread(new ParameterizedThreadStart(clientProc));
                clientThread.Start(tcpclient);
            }

        }
        private void broadcast(string message)
        {
            
            foreach (Talkconnection connection in connectionList)
            {
                NetworkStream clientStream;
                clientStream = connection.Client.GetStream();
                clientStream.Write(encoder.GetBytes(message), 0, 4096);
                clientStream.Flush();
            }

            
        }
        private void clientProc(object client)
        {
            //Convert to tcpclient
            //Get client stream
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            StreamReader clientStreamReader = new StreamReader(clientStream, Encoding.Default);
            //Create a user and put the user to Talkconnection
            User user = new User();
            Talkconnection connection = new Talkconnection(tcpClient , user);
            //Receive message buffer
            byte[] message = new byte[4096];
            int bytesRead;

            /*Initilize a user*/
            //Nickname
            clientStream.Read(message, 0, message.Length);
            user.Username = Encoding.Unicode.GetString(message);
            
            userlist.Add(user);
            connectionList.Add(connection);

            //Update stat once a new user connected
            updateStat();

            //Display a connection message
            updateUI("[" + user.Username + " Connceted]\n", monitor);
            updateUI(" Connceted]\n", monitor);


            //Receive message from client
            while (true)
            {
                //Read Data from client
                try
                {
                    Array.Clear(message, 0, message.Length);
                    bytesRead = clientStream.Read(message, 0, 4096);
                    broadcast(Encoding.Unicode.GetString(message));
                    updateUI(Encoding.Unicode.GetString(message) + "\n", monitor);
                    
                }
                catch
                {
                    break;
                }
                if (bytesRead == 0)
                    break;
            }
            //Display offline message
            updateUI("[", monitor);
            updateUI(user.Username, monitor);
            updateUI(" is Offline]\n", monitor);
            //Remove user from userlist
            userlist.Remove(user);
            connectionList.Remove(connection);
            //Update conncetion list
            updateStat();

            tcpClient.Close();
        }
        private void TalkServer_Load(object sender, EventArgs e)
        {

        }
        //Host
        private void button1_Click(object sender, EventArgs e)
        {
            stat.Text = "Listening";
            button1.Enabled = false;
            tcplistener = new TcpListener(localIP, port);
            listenThread = new Thread(new ThreadStart(listenProc));
            listenThread.Start();
        }

        private void domainUpDown1_SelectedItemChanged(object sender, EventArgs e)
        {

        }
    }
}
