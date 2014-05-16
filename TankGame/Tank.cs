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
        public Vector3 tankPosition = new Vector3(8.5f, 0, -3.5f);
        public Quaternion tankRotation = Quaternion.CreateFromAxisAngle(new Vector3(0,1,0),MathHelper.Pi);
        public float health;
        public int direction;
        public int x;
        public int y;
        public int num;
        public int score = 0;
        public int coins = 0;
        public Queue<Tuple<float, float, float>> moveQueue = new Queue<Tuple<float, float,float>>();


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
        float wheelRotationValue=0;
        float steerRotationValue=0;
        float turretRotationValue=0;
        float cannonRotationValue=0;
        float hatchRotationValue=0;


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

        public Tank(Vector3 position, Quaternion rotation, float health, int direction){
            this.tankPosition = position;
            this.tankRotation = rotation;
            this.health = health;
            this.direction = direction;
        }
        public Tank(int x,int y, int direction, int num)
        {
            this.tankPosition = new Vector3(x+0.5f,0,-y-0.5f);
            this.tankRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, direction * MathHelper.PiOver2);
            this.health = 1;
            this.direction = direction;
            this.x = x;
            this.y = y;
            this.num = num;
        }

        /// <summary>
        /// Loads the tank model.
        /// </summary>
        public static void Initialize()
        {
            // Load the tank model from the ContentManager.
            //tankModel = content.Load<Model>("tank");

            // Look up shortcut references to the bones we are going to animate.
            leftBackWheelBone = tankModel.Bones["l_back_wheel_geo"];
            rightBackWheelBone = tankModel.Bones["r_back_wheel_geo"];
            leftFrontWheelBone = tankModel.Bones["l_front_wheel_geo"];
            rightFrontWheelBone = tankModel.Bones["r_front_wheel_geo"];
            leftSteerBone = tankModel.Bones["l_steer_geo"];
            rightSteerBone = tankModel.Bones["r_steer_geo"];
            turretBone = tankModel.Bones["turret_geo"];
            cannonBone = tankModel.Bones["canon_geo"];
            hatchBone = tankModel.Bones["hatch_geo"];
            

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
            
            
            //Vector3 length = Vector3.Normalize(Vector3.Transform(camera-position, Matrix.CreateFromQuaternion(new Quaternion(camup,MathHelper.PiOver2))));
            Vector3 length = Vector3.Normalize(Vector3.Cross(camera, camup));
            Vector3 v = Vector3.Normalize(Vector3.Cross(length, camup));
            //Debug.WriteLine(length2+" and "+length);
            //length = length2;
            length *= 0.05f * barScale;
            Vector3 height = Vector3.Normalize(camup) * 0.01f * barScale;
            Debug.WriteLine("positions "+tankPosition+" "+camera);
            Vector3 position = tankPosition + v * 0.23f * barScale +camup * 0.22f * scale;
            scale *= 0.0005f;
            float image;
            if (health >= 1)
                image = 12;
            else
                image = 11 + health;

            float imagesInTexture = Game1.imagesInTexture;
            Game1.verticesList.Add(new VertexPositionNormalTexture(position + length + height, new Vector3(0, 0, 1), new Vector2(image / imagesInTexture, 1)));
            Game1.verticesList.Add(new VertexPositionNormalTexture(position + length - height, new Vector3(0, 0, 1), new Vector2((image) / imagesInTexture, 0)));
            Game1.verticesList.Add(new VertexPositionNormalTexture(position - length + height, new Vector3(0, 0, 1), new Vector2((image +1) / imagesInTexture, 1)));

            Game1.verticesList.Add(new VertexPositionNormalTexture(position + length - height, new Vector3(0, 0, 1), new Vector2((image) / imagesInTexture, 0)));
            Game1.verticesList.Add(new VertexPositionNormalTexture(position - length - height, new Vector3(0, 0, 1), new Vector2((image + 1) / imagesInTexture, 0)));
            Game1.verticesList.Add(new VertexPositionNormalTexture(position - length + height, new Vector3(0, 0, 1), new Vector2((image + 1) / imagesInTexture, 1)));
       
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
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index];
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();
                }

                mesh.Draw();
            }
        }/*
        public void Move()
        {
            if (moveQueue.Count != 0)
            {
                Tuple<float, float> tuple = moveQueue.Dequeue();
                float forwardSpeed = 1 / 60.0f;
                float turningSpeed = 1 * MathHelper.ToRadians(1.5f);
                float leftRightRot = tuple.Item1 * turningSpeed;
                float upDownRot = tuple.Item2 * forwardSpeed;

                WheelRotation += upDownRot * 2;
                SteerRotation -= leftRightRot * 2;
                if (leftRightRot == 0)
                {
                    if (SteerRotation >= turningSpeed * 2)
                        SteerRotation -= turningSpeed * 2;
                    else if (SteerRotation <= -turningSpeed * 2)
                        SteerRotation += turningSpeed * 2;
                }
                if (SteerRotation > 0.5f)
                    SteerRotation = 0.5f;
                else if (SteerRotation < -0.5f)
                    SteerRotation = -0.5f;
                //TurretRotation = SteerRotation;

                Quaternion additionalRot = //Quaternion.CreateFromAxisAngle(new Vector3(0, (float)(-0.5), 0), leftRightRot) *
                    Quaternion.CreateFromAxisAngle(new Vector3(0, -1, 0), leftRightRot);
                tankRotation *= additionalRot;
                Math.Acos(tankRotation.W * 2);
                Debug.WriteLine(MathHelper.ToDegrees((float)Math.Acos(tankRotation.W) * 2));
                Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), tankRotation);
                tankPosition += addVector * upDownRot;
            }
        }
        public void updatePosition(int x, int y, int direction,int shot, int score, int coins, float health)
        {
            float leftRightRot = 0;
            float upDownRot = 0;
            upDownRot-=Math.Abs(this.x-x)+Math.Abs(this.y-y);
            if (upDownRot != 1&&upDownRot!=0)
            {
                Debug.WriteLine("Error");
            }
            if(upDownRot!=0){
                for (int i = 0; i < 60; i++)
                    moveQueue.Enqueue(new Tuple<float, float>(0, upDownRot));
            }
            else if(this.direction!=direction){
                if(Math.Abs(this.direction-direction)==2)
                    leftRightRot -= 2;
                else
                    leftRightRot -=2-((direction-this.direction)%4); 
                 for (int i = 0; i < 60; i++)
                    moveQueue.Enqueue(new Tuple<float, float>(leftRightRot, 0));
            }
            this.x = x;
            this.y = y;
            this.direction = direction;
            this.score = score;
            this.coins = coins;
            this.health = health;
            if (shot == 1)
            {
                Bullet newBullet = new Bullet(tankPosition + Vector3.Transform(new Vector3(0, 0.17f, -0.05f),
                    tankRotation), tankRotation, 1.5f / 60.0f);
                Game1.bulletList.Add(newBullet);
            }

            
        }*/
        public void Move()
        {
            if (moveQueue.Count != 0)
            {
                Tuple<float, float,float> tuple = moveQueue.Dequeue();
                float turningSpeed = MathHelper.ToRadians(1.5f);
                float turn = tuple.Item1;

                WheelRotation += Math.Abs(tuple.Item3)+Math.Abs(tuple.Item2)* 2;

                if (turn == 0)
                {
                    if (SteerRotation >= turningSpeed * 2)
                        SteerRotation -= turningSpeed * 2;
                    else if (SteerRotation <= -turningSpeed * 2)
                        SteerRotation += turningSpeed * 2;
                }
                else
                {
                    tankRotation *= Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), turn);
                    SteerRotation -= turn*2;
                }

                if (SteerRotation > 0.5f)
                    SteerRotation = 0.5f;
                else if (SteerRotation < -0.5f)
                    SteerRotation = -0.5f;
                //TurretRotation = SteerRotation;

                tankPosition +=  new Vector3(tuple.Item2,0,-tuple.Item3);
            }
        }
        public void updatePosition(int x, int y, int direction, int shot, int score, int coins, float health)
        {
            float move= Math.Abs(this.x - x) + Math.Abs(this.y - y);
            if (move != 1 && move != 0)
            {
                Debug.WriteLine("Error");
            }
            if (move != 0)
            {
                for (int i = 0; i < 60; i++)
                    moveQueue.Enqueue(new Tuple<float, float,float>(0,(x-this.x)/60f,(y-this.y)/60f));
            }
            else if (this.direction != direction)
            {
                for (int i = 0; i < 60; i++)
                    moveQueue.Enqueue(new Tuple<float, float, float>(((direction - this.direction) % 4)*(float)MathHelper.PiOver2/60f, 0, 0));
            }
            this.x = x;
            this.y = y;
            this.direction = direction;
            this.score = score;
            this.coins = coins;
            this.health = health;
            if (shot == 1)
            {
                Bullet newBullet = new Bullet(tankPosition + Vector3.Transform(new Vector3(0, 0.17f, -0.05f),
                    tankRotation), tankRotation, 1.5f / 60.0f);
                Game1.bulletList.Add(newBullet);
            }


        }
    }
}
