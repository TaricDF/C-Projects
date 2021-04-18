//Author: Taric Folkes
//File Name: Elevens
//Project Name: ElevensXNA
//Creation Date: Sept. 14, 2018
//Modified Date: Sept. 23, 2018
//Description: Elevens is a solitaire card game using a standard 52-card deck. Cards are placed in a 6x2 grid, 
//pairs of cards which add up to eleven are covered up, 
//face cards may be replaced with another card form the deck(goes to bottom of the deck)
//the game is won when all the piles have face cards on top
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ElevensXNA
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Elevens : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //Will allow to switch between differnt game states
        enum GameState
        {
            Menu,
            Playing,
            Win,
            Lose
        }
        GameState currentGameState = GameState.Menu;

        //Menu
        Texture2D menu;
        Rectangle menuRec;

        //Title
        Texture2D title;
        Rectangle titleRec;

        //Play Button
        Texture2D startBtn;
        Rectangle startBtnRec;

        //Screen Res
        int width = 1920;
        int height = 1080;

        //Bool that will determine wheter card is selected
        bool isSelected = false;

        //A bool that will determine if an error message is displayed
        bool showError = false;
        string errorMessage;

        //A select delay 
        float selectedDelay = 0;

        //An error delay 
        float errorDelay = 0;

        //Card widths and heights
        int cardWidth = 91;
        int cardHeight = 128;

        //Column and row
        int cardCol;
        int cardRow;

        //Card coordinates
        int cardX;
        int cardY;

        //An array of integers stores the card number
        int[] selectedCard = new int[3];

        //A 2D array of ints that store the card numbers
        int[,] cards = new int[6, 2];

        //A 2D array of ints that will store the number of card in each pile
        int[,] pileSizes = new int[6, 2];

        //A list to keep all the cards inside
        List<int> deck = new List<int>();

        //Card Face
        Texture2D cardFaces;
        Rectangle[,] recCards = new Rectangle[13,4];

        //Card Back
        Texture2D cardBack;
        Rectangle cardBackRec;

        //Game Board
        Texture2D gameBoard;
        Rectangle boardRec;

        //Mouse Cursor
        Texture2D cursor;

        //Will show which cards can be switched
        Texture2D cardIndicator;
        Rectangle indicaterRec;

        //Shows what card is selected
        Texture2D cardSelected;
        Rectangle selectedRec;

        //Mouse states
        MouseState mouse;
        MouseState prevMouse;
        SpriteFont mouseFont;
        Vector2 mouseFontLoc;
        string font = "";

        //Keyboard State
        KeyboardState kb;
        KeyboardState prevKb;

        //Positions of each card
        Vector2 cardPos;
        Vector2 indicatorPos;
        Vector2 selectPos;

        //Sprite fonts
        SpriteFont deckCount;
        SpriteFont errorFont;
        SpriteFont endFont;

        //Shows where each msg should be displayed
        Vector2 deckCountPos = new Vector2(81, 99);
        Vector2 errorMsgPos = new Vector2(20, 300);
        Vector2 endMsgPos = new Vector2(200, 300);

        //Used to shuffle the deck
        Random rng = new Random();

        public Elevens()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            //Screen Resolution
            this.graphics.IsFullScreen = true;
            this.graphics.PreferredBackBufferHeight = 1080;
            this.graphics.PreferredBackBufferWidth = 1920;
            graphics.ApplyChanges();

            //Makes the mouse visible
            this.IsMouseVisible = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Menu Screen
            menu = Content.Load<Texture2D>(@"Menu\Background");
            menuRec = new Rectangle(0, 0, width, height);

            //Title
            title = Content.Load<Texture2D>(@"Menu\title");
            titleRec = new Rectangle(700, 130, (int)(title.Width * 1.4), (int)(title.Height * 1.4));

            //Play Button
            startBtn = Content.Load<Texture2D>(@"Menu\start button");
            startBtnRec = new Rectangle(787, 833, (int)(startBtn.Width * 1.6), (int)(startBtn.Height * 1.5));

            //Coordinates
            mouseFont = Content.Load<SpriteFont>(@"Fonts\deck");
            mouseFontLoc = new Vector2(0, 50);

            //Cards
            cardFaces = Content.Load<Texture2D>(@"Playing/CardFaces");
            cardBack = Content.Load<Texture2D>(@"Playing/CardBack");
            cardBackRec = new Rectangle(32, 20, (int)(cardBack.Width * 1.5), (int)(cardBack.Height * 1.5));
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 13; ++j)
                {
                    recCards[j, i] = new Rectangle(cardWidth * j, cardHeight * i, cardWidth, cardHeight);
                }
            }

            //The Game board
            gameBoard = Content.Load<Texture2D>(@"Playing/game board");
            boardRec = new Rectangle(0, 0, width, height);

            //Indcator
            cardIndicator = Content.Load<Texture2D>(@"Playing/indicator");
            indicaterRec = new Rectangle(0, 0, 30, 23);

            //Selected
            cardSelected = Content.Load<Texture2D>(@"Playing/highlight");
            selectedRec = new Rectangle(0, 0, 95, 132);

            //SpriteFonts
            deckCount = Content.Load<SpriteFont>(@"Fonts/deck");
            errorFont = Content.Load<SpriteFont>(@"Fonts/errorMsg");
            endFont = Content.Load<SpriteFont>(@"Fonts/end");

        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            //Allows game to exit
            if (kb.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            //Keyboard State
            prevKb = kb;
            kb = Keyboard.GetState();

            //Mouse State
            prevMouse = mouse;
            mouse = Mouse.GetState();
            font = "X: " + mouse.X + "\nY: " + mouse.Y;

            switch (currentGameState)
            {
                #region Menu
                case GameState.Menu:
                    if((mouse.LeftButton == ButtonState.Pressed && mouse.X >= startBtnRec.X && mouse.X <= startBtnRec.X + startBtnRec.Width
                && mouse.Y >= startBtnRec.Y && mouse.Y <= startBtnRec.Y + startBtnRec.Height))
                    {
                        currentGameState = GameState.Playing;
                        Start();
                    }
                    break;
                #endregion

                #region Playing
                case GameState.Playing:
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            //Handles the add up to eleven of cards
                            if (mouse.LeftButton == ButtonState.Pressed //&& prevMouse.LeftButton == ButtonState.Released
                            && mouse.X >= cardPos.X && mouse.X <= cardPos.X + recCards[j, i].Width && mouse.Y >= cardPos.Y && mouse.Y <= cardPos.Y + recCards[j, i].Height
                            && isSelected)
                            {
                                //Checks if the two cards add up to eleven
                                if ((cards[j, i] % 13) + (selectedCard[0] % 13) == 11)
                                {
                                    cards[j, i] = deck[0];
                                    deck.RemoveAt(0);
                                    cards[selectedCard[1], selectedCard[2]] = deck[0];
                                    deck.RemoveAt(0);
                                    pileSizes[j, i]++;
                                    pileSizes[selectedCard[1], selectedCard[2]]++;
                                    isSelected = false;
                                    selectedDelay = 10;
                                    errorDelay = 0;
                                }
                                else if (isFaceCard(cards[j, i]))
                                {
                                    isSelected = false;
                                    selectedDelay = 10;
                                    errorDelay = 120;
                                    errorMessage = "This card cannot be matched. It is a face card";
                                }
                                else if (cards[j, i] == selectedCard[0])
                                {
                                    isSelected = false;
                                    selectedDelay = 10;
                                }
                                else
                                {
                                    isSelected = false;
                                    selectedDelay = 10;
                                    errorDelay = 60;
                                    errorMessage = (selectedCard[0] % 13) + " and " + (cards[j, i] % 13) + " do not add up to 11";
                                }
                            }
                        }
                    }
                    if(deck.Count == 0)
                    {
                        currentGameState = GameState.Win;
                    }
                    break;
                    #endregion
            }



            if (selectedDelay > 0)
            {
                selectedDelay--;
            }


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            switch (currentGameState)
            {
                #region Menu
                case GameState.Menu:
                    spriteBatch.Draw(menu, menuRec, Color.White);
                    spriteBatch.Draw(title, titleRec, Color.White);
                    spriteBatch.Draw(startBtn, startBtnRec, Color.White);
                    //Coordinates
                    spriteBatch.DrawString(mouseFont, font, mouseFontLoc, Color.White);
                    break;
                #endregion

                #region Playing
                case GameState.Playing:
                    spriteBatch.Draw(gameBoard, boardRec, Color.White);
                    spriteBatch.Draw(cardBack, cardBackRec, Color.White);
                    CardDraw();
                    MoveFace();
                    spriteBatch.DrawString(deckCount, "" + deck.Count, deckCountPos, Color.White);
                    //Coordinates
                    spriteBatch.DrawString(mouseFont, font, mouseFontLoc, Color.Red);
                    if (errorDelay > 0)
                    {
                        spriteBatch.DrawString(errorFont, errorMessage, errorMsgPos, Color.White);
                        errorDelay--;
                    }
                    break;
                #endregion

                #region Win
                case GameState.Win:
                    if (deck.Count == 0)
                    {
                        spriteBatch.DrawString(endFont, "YOU WIN!", endMsgPos, Color.Gold);
                    }
                    break;
                    #endregion

            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Shuffles the deck
        /// </summary>
        /// <param name="deck">Deck list that stores all cards</param>
        private void ShuffleDeck(List<int> deck)
        {
            int deckCount = deck.Count;
            int rearrange;
            int fill;

            //Shuffle from top-down
            while (deckCount > 0)
            { 
                deckCount--;

                rearrange = rng.Next(deckCount);

                fill = deck[deckCount];

                deck[deckCount] = deck[rearrange];

                deck[rearrange] = fill;
            }
        }
        /// <summary>
        /// Shuffles the deck then removes the cards from the deck then puts them in the pile
        /// </summary>
        private void Start()
        {
            for (int i = 0; i < deck.Count; i++)
            {
                deck.RemoveAt(0);
            }

            for (int i = 1; i <= 52; i++)
            {
                deck.Add(i);
            }

            ShuffleDeck(deck);

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    cards[j, i] = deck[0];
                    deck.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Draws the cards in a cookie-cutter style
        /// </summary>
        private void CardDraw()
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    cardX = (cardWidth * j) + (20 * j) + 561;
                    cardY = (cardHeight * i) + (20 * i) + 357;

                    cardPos = new Vector2(cardX, cardY);

                    indicatorPos = new Vector2(cardX + cardWidth - 22, cardY - 4);

                    selectPos = new Vector2(cardX - 2, cardY - 2);

                    cardCol = (cards[j, i] - 1) % 13;
                    cardRow = (cards[j, i] - 1) / 13;

                    spriteBatch.Draw(cardFaces, cardPos, recCards[cardCol, cardRow], Color.White);
                }
            }
        }
        /// <summary>
        /// Determines whether you can move the face card
        /// </summary>
        private void MoveFace()
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (mouse.LeftButton == ButtonState.Pressed //&& mouse.LeftButton == ButtonState.Released
                    && mouse.X >= cardPos.X && mouse.X <= cardPos.X + recCards[j, i].Width && mouse.Y >= cardPos.Y && mouse.Y <= cardPos.Y + recCards[j, i].Height
                    && isSelected == false && selectedDelay == 0)
                    {
                        //if the card is a face replace it with another card, else not then display an error message
                        if (isFaceCard(cards[j, i]))
                        {
                            if (pileSizes[j, i] == 0
                                && deck.Count > 0)
                            {
                                deck.Add(cards[j, i]);
                                cards[j, i] = deck[0];
                                deck.RemoveAt(0);
                            }
                            else
                            {
                                errorMessage = "Cannot swap face card. Pile size is greater than 1.";
                                errorDelay = 180;
                            }
                        }
                        else
                        {
                            isSelected = true;
                            errorDelay = 0;
                            selectedCard[0] = cards[j, i];
                            selectedCard[1] = j;
                            selectedCard[2] = i;
                        }
                    }
                    //Determines if the cards are selected and if they are then draw the selected box around them
                    if (isSelected == true && selectedCard[1] == j && selectedCard[2] == i)
                    {
                        spriteBatch.Draw(cardSelected, selectPos, selectedRec, Color.White);
                    }
                    //Checks if the card is a face then checks the deck and pile size
                    if (isFaceCard(cards[j, i]))
                    {
                        //If the pile size is equal to 0 and the deck is greater than 0 draw the highlight around the card
                        if (pileSizes[j, i] == 0 && deck.Count > 0)
                        {
                            spriteBatch.Draw(cardIndicator, indicatorPos, indicaterRec, Color.White);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A boolean that determines wether the card is a face based on the position in the 2d array
        /// </summary>
        /// <param name="card">uses the card 2d array of integers</param>
        /// <returns>It returns which column from the 2d card array are face cards</returns>
        private bool isFaceCard(int card)
        {
            return (card % 13 == 11 || card % 13 == 12 || card % 13 == 0);
        }

    }
}
