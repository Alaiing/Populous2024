using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Populous2024
{
    public class Follower : Character
    {
        private const float MOVEMENT_SPEED = 0.33f;
        private Vector2 _mapPosition;
        private Point _mapDirection;
        private float _elevation;

        private Populous2024 _populousGame;
        public Follower(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            _populousGame = game as Populous2024;
            _mapDirection = new Point(1, 0);
        }

        public void SetMapPosition(int x, int y)
        {
            _mapPosition = new Vector2(x, y);
        }

        private void UpdateAnimation()
        {
            string animationName = "";
            if (_mapDirection == Point.Zero)
            {
                animationName = "Drown"; // TODO
            }
            else
            {
                animationName = "FollowerWalk_";
                if (_mapDirection.Y > 0)
                {
                    animationName += "S";
                }
                if (_mapDirection.Y < 0)
                {
                    animationName += "N";
                }
                if (_mapDirection.X > 0)
                {
                    animationName += "E";
                }
                if (_mapDirection.X < 0)
                {
                    animationName += "W";
                }
            }
            SetAnimation(animationName);
        }

        public override void Update(GameTime gameTime)
        {
            if (_mapDirection != Point.Zero)
            {
                Vector2 direction = _mapDirection.ToVector2();
                direction.Normalize();
                Vector2 newMapPosition = _mapPosition + direction * MOVEMENT_SPEED * Game.DeltaTime;

                if (_populousGame.IsWater((int)MathF.Floor(newMapPosition.X), (int)MathF.Floor(newMapPosition.Y)))
                {
                    _mapDirection = new Point(-_mapDirection.X, - _mapDirection.Y); 
                }
                else
                {
                    _mapPosition = newMapPosition;
                }
            }

            int x = (int)MathF.Floor(_mapPosition.X);
            int y = (int)MathF.Floor(_mapPosition.Y);
            float elevation1 = _populousGame.MapData[x, y] * (1 -(_mapPosition.X - x));
            elevation1 += _populousGame.MapData[x + 1, y] * (_mapPosition.X - x);

            float elevation2 = _populousGame.MapData[x, y + 1] * (1 - (_mapPosition.X - x));
            elevation2 += _populousGame.MapData[x + 1, y + 1] * (_mapPosition.X - x);

            float elevation = elevation1 * (1 - (_mapPosition.Y - y)) + elevation2 * (_mapPosition.Y - y);

            //if (elevation == 0)
            //{
            //    _mapDirection = Point.Zero;
            //    SetAnimation("Drown");
            //}

            UpdateAnimation();
            MoveTo(_populousGame.PlaygroundOrigin + _populousGame.IsoToScreen(_mapPosition.X - _populousGame.MapOffset.X, _mapPosition.Y - _populousGame.MapOffset.Y) + new Vector2(0, -elevation * 24 / 3));
            Animate(Game.DeltaTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_mapPosition.X < _populousGame.MapOffset.X
                || _mapPosition.Y < _populousGame.MapOffset.Y
                || _mapPosition.X > _populousGame.MapOffset.X + 8
                || _mapPosition.Y > _populousGame.MapOffset.Y + 8)
                return;

            base.Draw(gameTime);
        }
    }
}
