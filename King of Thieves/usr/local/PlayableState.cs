﻿using Gears.Navigation;
using King_of_Thieves.Actors;
using Gears.Cloud;
using King_of_Thieves.Input;
using King_of_Thieves.Graphics;
using Microsoft.Xna.Framework;

namespace King_of_Thieves.usr.local
{
    class PlayableState : MenuReadyGameState
    {
        Actors.Controllers.GameControllers.CDayClock clockTest = Actors.Controllers.GameControllers.CDayClock.instantiateSingleton();

        public PlayableState()
            : base()
        {
            //CMasterControl.mapManager.swapMap("castleTown.xml", "player",new Vector2(400,1000));
            _initMap();
            Graphics.CSprite.initFrameRateMapping();
        }

        protected virtual void _initMap()
        {
            CMasterControl.mapManager.swapMap("kokiriForest.xml", "player", new Vector2(100, 100));
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            CMasterControl.mapManager.drawMap();
            CEffects.drawThisShit();
            CMasterControl.healthController.drawMe(spriteBatch);
            CMasterControl.buttonController.drawMe(spriteBatch);
            CMasterControl.magicMeter.drawMe();

            if (CMasterControl.pickPocketMeter != null)
                CMasterControl.pickPocketMeter.drawMe();
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            CMasterControl.mapManager.updateMap(gameTime);
            CMasterControl.healthController.update(gameTime);
            CMasterControl.buttonController.update(gameTime);
            CMasterControl.magicMeter.update(gameTime);
            clockTest.update(gameTime);

            if (CMasterControl.pickPocketMeter != null)
                CMasterControl.pickPocketMeter.update(gameTime);
        }
    }
}
