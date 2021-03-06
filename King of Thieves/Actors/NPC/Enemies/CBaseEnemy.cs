﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using King_of_Thieves.Graphics;
using Gears.Cloud;
using King_of_Thieves.Input;
using King_of_Thieves.MathExt;
namespace King_of_Thieves.Actors.NPC.Enemies
{
    //has all the things that an enemy NPC will have
    //ex: item drops
    public class dropRate
    {
        public dropRate(Items.Drops.CDroppable drop, float rate)
        {
            item = drop;
            this.rate = rate;
        }

        public readonly Items.Drops.CDroppable item;
        public readonly float rate;
    }

    public enum ENEMY_PROPERTIES
    {
        ELECTRIC = 0,
        FIRE,
        ICE
    }

    public abstract class CBaseEnemy : Other.CBaseNpc
    {
        protected Dictionary<Items.Drops.CDroppable, float> _itemDrop = new Dictionary<Items.Drops.CDroppable,float>();
        protected bool _huntPlayer = false;
        protected List<ENEMY_PROPERTIES> _properties = new List<ENEMY_PROPERTIES>();
        protected int _health = int.MaxValue;

        public CBaseEnemy(params dropRate[] drops) 
            :  base()
        {
            float sum = 0;
            foreach (dropRate x in drops)
            {
                //this should add up to 1
                sum += x.rate;
                _itemDrop.Add(x.item, sum);
            }

            //calculate field of view
            _fovMagnitude = (int)Math.Cos(_visionRange * (Math.PI / 180.0));
            _visionSlope = (int)Math.Tan(_visionRange * (Math.PI/180.0));
        }

        protected override void _initializeResources()
        {
            base._initializeResources();
        }

        public bool hasProperty(ENEMY_PROPERTIES property)
        {
            return _properties.Contains(property);
        }

        //just chill there
        protected virtual void idle()
        {
            _huntPlayer = hunt();
        }

        //look for the player while idling
        protected bool hunt()
        {
            //check if the player is within the line of sight
            //switch (_direction)
            //{
            //    case DIRECTION.UP:
            //        if (Actors.Player.CPlayer.glblY <= _position.Y && Actors.Player.CPlayer.glblY >= (_position.Y - _lineOfSight))
            //        {

            //        }
            //        break;
            //}
            
            //check hearing field
            return isPointInHearingRange(new Vector2(Player.CPlayer.glblX, Player.CPlayer.glblY));
        }

        public override void update(GameTime gameTime)
        {
            base.update(gameTime);

            if (CMasterControl.buttonController.textBoxActive)
                return;

            if (_hp <= 0)
                _killMe = true;
        }

        protected bool isPointInHearingRange(Vector2 point)
        {
            return MathExt.MathExt.checkPointInCircle(_position, point, _hearingRadius);
        }

        //chase the player
        protected virtual void chase()
        {

        }

        protected void _doNpcCountCheck(ref int counter)
        {
            if (counter <= 0)
            {
                counter = 0;
                _flagResourceCleanup();
            }
        }

        public override void destroy(object sender)
        {
            Items.Drops.CDroppable itemToDrop = _dropItem();
            Vector2 explosionPos = new Vector2(_position.X - 10, _position.Y - 10);
            Graphics.CEffects.createEffect(Graphics.CEffects.EXPLOSION,explosionPos);
            CMasterControl.audioPlayer.addSfx(CMasterControl.audioPlayer.soundBank["Npc:die"]);

            if (itemToDrop != null)
            {
                itemToDrop.init(_name + "_itemDrop", _position, "", CReservedAddresses.DROP_CONTROLLER);
                itemToDrop.layer = this.layer;
                Map.CMapManager.addActorToComponent(itemToDrop, CReservedAddresses.DROP_CONTROLLER);
            }

            if (_hitBox != null)
                base.destroy(sender);

            cleanUp();
        }

        private Items.Drops.CDroppable _dropItem()
        {
            Random roller = new Random();
            
            //pick a random number and see which range it falls into
            double selection = _randNum.NextDouble() * 100;
            float previous = 0;

            foreach (KeyValuePair<Items.Drops.CDroppable, float> x in _itemDrop)
            {
                if (selection >= 0 && selection <= x.Value - previous)
                    return x.Key;

                previous = x.Value;
            }

            return null;
            
        }

        protected Vector2 getRandomPointInSightRange()
        {
            Vector2 point = Vector2.Zero;
            double halfAngle = _visionRange/2.0;
            double thetaMin = _angle - halfAngle;
            double thetaMax = _angle + halfAngle;
            double theta = _randNum.Next((int)thetaMin, (int)thetaMax);
            double pointInSight = _randNum.Next(0, _lineOfSight);

            point.X = (float)(_lineOfSight * Math.Cos(theta * (Math.PI / 180.0)));
            point.Y = (float)(_lineOfSight * Math.Sin(theta * (Math.PI / 180.0)));

            return point;
        }
    }
}
