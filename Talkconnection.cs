using System;
using System.Collections.Generic;
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
using System.Security.Permissions;

namespace Talk
{
    //Define a connection 
    public class Talkconnection
    {

        private TcpClient client;
        private User clientUser;

        public TcpClient Client { get { return client; } }
        public User ClientUser { get { return clientUser; } }

        public Talkconnection(TcpClient _client , User _clientuser)
        {
            client = _client;
            clientUser = _clientuser;
        }

    }
}
