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
        private List<String> lastLifePack;
        private List<String> lastCoin;
        private Boolean gameSAccepted = false;
        private Boolean gameIAccepted = false;
        private String SMsg;
        private String Imsg;
        private Boolean connection=false;
        private Boolean newGMsg = false;        
        #endregion

        public NetworkConnection()
        {
            lastCoin = new List<string>();
            lastLifePack = new List<string>();
        }

        public void SendData(object stateInfo)   //used when sendind data
        {
            DataObject dataObj = (DataObject)stateInfo;
            //Opening the connection
            this.client = new TcpClient();

            try
            {
                if (dataObj.ClientPort == Constant.SERVER_PORT)
                {

                    this.client.Connect(dataObj.ClientMachine, dataObj.ClientPort);

                    if (this.client.Connected)
                    {
                        //To write to the socket
                        this.clientStream = client.GetStream();

                        //Create objects for writing across stream
                        this.writer = new BinaryWriter(clientStream);
                        Byte[] tempStr = Encoding.ASCII.GetBytes(dataObj.MSG);

                        //writing to the port                
                        this.writer.Write(tempStr);
                        Console.WriteLine("\t Data: " + dataObj.MSG + " is written to " + dataObj.ClientMachine + " on " + dataObj.ClientPort);
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


        public void ReceiveData()   // allways rus and listen for msgs
        {
            bool errorOcurred = false;
            Socket connection = null; //The socket that is listened to       
            DataObject dataObj=null;
            try
            {
                //Creating listening Socket
                this.listener = new TcpListener(IPAddress.Parse(Constant.Client_IP), Constant.CLIENT_PORT);
                //this.listener = new TcpListener(IPAddress.Parse(serverIP), 7000);
                //Starts listening
                this.listener.Start();
                //Establish connection upon client request
                
                //int i = 0;
                while (true)
                {
                    //connection is connected socket
                    connection = listener.AcceptSocket();
                    if (connection.Connected)
                    {
                        //To read from socket create NetworkStream object associated with socket
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
                        /*try
                        {
                            string ss = reply.Substring(0, reply.IndexOf(";"));
                            port = Convert.ToInt32(ss);
                        }
                        catch (Exception)
                        {
                            port = 7000;//Constant.CLIENT_PORT;
                        }*/
                        Console.WriteLine(ip + ": " + reply.Substring(0, reply.Length - 1));
                        dataObj = new DataObject(reply.Substring(0, reply.Length - 1), ip, port);
                        lastReply = dataObj;
                        if(lastReply.MSG.StartsWith("G:")){
                            newGMsg = true;
                            lastVariableUpdate = lastReply.MSG;
                        }
                        else if (lastReply.MSG.StartsWith("L:"))
                        {
                            lastLifePack.Add(lastReply.MSG);
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
                            lastError=lastReply.MSG;
                        }
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(GameEngine.Resolve), (object)dataObj);
                        //i++;
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
                //listener.Stop();
                if (errorOcurred)
                {
                    this.ReceiveData();
                }
                
            }
            //return dataObj;
        }
        public DataObject giveLastReply()
        {
            return lastReply;
        }
        public String giveLastError()
        {
            String s=null;
            if(lastError!=null && lastError.Length!=1)
                s = (String)lastError.Substring(0,lastError.Length-1);
            lastError = null;
            return s;
        }
        public Boolean GameSAccepted()
        {
            return gameSAccepted;
        }
        public Boolean GameIAccepted()
        {
            return gameIAccepted;
        }
        public String giveSMsg()
        {
            String s = null;
            if(SMsg!=null)
                s = (String)SMsg.Clone() ;
            SMsg = null;
            return s;
        }
        public String giveLastCoin()
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
        public String giveLastLifePack()
        {
            if (lastLifePack.Count != 0)
            {
                String s = lastLifePack[0];
                lastLifePack.RemoveAt(0);
                return s;
            }
            else
            {
                return null;
            }
        }
        public String giveLastGmsg()
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
        public String giveIMsg()
        {
            return Imsg;
        }
        public Boolean connectionAvailability()
        {
            return connection;
        }
    }
}
