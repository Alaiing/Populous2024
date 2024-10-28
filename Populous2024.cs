using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Oudidon;
using System;
using System.Collections.Generic;

namespace Populous2024
{
    public class Populous2024 : OudidonGame
    {
        private Texture2D _hud;
        private SpriteSheet _tilesetSheet;

        private int[,] _mapData;
        private Point _mapOffset;

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _tilesetSheet = new SpriteSheet(Content, "tileset", 32, 24, new Point(16, 12));

            _hud = Content.Load<Texture2D>("HUD");
            _mapData = new int[64, 64];
            Texture2D mapTexture = Content.Load<Texture2D>("test-map");
            Color[] mapTextureData = new Color[mapTexture.Width * mapTexture.Height];
            mapTexture.GetData(mapTextureData);

            int maxHeight = 2;
            for (int i = 0; i < mapTextureData.Length; i++)
            {
                Color color = mapTextureData[i];
                int x = i % mapTexture.Width;
                int y = i / mapTexture.Height;
                _mapData[x, y] = color.R * maxHeight / 255;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            SimpleControls.GetStates();
            if (SimpleControls.IsLeftPressedThisFrame(PlayerIndex.One))
            {
                _mapOffset += new Point(-1,1);
            }
            if (SimpleControls.IsRightPressedThisFrame(PlayerIndex.One))
            {
                _mapOffset += new Point(1,-1);
            }
            if (SimpleControls.IsDownPressedThisFrame(PlayerIndex.One))
            {
                _mapOffset += new Point(1,1);
            }
            if (SimpleControls.IsUpPressedThisFrame(PlayerIndex.One))
            {
                _mapOffset += new Point(-1,-1);
            }

            _mapOffset.X = Math.Max(0, _mapOffset.X);
            _mapOffset.Y = Math.Max(0, _mapOffset.Y);

            _mapOffset.X = Math.Min(64-8, _mapOffset.X);
            _mapOffset.Y = Math.Min(64-8, _mapOffset.Y);

            base.Update(gameTime); // Updates state machine and components, in that order
        }

        protected override void DrawGameplay(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            SpriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            SpriteBatch.Draw(_hud, Vector2.Zero, Color.White);
            DrawMap(_mapOffset);
            SpriteBatch.End();

            base.DrawGameplay(gameTime); // Draws state machine and components, in that order
        }

        private void DrawMap(Point offset)
        {
            Vector2 startPosition = new(192, 76);
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Vector2 position = startPosition + new Vector2(32 / 2 * x, 24 / 3 * x) + new Vector2(-32 / 2 * y, 24 / 3 * y);

                    int top = _mapData[x + offset.X, y + offset.Y];
                    int right = _mapData[x + offset.X + 1, y + offset.Y];
                    int bottom = _mapData[x + offset.X + 1, y + offset.Y + 1];
                    int left = _mapData[x + offset.X, y + offset.Y + 1];

                    int minHeight = Math.Min(top, Math.Min(bottom, Math.Min(left, right)));
                    int maxHeight = Math.Max(top, Math.Max(bottom, Math.Max(left, right)));

                    int frameIndex = 15;
                    Vector2 heightOffset = new Vector2(0, -24 / 3 * minHeight);
                    if (minHeight != maxHeight)
                    {
                        top -= minHeight;
                        right -= minHeight;
                        bottom -= minHeight;
                        left -= minHeight;

                        frameIndex = 1 * Math.Sign(top) + 2 * Math.Sign(right) + 4 * Math.Sign(bottom) + 8 * Math.Sign(left);
                        heightOffset = new Vector2(0, -24 / 3 * minHeight);
                        if (minHeight == 0)
                        {
                            frameIndex += 16;
                        }
                    }
                    else
                    {
                        frameIndex = minHeight == 0 ? 0 : 15;
                        heightOffset = new Vector2(0, -24 / 3 * Math.Max(0, minHeight -1));
                    }

                    _tilesetSheet.DrawFrame(frameIndex, SpriteBatch, position + heightOffset, _tilesetSheet.DefaultPivot, 0, Vector2.One, Color.White);
                }
            }
        }

        private void Elevate(int x, int y)
        {

        }
    }
}
