﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gears.Cartography;
using Gears.Playable;
using Microsoft.Xna.Framework;

namespace King_of_Thieves.Map
{
    class CMapManager
    {
        private static CMap _currentMap;
        private Dictionary<string, CMap> mapPool = null;
        private static Actors.CComponent _droppableComponent = new Actors.CComponent(1);
        private static Dictionary<string, Graphics.CSprite> _droppableActorSpriteCache = new Dictionary<string, Graphics.CSprite>();
        private static bool _roomStart = false;

        private static bool _mapSwapIssued = false;
        private static string _mapName, _actorToFollow;
        private static Vector2 _followerCoords = new Vector2();

        private static Graphics.CTransitionEffect _transition = null;

        public const int TRANSITION_RUMPLE_SWIRL = 0;
        public const int TRANSITION_FADE_TO_BLACK = 1;

        public void checkAndSwapMap()
        {
            CMapManager.turnOffRoomStart();

            if (_mapSwapIssued)
            {
                if (_transition == null)
                    _swapMap(); 
            }
        }

        public void flipFlag(int flag)
        {
            _currentMap.flags[flag] = !_currentMap.flags[flag];
        }

        public bool checkFlag(int flag)
        {
            return _currentMap.flags[flag];
        }

        public static void swapDrawDepth(int newDepth, Actors.CActor sprite)
        {
            if (_currentMap != null)
                _currentMap.swapDrawDepth(newDepth, sprite);
        }

        public static object propertyGetter(string actorName, Map.EActorProperties property)
        {
            return _currentMap.getProperty(actorName, property);
        }

        public static void switchComponentLayer(Actors.CComponent component, int toLayer)
        {
            _currentMap.switchComponentToLayer(component, toLayer);
        }

        public static object propertyGetterFromComponent(int componentAddress, string actorName, Map.EActorProperties property)
        {
            return _currentMap.getProperty(componentAddress, actorName, property);
        }

        public CMapManager()
        {
            _currentMap = null;
            mapPool = new Dictionary<string, CMap>();

            //cache droppable sprites here
        }

        ~CMapManager()
        {
            clear();
        }

        public Actors.CActor setActorToFollow(string actorName)
        {
            CMasterControl.camera.actorToFollow = _currentMap.queryActorRegistry(actorName)[0];
            return CMasterControl.camera.actorToFollow;
        }

        public Graphics.CSprite getDroppableSprite(string droppable)
        {
            try
            {
                return _droppableActorSpriteCache[droppable];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public static bool roomStart
        {
            get
            {
                return _roomStart;
            }
        }

        public static void turnOffRoomStart()
        {
            _roomStart = false;
        }

        public void drawMap()
        {
            if (_currentMap != null)
                _currentMap.draw();

            //draw the transition if it's there
            if (_transition != null)
            {
                _transition.draw((int)CMasterControl.camera._normalizedPosition.X, (int)CMasterControl.camera._normalizedPosition.Y);

                if (_transition.fadeOutComplete)
                {
                    _transition = null;
                    return;
                }

                if (_transition.fadeInComplete)
                    _swapMap();
            }

        }

        public void updateMap(GameTime gameTime)
        {
            if (_currentMap != null)
                _currentMap.update(gameTime);
        }

        public static Actors.CActor[] queryActorRegistry(Type type, int layer)
        {
            try
            {
                return _currentMap.queryActorRegistry(type, layer);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        private void _prepareTransition(int transitionEffect)
        {
            if (transitionEffect > -1)
                switch(transitionEffect)
                {
                    case TRANSITION_RUMPLE_SWIRL:
                        _transition = new Graphics.CTransitionEffect(Graphics.CTextures.TRANSITION_RUMPLE, 30, 30);
                        break;

                    case TRANSITION_FADE_TO_BLACK:
                        _transition = new Graphics.CTransitionEffect(Graphics.CTextures.TRANSITION_FADE_TO_BLACK, 30, 30);
                        break;
                }
        }

        public void swapMap(string mapName, string actorToFollow, Vector2 followerCoords, int transitionEffect = -1)
        {
            _mapSwapIssued = true;
            _mapName = mapName;
            _actorToFollow = actorToFollow;
            _followerCoords = followerCoords;
            _prepareTransition(transitionEffect);

            if (_currentMap == null)
                _swapMap();
        }

        public void unloadMap(string mapName)
        {
            CMap map = null;
            if (mapPool.ContainsKey(mapName))
            {
                map = mapPool[mapName];

                if (map != null)
                {
                    map.Dispose();
                    mapPool.Remove(mapName);
                }
            }
        }

        public void unloadAllMaps()
        {
            for(int i = 0; i < mapPool.Keys.Count; i++)
            {
                string map = mapPool.Keys.ElementAt(i);
                unloadMap(map);
            }
        }

        private void _swapMap()
        {
            _currentMap = mapPool[_mapName];
            CMasterControl.commNet.Clear();
            _currentMap.registerWithCommNet();


            Actors.CActor actor = setActorToFollow(_actorToFollow);
            Vector3 cameraDiff = new Vector3(-CMasterControl.camera.position.X - _followerCoords.X, -CMasterControl.camera.position.Y - _followerCoords.Y, 0);
            CMasterControl.camera.jump(Vector3.Zero);
            CMasterControl.camera.translate(new Vector3(100 - _followerCoords.X, 60 - _followerCoords.Y, 0));
            CMasterControl.camera.setBoundary(_followerCoords);

            if (!string.IsNullOrWhiteSpace(_currentMap.bgmRef))
                CMasterControl.audioPlayer.addSfx(CMasterControl.audioPlayer.soundBank[_currentMap.bgmRef]);

            actor.position = _followerCoords;
            _setCameraLimit(actor);
            _mapSwapIssued = false;

            _roomStart = true;

        }

        public void cacheMaps(bool clearMaps, params string[] maps)
        {
            if (clearMaps)
                clear();

            foreach (string file in maps)
                mapPool.Add(file, new CMap(file)); //temporary
        }

        private void clear()
        {
            if (mapPool != null)
                mapPool.Clear();

            mapPool = null;
        }

        private static void _setCameraLimit(Actors.CActor follower)
        {
            Actors.CActor[] limiters = _currentMap.queryActorRegistry(typeof(Actors.Collision.CCameraLimit));
            CMasterControl.camera.cameraLimit = null;

            foreach(Actors.CActor limiter in limiters)
            {
                Actors.Collision.CCameraLimit limit = (Actors.Collision.CCameraLimit)limiter;

                if (limit.isPointWithin(follower.position))
                {
                    CMasterControl.camera.cameraLimit = limit;
                    break;
                }
            }

        }

        public static void removeFromActorRegistry(Actors.CActor actor)
        {
            _currentMap.removeFromActorRegistry(actor);
        }

        public static void removeComponent(Actors.CComponent component)
        {
            _currentMap.removeComponent(component, component.layer);
        }

        //builds a new component from the given actors
        public static void addComponent(Actors.CActor root, Dictionary<string, Actors.CActor> actors)
        {
            if (root == null)
                throw new KotException.KotInvalidActorException("Root actor cannot be null when calling addComponent");

            Actors.CComponent component = new Actors.CComponent(_currentMap.largestAddress + 1);
            component.layer = root.layer;

            addComponent(component);
            addActorToComponent(root, component.address);

            if (actors != null)
                foreach (Actors.CActor actor in actors.Values)
                    addActorToComponent(actor, component.address);
        }

        public static void addComponent(Actors.CComponent component)
        {
            _currentMap.addComponent(component, component.layer);
        }

        public static void addActorToComponent(Actors.CActor actor, int componentAddress)
        {
            _currentMap.addActorToComponent(actor, componentAddress);
        }

        public static int getActorComponentAddress(string actorName)
        {
            Actors.CActor[] actors = _currentMap.queryActorRegistry(actorName);

            if (actors.Length > 0)
                return actors[0].component.address;
            else
                throw new KotException.KotInvalidActorException("No actor found with the name " + actorName);
        }

        public static bool mapSwapIssued
        {
            get
            {
                return _mapSwapIssued;
            }
        }
    }
}
