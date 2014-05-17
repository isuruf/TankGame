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
        public static String[] message =  { "UP#", "RIGHT#", "DOWN#", "LEFT#", "SHOOT#" };
        public static int size = Game1.size;
        public static coord[,] coArr = new coord[size, size];
        //public static int[, ,] nextMove = new int[size * size, size, size];
        public AI()
        {
             for (int i = 0; i < Game1.size; i++)
                 for (int j = 0; j < Game1.size; j++)
                 {
                     coArr[i, j] = new coord(i, j);
                 }
        }
        public static String nextCommand(Tank tank)
        {
            Debug.WriteLine("starting calculating " + tank.x + " " + tank.y);
            int direction = tank.direction;
            Boolean[,] gridOccupied =  new Boolean[size,size];
            Boolean[,] coinArray = new Boolean[size, size];
            Queue<Tuple<coord,int>> queue = new Queue<Tuple<coord,int>>();
            for (int i = 0; i < Game1.size; i++)
                for (int j = 0; j < Game1.size; j++){
                    coArr[i,j].reset();
                    if (Game1.grid[i, j] == 2 || Game1.grid[i, j] == 4)
                        gridOccupied[i, j] = true;
                }
            for (int c = 0; c < Game1.coinList.Count; c++)
            {
                Coin coin = Game1.coinList.ElementAt(c);
                coinArray[coin.x, coin.y] = true;
                Debug.WriteLine("coin at " + coin.x + " " + coin.y);
            }
            
             
            foreach (Brick brick in Game1.brickList)
            {
                gridOccupied[brick.x, brick.y] = true;
            }
            coord cur = coArr[tank.x, tank.y];
            cur.length[tank.direction]=1;
            cur.dir = tank.direction;
            queue.Enqueue(new Tuple<coord,int>(cur,tank.direction));

            coord next;
            int loop, nextX, nextY;
            while (queue.Count != 0)
            {
                Tuple<coord,int> t = queue.Dequeue();
                coord c = t.Item1;
                int dir = t.Item2;
                if (coinArray[c.x, c.y])
                {
                    Debug.WriteLine("found coin");
                    loop = 0;
                    next = coArr[c.x, c.y];
                    while (loop < 50 && next != null )
                    {
                        Debug.WriteLine("coin " + c.x + " " + c.y + " " + next.x + " " + next.y);
                        loop++;
                        nextX = next.x;
                        nextY = next.y;
                        if(next.prevX == tank.x && next.prevY == tank.y)
                            break;
                        next = coArr[next.prevX, next.prevY];
                    }
                    for (int i = 0; i < 4; i++)
                        if (next != null && next.Equals(coArr[tank.x, tank.y].getNext(i)))
                        {
                            return message[i];
                        }
                }
                Debug.WriteLine("travel " + c.x + " " + c.y + " length:" + c.length[dir] + " " + dir);
                c.discovered[dir]=1;
                //rotate
                for(int i=0;i<4;i++){
                    if(c.length[i]==0){
                        c.length[i]=c.length[dir]+1;
                        queue.Enqueue(new Tuple<coord,int>(c,i));
                        if (c.x == tank.x && c.y == tank.y)
                            c.dir = i;
                    }
                }
                
                coord target = c.getNext(dir);
                if (target!=null&&target.length[dir]==0&&Game1.grid[target.x,target.y]==0)
                {
                    target.length[dir]=c.length[dir]+1;
                    if (target.prevX == target.x && target.prevY == target.y)
                    {
                        target.prevX = c.x;
                        target.prevY = c.y;
                        target.dir = c.dir;
                    }
                    Debug.WriteLine("travel " + c.x + " " + c.y + " target:" + target.x+" "+target.y + " dir:" + dir);
                    queue.Enqueue(new Tuple<coord,int>(target,dir));
                }
                else if (target != null )
                {
                    if (target.length[dir] != 0)
                        Debug.WriteLine("travel " + c.x + " " + c.y + " target:" + target.x + " " + target.y + " dir:" + dir + " cannot go" + target.length[dir]);
                    else
                        Debug.WriteLine("not free");
                }

                
            }
            coord temp=null;
            next=null;
            int distance =1000;
            nextX = -1; nextY = -1;

            for(int c=0;c<Game1.coinList.Count;c++)
            {
                Coin coin = Game1.coinList.ElementAt(c);
                Debug.WriteLine("coin " + coin.x + " " + coin.y + " " + coArr[coin.x, coin.y].getLength());
                if (coArr[coin.x, coin.y].getLength() < distance)
                {
                    loop = 0;
                    next = coArr[coin.x, coin.y];
                    while (loop<50&&next != null&&(next.x!=tank.x||next.y!=tank.y))
                    {
                        Debug.WriteLine("coin " + coin.x + " " + coin.y+" "+next.x+" "+next.y);
                        loop++;
                        nextX=next.x;
                        nextY=next.y;
                        next = coArr[next.prevX, next.prevY];
                    }
                    Boolean found=false;
               /*     for (int i = 0; i < 5; i++)
                    {
                        if (Game1.tankArr[i] != null&&Game1.tankArr[i].health!=0
                            &&(Game1.tankArr[i].x!=nextX||Game1.tankArr[i].y!=nextY))
                        {
                            found=true;
                        }  
                    }*/
                    if(!found){
                        distance = coArr[coin.x, coin.y].getLength();
                        temp=coArr[nextX,nextY];
                    }
                }
            }
            if (temp != null)
            {
                Console.WriteLine("ai " + temp.x + " " + temp.y);
            }
            for(int i=0;i<4;i++)
                if (temp != null && temp.Equals(coArr[tank.x, tank.y].getNext(i)))
                {
                    return message[i];
                }
            return "SHOOT";
        }
    }
    public class coord
    {
        public int x, y,prevX=0,prevY=0,dir=0;
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
            dir = 0;
        }
        public int getLength()
        {
            int min=1000;
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
            return AI.coArr[x,y];
        }
    }
}
