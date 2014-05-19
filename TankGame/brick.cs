using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TankGame
{
    public class Brick
    {
        public float health;
        public int x, y;
        public BoundingSphere sphere;

        public Brick(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.health = 4;
            sphere = new BoundingSphere(new Vector3(Game1.size - x - 0.5f, 0.5f, -y - 0.5f), 0.5f);
        }

        public void AddToDraw()
        {
            int currentbuilding = 5;
                   int currentheight = 1;

                   int x1 = Game1.offset-x;
                   int z = y;
                   float imagesInTexture = Game1.imagesInTexture;
            //floor or ceiling
            Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, currentheight, -z), new Vector3(0, 1, 0), new Vector2(currentbuilding * 2 / imagesInTexture, 1)));
            Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, currentheight, -z - 1), new Vector3(0, 1, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));
            Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, currentheight, -z), new Vector3(0, 1, 0), new Vector2((currentbuilding * 2 + 1) / imagesInTexture, 1)));

            Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, currentheight, -z - 1), new Vector3(0, 1, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));
            Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, currentheight, -z - 1), new Vector3(0, 1, 0), new Vector2((currentbuilding * 2 + 1) / imagesInTexture, 0)));
            Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, currentheight, -z), new Vector3(0, 1, 0), new Vector2((currentbuilding * 2 + 1) / imagesInTexture, 1)));

            
                //front wall
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, 0, -z - 1), new Vector3(0, 0, -1), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, currentheight, -z - 1), new Vector3(0, 0, -1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, 0, -z - 1), new Vector3(0, 0, -1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 1)));

                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, currentheight, -z - 1), new Vector3(0, 0, -1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, 0, -z - 1), new Vector3(0, 0, -1), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, currentheight, -z - 1), new Vector3(0, 0, -1), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));

                //back wall
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, 0, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, 0, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 1)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, currentheight, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));

                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, currentheight, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, currentheight, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, 0, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));

                //left wall
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, 0, -z), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, 0, -z - 1), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 1)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, currentheight, -z - 1), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));

                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, currentheight, -z - 1), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, currentheight, -z), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1, 0, -z), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));

                //right wall
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, 0, -z), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, currentheight, -z - 1), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, 0, -z - 1), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 1)));

                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, currentheight, -z - 1), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, 0, -z), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                Game1.verticesList.Add(new VertexPositionNormalTexture(new Vector3(x1 + 1, currentheight, -z), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));
            
        }
        public void update(float health)
        {
            this.health = health;
            if (health == 0)
                Console.WriteLine("brick " + x + " " + y);
        }
    }
}
