using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Talk
{
    public static class TalkServerSettings
    {

        public static int PORT = 16000;
        public static IPAddress IP = IPAddress.Parse("127.0.0.1");

     

            

    }
}
