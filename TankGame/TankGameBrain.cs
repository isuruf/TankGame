using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TankGame
{
    class TankGameBrain
    {        
        
        private NetworkConnection conn;
        private Thread reciveThread;
        private ErrorHandler eHandler;
        private bool finished;
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
            while (!conn.GameIAccepted())
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

            while (!conn.GameSAccepted())
            {
                Thread.Sleep(200);
            }
            //gHandler.loadGrid();
            //aiHandler.loadPlayers();
            
        }
        public bool isGameStarted()
        {
            return conn.GameSAccepted();
        }
        public void process()
        {
            String command = Game1.command;
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
                command = Game1.command;
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
