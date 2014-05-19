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
        public static int[,] stuffArray = new int[size, size];
        static coord nextcoin;
        static int nextCoinCost = 1000;
        public AI()
        {



        }
        public static void init()
        {
            
        }
        public static String nextCommand(Tank tank)
        {
            for (int i = 0; i < Game1.size; i++)
                for (int j = 0; j < Game1.size; j++)
                {
                    coArr[i, j] = new coord(i, j);
                    if (Game1.grid[i, j] == 2 || Game1.grid[i, j] == 4)
                        gridOccupied[i, j] = true;
                }
            for (int c = 0; c < Game1.brickList.Count; c++)
            {
                if (c >= Game1.brickList.Count)
                    break;
                Brick brick = Game1.brickList.ElementAt(c);
                if(brick.health>=0)
                    gridOccupied[brick.x, brick.y] = true;
            }
            for (int i = 0; i < Game1.size; i++)
                for (int j = 0; j < Game1.size; j++)
                    stuffArray[i, j] = 0;

            Debug.WriteLine("starting calculating " + tank.x + " " + tank.y);

            Queue<Tuple<coord, int>> queue = new Queue<Tuple<coord, int>>();
            for (int i = 0; i < Game1.size; i++)
                for (int j = 0; j < Game1.size; j++)
                {
                    coArr[i, j].reset();
                }

            for (int c = 0; c < Game1.coinList.Count; c++)
            {
                Coin coin = Game1.coinList.ElementAt(c);
                stuffArray[coin.x, coin.y] = 1;
            }
            for (int c = 0; c < Game1.medikitList.Count; c++)
            {
                Medikit medikit = Game1.medikitList.ElementAt(c);
                stuffArray[medikit.x, medikit.y] = 2;
            }
            for (int j = 0; j < 5; j++)
            {
                if (Game1.tankArr[j] != null && Game1.tankArr[j] != tank && Game1.tankArr[j].health>0)
                {
                    stuffArray[Game1.tankArr[j].x, Game1.tankArr[j].y] = 3;
                }
            }
            //for(int i=0;
            coord cur = coArr[tank.x, tank.y];
            cur.length[tank.direction] = 1;
            queue.Enqueue(new Tuple<coord, int>(cur, tank.direction));
            coord next;
            next = coArr[tank.x, tank.y];
            if(collisionTime(next)==1000){
                next= next.getNext(tank.direction);

                if (next!=null&&collisionTime(next) != 1000&&!gridOccupied[next.x,next.y])
                {
                    Debug.WriteLine("Avoiding bullet");
                    return message[tank.direction];
                }
                else
                {

                }
            }
            next = coArr[tank.x, tank.y];
            if(tank.health>0.5||Game1.medikitList.Count==0){
                for (int i = 0; i < 20; i++)
                {
                    next = next.getNext(tank.direction);
                    if (next == null||Game1.grid[next.x,next.y]==2)
                        break;
                    if (stuffArray[next.x, next.y] == 3)
                    {
                        Debug.WriteLine("shooting tank");
                        return message[4];
                    }
                }
                for (int di = 0; di < 4; di++)
                {
                    next = coArr[tank.x, tank.y];
                    for (int i = 0; i < 20; i++)
                    {
                        next = next.getNext(di);
                        if (next == null || Game1.grid[next.x, next.y] == 2)
                            break;
                        if (stuffArray[next.x, next.y] == 3)
                        {
                            Debug.WriteLine("shooting tank");
                            return message[di];
                        }
                    }
                }
            }
            int maxLength = 0;
            int medikitdir = -1;
            int tankdir = -1;
            while (queue.Count != 0&&maxLength<=100)
            {
                
                Tuple<coord, int> t = queue.Dequeue();
                coord c = t.Item1;
                int dir = t.Item2;
                if (c!=coArr[tank.x,tank.y]&&(stuffArray[c.x, c.y]==1||(stuffArray[c.x,c.y]==2&&medikitdir==-1)
                    || (stuffArray[c.x, c.y] == 3 && tankdir == -1)))
                {
                    
                    int length = c.getLength();
                    Boolean go = true;
                    for (int j = 0; j < 5; j++)
                    {
                        if (Game1.tankArr[j] != null && Game1.tankArr[j] != tank&&Game1.tankArr[j].health!=0)
                        {
                            if (Math.Abs(Game1.tankArr[j].x - c.x) + Math.Abs(Game1.tankArr[j].y - c.y)*2<length)
                            {
                                go = false;
                            }
                        }
                    }
                    if (go)
                    {
                        int nextdir = getDir(c, tank);
                        coord temp = coArr[tank.x, tank.y].getNext(nextdir);

                        if (temp==null||gridOccupied[temp.x, temp.y])
                            go=false;
                        if (nextdir != -1&&go)
                        {
                            if (stuffArray[c.x, c.y] == 2)
                            {
                                medikitdir = nextdir;
                            }
                            else if(stuffArray[c.x, c.y] == 1)
                            {
                                Debug.WriteLine("Going to coin at "+c.x+" "+c.y+" by going to "+temp.x+" "+temp.y+" by "+nextdir);
                              /*  for (int k = 0; k < Game1.size; k++)
                                {
                                    for (int l = 0; l < Game1.size; l++)
                                    {
                                        Debug.Write(coArr[k,l].getLength()+" \t");
                                    }
                                    Debug.WriteLine("");
                                }*/
                                    return message[nextdir];
                            }
                            else if (stuffArray[c.x, c.y] == 3)
                            {
                                tankdir = nextdir;
                            }

                        }
                        if (!go)
                            Debug.WriteLine("ERROR");
                    }
                }
                //Debug.WriteLine("travel " + c.x + " " + c.y + " length:" + c.length[dir] + " " + dir);
                c.discovered[dir] = 1;
                //rotate
                for (int i = 0; i < 4; i++)
                {
                    if (c.length[i] == 0)
                    {
                        c.length[i] = c.length[dir] + 1;
                        maxLength = c.length[i];
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
                    //Debug.WriteLine("travel " + c.x + " " + c.y + " target:" + target.x + " " + target.y + " dir:" + dir);
                    queue.Enqueue(new Tuple<coord, int>(target, dir));
                }
                /*   else if (target != null )
                   {
                       if (target.length[dir] != 0)
                           Debug.WriteLine("travel " + c.x + " " + c.y + " target:" + target.x + " " + target.y + " dir:" + dir + " cannot go" + target.length[dir]);
                       else
                           Debug.WriteLine("not free");
                   }*/
                if (Game1.coinList.Count == 0)
                    maxLength = 11;
                if (maxLength == 11)
                {
                    if (medikitdir != -1)
                    {
                        Debug.WriteLine("Going to medikit");
                        return message[medikitdir];
                    }
                    next = coArr[tank.x, tank.y];
                    for (int i = 0; i < 10; i++)
                    {
                        next = next.getNext(tank.direction);
                        if (next == null)
                            break;
                        if (stuffArray[next.x, next.y] == 3)
                        {
                            Debug.WriteLine("shooting tank");
                            return message[4];
                        }
                    }
                    if (tankdir != -1)
                    {
                        Debug.WriteLine("Going to tank");
                        return message[tankdir];
                    }
                    
                }
            }
           
            
            int dist = Math.Abs(tank.x - size / 4) + Math.Abs(tank.y - size / 2);
            for (int i = 0; i < 4; i++)
            {
                next = coArr[tank.x, tank.y].getNext(i);
                if (next == null)
                    continue;
                int dist2=Math.Abs(next.x - size /4 ) + Math.Abs(next.y - size / 2);
                if (!gridOccupied[next.x, next.y]&&dist2<dist)
                {
                    Debug.WriteLine("Going to middle");
                    return message[i];
                }
            }
            return "SHOOT#";
        }
        public static int getDir(coord target, Tank tank)
        {
            Debug.WriteLine("Going from " + tank.x + "," + tank.y + " to " + target.x + "," + target.y + " "+Game1.tank.direction);
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
        public static float collisionTime(coord tank)
        {
            float time =1000;
            for (int i = 0; i < Game1.bulletList.Count; i++)
            {
                float temp = collisionTime(Game1.bulletList.ElementAt(i),tank);
                if (temp < time)
                    time=temp;
            }
            return time;
        }
        public static float collisionTime(Bullet bullet, coord tank)
        {
            int direction = bullet.direction;
            if (direction == 0)
            {
                if (tank.x == bullet.x && tank.y >= bullet.y)
                {
                    for (int y = (int)Math.Ceiling(bullet.y); y < tank.y; y++)
                    {
                        if (y >= 0 && y < Game1.size && gridOccupied[tank.x, y])
                            return 1000;
                    }
                    return tank.y - bullet.y;
                }
            }
            else if (direction == 1)
            {
                if (tank.y == bullet.y && tank.x >= bullet.x)
                {
                    for (int x = (int)Math.Ceiling(bullet.x); x < tank.x; x++)
                    {
                        if (x >= 0 && x < Game1.size && gridOccupied[x, tank.y])
                            return 1000;
                    }
                    return tank.y - bullet.y;
                }
            }
            else if (direction == 2)
            {
                if (tank.x == bullet.x && tank.y <= bullet.y)
                {
                    for (int y = (int)Math.Floor(bullet.y); y > tank.y; y--)
                    {
                        if (y>=0&&y<Game1.size&&gridOccupied[tank.x, y])
                            return 1000;
                    }
                    return tank.y - bullet.y;
                }
            }
            else if (direction == 3)
            {
                if (tank.y == bullet.y && tank.x <= bullet.x)
                {
                    for (int x = (int)Math.Floor(bullet.x); x > tank.x; x--)
                    {
                        if (x >= 0 && x < Game1.size && gridOccupied[x, tank.y])
                            return 1000;
                    }
                    return tank.y - bullet.y;
                }
            }
            
            return 10000;
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
