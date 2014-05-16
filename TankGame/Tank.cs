#region File Description
//-----------------------------------------------------------------------------
// Tank.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
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
        public Quaternion tankRotation = Quaternion.Identity;
        float health = 4;


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
        float wheelRotationValue;
        float steerRotationValue;
        float turretRotationValue;
        float cannonRotationValue;
        float hatchRotationValue;


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

        public Tank(Vector3 position, Quaternion rotation, float health){
            this.tankPosition = position;
            this.tankRotation = rotation;
            this.health = health;
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
        }
    }
}
