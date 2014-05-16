#region File Description
//-----------------------------------------------------------------------------
// SimpleAnimation.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Media;
using System.Threading;
using System.Configuration;
#endregion

namespace TankGame
{
    /// <summary>
    /// Sample showing how to apply simple animation to a rigid body tank model.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        float pos = 0;
        public static Tank tank;
        public static Tank[] tankArr = new Tank[5];

        SpriteBatch spriteBatch;
        GraphicsDevice device;
        Effect effect;
        Texture2D sceneryTexture;
        
        public static List<VertexPositionNormalTexture> buildingVerticesList;
        public static List<VertexPositionNormalTexture> verticesList;
        Viewport[] viewports = new Viewport[3];
        float speed = 1.0f;
        public double lastCommandTime = 0;
        public static float imagesInTexture = 13;

        public static int[,] floorPlan;
        public static int[,] grid = new int[10,10];

        int[] buildingHeights = new int[] {0,2,2};
        Vector3 lightDirection = new Vector3(3, -2, 5);
        

        Matrix viewMatrix = Matrix.Identity;
        Matrix projectionMatrix;
    

        enum CollisionType { None, Building, Boundary, Target, Xwing };
        BoundingBox[] buildingBoundingBoxes;
        BoundingBox completeCityBox;

        public Vector3[] cameraPosition = new Vector3[3];
        public Vector3[] cameraUpDirection = new Vector3[3];
        Quaternion[] cameraRotation =new Quaternion[3];

        public static List<Bullet> bulletList = new List<Bullet>();

        public static List<coin> coinList = new List<coin>();
        public static List<medikit> medikitList = new List<medikit>();
        public static List<brick> brickList = new List<brick>();

        TankGameBrain tankBrain;
        private Thread processThread;
        public static String command = "DOWN#";
        #endregion

        #region Initialization


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content"; 
            
        }

        protected override void Initialize()
        {

            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 500;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "XNA 3D Tank Game";

            viewports[0] = new Viewport();
            viewports[0].X = 0;
            viewports[0].Y = 0;
            viewports[0].Width = 400;
            viewports[0].Height = 500;
            viewports[0].MinDepth = 0;
            viewports[0].MaxDepth = 1;


            viewports[1] = new Viewport();
            viewports[1].X = 400;
            viewports[1].Y = 0;
            viewports[1].Width = 400;
            viewports[1].Height = 500;
            viewports[1].MinDepth = 0;
            viewports[1].MaxDepth = 1;

            viewports[2] = new Viewport();
            viewports[2].X = 800;
            viewports[2].Y = 0;
            viewports[2].Width = 400;
            viewports[2].Height = 500;
            viewports[2].MinDepth = 0;
            viewports[2].MaxDepth = 1;

            for (int i = 0; i < 3; i++)
            {
                cameraRotation[i] = Quaternion.Identity;
            }

            lightDirection.Normalize();





            base.Initialize();
        }
        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            tankBrain = new TankGameBrain();

            processThread = new Thread(new ThreadStart(tankBrain.process));
            processThread.Priority = ThreadPriority.Normal;
            tankBrain.startGame();
            tankBrain.waitGameStarted();
            processThread.Start();
            

            spriteBatch = new SpriteBatch(GraphicsDevice);

            device = graphics.GraphicsDevice;

            effect = Content.Load<Effect>("effects");
            sceneryTexture = Content.Load<Texture2D>("texturemap");
            Tank.tankModel = Content.Load<Model>("tank");
            Bullet.bulletModel = Content.Load<Model>("bullet");
            Tank.Initialize();

            tankBrain.initGrid();
            tankBrain.initTanks();

            /*tank = new Tank(5,3,0);
            for (int i = 0; i < 4; i++)
            {
                tankArr[i] = new Tank(i,i,i);
            }
            tankArr[4]=tank;*/
            coin.coinModel = Content.Load<Model>("TyveKrone");
            medikit.medikitModel = Content.Load<Model>("medikit");
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 0.05f, 10000.0f);
                

            //LoadFloorPlan();
            SetUpVertices();
            //SetUpBoundingBoxes();
            coinList.Add(new coin(new Vector3(5.5f, .2f, -3.5f), MathHelper.ToRadians(6f)));
            medikitList.Add(new medikit(new Vector3(6.5f, .3f, -3.5f), MathHelper.ToRadians(6f)));
        }
        private void LoadFloorPlan()
        {
            grid = new int[10,10];
            for(int i=0;i<10;i++)
                for(int j=0;j<10;j++)
                    grid[i,j]=0;
        }

        private void SetUpVertices()
        {
            int differentBuildings = buildingHeights.Length - 1;
            

            int cityWidth = grid.GetLength(0);
            int cityLength = grid.GetLength(1);


            buildingVerticesList = new List<VertexPositionNormalTexture>();
            for (int x = -2; x < cityWidth+2; x++)
            {
                for (int z = -2; z < cityLength+2; z++)
                {
                    
                    int currentbuilding ;
                    if(x<0||x>=cityWidth||z<0||z>=cityLength)
                        currentbuilding=1;
                    else
                        currentbuilding= grid[x, z];
                    int currentheight = 1;
                    if (currentbuilding == 0 || currentbuilding == 4)
                    {
                        currentheight = 0;
                    }

                    //floor or ceiling
                    buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, currentheight, -z), new Vector3(0, 1, 0), new Vector2(currentbuilding * 2 / imagesInTexture, 1)));
                    buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, currentheight, -z - 1), new Vector3(0, 1, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));
                    buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, currentheight, -z), new Vector3(0, 1, 0), new Vector2((currentbuilding * 2 + 1) / imagesInTexture, 1)));

                    buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, currentheight, -z - 1), new Vector3(0, 1, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));
                    buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, currentheight, -z - 1), new Vector3(0, 1, 0), new Vector2((currentbuilding * 2 + 1) / imagesInTexture, 0)));
                    buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, currentheight, -z), new Vector3(0, 1, 0), new Vector2((currentbuilding * 2 + 1) / imagesInTexture, 1)));
                    
                    if (currentheight != 0)
                    {
                        //front wall
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z - 1), new Vector3(0, 0, -1), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, currentheight, -z - 1), new Vector3(0, 0, -1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z - 1), new Vector3(0, 0, -1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 1)));

                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, currentheight, -z - 1), new Vector3(0, 0, -1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z - 1), new Vector3(0, 0, -1), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, currentheight, -z - 1), new Vector3(0, 0, -1), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));

                        //back wall
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 1)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, currentheight, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));

                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, currentheight, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, currentheight, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z), new Vector3(0, 0, 1), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));

                        //left wall
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z - 1), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 1)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, currentheight, -z - 1), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));

                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, currentheight, -z - 1), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, currentheight, -z), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x, 0, -z), new Vector3(-1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));

                        //right wall
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, currentheight, -z - 1), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z - 1), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 1)));

                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, currentheight, -z - 1), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2 - 1) / imagesInTexture, 0)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, 0, -z), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 1)));
                        buildingVerticesList.Add(new VertexPositionNormalTexture(new Vector3(x + 1, currentheight, -z), new Vector3(1, 0, 0), new Vector2((currentbuilding * 2) / imagesInTexture, 0)));
                    }
                }
            }

            
        }
        private void SetUpBoundingBoxes()
        {
            int cityWidth = floorPlan.GetLength(0);
            int cityLength = floorPlan.GetLength(1);


            List<BoundingBox> bbList = new List<BoundingBox>();
            for (int x = 0; x < cityWidth; x++)
            {
                for (int z = 0; z < cityLength; z++)
                {
                    int buildingType = floorPlan[x, z];
                    if (buildingType != 0)
                    {
                        int buildingHeight = 1;
                        if (buildingType == 0 || buildingType == 3)
                            buildingHeight = 0;
                        Vector3[] buildingPoints = new Vector3[2];
                        buildingPoints[0] = new Vector3(x, 0, -z);
                        buildingPoints[1] = new Vector3(x + 1, buildingHeight, -z - 1);
                        BoundingBox buildingBox = BoundingBox.CreateFromPoints(buildingPoints);
                        bbList.Add(buildingBox);
                    }
                }
            }
            buildingBoundingBoxes = bbList.ToArray();
            Vector3[] boundaryPoints = new Vector3[2];
            boundaryPoints[0] = new Vector3(0, 0, 0);
            boundaryPoints[1] = new Vector3(cityWidth, 20, -cityLength);
            completeCityBox = BoundingBox.CreateFromPoints(boundaryPoints);

        }

        private Model LoadModel(string assetName)
        {

            Model newModel = Content.Load<Model>(assetName); foreach (ModelMesh mesh in newModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();
            return newModel;
        }

        private Model LoadModel(string assetName, out Texture2D[] textures)
        {

            Model newModel = Content.Load<Model>(assetName);
            textures = new Texture2D[newModel.Meshes.Count];
            int i = 0;
            foreach (ModelMesh mesh in newModel.Meshes)
                foreach (BasicEffect currentEffect in mesh.Effects)
                    textures[i++] = currentEffect.Texture;

            foreach (ModelMesh mesh in newModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = effect.Clone();


            return newModel;
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            tankBrain.updateGrid();
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            // Update the animation properties on the tank object. In a real game
            // you would probably take this data from user inputs or the physics
            // system, rather than just making everything rotate like this!

         /*   tank.WheelRotation = time * 5;
            tank.SteerRotation = (float)Math.Sin(time * 0.75f) * 0.5f;
            tank.TurretRotation = (float)Math.Sin(time * 0.333f) * 1.25f;
            tank.CannonRotation = (float)Math.Sin(time * 0.25f) * 0.333f - 0.333f;
            tank.HatchRotation = MathHelper.Clamp((float)Math.Sin(time * 2) * 2, -1, 0);*/
            ProcessKeyboard(gameTime);
            for (int i = 0; i < 5; i++)
            {
                if (tankArr[i] != null)
                    tankArr[i].Move();
            }
            UpdateCameras();
            

            base.Update(gameTime);
        }
        public void UpdateCameras()
        {
            for (int i = 0; i < 2; i++)
            {
                cameraRotation[i] = Quaternion.Lerp(cameraRotation[i], tank.tankRotation, 0.1f);
                
                Vector3 campos ;
                if (i == 0)
                    campos = new Vector3(0, 0.2f, -0.9f);
                else
                    campos = new Vector3(0, 0.2f, 0.9f);
                campos = Vector3.Transform(campos, Matrix.CreateFromQuaternion(cameraRotation[i]));
                campos += tank.tankPosition;

                Vector3 camup = new Vector3(0, 1.0f, 0);
                camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(cameraRotation[i]));
                cameraPosition[i] = campos;
                cameraUpDirection[i] = camup;
            }
        }
        private void ProcessKeyboard(GameTime gameTime)
        {/*
            KeyboardState keys = Keyboard.GetState();
            double currentTime = gameTime.TotalGameTime.TotalMilliseconds;
            float leftRightRot = 0;
            float upDownRot = 0;

            

            if (keys.IsKeyDown(Keys.Right))
                leftRightRot += 1;
            if (keys.IsKeyDown(Keys.Left))
                leftRightRot -= 1;
            if (keys.IsKeyDown(Keys.Up))
                upDownRot += 1;
            if (keys.IsKeyDown(Keys.Down))
                upDownRot -= 1;

            if (currentTime-lastCommandTime>900&&(
                keys.IsKeyDown(Keys.W) || keys.IsKeyDown(Keys.A) || keys.IsKeyDown(Keys.S) || keys.IsKeyDown(Keys.D)))
            {
                if (keys.IsKeyDown(Keys.W))
                {
                    upDownRot += 1;
                    command = "UP#";
                }
                else if (keys.IsKeyDown(Keys.A))
                {
                    leftRightRot -= 1;
                    command = "LEFT#";
                }
                else if (keys.IsKeyDown(Keys.D))
                {
                    leftRightRot += 1;
                    command = "RIGHT#";
                }
                else if (keys.IsKeyDown(Keys.S))
                {
                    leftRightRot += 2;
                    command = "DOWN#";
                }
                for(int i=0;i<60;i++)
                    tank.moveQueue.Enqueue(new Tuple<float, float>(leftRightRot, upDownRot));
                lastCommandTime = currentTime;
            }
            else if(tank.moveQueue.Count==0)
                tank.moveQueue.Enqueue(new Tuple<float,float>(leftRightRot,upDownRot));

            //tank.Move();

            if (keys.IsKeyDown(Keys.Space))
            {
                
                if (currentTime - tank.lastBulletTime > 500)
                {
                    Bullet newBullet = new Bullet(tank.tankPosition + Vector3.Transform(new Vector3(0, 0.17f, -0.05f), 
                        tank.tankRotation), tank.tankRotation, 1.5f*speed/60.0f);
                    bulletList.Add(newBullet);
                    tank.lastBulletTime = currentTime;
                    
                }
            }

            */
            
            
        }
        



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.DarkGray);
            

            // Calculate the camera matrices.
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            Matrix worldMatrix;
            for (int i = 1; i >=0; i--)
            {
                verticesList = new List<VertexPositionNormalTexture>();
           
                graphics.GraphicsDevice.Viewport = viewports[i];
                viewMatrix = Matrix.CreateLookAt(cameraPosition[i], tank.tankPosition, cameraUpDirection[i]);
                worldMatrix = Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateFromQuaternion(tank.tankRotation) * Matrix.CreateTranslation(tank.tankPosition);
                DrawTanks(tank.tankPosition - cameraPosition[i], cameraUpDirection[i], viewMatrix, 1, 1);
                DrawBullets(viewMatrix, 0.008f);
                DrawCoins(viewMatrix, 0.05f);
                DrawMedikits(viewMatrix, 0.005f);
                DrawBricks();
                DrawCity();
                pos += 0.002f;
                Debug.WriteLine("count"+verticesList.Count+" "+i);
               // DrawBullet(worldMatrix * Matrix.CreateTranslation(0, 0.17f,-pos), viewMatrix, projectionMatrix, 0.008f);
            }
            verticesList = new List<VertexPositionNormalTexture>();          
            graphics.GraphicsDevice.Viewport = viewports[2];
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 0.2f, 10000.0f);
            viewMatrix = Matrix.CreateLookAt(new Vector3(5, 15, -5f),new Vector3(5, 0, -5f),new Vector3(0, 0, 1));
            worldMatrix = Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateFromQuaternion(tank.tankRotation) * Matrix.CreateTranslation(tank.tankPosition);
            DrawTanks(new Vector3(5, 0, 5f) - new Vector3(5, 21, -5f), new Vector3(0, 0, 1), viewMatrix, 3, 7);
            DrawBullets(viewMatrix, 0.05f);
            DrawCoins(viewMatrix, 0.3f);
            DrawMedikits(viewMatrix, 0.015f);
            DrawBricks();
            DrawCity();
            //DrawBullet(worldMatrix * Matrix.CreateTranslation(0, 0.2f, -pos), viewMatrix, projectionMatrix, 0.015f);

            base.Draw(gameTime);
        }
        private void DrawCity()
        {
            verticesList.AddRange(buildingVerticesList);
            VertexBuffer cityVertexBuffer= new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, verticesList.Count, BufferUsage.WriteOnly);
            cityVertexBuffer.SetData<VertexPositionNormalTexture>(verticesList.ToArray());
            effect.CurrentTechnique = effect.Techniques["Textured"];
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(viewMatrix);
            effect.Parameters["xProjection"].SetValue(projectionMatrix);
            effect.Parameters["xTexture"].SetValue(sceneryTexture);
            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xLightDirection"].SetValue(lightDirection);
            effect.Parameters["xAmbient"].SetValue(0.8f);
            device.BlendState=BlendState.NonPremultiplied;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(cityVertexBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, cityVertexBuffer.VertexCount / 3);
            }
        }
        public void DrawTanks(Vector3 camera, Vector3 camup, Matrix view, float scale, float barScale)
        {
            for (int i = 0; i < 5; i++)
            {
                if(tankArr[i]!=null)
                tankArr[i].Draw(camera, camup, view, scale, barScale);
            }
        }
        public void DrawBullets(Matrix viewMatrix, float scale)
        {
            foreach (Bullet bullet in bulletList)
            {
                bullet.MoveForward();
                bullet.Draw(viewMatrix, scale);
            }
        }
        public void DrawBricks()
        {
            foreach (brick brik in brickList)
            {
                if(brik.health > 0)
                    brik.AddToDraw();
            }
        }
        public void DrawCoins(Matrix viewMatrix, float scale)
        {
            foreach (coin coin in coinList)
            {
                coin.MoveForward();
                coin.Draw(viewMatrix, scale);
            }
        }
        public void DrawMedikits(Matrix viewMatrix, float scale)
        {
            foreach (medikit medikit in medikitList)
            {
                medikit.MoveForward();
                medikit.Draw(viewMatrix, scale);
            }
        }
        public void MoveForward()
        {

            
        }

        #endregion

       
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }

    #endregion
}
