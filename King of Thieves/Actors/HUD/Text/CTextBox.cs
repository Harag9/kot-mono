﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace King_of_Thieves.Actors.HUD.Text
{
    public class CTextBox : CHUDElement
    {
        private static SpriteFont _sherwood = CMasterControl.glblContent.Load<SpriteFont>(@"Fonts/sherwood");
        private readonly Vector2 _CENTER_SCREEN = new Vector2(100, 100);
        private readonly Vector2 _BOX_SCREEN = new Vector2(40, 175);
        private const int _LINE_MAX_CHARS = 43;
        private const int _THREE_LINE_MAX = _LINE_MAX_CHARS * 3;
        private string[] _messageQueue = null;
        private string _processedMessage = "";
        private Graphics.CSprite _textBox = new Graphics.CSprite("HUD:text:textBox");
        private bool _active = false;
        private bool _showBox = false;
        private static bool _messageFinished = false;
        private bool _wait = false;
        private int _currentMessageIndex = 0;
        private Vector2 _offset = new Vector2(10, 93);

        public static bool messageFinished
        {
            get
            {
                return _messageFinished;
            }
        }

        public void displayMessageBox(params string[] messages)
        {
            if (!_wait)
            {
                _currentMessageIndex = 0;
                _showBox = true;
                _active = true;
                _messageQueue = new string[messages.Length];
                
                for(int i = 0; i < messages.Length; i++)
                    _messageQueue[i] = messages[i];

                _fixedPosition = _offset;
                _processedMessage = _processMessage(true);
            }
        }

        public void displayMessageBox(string message)
        {
            if (!_wait)
            {
                _currentMessageIndex = 0;
                _showBox = true;
                _active = true;
                _messageQueue = new string[1];
                _messageQueue[0] = message;
                _fixedPosition = _offset;
                _processedMessage = _processMessage(true);
            }
            
        }

        public void displayMessage(string message)
        {
            if (!_wait)
            {
                _currentMessageIndex = 0;
                _active = true;
                _messageQueue[0] = message;
                _fixedPosition = _offset;
                _processedMessage = _processMessage(true);
            }
        }

        public override void drawMe(bool useOverlay = false, SpriteBatch spriteBatch = null)
        {
            base.drawMe();
            if (_active)
                _drawText(_showBox);
        }

        private void _drawText(bool isMessageBox)
        {
            SpriteBatch spriteBatch = Graphics.CGraphics.spriteBatch;
            
            if (isMessageBox)
                _textBox.draw((int)_position.X - 30, (int)_position.Y - 7,false);

            spriteBatch.DrawString(_sherwood, _processedMessage, _position, Color.White);
        }

        public override void update(GameTime gameTime)
        {
            base.update(gameTime);
            _messageFinished = false;
            if (CMasterControl.glblInput.keysReleased.Contains(Microsoft.Xna.Framework.Input.Keys.C))
                _processedMessage = _processMessage();
        }

        //this function takes the original message and determines where things like line breaks should go
        private string _processMessage(bool isFirst = false)
        {
            if (!_active)
                return null;

            string output = "";
            string workingMessage = "";

            if (_currentMessageIndex >= _messageQueue.Length)
            {
                _active = false;
                _messageFinished = true;
                _wait = true;
                CMasterControl.audioPlayer.addSfx(CMasterControl.audioPlayer.soundBank["Text:textBoxClose"]);
                startTimer0(15);
                return string.Empty;
            }
            
            workingMessage = _messageQueue[_currentMessageIndex];

            _checkMessageLength(ref workingMessage);
            output = _divideLines(workingMessage).Trim();

            if (string.IsNullOrEmpty(output))
            {
                _currentMessageIndex += 1;

                output = _processMessage(isFirst);
            }
            else if (!isFirst)
                CMasterControl.audioPlayer.addSfx(CMasterControl.audioPlayer.soundBank["Text:textBoxContinue"]);

            return output;
        }

        private void _checkMessageLength(ref string workingMessage)
        {
            if (_messageQueue.Length > _THREE_LINE_MAX)
            {
                workingMessage = _messageQueue[_currentMessageIndex].Substring(0, _THREE_LINE_MAX);
                _messageQueue[_currentMessageIndex] = _messageQueue[_currentMessageIndex].Substring(_THREE_LINE_MAX);

                if (workingMessage.Last() != ' ' && _messageQueue[_currentMessageIndex].First() != ' ')
                {
                    string lastWord = workingMessage.Substring(workingMessage.LastIndexOf(' '));
                    workingMessage = workingMessage.Remove(workingMessage.LastIndexOf(lastWord));
                    _messageQueue[_currentMessageIndex] = _messageQueue[_currentMessageIndex].Insert(0, lastWord).TrimStart();
                }
            }
            else
                _messageQueue[_currentMessageIndex] = "";
        }

        private string _divideLines(string workingMessage)
        {
            string output = "";
            int currentLineLen = 0;
            string[] words = workingMessage.Split(' ');
            int lineCount = 1;

            for (int i = 0; i < words.Count(); i++)
            {
                String word = words[i];
                if (currentLineLen + word.Length >= _LINE_MAX_CHARS)
                {
                    if (lineCount == 3)
                    {
                        //join together remaining words
                        string joinedWords = "";
                        for (int j = i; j < words.Count(); j++)
                            joinedWords += words[j] + " ";

                        _messageQueue[_currentMessageIndex] = _messageQueue[_currentMessageIndex].Insert(0, joinedWords.Trim());
                        break;
                    }
                    output += "\n";
                    lineCount++;
                    currentLineLen = 0;
                }
                output += word + " ";
                currentLineLen += word.Length + 1;
            }
            return output;
        }

        public bool active
        {
            get
            {
                return _active;
            }
        }

        public override void timer0(object sender)
        {
            _wait = false;
        }

        public bool wait
        {
            get
            {
                return _wait;
            }
        }


    }
}
