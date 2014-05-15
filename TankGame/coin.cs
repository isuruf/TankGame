using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TankGame
{
    public class coin
    {
        public static Model coinModel;
        public Vector3 position;
   
        public float speed;
        public float angle=0;
        public Quaternion initRotation = new Quaternion(1, 0, 0, (float)Math.Cos(7*MathHelper.Pi/12));

        public void MoveForward()
        {
            angle+=speed;
        }
        public coin(Vector3 position, float speed)
        {
            this.position = position;
            this.speed = speed;
        }
        public coin(int x, int y, float speed)
        {
            this.position = new Vector3(x+0.5f,0.2f,-y-0.5f);
            this.speed = MathHelper.ToRadians(6f);
        }

        public void Draw(Matrix view,float scale)
        {
            Matrix[] boneTransforms = new Matrix[coinModel.Bones.Count];
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 0.2f, 10000.0f);
            Matrix world = Matrix.CreateRotationX(MathHelper.Pi / 2) * Matrix.CreateRotationY(MathHelper.ToRadians(angle)) * Matrix.CreateTranslation(position);

            
            coinModel.Root.Transform = Matrix.CreateScale(scale, scale, scale) * world;

            coinModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Draw the model.
            foreach (ModelMesh mesh in coinModel.Meshes)
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
