using System;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace TankGame
{
    class GameLogic
    {

        private NetworkConnection conn;
        private Thread reciveThread;
       // private ErrorHandler eHandler;
        private AI ai = new AI();
        private bool finished;
        private int playerName;
        private Stopwatch watch = Stopwatch.StartNew();

        public GameLogic()
        {
            conn = new NetworkConnection();
            reciveThread = new Thread(new ThreadStart(conn.receiveData));
            reciveThread.Priority = ThreadPriority.Highest;
           // eHandler = new ErrorHandler(conn);
            finished = false;
        }
        public void startGame()
        {
            reciveThread.Start();
            do
            {
                conn.sendData(new DataObject("JOIN#", Constant.SERVER_IP, Constant.SERVER_PORT));
                Thread.Sleep(4057);
            } while (!conn.isConnectionAvailable());   //loop until server starts listning to clients
            Console.WriteLine("joined");
            while (!conn.gameIAccepted)
            {
                Thread.Sleep(200);
                //if (eHandler.giveJoinError() != null)
                {
                    //return; ///a code to gui to display join error
                }
            }
        }

        public void waitGameStarted()
        {

            while (!conn.gameSAccepted)
            {
                Thread.Sleep(200);
            }
            //gHandler.loadGrid();
            //aiHandler.loadPlayers();

        }
        public bool isGameStarted()
        {
            return conn.gameSAccepted;
        }
        public void initGrid()
        {
            while (!conn.gameIAccepted) { };
            String Imessage = conn.getIMsg();
            Imessage = Imessage.Substring(0, Imessage.Length - 1);
            String[] IMsg = Imessage.Split(':');
            //String msgType = IMsg[0];
            playerName = int.Parse(IMsg[1].Substring(1));
            String[] brickLocations = IMsg[2].Split(';');
            String[] stoneLocations = IMsg[3].Split(';');
            String[] waterLocations = IMsg[4].Split(';');

            for (int i = 0; i < brickLocations.Length; ++i)
            {
                String[] coordinates = brickLocations[i].Split(',');
                int x = int.Parse(coordinates[0]);
                int y = int.Parse(coordinates[1]);
                Game1.grid[x, y] = 3;//Bricks are zeros
                Brick tempBrick = new Brick(x, y);
                Game1.brickList.Add(tempBrick);
                Game1.brickArray[x, y] = tempBrick;
            }

            for (int i = 0; i < stoneLocations.Length; ++i)
            {
                String[] coordinates = stoneLocations[i].Split(',');
                int x = int.Parse(coordinates[0]);
                int y = int.Parse(coordinates[1]);
                Game1.stoneList.Add(new BoundingSphere(new Vector3(Game1.size - x - 0.5f, 0.5f, -y - 0.5f), 0.5f));
                Game1.grid[x, y] = 2;//Stones are ones
            }

            for (int i = 0; i < waterLocations.Length; ++i)
            {
                String[] coordinates = waterLocations[i].Split(',');
                int x = int.Parse(coordinates[0]);
                int y = int.Parse(coordinates[1]);
                Game1.grid[x, y] = 4;//Water locations are twos
            }
        }

        public void initTanks()
        {

            while (!conn.gameSAccepted) { };
            String Smessage = conn.getSMsg();
            Debug.WriteLine("Smessage " + Smessage);
            Debug.WriteLine("Imessage " + conn.getIMsg());
            Debug.WriteLine(Smessage.Length - 1);
            Smessage = Smessage.Substring(2, Smessage.Length - 3);
            String[] SMsg = Smessage.Split(':');
            //String msgType = IMsg[0];
            //String[] brickLocations = SMsg[2].Split(';');
            //String[] stoneLocations = SMsg[3].Split(';');
            //String[] waterLocations = SMsg[4].Split(';');

            for (int i = 0; i < SMsg.Length; ++i)
            {
                String[] playerData = SMsg[i].Split(';');
                int playerNum = int.Parse(playerData[0].Substring(1));

                String[] coordinates = playerData[1].Split(',');
                int x = int.Parse(coordinates[0]);
                int y = int.Parse(coordinates[1]);
                int direction = int.Parse(playerData[2]);
                Game1.tankArr[playerNum] = new Tank(x, y, direction, playerNum);
                if (playerNum == this.playerName)
                    Game1.tank = Game1.tankArr[playerNum];

            }
        }

        public void updateGrid()
        {
            String Gmessage = conn.getLastGmsg();
            
            Gmessage = Gmessage.Substring(2, Gmessage.Length - 3);
            String[] GMsg = Gmessage.Split(':');
            //String msgType = IMsg[0];


            for (int i = 0; i < GMsg.Length - 1; ++i)
            {
                String[] playerData = GMsg[i].Split(';');
                int playerNum = int.Parse(playerData[0].Substring(1));
                String[] coordinates = playerData[1].Split(',');

                int x = int.Parse(coordinates[0]);
                int y = int.Parse(coordinates[1]);
                int direction = int.Parse(playerData[2]);
                int shot = int.Parse(playerData[3]);
                float health = int.Parse(playerData[4]) / 100f;
                int coins = int.Parse(playerData[5]);
                int score = int.Parse(playerData[6]);

                Game1.tankArr[playerNum].updatePosition(x, y, direction, shot, score, coins, health);
            }
            String[] brickHealth = GMsg[GMsg.Length - 1].Split(';');
            for (int i = 0; i < brickHealth.Length; ++i)
            {
                String[] status = brickHealth[i].Split(',');
                int x = int.Parse(status[0]);
                int y = int.Parse(status[1]);
                float health = (int.Parse(status[2])) / 4f;
                if (Game1.brickArray[x, y] != null)
                {
                    Game1.brickArray[x, y].update(health);
                    if (health == 0)
                    {
                        Debug.WriteLine("Removing brick at " + x + "," + y + " " + brickHealth[i]);
                        Game1.brickList.Remove(Game1.brickArray[x, y]);
                        Game1.brickArray[x, y] = null;
                    }
                }
            }

        }

        public void placeCoins()
        {
            String Cmessage = conn.getLastCoin();
            if (Cmessage != null)
            {
                Cmessage = Cmessage.Substring(2, Cmessage.Length - 3);
                String[] CMsg = Cmessage.Split(':');

                for (int i = 0; i < CMsg.Length; ++i)
                {
                    String[] coordinates = CMsg[0].Split(',');

                    int x = int.Parse(coordinates[0]);
                    int y = int.Parse(coordinates[1]);
                    float liveTime = float.Parse(CMsg[1]);
                    int value = int.Parse(CMsg[2]);
                    Game1.coinList.Add(new Coin(x, y, value, liveTime));
                }
            }
        }

        public void placeMedikits()
        {
            Queue<String> queue = conn.getMedikitQueue();
            while (queue.Count != 0)
            {
                String Lmessage = queue.Dequeue();
                if (Lmessage != null)
                {
                    Lmessage = Lmessage.Substring(2, Lmessage.Length - 3);
                    String[] LMsg = Lmessage.Split(':');

                    for (int i = 0; i < LMsg.Length; ++i)
                    {
                        String[] coordinates = LMsg[0].Split(',');

                        int x = int.Parse(coordinates[0]);
                        int y = int.Parse(coordinates[1]);
                        float liveTime = float.Parse(LMsg[1]);
                        Game1.medikitList.Add(new Medikit(x, y, liveTime));
                    }
                }
            }
        }

        public void process()
        {
            //updateGrid();


            String command = "SHOOT#";
            if (conn.gameSAccepted && Game1.tank != null)
                command = AI.nextCommand(Game1.tank);
            while (true)
            {
                if (true)
                {
                    if (command != Constant.STOP)
                    {
                        conn.sendData(new DataObject(command, Constant.SERVER_IP, Constant.SERVER_PORT));
                    }
                }
                
                while (!conn.isNewGMsg())
                {
                }
                //watch.Stop();
                //var elapsedMs = watch.ElapsedMilliseconds;

                Debug.WriteLine("turn " + conn.turn);
                command = "SHOOT#";
                if (conn.gameSAccepted && Game1.tank != null)
                {
                    watch = Stopwatch.StartNew();
                    updateGrid();
                    command = AI.nextCommand(Game1.tank);
                    watch.Stop();
                    Debug.WriteLine("time-elapsed " + watch.ElapsedMilliseconds);
                }

            }

        }
        public bool isFinished()
        {
            return finished;
        }
        public Boolean isCommunicationAvailabel()
        {
            return conn.isConnectionAvailable();
        }
    }
}
