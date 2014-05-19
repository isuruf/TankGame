#region File Description
//-----------------------------------------------------------------------------
// cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System;
using System.Linq;
#endregion

namespace TankGame
{
    /// <summary>
    /// Helper class for drawing a tank model with animated wheels and turret.
    /// </summary>
    public class Tank
    {
        #region Fields


        // The XNA framework Model object that we are going to display.
        public static Model tankModel;
        public double lastBulletTime = 0;
        public Vector3 tankPosition;
        public Quaternion tankRotation = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.Pi);
        public float health;
        public int direction;
        public int x;
        public int y;
        public int num;
        public int score = 0;
        public int coins = 0;
        public Queue<Tuple<float, float, float>> moveQueue = new Queue<Tuple<float, float, float>>();
        public float dead = 0;
        public BoundingSphere sphere;
        public Vector3[] cameraPosition = new Vector3[2];
        public Vector3[] cameraUpDirection = new Vector3[2];
        Quaternion[] cameraRotation = new Quaternion[2];
        private Boolean deadcoin = false;
        private Boolean draw = true;


        // Shortcut references to the bones that we are going to animate.
        // We could just look these up inside the Draw method, but it is more
        // efficient to do the lookups while loading and cache the results.
        static ModelBone leftBackWheelBone;
        static ModelBone rightBackWheelBone;
        static ModelBone leftFrontWheelBone;
        static ModelBone rightFrontWheelBone;
        static ModelBone leftSteerBone;
        static ModelBone rightSteerBone;
        static ModelBone turretBone;
        static ModelBone cannonBone;
        static ModelBone hatchBone;


        // Store the original transform matrix for each animating bone.
        static Matrix leftBackWheelTransform;
        static Matrix rightBackWheelTransform;
        static Matrix leftFrontWheelTransform;
        static Matrix rightFrontWheelTransform;
        static Matrix leftSteerTransform;
        static Matrix rightSteerTransform;
        static Matrix turretTransform;
        static Matrix cannonTransform;
        static Matrix hatchTransform;


        // Array holding all the bone transform matrices for the entire model.
        // We could just allocate this locally inside the Draw method, but it
        // is more efficient to reuse a single array, as this avoids creating
        // unnecessary garbage.


        // Current animation positions.
        float wheelRotationValue = 0;
        float steerRotationValue = 0;
        float turretRotationValue = 0;
        float cannonRotationValue = 0;
        float hatchRotationValue = 0;
        Color[] colors = { Color.Pink, Color.Yellow, Color.Cyan, Color.LawnGreen, Color.Orange };

        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the wheel rotation amount.
        /// </summary>
        public float WheelRotation
        {
            get { return wheelRotationValue; }
            set { wheelRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the steering rotation amount.
        /// </summary>
        public float SteerRotation
        {
            get { return steerRotationValue; }
            set { steerRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the turret rotation amount.
        /// </summary>
        public float TurretRotation
        {
            get { return turretRotationValue; }
            set { turretRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the cannon rotation amount.
        /// </summary>
        public float CannonRotation
        {
            get { return cannonRotationValue; }
            set { cannonRotationValue = value; }
        }


        /// <summary>
        /// Gets or sets the entry hatch rotation amount.
        /// </summary>
        public float HatchRotation
        {
            get { return hatchRotationValue; }
            set { hatchRotationValue = value; }
        }


        #endregion

        public Tank(Vector3 position, Quaternion rotation, float health, int direction)
        {
            this.tankPosition = position;
            this.tankRotation = rotation;
            this.health = health;
            this.direction = direction;
        }
        public Tank(int x, int y, int direction, int num)
        {
            this.tankPosition = new Vector3(Game1.size - x - 0.5f, 0, -y - 0.5f);
            this.tankRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, direction * MathHelper.PiOver2);
            this.health = 1;
            this.direction = direction;
            this.x = x;
            this.y = y;
            this.num = num;
            sphere = new BoundingSphere(tankPosition + Vector3.Up * 0.5f, 0.5f);
            for (int i = 0; i < 2; i++)
            {
                cameraRotation[i] = Quaternion.Identity;
            }
        }

        public void checkCollisions()
        {
            for (int i = 0; i < Game1.medikitList.Count; ++i)
            {
                if (Game1.medikitList.ElementAt(i).sphere.Contains(this.sphere) != ContainmentType.Disjoint)
                {
                    //Debug.WriteLine("Medikit removed " + Game1.medikitList.ElementAt(i).x + " " + Game1.medikitList.ElementAt(i).y);
                    Game1.medikitList.RemoveAt(i--);
                }
            }
            for (int i = 0; i < Game1.coinList.Count; ++i)
            {
                if (Game1.coinList.ElementAt(i).sphere.Contains(this.sphere) != ContainmentType.Disjoint)
                {
                    Game1.coinList.RemoveAt(i--);
                }
            }
        }

        /// <summary>
        /// Loads the tank model.
        /// </summary>
        public static void Initialize()
        {
            // Load the tank model from the ContentManager.
            //tankModel = content.Load<Model>("tank");

            // Look up shortcut references to the bones we are going to animate.
            foreach (ModelBone b in tankModel.Bones)
            {

                Debug.WriteLine(b.Index + " " + b.Name);
            }
            leftBackWheelBone = tankModel.Bones[6];
            rightBackWheelBone = tankModel.Bones[2];
            leftFrontWheelBone = tankModel.Bones[8];
            rightFrontWheelBone = tankModel.Bones[4];
            leftSteerBone = tankModel.Bones[7];
            rightSteerBone = tankModel.Bones[3];
            turretBone = tankModel.Bones[9];
            cannonBone = tankModel.Bones[10];
            hatchBone = tankModel.Bones[11];


            // Store the original transform matrix for each animating bone.
            leftBackWheelTransform = leftBackWheelBone.Transform;
            rightBackWheelTransform = rightBackWheelBone.Transform;
            leftFrontWheelTransform = leftFrontWheelBone.Transform;
            rightFrontWheelTransform = rightFrontWheelBone.Transform;
            leftSteerTransform = leftSteerBone.Transform;
            rightSteerTransform = rightSteerBone.Transform;
            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;
            hatchTransform = hatchBone.Transform;

            // Allocate the transform matrix array.

        }


        /// <summary>
        /// Draws the tank model, using the current animation settings.
        /// </summary>
        public void Draw(Vector3 camera, Vector3 camup, Matrix view, float scale, float barScale)
        {
            if (!draw)
                return;

            Vector3 length = Vector3.Normalize(Vector3.Cross(camera, camup));
            Vector3 v = Vector3.Normalize(Vector3.Cross(length, camup));
            length *= 0.05f * barScale;
            Vector3 height = Vector3.Normalize(camup) * 0.01f * barScale;
            Vector3 position = tankPosition + v * 0.23f * barScale + camup * 0.22f * scale;
            scale *= 0.0005f;
            float image;
            if (health >= 1)
                image = 10;
            else
                image = 9 + health;

            float imagesInTexture = Game1.imagesInTexture+0f;

            Game1.verticesList.Add(new VertexPositionNormalTexture(position + length + height, new Vector3(0, 0, 1), new Vector2(image / imagesInTexture, 1)));
            Game1.verticesList.Add(new VertexPositionNormalTexture(position + length - height, new Vector3(0, 0, 1), new Vector2((image) / imagesInTexture, 0)));
            Game1.verticesList.Add(new VertexPositionNormalTexture(position - length + height, new Vector3(0, 0, 1), new Vector2((image + 1f) / imagesInTexture, 1)));
            
            Game1.verticesList.Add(new VertexPositionNormalTexture(position + length - height, new Vector3(0, 0, 1), new Vector2((image) / imagesInTexture, 0)));
            Game1.verticesList.Add(new VertexPositionNormalTexture(position - length - height, new Vector3(0, 0, 1), new Vector2((image + 1f) / imagesInTexture, 0)));
            Game1.verticesList.Add(new VertexPositionNormalTexture(position - length + height, new Vector3(0, 0, 1), new Vector2((image + 1f) / imagesInTexture, 1)));
            
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 0.2f, 10000.0f);
            Matrix world = Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateFromQuaternion(tankRotation) * Matrix.CreateTranslation(tankPosition);

            // Set the world matrix as the root transform of the model.
            tankModel.Root.Transform = Matrix.CreateScale(scale, scale, scale) * world;

            // Calculate matrices based on the current animation position.
            Matrix wheelRotation = Matrix.CreateRotationX(wheelRotationValue);
            Matrix steerRotation = Matrix.CreateRotationY(steerRotationValue);
            Matrix turretRotation = Matrix.CreateRotationY(turretRotationValue);
            Matrix cannonRotation = Matrix.CreateRotationX(cannonRotationValue);
            Matrix hatchRotation = Matrix.CreateRotationX(hatchRotationValue);

            // Apply matrices to the relevant bones.
            leftBackWheelBone.Transform = wheelRotation * leftBackWheelTransform;
            rightBackWheelBone.Transform = wheelRotation * rightBackWheelTransform;
            leftFrontWheelBone.Transform = wheelRotation * leftFrontWheelTransform;
            rightFrontWheelBone.Transform = wheelRotation * rightFrontWheelTransform;
            leftSteerBone.Transform = steerRotation * leftSteerTransform;
            rightSteerBone.Transform = steerRotation * rightSteerTransform;
            turretBone.Transform = turretRotation * turretTransform;
            cannonBone.Transform = cannonRotation * cannonTransform;
            hatchBone.Transform = hatchRotation * hatchTransform;

            Matrix[] boneTransforms = new Matrix[tankModel.Bones.Count];
            // Look up combined bone matrices for the entire model.
            tankModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Draw the model.
            foreach (ModelMesh mesh in tankModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {

                    Effect effect = part.Effect;
                    //float time = 0; Game1.time / 4;
                    effect.Parameters["TranslationAmount"].SetValue(50 * dead);
                    effect.Parameters["RotationAmount"].SetValue(MathHelper.Pi * 3 * dead);
                    effect.Parameters["time"].SetValue(dead);
                    effect.Parameters["WorldViewProjection"].SetValue(
                        boneTransforms[mesh.ParentBone.Index] * view * projection);
                    effect.Parameters["World"].SetValue(boneTransforms[mesh.ParentBone.Index]);
                    effect.Parameters["eyePosition"].SetValue(camera);
                    effect.Parameters["lightPosition"].SetValue(Game1.lightDirection);
                    effect.Parameters["ambientColor"].SetValue(colors[num].ToVector4());
                    effect.Parameters["diffuseColor"].SetValue(colors[num].ToVector4());
                    effect.Parameters["specularColor"].SetValue(colors[num].ToVector4());
                    effect.Parameters["specularPower"].SetValue(50);
                    /*
                    effect.World = boneTransforms[mesh.ParentBone.Index];
                    effect.View = view;
                    effect.Projection = projection;
                    
                    effect.EnableDefaultLighting();*/
                }

                mesh.Draw();
            }
        }

        public void update()
        {
            if (health<= 0 && dead < 1f) dead+=0.01f;
            if (dead >= 1) draw = false;
            if (moveQueue.Count != 0)
            {
                Tuple<float, float, float> tuple = moveQueue.Dequeue();
                float turningSpeed = MathHelper.ToRadians(1.5f);
                if (tuple == null)
                    return;
                float turn = tuple.Item1;

                WheelRotation += Math.Abs(tuple.Item3) + Math.Abs(tuple.Item2) * 2;

                if (turn == 0)
                {
                    if (SteerRotation >= turningSpeed * 2)
                        SteerRotation -= turningSpeed * 2;
                    else if (SteerRotation <= -turningSpeed * 2)
                        SteerRotation += turningSpeed * 2;
                }
                else
                {
                    tankRotation *= Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), -turn);
                    SteerRotation -= turn * 2;
                }

                if (SteerRotation > 0.5f)
                    SteerRotation = 0.5f;
                else if (SteerRotation < -0.5f)
                    SteerRotation = -0.5f;
                //TurretRotation = SteerRotation;

                tankPosition += new Vector3(-tuple.Item2, 0, -tuple.Item3);
                sphere.Center = tankPosition + Vector3.Up * 0.5f;
            }
        }
        public void updatePosition(int x, int y, int direction, int shot, int score, int coins, float health)
        {
            float move = Math.Abs(this.x - x) + Math.Abs(this.y - y);
            if ((move != 1 || this.direction != direction) && move != 0)
            {
                Debug.WriteLine("Error1");
            }
            if (move != 0)
            {
                for (int i = 0; i < 60; i++)
                    moveQueue.Enqueue(new Tuple<float, float, float>(0, (x - this.x) / 60f, (y - this.y) / 60f));

            }
            if (this.direction != direction)
            {
                float angle = (direction - this.direction) % 4;
                if (angle == 3)
                    angle = -1;
                for (int i = 0; i < 60; i++)
                    moveQueue.Enqueue(new Tuple<float, float, float>(angle * (float)MathHelper.PiOver2 / 60f, 0, 0));
            }
            this.x = x;
            this.y = y;
            this.direction = direction;
            this.coins = coins;
            if (health == 0&&!deadcoin)
            {
                Game1.coinList.Add(new Coin(this.x, this.y, this.coins / 2, 1000000));
                deadcoin = true;
                
            }
            this.health = health;
            this.score = score;
            Debug.WriteLine("tank"+num+" "+health);

            if (shot == 1)
            {
                Bullet newBullet = new Bullet(tankPosition + Vector3.Transform(new Vector3(0, 0.17f, -0.05f),
                    tankRotation), tankRotation, 3f / 60.0f, num,x,y,direction);
                Game1.bulletList.Add(newBullet);
            }


        }
        public void UpdateCameras()
        {
            for (int i = 0; i < 2; i++)
            {
                cameraRotation[i] = Quaternion.Lerp(cameraRotation[i], tankRotation, 0.1f);

                Vector3 campos;
                if (i == 0)
                    campos = new Vector3(0, 0.2f, -0.9f);
                else
                    campos = new Vector3(0, 0.2f, 0.9f);
                campos = Vector3.Transform(campos, Matrix.CreateFromQuaternion(cameraRotation[i]));
                campos += tankPosition;

                Vector3 camup = new Vector3(0, 1.0f, 0);
                camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(cameraRotation[i]));
                cameraPosition[i] = campos;
                cameraUpDirection[i] = camup;
            }
        }
    }
}
