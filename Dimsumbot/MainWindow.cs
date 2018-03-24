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
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Media;
using System.Diagnostics;

namespace Dimsumbot
{
    public partial class MainWindow : Form
    {

        #region Variables
        private static string userName = "dimsumbot";
        private static string password = "oauth:k4q7b4rv89aajmm0gtv11entl13kam";

        IrcClient irc = new IrcClient("irc.chat.twitch.tv", 6667, userName, password);
        NetworkStream serverStream = default(NetworkStream);
        string readData = "";
        Thread chatThread;
        #endregion


        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            irc.JoinRoom("WizyTV");
            chatThread = new Thread(GetMessage);
            chatThread.Start();
        }

        private void GetMessage()
        {
            serverStream = irc.tcpClient.GetStream();
            int bufferSize = 0;
            byte[] inStream = new byte[10025];
            bufferSize = irc.tcpClient.ReceiveBufferSize;

            while(true)
            {
                try
                {
                    readData = irc.ReadMessage();
                    msg();
                }
                catch(Exception e)
                {

                }
            }
        }

        
        private void msg()
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(msg));
            }
            else
            {
                tbChatbox.Text = tbChatbox.Text + readData.ToString() + Environment.NewLine;
            }
        }

    }

    class IrcClient
    {
        private string userName;    //bot username
        private string channel1;    //channel name

        public TcpClient tcpClient;
        private StreamReader inputStream;   //Received from chat
        private StreamWriter outputStream;  //Sent to chat

        public IrcClient(string ip, int port, string userName, string password)
        {
            tcpClient = new TcpClient(ip, port);
            inputStream = new StreamReader(tcpClient.GetStream());
            outputStream = new StreamWriter(tcpClient.GetStream());

            //IRC Client setup
            outputStream.WriteLine("PASS " + password);
            outputStream.WriteLine("NICK " + userName);
            outputStream.WriteLine("USER " + userName + " 8 * :" + userName);

            //Permission request for certain libraries (according to youtuber)
            outputStream.WriteLine("CAP REQ :twitch.tv/membership");
            outputStream.WriteLine("CAP REQ :twitch.tv/commands");
            outputStream.Flush();
        }

        public void JoinRoom(string channel)
        {
            this.channel1 = channel; //passes channel into class variable
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
        }

        public void LeaveRoom()
        {
            outputStream.Close();
            inputStream.Close();
        }

        //Send anything to IRC
        public void SendIrcMessage(string message)
        {
            outputStream.WriteLine(message);
            outputStream.Flush();
        }

        //Specifically sends chat message
        public void SendChatMessage(string message)
        {
            SendIrcMessage(":" + userName + "!" + userName + "@" + userName + ".tmi.twitch.tv PRIVMSG #" + channel1 + " :" + message);
        }

        public void PingResponse()
        {
            SendIrcMessage("POING tmi.twitch.tv\r\n");
        }

        public string ReadMessage()
        {
            string message = "";
            message = inputStream.ReadLine();
            return message;
        }
    }
}
