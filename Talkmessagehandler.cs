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
using System.Runtime.Serialization.Formatters.Binary;
using TalkLibrary;


namespace Talk
{
    public class Talkmessagehandler : Talkgeneral
    {

        //連線列表
        //使用者列表(用於廣播給client)
        private List<Talkconnection> connectionlist;
        private List<Talkuser> userlist;
        
        //編碼器
        private UnicodeEncoding encoder;

        //建構式
        public Talkmessagehandler()
        {
            connectionlist = new List<Talkconnection>();
            userlist = new List<Talkuser>();
            encoder = new UnicodeEncoding();
        }
        //功能 : 添加一個Talkconnection 到connectionlist中 , 並且訂閱其事件
        //       添加一個User 到 userlist中
        public void addconnection(Talkconnection talkconnection)
        {
            connectionlist.Add(talkconnection);
            userlist.Add(talkconnection.ClientUser);
            talkconnection.messageEvent += new messageEventhandler(messageBroadcast);
        }

        //功能 : 刪除一個Talkconnection , 並且取消訂閱其事件 (在server的clientproc中當client離線時執行)
        //       刪除一個User
        public void removeconnection(Talkconnection talkconnection)
        {
            connectionlist.Remove(talkconnection);
            userlist.Remove(talkconnection.ClientUser);
            talkconnection.messageEvent -= messageBroadcast;
        }
        
        //功能 : 廣播訊息給所有使用者 (如果連線列表有更新的話優先傳送)
        public void messageBroadcast(object sender, TalkmessageEventArgs e)
        {
                BinaryFormatter bin = new BinaryFormatter();

                foreach (Talkconnection talkconnection in connectionlist)
                {
                    
                    if (talkconnection.Client.Connected)
                    {
                        NetworkStream clientStream = talkconnection.Client.GetStream();
                        byte[] messageType = encoder.GetBytes(e.MessageType);
                        byte[] message = new byte[4096];

                        clientStream.Write(messageType, 0, messageType.Length);
                        if (e.MessageType.CompareTo("TEXT") == 0)
                            clientStream.Write(encoder.GetBytes(e.Message), 0, e.Message.Length);
                        if (e.MessageType.CompareTo("USER") == 0)
                            bin.Serialize(clientStream, userlist);
                    }
                    else
                        continue;
                }
        }

    }
}
