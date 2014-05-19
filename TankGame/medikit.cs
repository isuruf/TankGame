using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TankGame
{
    public class Medikit
    {
        public static Model medikitModel;
        public Vector3 position;
        public int x, y;
        public float liveTime;
        public float deadTime;
        public float speed;
        public float angle = 0;
        public Quaternion initRotation = new Quaternion(1, 0, 0, (float)Math.Cos(7 * MathHelper.Pi / 12));
        public BoundingSphere sphere;

        public Medikit(Vector3 position, float speed)
        {
            this.position = position;
            this.speed = speed;
            sphere = new BoundingSphere(position, 0.5f);
        }

        public Medikit(int x, int y, float liveTime)
        {
            this.position = new Vector3(Game1.size - x - 0.5f, 0.2f, -y - 0.5f);
            this.x = x;
            this.y = y;
            this.deadTime = Game1.time + liveTime;
            this.liveTime = liveTime;
            this.speed = MathHelper.ToRadians(20f);
            sphere = new BoundingSphere(position, 0.5f);
        }

        public void update(float currentTime)
        {
            float remainingTime = (deadTime - currentTime);
            if (remainingTime < 6000)
                this.speed = MathHelper.ToRadians(20f + (6000 - remainingTime) / 20f);
            if (remainingTime <= 0)
                Game1.medikitList.Remove(this);
            angle += speed;
            //Console.WriteLine("x: "+x+" y: "+y+ " ded: " + deadTime + " liv: " + liveTime + " rem: " + remainingTime+" curr: "+currentTime);
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
