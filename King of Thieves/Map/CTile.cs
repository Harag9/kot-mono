﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace King_of_Thieves.Map
{
    public class CTile : IDisposable
    {
        protected Vector2 _tileBounds;
        public Vector2 tileCoords;
        public string tileSet;
        private Actors.Collision.CHitBox _boundary = null;
        public Vector2 _mapDrawScale = Vector2.Zero;
        public bool shouldDraw = true;
        private Vector2 _dimensions = Vector2.Zero;

        public CTile(CTile copy)
        {
            this.tileCoords = copy.tileCoords;
            this.tileSet = copy.tileSet;
            this._tileBounds = copy._tileBounds;

            if (_boundary == null)
                _boundary = new Actors.Collision.CHitBox(null, tileCoords.X, tileCoords.Y, Graphics.CTextures.textures[tileSet].FrameWidth, Graphics.CTextures.textures[tileSet].FrameHeight);
            else
                _dimensions = new Vector2(Graphics.CTextures.textures[tileSet].FrameWidth, Graphics.CTextures.textures[tileSet].FrameHeight);
        }

        public CTile(Vector2 atlasCoords, Vector2 mapCoords, string tileSet)
        {
            _tileBounds = atlasCoords;
            tileCoords = mapCoords;
            this.tileSet = tileSet;

            //if tileSet is null, we're probably in game
            if (tileSet != null)
            { 
                _boundary = new Actors.Collision.CHitBox(null, mapCoords.X, mapCoords.Y, Graphics.CTextures.textures[tileSet].FrameWidth, Graphics.CTextures.textures[tileSet].FrameHeight);
                _dimensions = new Vector2(Graphics.CTextures.textures[tileSet].FrameWidth, Graphics.CTextures.textures[tileSet].FrameHeight);
            }
        }

        public Vector2 dimensions
        {
            get
            {
                return _dimensions;
            }
        }

        public Vector2 atlasCoords
        {
            get
            {
                return _tileBounds;
            }
        }

        public Vector2 tileSize
        {
            get
            {
                Vector2 size = new Vector2(_boundary.halfWidth * 2, _boundary.halfHeight * 2);
                return size;
            }
        }

        public bool checkForClick(Vector2 mouseCoords)
        {
            return (_boundary.checkCollision(mouseCoords));
        }

        public virtual void draw(King_of_Thieves.Graphics.CSprite image, SpriteBatch spriteBatch, int offSetX = 0, int offsetY = 0)
        {
            Vector2 dimensions = Vector2.Zero;
            dimensions = new Vector2(Graphics.CTextures.textures[tileSet].FrameWidth, Graphics.CTextures.textures[tileSet].FrameHeight);

            image.draw((int)(tileCoords.X + offSetX), (int)(tileCoords.Y + offsetY), (int)(atlasCoords.X), (int)(atlasCoords.Y), 1, 1, true, spriteBatch);
        }

        public void draw(King_of_Thieves.Graphics.CSprite image, SpriteBatch spriteBatch, double width, double height)
        {
            Vector2 dimensions = Vector2.Zero;
            dimensions = new Vector2((float)width, (float)height);

            image.draw((int)tileCoords.X, (int)tileCoords.Y, (int)(atlasCoords.X), (int)(atlasCoords.Y), 1,1, true, spriteBatch);
        }

        //used in inherited classes only
        public virtual void update() { }

        public void Dispose()
        {
            _boundary = null;
        }
    }
}
