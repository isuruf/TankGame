using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace TankGame
{
    public class AI
    {
        //bricks =3
        //stone=2;
        //water =4;
        //tank =5
        //coin =6
        //medikit =7
        public static String[] message = { "UP#", "RIGHT#", "DOWN#", "LEFT#", "SHOOT#" };
        public static int size = Game1.size;
        public static coord[,] coArr = new coord[size, size];
        public static Boolean[,] gridOccupied = new Boolean[size, size];
        public static Boolean[,] coinArray = new Boolean[size, size];
        static coord nextcoin;
        static int nextCoinCost = 1000;
        public AI()
        {



        }
        public static void init()
        {
            for (int i = 0; i < Game1.size; i++)
                for (int j = 0; j < Game1.size; j++)
                {
                    coArr[i, j] = new coord(i, j);
                    if (Game1.grid[i, j] == 2 || Game1.grid[i, j] == 4)
                        gridOccupied[i, j] = true;
                }
            foreach (Brick brick in Game1.brickList)
            {
                gridOccupied[brick.x, brick.y] = true;
            }
        }
        public static String nextCommand(Tank tank)
        {
            for (int i = 0; i < Game1.size; i++)
                for (int j = 0; j < Game1.size; j++)
                    coinArray[i, j] = false;

            Debug.WriteLine("starting calculating " + tank.x + " " + tank.y);
            /* int direction = tank.direction;
             Boolean found = false;
             for (int c = 0; c < Game1.coinList.Count; c++)
             {
                 if (c >= Game1.coinList.Count)
                     break;
                 Coin coin = Game1.coinList.ElementAt(c);

                 coinArray[coin.x, coin.y] = true;
                 if (nextcoin != null && Math.Abs(coin.x - tank.x) + Math.Abs(coin.y - tank.y) <= nextCoinCost - 1)
                 {
                     found = true;
                 }
                 //Debug.WriteLine("coin at " + coin.x + " " + coin.y);
             }
             if (nextcoin!=null&&!found&&(tank.x!=nextcoin.x||tank.y!=nextcoin.y))
             {
                 int nextdir = getDir(nextcoin, tank);
                 if (nextdir != -1)
                 {
                     nextCoinCost--;
                     return message[nextdir];
                 }
             }*/
            Queue<Tuple<coord, int>> queue = new Queue<Tuple<coord, int>>();
            for (int i = 0; i < Game1.size; i++)
                for (int j = 0; j < Game1.size; j++)
                {
                    coArr[i, j].reset();
                }
            int cacheDir = 0;
            int coinDist = 1000;
            Boolean cache = true;


            if (Game1.coinList.Count == 0)
                return message[4];

            for (int c = 0; c < Game1.coinList.Count; c++)
            {
                Coin coin = Game1.coinList.ElementAt(c);
                coinArray[coin.x, coin.y] = true;
            }

            coord cur = coArr[tank.x, tank.y];
            cur.length[tank.direction] = 1;
            queue.Enqueue(new Tuple<coord, int>(cur, tank.direction));

            coord next;
            int loop, nextX, nextY;
            while (queue.Count != 0)
            {
                Tuple<coord, int> t = queue.Dequeue();
                coord c = t.Item1;
                int dir = t.Item2;
                if (coinArray[c.x, c.y])
                {
                    Debug.WriteLine("found coin");
                    int nextdir = getDir(c, tank);

                    if (nextdir != -1)
                    {
                        //nextcoin = coArr[c.x, c.y];
                        //nextCoinCost=nextcoin.getLength()-1;
                        return message[nextdir];
                    }
                }
                Debug.WriteLine("travel " + c.x + " " + c.y + " length:" + c.length[dir] + " " + dir);
                c.discovered[dir] = 1;
                //rotate
                for (int i = 0; i < 4; i++)
                {
                    if (c.length[i] == 0)
                    {
                        c.length[i] = c.length[dir] + 1;
                        queue.Enqueue(new Tuple<coord, int>(c, i));
                    }
                }

                coord target = c.getNext(dir);
                if (target != null && target.length[dir] == 0 && !gridOccupied[target.x, target.y])
                {
                    target.length[dir] = c.length[dir] + 1;
                    if (target.prevX == target.x && target.prevY == target.y)
                    {
                        target.prevX = c.x;
                        target.prevY = c.y;
                        //nextMove[tank.x,tank.y,tank.direction,target.x,target.y] = c.dir+1;
                    }
                    Debug.WriteLine("travel " + c.x + " " + c.y + " target:" + target.x + " " + target.y + " dir:" + dir);
                    queue.Enqueue(new Tuple<coord, int>(target, dir));
                }
                /*   else if (target != null )
                   {
                       if (target.length[dir] != 0)
                           Debug.WriteLine("travel " + c.x + " " + c.y + " target:" + target.x + " " + target.y + " dir:" + dir + " cannot go" + target.length[dir]);
                       else
                           Debug.WriteLine("not free");
                   }*/


            }

            return "SHOOT";
        }
        public static int getDir(coord target, Tank tank)
        {
            Debug.WriteLine("Going from " + tank.x + "," + tank.y + " to " + target.x + "," + target.y + " ");
            int loop = 0;
            coord next = target;
            while (loop < 200 && next != null)
            {
                Debug.WriteLine("Next " + next.x + "," + next.y);
                loop++;
                if (next.prevX == tank.x && next.prevY == tank.y)
                    break;
                next = coArr[next.prevX, next.prevY];
            }
            for (int i = 0; i < 4; i++)
                if (next != null && next.Equals(coArr[tank.x, tank.y].getNext(i)))
                {
                    return i;
                }
            return -1;
        }
    }
    public class coord
    {
        public int x, y, prevX = 0, prevY = 0;
        public int[] discovered = new int[4];
        public int[] length = new int[4];
        public coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public void reset()
        {
            for (int i = 0; i < 4; i++)
            {
                discovered[i] = 0;
                length[i] = 0;
            }
            prevX = x;
            prevY = y;

        }
        public int getLength()
        {
            int min = 1000;
            for (int i = 0; i < 4; i++)
                if (length[i] > 0 && min > length[i])
                    min = length[i];
            return min;
        }
        public coord getNext(int dir)
        {
            int x = this.x, y = this.y;
            if (dir == 0)
                y--;
            else if (dir == 1)
                x++;
            else if (dir == 2)
                y++;
            else
                x--;
            if (x < 0 || x >= AI.size)
                return null;
            if (y < 0 || y >= AI.size)
                return null;
            return AI.coArr[x, y];
        }
    }
}
