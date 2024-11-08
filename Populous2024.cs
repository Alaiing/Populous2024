using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Populous2024
{
    public class Populous2024 : OudidonGame
    {
        private const int MAP_SIZE = 64;
        private const int MAX_HEIGHT = 7;
        private const int Y_TILE_TOLERANCE = 4;
        private const int X_TILE_TOLERANCE = 8;

        private Texture2D _hud;
        private SpriteSheet _tilesetSheet;
        private SpriteSheet _spriteSheet;

        private int[,] _mapData;
        public int[,] MapData => _mapData;
        private Point _mapOffset;
        private Vector2 _playgroundOrigin = new(191, 71);
        public Vector2 PlaygroundOrigin => _playgroundOrigin;
        public Point MapOffset => _mapOffset;
        private Point _mouseScreenPosition;
        private Point _tileUnderMouse;
        private bool _isMouseOnTile;

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _tilesetSheet = new SpriteSheet(Content, "tileset", 32, 24, new Point(16, 8));
            _spriteSheet = new SpriteSheet(Content, "sprites", 16, 16, new Point(8, 16));
            float animationSpeed = 4f;
            _spriteSheet.RegisterAnimation("FollowerWalk_NW", 0, 1, animationSpeed);
            _spriteSheet.RegisterAnimation("FollowerWalk_N", 2, 3, animationSpeed);
            _spriteSheet.RegisterAnimation("FollowerWalk_NE", 4, 5, animationSpeed);
            _spriteSheet.RegisterAnimation("FollowerWalk_E", 6, 7, animationSpeed);
            _spriteSheet.RegisterAnimation("FollowerWalk_SE", 8, 9, animationSpeed);
            _spriteSheet.RegisterAnimation("FollowerWalk_S", 10, 11, animationSpeed);
            _spriteSheet.RegisterAnimation("FollowerWalk_SW", 12, 13, animationSpeed);
            _spriteSheet.RegisterAnimation("FollowerWalk_W", 14, 15, animationSpeed);
            _spriteSheet.RegisterAnimation("Drown", 88, 91, animationSpeed);

            _hud = Content.Load<Texture2D>("HUD");
            _mapData = new int[MAP_SIZE, MAP_SIZE];
            Texture2D mapTexture = Content.Load<Texture2D>("test-map");
            Color[] mapTextureData = new Color[mapTexture.Width * mapTexture.Height];
            mapTexture.GetData(mapTextureData);

            GenerateMap();

            Follower newFollower = new Follower(_spriteSheet, this);
            newFollower.SetMapPosition(4, 4);
            Components.Add(newFollower);
            newFollower.Activate();
        }

        private void GenerateMap()
        {
            for (int i = 0; i < 1000; i++)
            {
                int x = CommonRandom.Random.Next(0, 63);
                int y = CommonRandom.Random.Next(0, 63);

                bool elevate = CommonRandom.Random.Next(0, 1) == 0;

                if (elevate)
                {
                    Elevate(x, y);
                }
                else
                {
                    Lower(x, y);
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            SimpleControls.GetStates();
            if (SimpleControls.IsLeftPressedThisFrame(PlayerIndex.One))
            {
                _mapOffset += new Point(-1, 1);
            }
            if (SimpleControls.IsRightPressedThisFrame(PlayerIndex.One))
            {
                _mapOffset += new Point(1, -1);
            }
            if (SimpleControls.IsDownPressedThisFrame(PlayerIndex.One))
            {
                _mapOffset += new Point(1, 1);
            }
            if (SimpleControls.IsUpPressedThisFrame(PlayerIndex.One))
            {
                _mapOffset += new Point(-1, -1);
            }

            if (_isMouseOnTile)
            {
                if (SimpleControls.LeftMouseButtonPressedThisFrame())
                {
                    Elevate(_tileUnderMouse.X, _tileUnderMouse.Y);
                }

                if (SimpleControls.RightMouseButtonPressedThisFrame())
                {
                    Lower(_tileUnderMouse.X, _tileUnderMouse.Y);
                }
            }

            _mouseScreenPosition = SimpleControls.GetMousePosition();
            _mouseScreenPosition.X /= 4;
            _mouseScreenPosition.Y /= 4;

            Point isoPosition = new Point(_mouseScreenPosition.Y / 8 + _mouseScreenPosition.X / 16, _mouseScreenPosition.Y / 8 - _mouseScreenPosition.X / 16);

            //Vector2 isoX = new Vector2(1, 0.5f);
            //isoX.Normalize();
            //Vector2 isoY = new Vector2(-1, 0.5f);
            //isoY.Normalize();
            //Vector2 isoPosition = new Vector2(mousePosition.X * isoX.X + mousePosition.Y * isoY.X, mousePosition.X * isoX.Y + mousePosition.Y * isoY.Y);
            //isoPosition.X = MathF.Floor(isoPosition.X / 16);
            //isoPosition.Y = MathF.Floor(isoPosition.Y / 16);
            //_mouseIsoPosition.X = (int)isoPosition.X;
            //_mouseIsoPosition.Y = (int)isoPosition.Y;
            //Debug.WriteLine($"{isoPosition}");


            _mapOffset.X = Math.Max(0, _mapOffset.X);
            _mapOffset.Y = Math.Max(0, _mapOffset.Y);

            _mapOffset.X = Math.Min(MAP_SIZE - 9, _mapOffset.X);
            _mapOffset.Y = Math.Min(MAP_SIZE - 9, _mapOffset.Y);

            base.Update(gameTime); // Updates state machine and components, in that order
        }

        protected override void DrawGameplay(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            SpriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            SpriteBatch.Draw(_hud, Vector2.Zero, Color.White);
            DrawMap(_mapOffset);

            base.DrawGameplay(gameTime); // Draws state machine and components, in that order
            DrawComponents(gameTime);
            SpriteBatch.End();
        }

        private void DrawMap(Point offset)
        {
            float minDistanceToMouse = 999f;
            int closestTileX = 0;
            int closestTileY = 0;
            Vector2 closestTilePosition = Vector2.Zero;
            Vector2 closestTileOffset = Vector2.Zero;
            _isMouseOnTile = false;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Vector2 position = _playgroundOrigin + IsoToScreen(x, y);

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
                        heightOffset = new Vector2(0, -24 / 3 * Math.Max(0, minHeight - 1));
                    }

                    Vector2 spritePosition = position + heightOffset;
                    _tilesetSheet.DrawFrame(frameIndex, SpriteBatch, spritePosition, _tilesetSheet.DefaultPivot, 0, Vector2.One, Color.White);
                    Rectangle spriteBounds = new Rectangle((int)spritePosition.X - _tilesetSheet.DefaultPivot.X, (int)spritePosition.Y - _tilesetSheet.DefaultPivot.Y + 8, _tilesetSheet.FrameWidth, _tilesetSheet.FrameHeight - 8);
                    if (spriteBounds.Contains(_mouseScreenPosition))
                    {
                        float distance = Vector2.Distance(spriteBounds.Center.ToVector2(), _mouseScreenPosition.ToVector2());
                        if (distance < minDistanceToMouse)
                        {
                            _isMouseOnTile = true;
                            minDistanceToMouse = distance;

                            closestTileX = x;
                            closestTileY = y;

                            if (_mouseScreenPosition.X - spritePosition.X > X_TILE_TOLERANCE)
                            {
                                closestTileX++;
                            }
                            else if (spritePosition.X - _mouseScreenPosition.X > X_TILE_TOLERANCE)
                            {
                                closestTileY++;
                            }
                            else if (_mouseScreenPosition.Y - spritePosition.Y > Y_TILE_TOLERANCE)
                            {
                                closestTileX++;
                                closestTileY++;
                            }

                            closestTilePosition = spritePosition;
                            closestTileOffset = heightOffset;
                            _tileUnderMouse = new Point(closestTileX + offset.X, closestTileY + offset.Y);
                        }
                    }
                    //SpriteBatch.DrawLine(position, position + new Vector2(0, -1), Color.White);
                }
            }
            if (_isMouseOnTile)
            {
                Vector2 cursorPosition = _playgroundOrigin + IsoToScreen(closestTileX, closestTileY);
                int height = _mapData[_tileUnderMouse.X, _tileUnderMouse.Y];
                float heightOffset = -24 / 3 * height;
                cursorPosition.Y += heightOffset;
                _spriteSheet.DrawFrame(139, SpriteBatch, cursorPosition, new Point(4, 4), 0, Vector2.One, Color.White);
            }
        }

        public Vector2 IsoToScreen(float x, float y)
        {
            return new Vector2(32 / 2 * x, 24 / 3 * x) + new Vector2(-32 / 2 * y, 24 / 3 * y);
        }


        private void Elevate(int x, int y)
        {
            int height = _mapData[x, y];
            if (height == MAX_HEIGHT)
                return;
            height++;
            _mapData[x, y] = height;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (x + i >= 0 && x + i < MAP_SIZE && y + j >= 0 && y + j < MAP_SIZE)
                    {
                        if (height - _mapData[x + i, y + j] > 1)
                        {
                            Elevate(x + i, y + j);
                        }
                    }
                }
            }
        }

        private void Lower(int x, int y)
        {
            int height = _mapData[x, y];
            if (height == 0)
                return;
            height--;
            _mapData[x, y] = height;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (x + i >= 0 && x + i < MAP_SIZE && y + j >= 0 && y + j < MAP_SIZE)
                    {
                        if (_mapData[x + i, y + j] - height > 1)
                        {
                            Lower(x + i, y + j);
                        }
                    }
                }
            }
        }

        public bool IsWater(int x, int y)
        {
            return _mapData[x, y] + _mapData[x + 1, y] + _mapData[x, y + 1] + _mapData[x + 1, y + 1] == 0;
        }
    }
}
