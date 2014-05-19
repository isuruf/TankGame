using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace TankGame
{
    class NetworkConnection
    {
        #region "Variables"
        private NetworkStream clientStream; //Stream - outgoing
        private TcpClient client; //To talk back to the client
        private BinaryWriter writer; //To write to the clients

        private NetworkStream serverStream; //Stream - incoming        
        private TcpListener listener; //To listen to the clinets        
        public string reply = ""; //The message to be written

        private DataObject lastReply = null;
        private String lastError;
        private String lastVariableUpdate;
        private Queue<String> lastLifePack;
        private List<String> lastCoin;
        public Boolean gameSAccepted = false;
        public Boolean gameIAccepted = false;
        private String SMsg;
        public int turn = 0;
        private String Imsg;
        private Boolean connection = false;
        private Boolean newGMsg = false;
        private Boolean newGMsg2 = false;
        #endregion

        public NetworkConnection()
        {
            lastCoin = new List<string>();
            lastLifePack = new Queue<string>();
        }

        public void sendData(object stateInfo)   //used when sendind data
        {
            DataObject dataObj = (DataObject)stateInfo;
            this.client = new TcpClient();

            try
            {
                if (dataObj.ClientPort == Constant.SERVER_PORT)
                {

                    this.client.Connect(dataObj.ClientMachine, dataObj.ClientPort);

                    if (this.client.Connected)
                    {
                        this.clientStream = client.GetStream();

                        this.writer = new BinaryWriter(clientStream);
                        Byte[] tempStr = Encoding.ASCII.GetBytes(dataObj.MSG);

                        this.writer.Write(tempStr);
                        connection = true;
                        this.writer.Close();
                        this.clientStream.Close();

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Communication (WRITING) to " + dataObj.ClientMachine + " on " + dataObj.ClientPort + "Failed! \n " + e.Message);
                connection = false;
            }
            finally
            {
                this.client.Close();
            }
        }

        public void receiveData()
        {
            bool errorOcurred = false;
            Socket connection = null;
            DataObject dataObj = null;
            try
            {
                //Creating listening Socket
                this.listener = new TcpListener(IPAddress.Any, Constant.CLIENT_PORT);

                //this.listener = new TcpListener(IPAddress.Parse(Constant.Client_IP), Constant.CLIENT_PORT);
                //this.listener = new TcpListener(IPAddress.Parse(serverIP), 7000);
                //Starts listening
                this.listener.Start();
                while (true)
                {
                    connection = listener.AcceptSocket();
                    if (connection.Connected)
                    {
                        this.serverStream = new NetworkStream(connection);

                        SocketAddress sockAdd = connection.RemoteEndPoint.Serialize();
                        string s = connection.RemoteEndPoint.ToString();
                        List<Byte> inputStr = new List<byte>();

                        int asw = 0;
                        while (asw != -1)
                        {
                            asw = this.serverStream.ReadByte();
                            inputStr.Add((Byte)asw);
                        }

                        reply = Encoding.UTF8.GetString(inputStr.ToArray());
                        this.serverStream.Close();
                        string ip = s.Substring(0, s.IndexOf(":"));
                        int port = Constant.CLIENT_PORT;

                        dataObj = new DataObject(reply.Substring(0, reply.Length - 1), ip, port);
                        lastReply = dataObj;
                        if (lastReply.MSG.StartsWith("G:"))
                        {
                            newGMsg = true;
                            newGMsg2 = true;
                            turn++;
                            lastVariableUpdate = lastReply.MSG;
                        }
                        else if (lastReply.MSG.StartsWith("L:"))
                        {
                            lastLifePack.Enqueue(lastReply.MSG);
                        }
                        else if (lastReply.MSG.StartsWith("C:"))
                        {
                            lastCoin.Add(lastReply.MSG);
                        }
                        else if (lastReply.MSG.StartsWith("S:"))
                        {
                            gameSAccepted = true;
                            SMsg = lastReply.MSG;
                        }
                        else if (lastReply.MSG.StartsWith("I:"))
                        {
                            gameIAccepted = true;
                            Imsg = lastReply.MSG;
                        }
                        else
                        {
                            lastError = lastReply.MSG;
                        }
                        Thread.Sleep(90);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Communication (RECEIVING) Failed! \n " + e.Message);
                errorOcurred = true;
                this.listener.Stop();
            }
            finally
            {
                if (connection != null)
                    if (connection.Connected)
                    {
                        connection.Close();
                    }
                if (errorOcurred)
                {
                    this.receiveData();
                }

            }
        }

        public String getSMsg()
        {
            String s = null;
            if (SMsg != null)
                s = (String)SMsg.Clone();
            SMsg = null;
            return s;
        }

        public String getLastCoin()
        {
            if (lastCoin.Count != 0)
            {
                String s = lastCoin[0];
                lastCoin.RemoveAt(0);
                return s;
            }
            else
                return null;
        }

        public Queue<String> getMedikitQueue()
        {
            return lastLifePack;
        }

        public String getLastGmsg()
        {
            newGMsg = false;
            return lastVariableUpdate;
        }

        public Boolean isNewGMsg()
        {
            if (newGMsg)
            {
                newGMsg = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public String getIMsg()
        {
            return Imsg;
        }
        public Boolean isConnectionAvailable()
        {
            return connection;
        }
    }
}
