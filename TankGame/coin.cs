using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TankGame
{
    public class Coin
    {
        public static Model coinModel;
        public Vector3 position;
        public int value, x, y; 
        public float liveTime;
        public float deadTime;
        public float speed;
        public float angle=0;
        public Quaternion initRotation = new Quaternion(1, 0, 0, (float)Math.Cos(7 * MathHelper.Pi / 12));
        public BoundingSphere sphere;

        public Coin(Vector3 position, float speed)
        {
            this.position = position;
            this.speed = speed;
            sphere = new BoundingSphere(position, 0.5f);
        }
        public Coin(int x, int y, int value, float liveTime)
        {
            this.position = new Vector3(Game1.size-x-0.5f,0.2f,-y-0.5f);
            this.x = x;
            this.y = y;
            this.value = value;
            this.deadTime = Game1.time + liveTime;
            this.liveTime = liveTime;
            this.speed = MathHelper.ToRadians(20f);
            sphere = new BoundingSphere(position, 0.5f);
        }

        public void update(float currentTime)
        {
            float remainingTime = (deadTime - currentTime);
            if (remainingTime<6000)
                this.speed = MathHelper.ToRadians(20f+(6000-remainingTime)/20f);
            if (remainingTime <= 0)
                Game1.coinList.Remove(this);
            angle+=speed;
        }

        public void Draw(Matrix view,float scale)
        {
            Matrix[] boneTransforms = new Matrix[coinModel.Bones.Count];
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 0.2f, 10000.0f);
            Matrix world = Matrix.CreateRotationX(MathHelper.Pi / 2) * Matrix.CreateRotationY(MathHelper.ToRadians(angle)) * Matrix.CreateTranslation(position);

            
            coinModel.Root.Transform = Matrix.CreateScale(scale, scale*4, scale) * world;

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
