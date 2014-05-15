using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankGame
{
    class ErrorHandler
    {
        private String joinError;
        private String movingshootingError;
        private Boolean gameFinished;
        private String error;
        private Boolean myPlayerDead;
        NetworkConnection conn;
        public ErrorHandler(NetworkConnection conn)
        {
            this.conn = conn;
            joinError = null;
            movingshootingError = null;
            error = null;
        }
        public void loadLastError()
        {
            String s = conn.giveLastError();
            if (s != null)
            {
                if (s.Equals(Constant.S2C_CONTESTANTSFULL) || s.Equals(Constant.S2C_ALREADYADDED) || s.Equals(Constant.S2C_GAMESTARTED))
                {
                    joinError = s;
                }
                else if (s.Equals(Constant.S2C_HITONOBSTACLE) || s.Equals(Constant.S2C_CELLOCCUPIED)  || s.Equals(Constant.S2C_TOOEARLY) || s.Equals(Constant.S2C_INVALIDCELL)  || s.Equals(Constant.S2C_NOTSTARTED) || s.Equals(Constant.S2C_NOTACONTESTANT))
                {
                    movingshootingError = s;
                }
                else if (s.Equals(Constant.S2C_GAMEJUSTFINISHED) || s.Equals(Constant.S2C_GAMEOVER))
                {
                    gameFinished = true;
                }
                else if(s.Equals(Constant.S2C_NOTALIVE) || s.Equals(Constant.S2C_FALLENTOPIT))
                {
                    myPlayerDead=true;
                }
                else
                {
                    error = s;
                }
            }
        }
        public String giveJoinError()
        {
            loadLastError();
            return joinError;
        }
        public String giveMovingShootingError()
        {
            loadLastError();
            String s = null;
            if(movingshootingError!=null)
            {
            s =(String) movingshootingError.Clone();
            movingshootingError = null;
            }
            return s;
        }
        public Boolean isGameFinished()
        {
            loadLastError();
            return gameFinished;
        }
        public String giveError()
        {
            loadLastError();
            return error;
        }
        public Boolean isMyPlayerDead()
        {
            return myPlayerDead;
        }
    }
}
