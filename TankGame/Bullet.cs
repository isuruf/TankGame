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
 //       public int player;

        public void MoveForward()
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), rotation);
            position += addVector * speed;
        }
        public Bullet(Vector3 position, Quaternion rotation, float speed, int tankNum)
        {
            this.position = position;
            this.rotation = rotation;
            this.speed = speed;
            this.tankNum = tankNum;
            sphere = new BoundingSphere(new Vector3(position.X,0.5f,position.Z), 0.5f);
//            this.player = player;
        }
        public void Draw(Matrix view,float scale)
        {
            Matrix[] boneTransforms = new Matrix[bulletModel.Bones.Count];
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 0.2f, 10000.0f);
            Matrix world = Matrix.CreateRotationY(MathHelper.Pi/2) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);

            
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
    }
}
