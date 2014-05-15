using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TankGame
{
    public class medikit
    {
        public static Model medikitModel;
        public Vector3 position;
        public int x,y;
        public float speed;
        public float angle = 0;
        public Quaternion initRotation = new Quaternion(1, 0, 0, (float)Math.Cos(7 * MathHelper.Pi / 12));
        //       public int player;

        public void MoveForward()
        {
            angle += speed;
        }
        public medikit(Vector3 position, float speed)
        {
            this.position = position;

            this.speed = speed;
            //            this.player = player;
        }
        public medikit(int x, int y)
        {
            this.position = new Vector3(x+0.5f,0.2f,-y-0.5f);
            this.speed = MathHelper.ToRadians(6f);
            this.x = x;
            this.y = y;
        }
        public void Draw(Matrix view, float scale)
        {
            Matrix[] boneTransforms = new Matrix[medikitModel.Bones.Count];
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 0.2f, 10000.0f);
            Matrix world = Matrix.CreateRotationX(MathHelper.Pi / 2) * Matrix.CreateRotationY(MathHelper.ToRadians(angle)) * Matrix.CreateTranslation(position);


            medikitModel.Root.Transform = Matrix.CreateScale(scale, scale, scale) * world;

            medikitModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Draw the model.
            foreach (ModelMesh mesh in medikitModel.Meshes)
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
