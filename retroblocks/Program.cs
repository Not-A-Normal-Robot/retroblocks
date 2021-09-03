﻿using System;
using System.Timers;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace Game
{
    // Starter
    public class Program
    {
        public static bool paused = false;
        private static Timer frameTimer;
        public static string mode;
        static void Main()
        {
            #region Setup console
            DisableQuickEdit(null, null);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Loading Retroblocks\nPlease wait...");
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetWindowSize(50, 30);
            Console.SetBufferSize(50, 30);
            DisableResize();
            #endregion
            #region Main Menu
            Menu.Main.Start();
            Console.Clear();
            #endregion
            #region Setup game
            Console.SetWindowSize(48, 30);
            Console.SetBufferSize(48, 30);
            Levels.Setup();
            Piece.Setup();
            BagRandomizer.Setup();
            Matrix.Setup();
            CurrentPiece.Setup();
            CurrentPiece.UpdateTimers();
            CurrentPiece.UpdateGravity();
            CurrentPiece.UpdateLines();
            HoldPiece.Setup();
            Drawer.Setup();
            CurrentPiece.Spawn();
            #endregion
            #region Run game
            frameTimer = new Timer(16);
            frameTimer.Elapsed += CurrentPiece.UpdateTimers;
            frameTimer.Elapsed += CurrentPiece.UpdateGravity;
            frameTimer.Elapsed += CurrentPiece.UpdateLines;
            frameTimer.Elapsed += CurrentPiece.ControlPiece;
            frameTimer.AutoReset = true;
            frameTimer.Enabled = true;
            while (true)
            {
                Drawer.DrawToConsole();
                Console.CursorVisible = false;
            }
            #endregion
        }
        #region Disable Selecting Text
        const uint ENABLE_QUICK_EDIT = 0x0040;
        const int STD_INPUT_HANDLE = -10;
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        internal static bool Go()
        {

            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            // get current console mode
            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                // ERROR: Unable to get console mode.
                return false;
            }

            // Clear the quick edit bit in the mode flags
            consoleMode &= ~ENABLE_QUICK_EDIT;

            // set the new mode
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode
                return false;
            }

            return true;
        }
        public static void DisableQuickEdit(object o, ElapsedEventArgs _)
        {
            Go();
        }
        #endregion
        #region Disable resizing
        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        static void DisableResize()
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, 0xF030, 0x00000000);
                DeleteMenu(sysMenu, 0xF000, 0x00000000);
            }
        }
        #endregion
    }
    public static class Matrix
    {
        public static bool[][] state; // board state = state[x][y]
        // 0x = left
        // 0y = bottom
        public static void Setup()
        {
            state = new bool[10][];
            for (int i = 0; i < state.Length; i++)
            {
                state[i] = new bool[40];
            }
        }
    }
    public static class CurrentPiece
    {
        public static bool[][] state; // board state = state[x][y]
        // 0x = left
        // 0y = bottom
        public static bool[][] ghost;
        public static bool[][] nextPieceSpawn;
        public static char piece;
        public static int piecenum;
        private static int _rotState;
        public static int rotState
        {
            get
            {
                return _rotState;
            }
            set
            {
                // modulo operation that supports negative numbers
                _rotState = (value % 4 + 4) % 4;
            }
        }
        public static bool landed
        {
            get
            {
                for(int y = 0; y < 40; y++)
                {
                    for(int x = 0; x < 10; x++)
                    {
                        if (y == 0)
                        {
                            if (state[x][y] /* == true */)
                            {
                                return true;
                            }
                        }
                        else if (state[x][y] && Matrix.state[x][y - 1])
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        private static int leftDasTimer;
        private static int rightDasTimer;
        public static Levels level;
        public static int lockDelayTimer;
        public static int lines;
        public static int lockDelayResets;
        public static bool useSonicDrop;
        /// <summary>
        /// 0 = left mino of piece border is on left part of board. If positive, then left mino of piece border is on the right of left part of board. Can go negative.
        /// </summary>
        public static int xoffset;
        /// <summary>
        /// 0 = bottom mino of piece border is on bottom of board. If positive, then bottom left mino of piece border is on top of bottom of board. Can go negative.
        /// </summary>
        public static int yoffset;
        public static int score;
        public static int areTimer;
        public static int lineAreTimer;
        public static double leftoverG;
        public static bool[] lined;
        public static int levelNum {
            get
            {
                return (int)Math.Floor(lines / 10d);
            }
        }
        public static void UpdateTimers(object o, ElapsedEventArgs _)
        {
            UpdateTimers();
        }
        public static void UpdateGravity(object o, ElapsedEventArgs _)
        {
            UpdateGravity();
        }
        public static void UpdateLines(object o, ElapsedEventArgs _)
        {
            UpdateLines();
        }
        public static void UpdateTimers()
        {
            if (lineAreTimer >= 0)
            {
                UpdateGhost();
                lineAreTimer++;
            }
            else if (areTimer >= 0)
            {
                areTimer++;
            }

            if (landed)
            {
                lockDelayTimer++;
                if (lockDelayTimer > level.lockDelay)
                {
                    LockPiece(null, null);
                }
            }
            else
            {
                lockDelayTimer = 0;
            }
            if (areTimer > level.are)
            {
                Spawn();
                areTimer = -1;
            }
        }
        public static void UpdateGravity()
        {
            for (int i = 0; i < (int)Math.Floor(level.g); i++)
            {
                Fall(null, null);
            }
            leftoverG += level.g - (int)Math.Floor(level.g);
            if (leftoverG >= 1)
            {
                Fall(null, null);
                leftoverG -= 1;
            }
        }
        public static void UpdateLines()
        {
            if (lineAreTimer > level.lineAre)
            {
                int[] rowsLined = new int[4];
                int firstUnused = 0;
                // lines cleared == firstUnused + 1
                for (int i = 0; i < 40; i++)
                {
                    if (lined[i])
                    {
                        rowsLined[firstUnused] = i;
                        firstUnused++;
                    }
                }
                for (int i = 0; i < firstUnused; i++)
                {
                    for (int y = 0; y < 40; y++)
                    {
                        if (y >= rowsLined[i])
                        {
                            for (int x = 0; x < 10; x++)
                            {
                                if (y == 39)
                                {
                                    Matrix.state[x][y] = false;
                                }
                                else
                                {
                                    Matrix.state[x][y] = Matrix.state[x][y + 1];
                                }
                            }
                        }
                    }
                }
                lineAreTimer = -1;
                lined = new bool[40];
            } // blocks above cleared line fall

        }
        public static void ControlPiece(object o, ElapsedEventArgs _)
        {
            // Left
            if (NativeKeyboard.IsKeyDown(Controls.left))
            {
                leftDasTimer++;
                if (areTimer < 0)
                {
                    if (!Controls.prevFramePresses[0])
                    {
                        Left();
                    }
                    if (leftDasTimer > level.das)
                    {
                        if (level.arr == 0)
                        {
                            for (int i = 0; i < 9; i++)
                            {
                                Left();
                            }
                        }
                        else
                        {
                            if (leftDasTimer > level.das + level.arr)
                            {
                                leftDasTimer = level.das;
                                Left();
                            }
                            leftDasTimer++;
                        }
                    }
                }
            }
            else { leftDasTimer = 0; }

            // Right
            if (NativeKeyboard.IsKeyDown(Controls.right))
            {
                rightDasTimer++;
                if (!Controls.prevFramePresses[1])
                {
                    Right();
                }
                if (rightDasTimer > level.das)
                {
                    if (level.arr == 0)
                    {
                        for (int i = 0; i < 9; i++)
                        {
                            Right();
                        }
                    }
                    else
                    {
                        if (rightDasTimer > level.das + level.arr)
                        {
                            rightDasTimer = level.das;
                            Right();
                        }
                        rightDasTimer++;
                    }
                }
            }
            else { rightDasTimer = 0; }

            // Hard drop
            if (NativeKeyboard.IsKeyDown(Controls.hardDrop) && !Controls.prevFramePresses[2] && areTimer == -1)
            {
                LockPiece(null, null);
            }

            // Soft drop
            if (NativeKeyboard.IsKeyDown(Controls.softDrop))
            {
                Fall(null, null);
                score += 1;
                while(!landed && Controls.useSonicDrop && areTimer == -1)
                {
                    Fall(null, null);
                    score += 1;
                }
            }

            // Clockwise
            if(NativeKeyboard.IsKeyDown(Controls.rotCw) && !Controls.prevFramePresses[4] && areTimer == -1)
            {
                RotCW();
            }

            // Counterclockwise
            if(NativeKeyboard.IsKeyDown(Controls.rotCcw) && !Controls.prevFramePresses[5] && areTimer == -1)
            {
                RotCCW();
            }

            // 180 rotation
            if(NativeKeyboard.IsKeyDown(Controls.rot180) && !Controls.prevFramePresses[6])
            {
                Rot180();
            }

            // Hold
            if(NativeKeyboard.IsKeyDown(Controls.hold) && !Controls.prevFramePresses[7] && areTimer < 0)
            {
                Hold();
            }
            Controls.SaveFramePresses();
        }
        public static void Setup()
        {
            #region Setup Active Piece State
            state = new bool[10][];
            for (int i = 0; i < state.Length; i++)
            {
                state[i] = new bool[40];
            }
            #endregion
            #region Setup Ghost Piece State
            ghost = new bool[10][];
            for (int i = 0; i < ghost.Length; i++)
            {
                ghost[i] = new bool[40];
            }
            #endregion
            #region Setup Next Piece Position State
            nextPieceSpawn = new bool[10][];
            for (int i = 0; i < nextPieceSpawn.Length; i++)
            {
                nextPieceSpawn[i] = new bool[40];
            }
            #endregion
            lined = new bool[40];
            piece = BagRandomizer.output[BagRandomizer.current][0];
            piecenum = 0;
            leftDasTimer = Controls.das;
            rightDasTimer = Controls.das;
            level = Levels.list[0];
            score = 0;
            leftoverG = 0d;
            areTimer = -1;
            lineAreTimer = -1;
            Controls.Setup();
        }
        public static void NextPiece()
        {
            bool newBag = false;
            if (piecenum == 6)
            {
                newBag = true;
                BagRandomizer.GetNew();
                piecenum = 0;
            }
            piece = BagRandomizer.output[BagRandomizer.current][piecenum];
            if (!newBag)
            {
                piecenum++;
            }
        }
        public static void Fall(object o, ElapsedEventArgs _)
        {
            if(!landed)
            {
                bool[][] newState = new bool[10][]  
                {
                    new bool[40],
                    new bool[40],
                    new bool[40],
                    new bool[40],
                    new bool[40],
                    new bool[40],
                    new bool[40],
                    new bool[40],
                    new bool[40],
                    new bool[40],
                };
                for(int y = 0; y < 39; y++)
                {
                    for(int x = 0; x < 10; x++)
                    {
                        newState[x][y] = state[x][y + 1];
                    }
                }
                state = newState;
                yoffset--;
            }
        }
        public static void LockPiece(object o, ElapsedEventArgs _)
        {
            HoldPiece.used = false;
            while(!landed)
            {
                Fall(null, null);
                score += 2;
            }
            for(int y = 0; y < 40; y++)
            {
                for(int x = 0; x < 10; x++)
                {
                    if(state[x][y])
                    {
                        Matrix.state[x][y] = true;
                        state[x][y] = false;
                    }
                }
            }
            int linesCleared = 0;
            for(int y = 0; y < 40; y++)
            {
                if(
                    #region Check for line
                    Matrix.state[0][y] && Matrix.state[1][y] && Matrix.state[2][y] && Matrix.state[3][y] && Matrix.state[4][y] && Matrix.state[5][y] && Matrix.state[6][y] && Matrix.state[7][y] && Matrix.state[8][y] && Matrix.state[9][y]
                #endregion
                    )
                {
                    ClearLine(y);
                    linesCleared++;
                    lined[y] = true;
                    for(int e = 0; e < 10; e++)
                    {
                        Matrix.state[e][y] = false;
                    }
                }
            }
            switch (linesCleared)
            {
                case 0:
                    lineAreTimer = -1;
                    break;
                case 1:
                    lines++;
                    score += (int)Math.Floor(lines / 10d) * 100;
                    lineAreTimer = 0;
                    level = Levels.list[(int)Math.Floor(lines / 10d)];
                    break;
                case 2:
                    lines += 2;
                    score += (int)Math.Floor(lines / 10d) * 100;
                    lineAreTimer = 0;
                    break;
                case 3:
                    lines += 3;
                    score += (int)Math.Floor(lines / 10d) * 100;
                    lineAreTimer = 0;
                    break;
                case 4:
                    lines += 4;
                    score += (int)Math.Floor(lines / 10d) * 100;
                    lineAreTimer = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("5+ line clear!");
            }
            areTimer = 0;
        }
        public static void Spawn()
        {
            xoffset = 2;
            yoffset = 18;
            if (NativeKeyboard.IsKeyDown(Controls.rotCcw)) { rotState = 3; }
       else if (NativeKeyboard.IsKeyDown(Controls.rot180)) { rotState = 2; }
       else if (NativeKeyboard.IsKeyDown(Controls.rotCw )) { rotState = 1; }
       else                                                { rotState = 0; }
            for(int x = 0; x < 5; x++)
            {
                for(int y = 0; y < 5; y++)
                {
                    state[x + 2][-y + 23] = Piece.GetPiece(BagRandomizer.output[BagRandomizer.current][piecenum]).piece[rotState][y][x];
                }
            }
            NextPiece();
            UpdateGhost();
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    nextPieceSpawn[x + 2][-y + 23] = Piece.GetPiece(BagRandomizer.output[BagRandomizer.current][piecenum]).piece[0][y][x];
                }
            }
            if(NativeKeyboard.IsKeyDown(Controls.hold)) { Hold(); }
            lockDelayResets = 0;
        }
        public static void Left()
        {
            if(Array.IndexOf(state[0], true) == -1)
            {
                bool[][] shifted = new bool[10][]
                    {
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                    };

                for(int x = 0; x < 10; x++)
                {
                    for(int y = 0; y < 40; y++)
                    {
                        if(x == 9)
                        {
                            shifted[x][y] = false;
                        }
                        else
                        {
                            shifted[x][y] = state[x + 1][y];
                        }
                        if(shifted[x][y] && Matrix.state[x][y])
                        {
                            return;
                        }
                    }
                }
                state = shifted;
                xoffset--;
                if (landed)
                {
                    ResetLockDelay();
                }
                if(areTimer < 0)
                {
                    UpdateGhost();
                }
            }
        }
        public static void Right()
        {
            if (Array.IndexOf(state[9], true) == -1)
            {
                bool[][] shifted = new bool[10][]
                    {
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                        new bool[40],
                    };

                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 40; y++)
                    {
                        if (x == 0)
                        {
                            shifted[x][y] = false;
                        }
                        else
                        {
                            shifted[x][y] = state[x - 1][y];
                        }
                        if (shifted[x][y] && Matrix.state[x][y])
                        {
                            return;
                        }
                    }
                }
                state = shifted;
                xoffset++;
                if (landed)
                {
                    ResetLockDelay();
                }
                if(areTimer < 0)
                {
                    UpdateGhost();
                }
            }
        }
        public static void RotCW()
        {
            bool[][] rotated = new bool[10][]
            {
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
            };
            bool totalFail = true;
            bool failed = false;
            Piece p = Piece.GetPiece(piece);
            bool[][] m = Matrix.state;
            for(int i = 0; i < p.cwKicks.Length; i++)
            {
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        if (p.piece[(rotState + 1) % 4][-y + 4][x])
                        {
                            // Detect overlap with wall, floor, ceiling or other minos
                            if (x + xoffset + p.cwKicks[rotState][i].x < 0 
                            || x + xoffset + p.cwKicks[rotState][i].x > 9 
                            || y + yoffset + p.cwKicks[rotState][i].y < 0 
                            || y + yoffset + p.cwKicks[rotState][i].y > 40 
                            || m[x + xoffset + p.cwKicks[rotState][i].x][y + yoffset + p.cwKicks[rotState][i].y])
                            {
                                failed = true;
                                break;
                            }
                            else
                            {
                                rotated[x + xoffset + p.cwKicks[rotState][i].x][y + yoffset + p.cwKicks[rotState][i].y] = true;
                            }
                        }
                        if (failed) { break; }
                    }
                    if (failed) { break; }
                }
                if (!failed)
                {
                    totalFail = false;
                    break;
                }
                else
                {
                    failed = false;
                    rotated = new bool[10][] { new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40] };
                }
            }
            if (!totalFail)
            {
                rotState++;
                state = rotated;
                UpdateGhost();
                if (!landed)
                {
                    ResetLockDelay();
                }
            }
        }
        public static void RotCCW()
        {
            bool[][] rotated = new bool[10][]
            {
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
            };
            bool totalFail = true;
            bool failed = false;
            Piece p = Piece.GetPiece(piece);
            bool[][] m = Matrix.state;
            for (int i = 0; i < p.ccwKicks.Length; i++)
            {
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        if (p.piece[(rotState + 3) % 4][-y + 4][x])
                        {
                            // Detect overlap with wall, floor, ceiling or other minos
                            if (x + xoffset + p.ccwKicks[rotState][i].x < 0 
                                || x + xoffset + p.ccwKicks[rotState][i].x > 9 
                                || y + yoffset + p.ccwKicks[rotState][i].y < 0 
                                || y + yoffset + p.ccwKicks[rotState][i].y > 40 
                                || m[x + xoffset + p.ccwKicks[rotState][i].x][y + yoffset + p.ccwKicks[rotState][i].y])
                            {
                                failed = true;
                                break;
                            }
                            else
                            {
                                rotated[x + xoffset + p.ccwKicks[rotState][i].x][y + yoffset + p.ccwKicks[rotState][i].y] = true;
                            }
                        }
                        if (failed) { break; }
                    }
                    if (failed) { break; }
                }
                if (!failed)
                {
                    totalFail = false;
                    break;
                }
                else
                {
                    failed = false;
                    rotated = new bool[10][] { new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40] };
                }
            }
            if (!totalFail)
            {
                rotState--;
                state = rotated;
                UpdateGhost();
                if (!landed)
                {
                    ResetLockDelay();
                }
            }
        }
        public static void Rot180()
        {
            bool[][] rotated = new bool[10][]
            {
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
                new bool[40],
            };
            bool totalFail = true;
            bool failed = false;
            Piece p = Piece.GetPiece(piece);
            bool[][] m = Matrix.state;
            for (int i = 0; i < p.flipKicks.Length; i++)
            {
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        if (p.piece[(rotState + 2) % 4][-y + 4][x])
                        {
                            // Detect overlap with wall, floor, ceiling or other minos
                            if (x + xoffset + p.flipKicks[rotState][i].x < 0
                                || x + xoffset + p.flipKicks[rotState][i].x > 9
                                || y + yoffset + p.flipKicks[rotState][i].y < 0
                                || y + yoffset + p.flipKicks[rotState][i].y > 40
                                || m[x + xoffset + p.flipKicks[rotState][i].x][y + yoffset + p.flipKicks[rotState][i].y])
                            {
                                failed = true;
                                break;
                            }
                            else
                            {
                                rotated[x + xoffset + p.flipKicks[rotState][i].x][y + yoffset + p.flipKicks[rotState][i].y] = true;
                            }
                        }
                        if (failed) { break; }
                    }
                    if (failed) { break; }
                }
                if (!failed)
                {
                    totalFail = false;
                    break;
                }
                else
                {
                    failed = false;
                    rotated = new bool[10][] { new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40] };
                }
            }
            if (!totalFail)
            {
                rotState += 2;
                state = rotated;
                UpdateGhost();
                if (!landed)
                {
                    ResetLockDelay();
                }
            }
        }
        public static void Hold()
        {

            if (HoldPiece.current == 'N')
            {
                HoldPiece.current = piece;
                state = new bool[10][] { new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40] };
                Spawn();
            }
            else if (HoldPiece.used == false)
            {
                char heldPiece = HoldPiece.current;
                HoldPiece.used = true;
                state = new bool[10][] { new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40] };
                xoffset = 2;
                yoffset = 18;
                if (NativeKeyboard.IsKeyDown(Controls.rotCcw)) { rotState = 3; }
                else if (NativeKeyboard.IsKeyDown(Controls.rot180)) { rotState = 2; }
                else if (NativeKeyboard.IsKeyDown(Controls.rotCw)) { rotState = 1; }
                else { rotState = 0; }
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        state[x + 2][-y + 23] = Piece.GetPiece(HoldPiece.current).piece[rotState][y][x];
                    }
                }
                HoldPiece.current = piece;
                piece = heldPiece;
                UpdateGhost();
            }
        }
        /// <summary>
        /// Get info of a future piece, up to 7 pieces
        /// </summary>
        /// <param name="intoFuture">How many pieces into the future. 0 = next piece. Min = 0, Max = 7, inclusively.</param>
        /// <returns></returns>
        public static Piece GetFuturePiece(int intoFuture)
        {
            if(intoFuture < 0 || intoFuture > 7)
            {
                throw new ArgumentOutOfRangeException(intoFuture < 0 ? $"Minimum value is 1, got {intoFuture}" : $"Maximum value is 7, got {intoFuture}");
            }
            if(piecenum + intoFuture > 6)
            {
                return Piece.GetPiece(BagRandomizer.output[BagRandomizer.next][piecenum + intoFuture - 7]);
            }
            else
            {
                return Piece.GetPiece(BagRandomizer.output[BagRandomizer.current][piecenum + intoFuture]);
            }
        }
        public static void ResetLockDelay()
        {
            if(landed && lockDelayResets < 30)
            {
                lockDelayTimer = 0;
                lockDelayResets++;
            }
            if(!landed)
            {
                lockDelayTimer = 0;
                lockDelayResets++;
            }
        }
        public static void UpdateGhost()
        {
            for(int y = 0; y < 40; y++)
            {
                for(int x = 0; x < 10; x++)
                {
                    ghost[x][y] = state[x][y];
                }
            }
            int i = 0;
            while (!IsLanded(ghost) && i < 40)
            {
                for (int y = 0; y < 40; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        ghost[x][Math.Max(y - 1, 0)] = ghost[x][y];
                    }
                }
                i++;
            }
        }
        private static bool IsLanded(bool[][] _state)
        {

            for (int y = 0; y < 40; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (y == 0)
                    {
                        if (_state[x][y] /* == true */)
                        {
                            return true;
                        }
                    }
                    else if (_state[x][y] && Matrix.state[x][y - 1])
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static void ClearLine(int y)
        {
            level = Levels.list[(int)Math.Floor(lines/10d)];
        }
    }
    static class Drawer
    {
        private static string[] Picture
        {
            get
            {
                string[] ycache = new string[24];
                string xcache = "";
                for(int y = 0; y < 24; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        if (Matrix.state[x][y])
                        {
                            xcache += "██";
                        }
                        else if (CurrentPiece.state[x][y])
                        {
                            if(CurrentPiece.lockDelayTimer < 0.25 * CurrentPiece.level.lockDelay)
                            {
                                xcache += "[]";
                            }
                            else if (CurrentPiece.lockDelayTimer < 0.5 * CurrentPiece.level.lockDelay)
                            {
                                xcache += "░░";
                            }
                            else if (CurrentPiece.lockDelayTimer < 0.75 * CurrentPiece.level.lockDelay)
                            {
                                xcache += "▒▒";
                            }
                            else
                            {
                                xcache += "▓▓";
                            }
                        }
                        else if (CurrentPiece.ghost[x][y])
                        {
                            xcache += "##";
                        }
                        else if (CurrentPiece.nextPieceSpawn[x][y])
                        {
                            xcache += "XX";
                        }
                        else
                        {
                            xcache += "  ";
                        }
                    }
                    ycache[y] = xcache;
                    xcache = "";
                }
                return ycache;
            }
        }
        public static void Setup(object o, ElapsedEventArgs _)
        {
            Setup();
        }
        public static void Setup()
        {
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(
  /* 0*/         $"HOLD        |                    |    NEXT\n" +
  /* 1*/         $"            |                    |\n" +
  /* 2*/         $"            |                    |\n" +
  /* 3*/         $"            |                    |\n" +
  /* 4*/         $"            |                    |\n" +
  /* 5*/         $"            |                    |\n" +
  /* 6*/         $"            |                    |\n" +
  /* 7*/         $"            |                    |\n" +
  /* 8*/         $"            |                    |\n" +
  /* 9*/         $"            |                    |\n" +
  /*10*/         $"            |                    |\n" +
  /*11*/         $"            |                    |\n" +
  /*12*/         $"            |                    |\n" +
  /*13*/         $"            |                    |\n" +
  /*14*/         $"            |                    |\n" +
  /*15*/         $"            |                    |\n" +
  /*16*/         $"            |                    |\n" +
  /*17*/         $"            |                    |\n" +
  /*18*/         $"            |                    |\n" +
  /*19*/         $"            |                    |\n" +
  /*20*/         $"            |                    |\n" +
  /*21*/         $"            |                    |\n" +
  /*22*/         $"            |                    |\n" +
  /*23*/         $"            |                    |\n"
              //   0123456789012345678901234567890123456
              //   0         10        20        30
                );
            Console.ForegroundColor = ConsoleColor.Red;
            for(int i = 0; i < 4; i++)
            {
                Console.SetCursorPosition(12, i);
                Console.Write("|                    |");
            }
            DrawToConsole();
            #region Starting Animation
            System.Threading.Thread.Sleep(750);
            Console.SetCursorPosition(18, 12);
            Console.Write("-       -");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("--     --");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("---   ---");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("---- ----");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("---------");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("---------");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("---------");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("----■----");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("---<■>---");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("---[■]---");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("--<[■]>--");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("--<[A]>--");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("-<[ A ]>-");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("<[E A D]>");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("[ E A D ]");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("R E A D Y");
            System.Threading.Thread.Sleep(760);
            Console.SetCursorPosition(18, 12);
            Console.Write("[ E A D ]");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("<[E A D]>");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("-<[ A ]>-");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("--<[A]>--");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("--<[■]>--");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("---<■>---");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("----■----");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("---------");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write(" ------- ");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("  -----  ");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("   ---   ");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("   G-O   ");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("   G O   ");
            System.Threading.Thread.Sleep(840);
            Console.SetCursorPosition(18, 12);
            Console.Write("-  G O  -");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("-- G O --");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("---G O---");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("--<G O>--");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("--[G O]--");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("-<[G O]>-");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("--<[■]>--");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("---<■>---");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("----■----");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("---------");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write(" ------- ");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("  -----  ");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("   ---   ");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("    -    ");
            System.Threading.Thread.Sleep(20);
            Console.SetCursorPosition(18, 12);
            Console.Write("         ");
            #endregion
        }
        public static void DrawToConsole()
        {
            Console.SetCursorPosition(0, 0);
            if (!Program.paused)
            {
                string[] Picture = Drawer.Picture;
                for(int i = 0, j = 23; i < 24; i++, j--)
                {
                    Console.SetCursorPosition(13, i);
                    if (j == 23)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else if(j == 19)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    Console.Write(Picture[j]);
                }
                DrawHoldPiece();
                DrawNextPieces();
                Console.SetCursorPosition(12, 24);
                double lockDelayPercentage = CurrentPiece.lockDelayTimer / CurrentPiece.level.lockDelay;
                int equals = (int)(lockDelayPercentage * 24);
                for(int i = 0; i < 24; i++)
                {
                    if(i >= equals)
                    {
                        Console.Write("=");
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                }
                Console.SetCursorPosition(12, 25);
                int ldr = 30 - CurrentPiece.lockDelayResets;
                for(int i = 0; i < 15; i++)
                {
                    if(ldr >= 2)
                    {
                        Console.Write("″");
                    }
                    else if(ldr == 1)
                    {
                        Console.Write("'");
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                    ldr -= 2;
                }
                Console.SetCursorPosition(0, 5);
                Console.Write($"Lvl {CurrentPiece.levelNum}");
                Console.SetCursorPosition(0, 6);
                Console.Write($"{(int)(CurrentPiece.level.g * 100) / 100d}G {CurrentPiece.level.lockDelay}LD");
                Console.SetCursorPosition(0, 7);
                Console.Write($"{CurrentPiece.lines} lines");
            }
            else
            {
                Console.Write($"[ ==========PAUSED========== ]\n" +
                    $"Press Esc to unpause\n" + 
                    $"Press R to restart\n"
                    );
            }
        }
        private static void DrawHoldPiece()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(0, 1);
            bool[][] _ = Piece.GetPiece(HoldPiece.current).piece[0];
            Console.Write(GetMinos(_[1]));
            Console.SetCursorPosition(0, 2);
            Console.Write(GetMinos(_[2]));
        }
        private static void DrawNextPieces()
        {
            Console.SetCursorPosition(36, 2);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(0).piece[0][1]));

            Console.SetCursorPosition(36, 3);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(0).piece[0][2]));

            Console.SetCursorPosition(36, 5);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(1).piece[0][1]));

            Console.SetCursorPosition(36, 6);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(1).piece[0][2]));

            Console.SetCursorPosition(36, 8);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(2).piece[0][1]));

            Console.SetCursorPosition(36, 9);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(2).piece[0][2]));

            Console.SetCursorPosition(36, 11);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(3).piece[0][1]));

            Console.SetCursorPosition(36, 12);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(3).piece[0][2]));

            Console.SetCursorPosition(36, 14);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(4).piece[0][1]));

            Console.SetCursorPosition(36, 15);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(4).piece[0][2]));

            Console.SetCursorPosition(36, 17);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(5).piece[0][1]));

            Console.SetCursorPosition(36, 18);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(5).piece[0][2]));

            Console.SetCursorPosition(36, 20);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(6).piece[0][1]));

            Console.SetCursorPosition(36, 21);
            Console.Write(GetMinos(CurrentPiece.GetFuturePiece(6).piece[0][2]));
        }
        private static string GetMinos(bool[] minoTypes)
        {
            string _ = "";
            for(int i = 0; i < minoTypes.Length; i++)
            {
                _ += minoTypes[i] ? "[]" : "  ";
            }
            return _;
        }
    }
    static class BagRandomizer
    {
        public static string[] output = new string[2];
        public static int current = 0;
        public static int next = 1;
        /// <returns>Index of output that is supposed to be the current one</returns>
        public static int GetNew()
        {
            output[current] = GetSeq();
            if(current == 0)
            {
                next = 0;
                current = 1;
            }
            else
            {
                next = 1;
                current = 0;
            }
            return current;
        }
        public static void Setup()
        {
            output[current] = GetSeq();
            output[next] = GetSeq();
        }

        #region Private methods
        private static string GetSeq()
        {
            char currentChar;
            string stringSoFar = "";
            Random r = new Random();
            while(stringSoFar.Length < 7)
            {
                currentChar = GetChar(r.Next(0, 7));
                if (stringSoFar.IndexOf(currentChar) == -1)
                {
                    stringSoFar += currentChar;
                }
            }
            return stringSoFar;
        }
        private static char GetChar(int i) // 0 = Z
        {
            switch (i)
            {
                case 0:
                    return 'Z';
                case 1:
                    return 'L';
                case 2:
                    return 'O';
                case 3:
                    return 'S';
                case 4:
                    return 'I';
                case 5:
                    return 'J';
                case 6:
                    return 'T';
                default:
                    return 'N';
            }
        }
        #endregion
    }
    static class HoldPiece
    {
        public static char current;
        public static bool used;
        public static void Setup()
        {
            current = 'N';
            used = false;
        }
    }
    public class Piece
    {
        //            WARNING

        //    Corner detection systems
        //          incomplete.

        #region Data
        /// <summary>
        /// The state for the piece. [rot state], [y], [x]
        /// </summary>
        public bool[][][] piece = new bool[4][][]
        {
            new bool[5][] { new bool[5], new bool[5], new bool[5], new bool[5], new bool[5] }, // init rot
            new bool[5][] { new bool[5], new bool[5], new bool[5], new bool[5], new bool[5] }, // cw
            new bool[5][] { new bool[5], new bool[5], new bool[5], new bool[5], new bool[5] }, // 180
            new bool[5][] { new bool[5], new bool[5], new bool[5], new bool[5], new bool[5] }, // ccw
        }; // States for each rotation
        /// <summary>
        /// Corners to detect for spin detection. 0, 0 is bottom left.
        /// </summary>
        public Vector2D[] primaryCorners; // Check corners for spin
        public Vector2D[] secondaryCorners;
        public Vector2D[][] cwKicks; // Clockwise kicks
        public Vector2D[][] ccwKicks; // Counterclockwise kicks
        public Vector2D[][] flipKicks; // 180 degree kicks
        #endregion
        public Piece(bool[][][] piece, Vector2D[][] cwKicks, Vector2D[][] ccwKicks, Vector2D[][] flipKicks, Vector2D[] primaryCorners, Vector2D[] secondaryCorners)
        {
            this.piece = piece;
            this.cwKicks = cwKicks;
            this.ccwKicks = ccwKicks;
            this.flipKicks = flipKicks;
            this.primaryCorners = primaryCorners;
            this.secondaryCorners = secondaryCorners;
        }
        #region Piece List
        public static Piece Z;
        public static Piece L;
        public static Piece O;
        public static Piece S;
        public static Piece I;
        public static Piece J;
        public static Piece T;
        public static Piece None;
        #endregion
        public static void Setup()
        {
            #region Setup Piece Properties
            #region Z
            Z = new Piece(
                #region States
                new bool[4][][]{
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // 
                        new bool[5] { false, true,  true,  false, false }, // [][]##
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, false, false, false, false }, // 
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, //
                        new bool[5] { false, false, false, true,  false }, // ####[]
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, false, false, false }, //
                        },                                              
                    new bool[5][] {                                     
                        new bool[5] { false, false, false, false, false }, // 
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, true,  true,  false, false }, // [][]##
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, false, false, false }, // 
                        },                                              
                    new bool[5][] {                                     
                        new bool[5] { false, false, false, false, false }, //
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, true,  true,  false, false }, // [][]##
                        new bool[5] { false, true,  false, false, false }, // []####
                        new bool[5] { false, false, false, false, false }, //
                        },
                },
            #endregion
                #region Kicks
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1,  1), new Vector2D(0, -2), new Vector2D(-1, -2) }, // 0 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1, -1), new Vector2D(0,  2), new Vector2D( 1,  2) }, // R => 2
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // 2 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // L => 0
                },
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // 0 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1, -1), new Vector2D(0,  2), new Vector2D( 1,  2) }, // R => 0
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1,  1), new Vector2D(0, -2), new Vector2D(-1, -2) }, // 2 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1, -1), new Vector2D(0,  2), new Vector2D(-1,  2) }, // L => 2
                },
                new Vector2D[4][]
                {                    // 1                 2                      3                   4                       5                  6                   7                   8                  9                 10                  11                   12                      13
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 0,  1), new Vector2D( 1,  0), new Vector2D( 2,  0), new Vector2D( 1, -1), new Vector2D( 2, -1), new Vector2D(-1,  0), new Vector2D(-2,  0), new Vector2D(-1, -1), new Vector2D(-2, -1), new Vector2D( 0, -1), new Vector2D( 3,  0), new Vector2D(-3,  0) },  // 0 => 2
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 1,  0), new Vector2D( 0, -1), new Vector2D( 0, -2), new Vector2D(-1, -1), new Vector2D(-1, -2), new Vector2D( 0,  1), new Vector2D( 0,  2), new Vector2D(-1,  1), new Vector2D(-1,  2), new Vector2D( 1,  0), new Vector2D( 0,  3), new Vector2D( 0,  3) },  // R => L
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 0, -1), new Vector2D(-1,  0), new Vector2D(-2,  0), new Vector2D(-1,  1), new Vector2D(-2,  1), new Vector2D( 1,  0), new Vector2D( 2,  0), new Vector2D( 1,  1), new Vector2D( 2,  1), new Vector2D( 0,  1), new Vector2D(-3,  0), new Vector2D( 3,  0) },  // 2 => 0
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D(-1,  0), new Vector2D( 0,  1), new Vector2D( 0,  2), new Vector2D( 1,  1), new Vector2D( 1, -2), new Vector2D( 0, -1), new Vector2D( 0,  2), new Vector2D( 1,  1), new Vector2D( 1,  2), new Vector2D(-1,  0), new Vector2D( 0,  3), new Vector2D( 0,  3) },  // L => R
                }
                #endregion
                #region Corner detection
                ,null, null // TODO
                #endregion
                );
            #endregion
            #region L
            L = new Piece(
                #region States
                new bool[4][][]{
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // 
                        new bool[5] { false, false, false, true,  false }, // ####[]
                        new bool[5] { false, true,  true,  true,  false }, // [][][]
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, false, false, false, false }, // 
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // 
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, false, false, false }, // 
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // 
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, true,  true,  true,  false }, // [][][]
                        new bool[5] { false, true,  false, false, false }, // []####
                        new bool[5] { false, false, false, false, false }, // 
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // 
                        new bool[5] { false, true,  true,  false, false }, // [][]##
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, false, false, false }, // 
                        },

                },
            #endregion
                #region Kicks
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1,  1), new Vector2D(0, -2), new Vector2D(-1, -2) }, // 0 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1, -1), new Vector2D(0,  2), new Vector2D( 1,  2) }, // R => 2
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // 2 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // L => 0
                },
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // 0 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1, -1), new Vector2D(0,  2), new Vector2D( 1,  2) }, // R => 0
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1,  1), new Vector2D(0, -2), new Vector2D(-1, -2) }, // 2 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1, -1), new Vector2D(0,  2), new Vector2D(-1,  2) }, // L => 2
                },
                new Vector2D[4][]
                {                    // 1                 2                      3                   4                       5                  6                   7                   8                  9                 10                  11                   12                      13
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 0,  1), new Vector2D( 1,  0), new Vector2D( 2,  0), new Vector2D( 1, -1), new Vector2D( 2, -1), new Vector2D(-1,  0), new Vector2D(-2,  0), new Vector2D(-1, -1), new Vector2D(-2, -1), new Vector2D( 0, -1), new Vector2D( 3,  0), new Vector2D(-3,  0) },  // 0 => 2
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 1,  0), new Vector2D( 0, -1), new Vector2D( 0, -2), new Vector2D(-1, -1), new Vector2D(-1, -2), new Vector2D( 0,  1), new Vector2D( 0,  2), new Vector2D(-1,  1), new Vector2D(-1,  2), new Vector2D( 1,  0), new Vector2D( 0,  3), new Vector2D( 0,  3) },  // R => L
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 0, -1), new Vector2D(-1,  0), new Vector2D(-2,  0), new Vector2D(-1,  1), new Vector2D(-2,  1), new Vector2D( 1,  0), new Vector2D( 2,  0), new Vector2D( 1,  1), new Vector2D( 2,  1), new Vector2D( 0,  1), new Vector2D(-3,  0), new Vector2D( 3,  0) },  // 2 => 0
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D(-1,  0), new Vector2D( 0,  1), new Vector2D( 0,  2), new Vector2D( 1,  1), new Vector2D( 1, -2), new Vector2D( 0, -1), new Vector2D( 0,  2), new Vector2D( 1,  1), new Vector2D( 1,  2), new Vector2D(-1,  0), new Vector2D( 0,  3), new Vector2D( 0,  3) },  // L => R
                }
                #endregion
                #region Corner detection
                , null, null // TODO
                #endregion
                );
            #endregion
            #region O
            O = new Piece(
                #region States
                new bool[4][][]{
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // 
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, false, false, false, false }, // 
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // 
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, false, false, false, false }, // 
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // 
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, false, false, false, false }, // 
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // 
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, false, false, false, false }, // 
                        },
                },
            #endregion
                #region Kicks
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1,  1), new Vector2D(0, -2), new Vector2D(-1, -2) }, // 0 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1, -1), new Vector2D(0,  2), new Vector2D( 1,  2) }, // R => 2
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // 2 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // L => 0
                },
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // 0 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1, -1), new Vector2D(0,  2), new Vector2D( 1,  2) }, // R => 0
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1,  1), new Vector2D(0, -2), new Vector2D(-1, -2) }, // 2 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1, -1), new Vector2D(0,  2), new Vector2D(-1,  2) }, // L => 2
                },
                new Vector2D[4][]
                {                    // 1                 2                      3                   4                       5                  6                   7                   8                  9                 10                  11                   12                      13
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 0,  1), new Vector2D( 1,  0), new Vector2D( 2,  0), new Vector2D( 1, -1), new Vector2D( 2, -1), new Vector2D(-1,  0), new Vector2D(-2,  0), new Vector2D(-1, -1), new Vector2D(-2, -1), new Vector2D( 0, -1), new Vector2D( 3,  0), new Vector2D(-3,  0) },  // 0 => 2
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 1,  0), new Vector2D( 0, -1), new Vector2D( 0, -2), new Vector2D(-1, -1), new Vector2D(-1, -2), new Vector2D( 0,  1), new Vector2D( 0,  2), new Vector2D(-1,  1), new Vector2D(-1,  2), new Vector2D( 1,  0), new Vector2D( 0,  3), new Vector2D( 0,  3) },  // R => L
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 0, -1), new Vector2D(-1,  0), new Vector2D(-2,  0), new Vector2D(-1,  1), new Vector2D(-2,  1), new Vector2D( 1,  0), new Vector2D( 2,  0), new Vector2D( 1,  1), new Vector2D( 2,  1), new Vector2D( 0,  1), new Vector2D(-3,  0), new Vector2D( 3,  0) },  // 2 => 0
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D(-1,  0), new Vector2D( 0,  1), new Vector2D( 0,  2), new Vector2D( 1,  1), new Vector2D( 1, -2), new Vector2D( 0, -1), new Vector2D( 0,  2), new Vector2D( 1,  1), new Vector2D( 1,  2), new Vector2D(-1,  0), new Vector2D( 0,  3), new Vector2D( 0,  3) },  // L => R
                }
                #endregion
                #region Corner detection
                , null, null // TODO
                #endregion
                );
            #endregion
            #region S
            S = new Piece(
                #region States
                new bool[4][][]{
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, //
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, true,  true,  false, false }, // [][]##
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, false, false, false, false }, //
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, //
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, false, true,  false }, // ####[]
                        new bool[5] { false, false, false, false, false }, //
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, //
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, true,  true,  false, false }, // [][]##
                        new bool[5] { false, false, false, false, false }, //
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, //
                        new bool[5] { false, true,  false, false, false }, // []####
                        new bool[5] { false, true,  true,  false, false }, // [][]##
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, false, false, false }, //
                        },
                },
            #endregion
                #region Kicks
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1,  1), new Vector2D(0, -2), new Vector2D(-1, -2) }, // 0 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1, -1), new Vector2D(0,  2), new Vector2D( 1,  2) }, // R => 2
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // 2 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // L => 0
                },
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // 0 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1, -1), new Vector2D(0,  2), new Vector2D( 1,  2) }, // R => 0
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1,  1), new Vector2D(0, -2), new Vector2D(-1, -2) }, // 2 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1, -1), new Vector2D(0,  2), new Vector2D(-1,  2) }, // L => 2
                },
                new Vector2D[4][]
                {                    // 1                 2                      3                   4                       5                  6                   7                   8                  9                 10                  11                   12                      13
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 0,  1), new Vector2D( 1,  0), new Vector2D( 2,  0), new Vector2D( 1, -1), new Vector2D( 2, -1), new Vector2D(-1,  0), new Vector2D(-2,  0), new Vector2D(-1, -1), new Vector2D(-2, -1), new Vector2D( 0, -1), new Vector2D( 3,  0), new Vector2D(-3,  0) },  // 0 => 2
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 1,  0), new Vector2D( 0, -1), new Vector2D( 0, -2), new Vector2D(-1, -1), new Vector2D(-1, -2), new Vector2D( 0,  1), new Vector2D( 0,  2), new Vector2D(-1,  1), new Vector2D(-1,  2), new Vector2D( 1,  0), new Vector2D( 0,  3), new Vector2D( 0,  3) },  // R => L
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 0, -1), new Vector2D(-1,  0), new Vector2D(-2,  0), new Vector2D(-1,  1), new Vector2D(-2,  1), new Vector2D( 1,  0), new Vector2D( 2,  0), new Vector2D( 1,  1), new Vector2D( 2,  1), new Vector2D( 0,  1), new Vector2D(-3,  0), new Vector2D( 3,  0) },  // 2 => 0
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D(-1,  0), new Vector2D( 0,  1), new Vector2D( 0,  2), new Vector2D( 1,  1), new Vector2D( 1, -2), new Vector2D( 0, -1), new Vector2D( 0,  2), new Vector2D( 1,  1), new Vector2D( 1,  2), new Vector2D(-1,  0), new Vector2D( 0,  3), new Vector2D( 0,  3) },  // L => R
                }
                #endregion
                #region Corner detection
                , null, null // TODO
                #endregion
                );
            #endregion
            #region I
            I = new Piece(
                #region States
                new bool[4][][]{
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // ##########
                        new bool[5] { false, false, false, false, false }, // ##########
                        new bool[5] { false, true,  true,  true,  true  }, // ##[][][][]
                        new bool[5] { false, false, false, false, false }, // ##########
                        new bool[5] { false, false, false, false, false }, // ##########
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // ##########
                        new bool[5] { false, false, false, true,  false }, // ######[]##
                        new bool[5] { false, false, false, true,  false }, // ######[]##
                        new bool[5] { false, false, false, true,  false }, // ######[]##
                        new bool[5] { false, false, false, true,  false }, // ######[]##
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // ##########
                        new bool[5] { false, false, false, false, false }, // ##########
                        new bool[5] { false, false, false, false, false }, // ##########
                        new bool[5] { false, true,  true,  true,  true  }, // ##[][][][]
                        new bool[5] { false, false, false, false, false }, // ##########
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // ##########
                        new bool[5] { false, false, true,  false, false }, // ####[]####
                        new bool[5] { false, false, true,  false, false }, // ####[]####
                        new bool[5] { false, false, true,  false, false }, // ####[]####
                        new bool[5] { false, false, true,  false, false }, // ####[]####
                        },
                },
            #endregion
                #region Kicks
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-2, 0), new Vector2D( 1, 0), new Vector2D(-2,-1), new Vector2D( 1, 2) }, // 0 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D( 2, 0), new Vector2D(-1, 2), new Vector2D( 2,-1) }, // R => 2
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 2, 0), new Vector2D(-1, 0), new Vector2D( 2, 1), new Vector2D(-1,-2) }, // 2 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D(-2, 0), new Vector2D( 1,-2), new Vector2D(-2, 1) }, // L => 0
                },
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D( 2, 0), new Vector2D(-1, 2), new Vector2D( 2,-1) }, // 0 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 2, 0), new Vector2D(-1, 0), new Vector2D( 2, 1), new Vector2D(-1,-2) }, // R => 0
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D(-2, 0), new Vector2D( 1,-2), new Vector2D(-2, 1) }, // 2 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-2, 0), new Vector2D( 1, 0), new Vector2D(-2,-1), new Vector2D( 1, 2) }, // L => 2
                },
                new Vector2D[4][]
                {
                    new Vector2D[6] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-2, 0), new Vector2D( 1, 0), new Vector2D( 2, 0), new Vector2D( 0,-1) }, // 0 => 2
                    new Vector2D[6] { new Vector2D(0, 0), new Vector2D( 0,-1), new Vector2D( 0,-2), new Vector2D( 0, 1), new Vector2D( 0, 2), new Vector2D(-1, 0) }, // R => L
                    new Vector2D[6] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 2, 0), new Vector2D(-1, 0), new Vector2D(-2, 0), new Vector2D( 0, 1) }, // 2 => 0
                    new Vector2D[6] { new Vector2D(0, 0), new Vector2D( 0,-1), new Vector2D( 0,-2), new Vector2D( 0, 1), new Vector2D( 0, 2), new Vector2D( 1, 0) }, // L => R
                }
                #endregion
                #region Corner detection
                , null, null // TODO
                #endregion
                );
            #endregion
            #region J
            J = new Piece(
                #region States
                new bool[4][][]{
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, //
                        new bool[5] { false, true,  false, false, false }, // []####
                        new bool[5] { false, true,  true,  true,  false }, // [][][]
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, false, false, false, false }, //
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, //
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, false, false, false }, //
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, //
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, true,  true,  true,  false }, // [][][]
                        new bool[5] { false, false, false, true,  false }, // ####[]
                        new bool[5] { false, false, false, false, false }, //
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, // 
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, true,  true,  false, false }, // [][]##
                        new bool[5] { false, false, false, false, false }, // 
                        },
                },
            #endregion
                #region Kicks
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1,  1), new Vector2D(0, -2), new Vector2D(-1, -2) }, // 0 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1, -1), new Vector2D(0,  2), new Vector2D( 1,  2) }, // R => 2
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // 2 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // L => 0
                },
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // 0 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1, -1), new Vector2D(0,  2), new Vector2D( 1,  2) }, // R => 0
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1,  1), new Vector2D(0, -2), new Vector2D(-1, -2) }, // 2 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1, -1), new Vector2D(0,  2), new Vector2D(-1,  2) }, // L => 2
                },
                new Vector2D[4][]
                {                    // 1                 2                      3                   4                       5                  6                   7                   8                  9                 10                  11                   12                      13
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 0,  1), new Vector2D( 1,  0), new Vector2D( 2,  0), new Vector2D( 1, -1), new Vector2D( 2, -1), new Vector2D(-1,  0), new Vector2D(-2,  0), new Vector2D(-1, -1), new Vector2D(-2, -1), new Vector2D( 0, -1), new Vector2D( 3,  0), new Vector2D(-3,  0) },  // 0 => 2
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 1,  0), new Vector2D( 0, -1), new Vector2D( 0, -2), new Vector2D(-1, -1), new Vector2D(-1, -2), new Vector2D( 0,  1), new Vector2D( 0,  2), new Vector2D(-1,  1), new Vector2D(-1,  2), new Vector2D( 1,  0), new Vector2D( 0,  3), new Vector2D( 0,  3) },  // R => L
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 0, -1), new Vector2D(-1,  0), new Vector2D(-2,  0), new Vector2D(-1,  1), new Vector2D(-2,  1), new Vector2D( 1,  0), new Vector2D( 2,  0), new Vector2D( 1,  1), new Vector2D( 2,  1), new Vector2D( 0,  1), new Vector2D(-3,  0), new Vector2D( 3,  0) },  // 2 => 0
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D(-1,  0), new Vector2D( 0,  1), new Vector2D( 0,  2), new Vector2D( 1,  1), new Vector2D( 1, -2), new Vector2D( 0, -1), new Vector2D( 0,  2), new Vector2D( 1,  1), new Vector2D( 1,  2), new Vector2D(-1,  0), new Vector2D( 0,  3), new Vector2D( 0,  3) },  // L => R
                }
                #endregion
                #region Corner detection
                , null, null // TODO
                #endregion
                );
            #endregion
            #region T
            T = new Piece(
                #region States
                new bool[4][][]{
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, //
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, true,  true,  true,  false }, // [][][]
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, false, false, false, false }, //
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, //
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, true,  true,  false }, // ##[][]
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, false, false, false }, //
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, //
                        new bool[5] { false, false, false, false, false }, // ######
                        new bool[5] { false, true,  true,  true,  false }, // [][][]
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, false, false, false }, //
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false }, //
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, true,  true,  false, false }, // [][]##
                        new bool[5] { false, false, true,  false, false }, // ##[]##
                        new bool[5] { false, false, false, false, false }, //
                        },
                },
            #endregion
                #region Kicks
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1,  1), new Vector2D(0, -2), new Vector2D(-1, -2) }, // 0 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1, -1), new Vector2D(0,  2), new Vector2D( 1,  2) }, // R => 2
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // 2 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // L => 0
                },
                new Vector2D[4][]
                {
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1,  1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // 0 => L
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D( 1, 0), new Vector2D( 1, -1), new Vector2D(0,  2), new Vector2D( 1,  2) }, // R => 0
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1,  1), new Vector2D(0, -2), new Vector2D(-1, -2) }, // 2 => R
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1, -1), new Vector2D(0,  2), new Vector2D(-1,  2) }, // L => 2
                },
                new Vector2D[4][]
                {                    // 1                 2                      3                   4                       5                  6                   7                   8                  9                 10                  11                   12                      13
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 0,  1), new Vector2D( 1,  0), new Vector2D( 2,  0), new Vector2D( 1, -1), new Vector2D( 2, -1), new Vector2D(-1,  0), new Vector2D(-2,  0), new Vector2D(-1, -1), new Vector2D(-2, -1), new Vector2D( 0, -1), new Vector2D( 3,  0), new Vector2D(-3,  0) },  // 0 => 2
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 1,  0), new Vector2D( 0, -1), new Vector2D( 0, -2), new Vector2D(-1, -1), new Vector2D(-1, -2), new Vector2D( 0,  1), new Vector2D( 0,  2), new Vector2D(-1,  1), new Vector2D(-1,  2), new Vector2D( 1,  0), new Vector2D( 0,  3), new Vector2D( 0,  3) },  // R => L
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D( 0, -1), new Vector2D(-1,  0), new Vector2D(-2,  0), new Vector2D(-1,  1), new Vector2D(-2,  1), new Vector2D( 1,  0), new Vector2D( 2,  0), new Vector2D( 1,  1), new Vector2D( 2,  1), new Vector2D( 0,  1), new Vector2D(-3,  0), new Vector2D( 3,  0) },  // 2 => 0
                    new Vector2D[13] { new Vector2D(0, 0), new Vector2D(-1,  0), new Vector2D( 0,  1), new Vector2D( 0,  2), new Vector2D( 1,  1), new Vector2D( 1, -2), new Vector2D( 0, -1), new Vector2D( 0,  2), new Vector2D( 1,  1), new Vector2D( 1,  2), new Vector2D(-1,  0), new Vector2D( 0,  3), new Vector2D( 0,  3) },  // L => R
                }
                #endregion
                #region Corner detection
                , null, null // TODO
                #endregion
                );
            #endregion
            #region None
            None = new Piece(
                new bool[4][][]{
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        },
                    new bool[5][] {
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        new bool[5] { false, false, false, false, false },
                        },
                },
                new Vector2D[4][]
                {
                    new Vector2D[1] { new Vector2D(0, 0) },
                    new Vector2D[1] { new Vector2D(0, 0) },
                    new Vector2D[1] { new Vector2D(0, 0) },
                    new Vector2D[1] { new Vector2D(0, 0) },
                },
                new Vector2D[4][]
                {
                    new Vector2D[1] { new Vector2D(0, 0) },
                    new Vector2D[1] { new Vector2D(0, 0) },
                    new Vector2D[1] { new Vector2D(0, 0) },
                    new Vector2D[1] { new Vector2D(0, 0) },
                },
                new Vector2D[4][]
                {
                    new Vector2D[1] { new Vector2D(0, 0) },
                    new Vector2D[1] { new Vector2D(0, 0) },
                    new Vector2D[1] { new Vector2D(0, 0) },
                    new Vector2D[1] { new Vector2D(0, 0) },
                },
                new Vector2D[1] { new Vector2D(0, 0) },
                new Vector2D[1] { new Vector2D(0, 0) }
                );
            #endregion
            #endregion
        }
        public static Piece GetPiece(char pieceChar)
        {
            switch(pieceChar)
            {
                case 'Z':
                    return Z;
                case 'L':
                    return L;
                case 'O':
                    return O;
                case 'S':
                    return S;
                case 'I':
                    return I;
                case 'J':
                    return J;
                case 'T':
                    return T;
                default:
                    return None;
            }
        }
    }
    public class Levels
    {
        public double g;
        public int lockDelay;
        public int invisibleTimer;
        public int are;
        public int lineAre;
        public bool pieceInvisible;
        public bool hold;
        public bool nextInvisible;
        public int das;
        public int arr;
        public static Levels[] list;
        public Levels(double gravity, int lockDelay, int invisibleTimer, int are, int lineAre, bool pieceInvisible, bool hold, bool nextInvisible, int das, int arr)
        {
            g = gravity;
            this.lockDelay = lockDelay;
            this.invisibleTimer = invisibleTimer;
            this.are = are;
            this.lineAre = lineAre;
            this.pieceInvisible = pieceInvisible;
            this.hold = hold;
            this.nextInvisible = nextInvisible;
            this.das = Math.Min(Controls.das, das);
            this.arr = Math.Min(Controls.arr, arr);
        }
        public static void Setup()
        {
            list = new Levels[80];
            // TODO: make other levels when different mode
            for (int i = 0; i < 80; i++)
            {
                if(i < 20)
                {
                    list[i] = new Levels(0.001 * (1 / Math.Pow(0.8 - (i * 0.007), i)), 30, -1, 10, 20, false, true, false, 20, 15);
                } // Actual Gravity Increase
                else if(i < 26)
                {                     // G    lockdelay  invis        are                   line are
                    list[i] = new Levels(21, -3 * i + 87, -1, (int)(0.2 * -i + 14), (int)(0.4 * -i + 28), false, true, false, (int)(0.6 * (-3 * i + 87)), (int)(0.6 * (-3 * i + 87)));
                } // 20G, Lock delay and ARE decrease
                else if(i < 32)
                {
                    list[i] = new Levels(21, (int)(-0.5 * i + 24.5), -1, (int)(0.2 * -i + 14), (int)(0.4 * -i + 28), false, true, false, (int)(0.5 * (-0.5 * i + 24.5)), (int)(0.5 * (-0.5 * i + 24.5)));
                } // Lock delay decreases slower
                else if(i < 45)
                {
                    list[i] = new Levels(21, (int)(-0.2 * i + 15), -1, (int)(0.2 * -i + 14), (int)(0.4 * -i + 28), false, true, false, (int)(0.5 * (-0.2 * i + 15)), (int)(0.5 * (-0.2 * i + 15)));
                } // Lock delay decreases even slower
                else if(i < 60)
                {
                    list[i] = new Levels(21, 6, -20 * i + 1200, (int)(0.2 * -i + 14), (int)(0.4 * -i + 28), false, true, false, 3, 3);
                } // Lock delay fixed at 6, pieces disappear slowly and gets faster
                else if(i < 65)
                {
                    list[i] = new Levels(21, 6, 0, 2, 4, false, false, false, 3, 3);
                } // Invisible board, holdless
                else if(i < 70)
                {
                    list[i] = new Levels(21, 6, 0, 2, 4, true, false, false, 3, 3);
                } // Invisible board and piece, holdless
                else if(i < 80)
                {
                    list[i] = new Levels(21, 6, 0, 2, 4, true, false, true, 3, 3);
                } // Blind tetris simulator
                // Get GM grade
            }
        }
    }
    public static class Controls
    {
        #region Controls
        public static int left;
        public static int right;
        public static int hardDrop;
        public static int softDrop;
        public static int rotCw;
        public static int rotCcw;
        public static int rot180;
        public static int hold;
        public static int das;
        public static int arr;
        public static bool useSonicDrop;
        public static int[] buttons
        {
            get
            {
                return new int[] { left, right, hardDrop, softDrop, rotCw, rotCcw, rot180, hold };
            }
        }
        public readonly static int retry = 82;
        public readonly static int pause = 27;
        #endregion
        /// <summary>
        /// Previous frame presses. Index (0 - 7): L, R, HD, SD, CW, CCW, 180, Hold
        /// </summary>
        public static bool[] prevFramePresses;
        public static void Setup()
        {
            prevFramePresses = new bool[8];
        }
        public static void SaveFramePresses()
        {
            for (int i = 0; i < 8; i++)
            {
                prevFramePresses[i] = NativeKeyboard.IsKeyDown(buttons[i]);
            }
        }
        public static void LoadControls()
        {
            string dataLoc = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!File.Exists(dataLoc + "\\Retroblocks\\config.txt"))
            {
                ResetControls();
            }
            else
            {

                StreamReader reader = new StreamReader(dataLoc + "\\Retroblocks\\config.txt");
                try
                {
                    #region Get controls
                    left = Convert.ToInt32(reader.ReadLine());
                    right = Convert.ToInt32(reader.ReadLine());
                    hardDrop = Convert.ToInt32(reader.ReadLine());
                    softDrop = Convert.ToInt32(reader.ReadLine());
                    rotCw = Convert.ToInt32(reader.ReadLine());
                    rotCcw = Convert.ToInt32(reader.ReadLine());
                    rot180 = Convert.ToInt32(reader.ReadLine());
                    hold = Convert.ToInt32(reader.ReadLine());
                    das = Convert.ToInt32(reader.ReadLine());
                    arr = Convert.ToInt32(reader.ReadLine());
                    useSonicDrop = Convert.ToBoolean(reader.ReadLine());
                    reader.Close();
                    // Checks for duplicates
                    if (buttons.GroupBy(x => x).Any(g => g.Count() > 1))
                    {
                        ResetControls();
                    }
                    #endregion
                }
                catch
                {
                    reader.Close();
                    ResetControls();
                }
            }
        }
        public static void SaveControls()
        {
            #region Check for duplicates
            if (buttons.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                ResetControls();
            }
            #endregion
            string dataLoc = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            #region Check for file
            if (!File.Exists(dataLoc + "\\Retroblocks\\config.txt"))
            {
                if (!Directory.Exists(dataLoc + "\\Retroblocks"))
                {
                    Directory.CreateDirectory(dataLoc + "\\Retroblocks");
                }
                File.Create(dataLoc + "\\Retroblocks\\config.txt");
            }
            #endregion
            #region Save
            StreamWriter writer = new StreamWriter(dataLoc + "\\Retroblocks\\config.txt");
            writer.Write($"{left}\n{right}\n{hardDrop}\n{softDrop}\n{rotCw}\n{rotCcw}\n{rot180}\n{das}\n{arr}\n{useSonicDrop}");
            writer.Close();
            #endregion
        }
        private static void ResetControls()
        {
            left = 37;
            right = 39;
            hardDrop = 32;
            softDrop = 40;
            rotCw = 38;
            rotCcw = 90;
            rot180 = 88;
            hold = 67;
            das = 10;
            arr = 1;
            useSonicDrop = false;
            SaveControls();
        }

    }
}
#region Keyboard integration
/// <summary>
/// Provides keyboard access.
/// </summary>
internal static class NativeKeyboard
{
    /// <summary>
    /// A positional bit flag indicating the part of a key state denoting
    /// key pressed.
    /// </summary>
    private const int KeyPressed = 0x8000;

    /// <summary>
    /// Returns a value indicating if a given key is pressed.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>
    /// <c>true</c> if the key is pressed, otherwise <c>false</c>.
    /// </returns>
    public static bool IsKeyDown(int keycode)
    {
        return (GetKeyState(keycode) & KeyPressed) != 0;
    }

    /// <summary>
    /// Gets the key state of a key.
    /// </summary>
    /// <param name="key">Virtuak-key code for key.</param>
    /// <returns>The state of the key.</returns>
    [DllImport("user32.dll")]
    private static extern short GetKeyState(int key);
}
#endregion
public static class HighScores
{
    public static int[] Scores;
    public static void LoadScores()
    {
        Scores = new int[5];
        string dataLoc = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!File.Exists(dataLoc + "\\Retroblocks\\scores.txt"))
        {
            if (!Directory.Exists(dataLoc + "\\Retroblocks"))
            {
                Directory.CreateDirectory(dataLoc + "\\Retroblocks");
            }
            ResetScores();
        }
        else
        {
            StreamReader reader = new StreamReader(dataLoc + "\\Retroblocks\\scores.txt");
            try
            {
                for(int i = 0; i < 5; i++)
                {
                    Scores[i] = Convert.ToInt32(reader.ReadLine());
                }
                reader.Close();
                Array.Sort(Scores); // lowest to highest
                Array.Reverse(Scores); // highest to lowest
            }
            catch
            {
                ResetScores();
            }
        }
        if(Scores[0] == 0)
        {
            ResetScores();
        }
    }
    public static void SaveScore(int score)
    {
        string dataLoc = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            #region Get score list
        Array.Sort(Scores);
        Array.Reverse(Scores);
        if(score > Scores[5])
        {
            int[] newScores = new int[6];
            for(int i = 0; i < 5; i++)
            {
                newScores[i] = Scores[i];
            }
            score = newScores[5];
            Array.Sort(newScores);
            Array.Reverse(newScores);
            for(int i = 0; i < 5; i++)
            {
                Scores[i] = newScores[i];
            }
            #endregion
            #region Check for directory
            if (!Directory.Exists(dataLoc + "\\Retroblocks"))
            {
                Directory.CreateDirectory(dataLoc + "\\Retroblocks");
            }
            #endregion
            #region Save
            StreamWriter writer = new StreamWriter(File.Create(dataLoc + "\\Retroblocks\\scores.txt"));
            writer.Write($"{Scores[0]}\n{Scores[1]}\n{Scores[2]}\n{Scores[3]}\n{Scores[4]}\n");
            writer.Close();
            #endregion
        }
    }
    private static void SaveScores()
    {
        string dataLoc = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        #region Check for directory
        if (!Directory.Exists(dataLoc + "\\Retroblocks"))
        {
            Directory.CreateDirectory(dataLoc + "\\Retroblocks");
        }
        #endregion
        #region Save
        StreamWriter writer = new StreamWriter(File.Create(dataLoc + "\\Retroblocks\\scores.txt"));
        writer.Write($"{Scores[0]}\n{Scores[1]}\n{Scores[2]}\n{Scores[3]}\n{Scores[4]}\n");
        writer.Close();
        #endregion

    }
    private static void ResetScores()
    {
        Scores = new int[5] { 10000, 8000, 5000, 4000, 2000 };
        SaveScores();
    }
}
public class Vector2D
{
    public int x;
    public int y;
    public Vector2D(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}