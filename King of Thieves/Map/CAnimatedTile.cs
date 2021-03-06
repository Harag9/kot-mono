﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace King_of_Thieves.Map
{
    public class CAnimatedTile : CTile
    {
        private int _speed = 0;
        private int _timeForCurrentFrame = 0;
        private Vector2 _startingPosition = Vector2.Zero;
        private Vector2 _endingPosition = Vector2.Zero;
        private int _frameX = 0;
        private int _frameY = 0;
        private int _tileXCount = 0;
        private int _tileYCount = 0;
        private int _width = 0;
        private int _height = 0;

        public CAnimatedTile(CAnimatedTile copy) : 
            base(copy)
        {
            _speed = copy._speed;
            _startingPosition = copy._startingPosition;
            _endingPosition = copy._endingPosition;
            _tileXCount = copy._tileXCount;
            _tileYCount = copy._tileYCount;
        }

        public CAnimatedTile(Vector2 atlasCoords, Vector2 atlasCoordsEnd, Vector2 mapCoords, string tileSet, int speed) :
            base(atlasCoords, mapCoords, tileSet)
        {
            _speed = speed;
            _startingPosition = new Vector2(atlasCoords.X,atlasCoords.Y);
            _endingPosition = atlasCoordsEnd;
            _tileXCount = (int)(_endingPosition.X - _startingPosition.X);
            _tileYCount = (int)(_endingPosition.Y - _startingPosition.Y);
        }

        public Vector2 startingPosition
        {
            get
            {
                return _startingPosition;
            }
        }

        public int speed
        {
            get
            {
                return _speed;
            }
        }

        public Vector2 atlasCoordsEnd
        {
            get
            {
                return _endingPosition;
            }
        }

        public override void update()
        {
            _timeForCurrentFrame += CMasterControl.gameTime.ElapsedGameTime.Milliseconds;


            if (_timeForCurrentFrame >= Graphics.CSprite._frameRateLookup[_speed])
            {
                _timeForCurrentFrame = 0;
                _tileBounds.X += 1;

                if (_tileBounds.X > _endingPosition.X)
                {
                    _tileBounds.X = _startingPosition.X;
                    _tileBounds.Y++;

                    if (_tileBounds.Y > _endingPosition.Y)
                        _tileBounds.Y = _startingPosition.Y;
                }
            }
        }


    }
}
