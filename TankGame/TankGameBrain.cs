using System;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace TankGame
{
    class TankGameBrain
    {        
        
        private NetworkConnection conn;
        private Thread reciveThread;
        private ErrorHandler eHandler;
        private AI ai = new AI();
        private bool finished;
        private int playerName;
        public TankGameBrain()
        {
            conn = new NetworkConnection();            
            reciveThread = new Thread(new ThreadStart(conn.ReceiveData));
            reciveThread.Priority = ThreadPriority.Highest;
            eHandler = new ErrorHandler(conn);
            finished = false;
        }
        public void startGame()
        {
            reciveThread.Start();
            do
            {
                conn.SendData(new DataObject("JOIN#", Constant.SERVER_IP, Constant.SERVER_PORT));
                Thread.Sleep(4057);
            } while (!conn.connectionAvailability());   //loop until server starts listning to clients
            Console.WriteLine("joined");
            while (!conn.gameIAccepted)
            {
                Thread.Sleep(200);
                if (eHandler.giveJoinError() != null)
                {
                    return; ///a code to gui to display join error
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
            String Imessage = conn.giveIMsg();
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
                Brick tempBrick = new Brick(x,y);
                Game1.brickList.Add(tempBrick);
                Game1.brickArray[x, y] = tempBrick;
            }

            for (int i = 0; i < stoneLocations.Length; ++i)
            {
                String[] coordinates = stoneLocations[i].Split(',');
                int x = int.Parse(coordinates[0]);
                int y = int.Parse(coordinates[1]);
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
            String Smessage = conn.giveSMsg();
            Debug.WriteLine("Smessage " + Smessage);
            Debug.WriteLine("Imessage " + conn.giveIMsg());
            Debug.WriteLine(Smessage.Length-1);
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
                Game1.tankArr[playerNum] = new Tank(x,y,direction,playerNum);
                if (playerNum == this.playerName)
                    Game1.tank = Game1.tankArr[playerNum];

            }
        }

        public void updateGrid()
        {
            if (conn.isNewGMsg())
            {
                String Gmessage = conn.giveLastGmsg();
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
                    int health = int.Parse(playerData[4]);
                    int coins = int.Parse(playerData[5]);
                    int score = int.Parse(playerData[6]);

                    Game1.tankArr[playerNum].updatePosition(x, y, direction, shot, score, coins, health / 4);
                }
                String[] brickHealth = GMsg[GMsg.Length - 1].Split(';');
                for (int i = 0; i < brickHealth.Length; ++i)
                {
                    String[] status = brickHealth[i].Split(',');
                    int x = int.Parse(status[0]);
                    int y = int.Parse(status[1]);
                    int health = int.Parse(status[2]);
                    if (Game1.brickArray[x, y] != null)
                    {
                        Game1.brickArray[x, y].update(health);
                        if (health == 0)
                        {
                            Game1.brickList.Remove(Game1.brickArray[x, y]);
                            Game1.brickArray[x, y] = null;
                        }
                    }
                }
            }
        }

        public void placeCoins()
        {
            String Cmessage = conn.giveLastCoin();
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
                    //Game1.coinList.Add(new Coin(new Vector3(x+5.5f, .2f, y-3.5f), MathHelper.ToRadians(6f)));

                    //Console.WriteLine("x coo = " + x);
                    //Console.WriteLine("y coo = " + y);
                    Game1.coinList.Add(new Coin(x, y, value, liveTime));
                }
            }
        }

        public void placeMedikits()
        {
            String Lmessage = conn.giveLastLifePack();
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

        public void process()
        {
            String command ="SHOOT#";
            if (conn.gameSAccepted && Game1.tank != null)
                command = AI.nextCommand(Game1.tank);
            while (!eHandler.isGameFinished())
            {
                if (!eHandler.isMyPlayerDead())
                {
                    if (command != Constant.STOP)
                    {
                        if (!conn.isNewGMsg())
                        {
                            conn.SendData(new DataObject(command, Constant.SERVER_IP, Constant.SERVER_PORT));
                            Thread.Sleep(1000);
                        }
                    }
                    if (eHandler.giveMovingShootingError() == Constant.S2C_TOOEARLY)
                    {
                        Random sleepTime = new Random();
                        Thread.Sleep(sleepTime.Next(1, 25));
                        Console.WriteLine("Wait random time to resend=====================================================");
                        continue;
                    }
                }
                if (eHandler.isGameFinished())
                {
                    finished = true;
                    break;///display in gui
                    //throw new GameFinishedException();
                }
                command = "SHOOT#";
                if (conn.gameSAccepted&&Game1.tank!=null)
                    command = AI.nextCommand(Game1.tank);
                //Thread.Sleep(1000);
            }

        }
        public bool isFinished()
        {
            return finished;
        }
        public Boolean isCommunicationAvailabel()
        {
            return conn.connectionAvailability();
        }
    }
}
