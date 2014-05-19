using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TankGame
{
    public class Bullet
    {
        public static Model bulletModel;
        public Vector3 position;
        public Quaternion rotation;
        public float speed;
        public int tankNum;
        public BoundingSphere sphere;
        public float x, y;
        public int direction;
 //       public int player;
        public void MoveForward()
        {

            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), rotation);
            position += addVector * speed;
            sphere.Center = new Vector3(position.X,0.5f,position.Z);
            if (direction == 0)
                y -= 4f / 60f;
            else if (direction == 1)
                x += 4f / 60f;
            else if (direction == 2)
                y += 4f / 60f;
            else
                x -= 4f / 60f;
        }
        public Bullet(Vector3 position, Quaternion rotation, float speed, int tankNum,float x, float y,int direction)
        {
            this.position = position;
            this.rotation = rotation;
            this.speed = speed;
            this.tankNum = tankNum;
            this.x = x;
            this.y = y;
            this.direction = direction;
            sphere = new BoundingSphere(new Vector3(position.X, 0.5f, position.Z), 0.35f);
        }

        public void Draw(Matrix view, float scale)
        {
            Matrix[] boneTransforms = new Matrix[bulletModel.Bones.Count];
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 0.2f, 10000.0f);
            Matrix world = Matrix.CreateRotationY(MathHelper.Pi / 2) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);


            bulletModel.Root.Transform = Matrix.CreateScale(scale, scale, scale) * world;

            bulletModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Draw the model.
            foreach (ModelMesh mesh in bulletModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index];
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();
                }

                mesh.Draw();
            }
        }

        public void checkCollisions()
        {
            for (int i = 0; i < 5; ++i)
            {
                if (Game1.tankArr[i] != null && Game1.tankArr[i].sphere.Contains(this.sphere) != ContainmentType.Disjoint)
                {
                    //Debug.WriteLine("Medikit removed " + Game1.medikitList.ElementAt(i).x + " " + Game1.medikitList.ElementAt(i).y);
                    if (Game1.tankArr[i].num != tankNum)
                        Game1.bulletList.Remove(this);
                }
            }
            for (int i = 0; i < Game1.brickList.Count; ++i)
            {
                if (Game1.brickList.ElementAt(i).sphere.Contains(this.sphere) != ContainmentType.Disjoint)
                {
                    Game1.bulletList.Remove(this);
                }
            }
            for (int i = 0; i < Game1.stoneList.Count; ++i)
            {
                if (Game1.stoneList.ElementAt(i).Contains(this.sphere) != ContainmentType.Disjoint)
                {
                    Game1.bulletList.Remove(this);
                }
            }
        }
    }
}
