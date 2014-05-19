using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace TankGame
{
    /// <summary>
    /// Defined Constants of the Game
    /// </summary>
    public class Constant
    {
      
        #region "Server Configurations"
        public static string SERVER_IP = ConfigurationManager.AppSettings.Get("ServerIP");
        public static int SERVER_PORT = int.Parse(ConfigurationManager.AppSettings.Get("ServerPort"));
        public static string Client_IP = ConfigurationManager.AppSettings.Get("ClientIP");
        public static int CLIENT_PORT = int.Parse(ConfigurationManager.AppSettings.Get("ClientPort"));
        public static int BULLET_MULTI = int.Parse(ConfigurationManager.AppSettings.Get("BulletSpeedMulti"));
        public static int PLUNDER_TREASUR_LIFE = int.Parse(ConfigurationManager.AppSettings.Get("PlunderCoinPileLife"));

        #endregion
    }
}
