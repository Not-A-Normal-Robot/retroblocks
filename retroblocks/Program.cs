using System;
using System.Timers;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Media;
using System.Diagnostics;

namespace Game
{
    // Starter
    public class Program
    {
        public static bool paused = false;
        private static Timer frameTimer;
        private static Timer secondTimer;
        public static string mode;
        public static bool firstRun = true;
        public static int tps { get; private set; }
        public static int fps { get; private set; }
        private static int framesThisSecond;
        public static bool toppedOut;
        private static bool clearedConsole = false;
        private static int ranking;
        static void Main()
        {
            #region Setup console
            DisableQuickEdit(null, null);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Loading Retroblocks\nPlease wait...");
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Green;
            Config.LoadConfig();
            ConsoleHelper.SetCurrentFont("Consolas", (short)Config.fontSize);
            Console.SetWindowSize(50, 32);
            Console.SetBufferSize(50, 32);
            DisableResize();
            #endregion
            while (true)
            {
                #region Main Menu
                Sounds.Setup();
                Menu.Main.Start();
                Console.Clear();
                tps = 0;
                fps = 0;
                framesThisSecond = 0;
                #endregion

                while (true)
                {
                    #region Setup game
                    paused = false;
                    Levels.Setup();
                    Piece.Setup();
                    BagRandomizer.Setup();
                    Matrix.Setup();
                    CurrentPiece.Setup();
                    HoldPiece.Setup();
                    Drawer.Setup(true);
                    CurrentPiece.Spawn();
                    #endregion
                    #region Run game

                    if (firstRun)
                    {
                        frameTimer = new Timer(16);
                        frameTimer.Elapsed += CurrentPiece.UpdateTimers;
                        frameTimer.Elapsed += CurrentPiece.UpdateGravity;
                        frameTimer.Elapsed += CurrentPiece.UpdateLines;
                        frameTimer.Elapsed += CurrentPiece.MovePiece;
                        frameTimer.Elapsed += CurrentPiece.SpinPiece;
                        frameTimer.Elapsed += CurrentPiece.DasPiece;
                        frameTimer.AutoReset = true;
                        frameTimer.Enabled = true;
                        secondTimer = new Timer(1000);
                        secondTimer.Elapsed += UpdateCounters;
                        secondTimer.AutoReset = true;
                        secondTimer.Enabled = true;
                    }
                    frameTimer.Enabled = true;
                    while (true)
                    {
                        
                        Console.CursorVisible = false;
                        if (!toppedOut)
                        {
                            if (!paused)
                            {
                                framesThisSecond++;
                            }
                            Drawer.DrawToConsole();
                            if (frameTimer.Enabled == paused) { frameTimer.Enabled = !paused; }
                            bool p = NativeKeyboard.IsKeyDown(Config.pause);
                            if (p && Config.prevFramePresses[8] == false)
                            {
                                paused = !paused;
                                Config.prevFramePresses[8] = true;
                            }
                            else if ((!p) && Config.prevFramePresses[8] == true)
                            {
                                Config.prevFramePresses[8] = false;
                            }

                            if (NativeKeyboard.IsKeyDown(Config.retry))
                            {
                                firstRun = false;
                                HighScores.SaveScore(CurrentPiece.score);
                                break;
                            }
                        }
                        else
                        {
                            if (!clearedConsole)
                            {
                                Console.Clear();
                                clearedConsole = true;
                                ranking = HighScores.SaveScore(CurrentPiece.score);
                            }
                            frameTimer.Enabled = false;
                            System.Threading.Thread.Sleep(20);
                            Console.SetCursorPosition(0, 0);
                            Console.Write(
                                $"TOP OUT\n" +
                                $"\n" +
                                $"Score: {CurrentPiece.score}\n" +
                                $"Ranking: {ranking}\n" +
                                $"\n" +
                                $"\n" +
                                $"Lines: {CurrentPiece.lines}\n" +
                                $"Level: {CurrentPiece.levelNum}\n" +
                                $"{CurrentPiece.level.g}G {CurrentPiece.level.lockDelay}LD\n" +
                                $"{CurrentPiece.level.invisibleTimer} invTimer\n" +
                                $"{Config.das} --> {CurrentPiece.level.das} DAS\n" +
                                $"{Config.arr} --> {CurrentPiece.level.arr} ARR\n" +
                                $"\n" +
                                $"\n" +
                                $"- Press R to retry\n" +
                                $"- Press Esc to return to main menu"
                                );
                            if (NativeKeyboard.IsKeyDown(Config.retry))
                            {
                                Console.Clear();
                                toppedOut = false;
                                clearedConsole = false;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(Config.pause))
                            {
                                Console.Clear();
                                clearedConsole = false;
                                break;
                            }
                        }

                    }
                    if (toppedOut)
                    {
                        toppedOut = false;
                        break;
                    }
                    #endregion
                }
            }
        }
        static void UpdateCounters(object o, ElapsedEventArgs _)
        {
            tps = CurrentPiece.ticksThisSecond;
            CurrentPiece.ticksThisSecond = 0;
            fps = framesThisSecond;
            framesThisSecond = 0;
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
        #region Application focus checker
        /// <summary>Returns true if the current application has focus, false otherwise</summary>
        public static bool IsApplicationFocused()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        #endregion
    }
    public static class Matrix
    {
        public static bool[][] state; // board state = state[x][y]
        // 0x = left
        // 0y = bottom
        /// <summary>
        /// Each block increases by 1 every tick. Gets reset to 0 when block placed.
        /// </summary>
        public static int[][] invisTimer;
        public static void Setup()
        {
            state = new bool[10][];
            invisTimer = new int[10][];
            for (int i = 0; i < state.Length; i++)
            {
                state[i] = new bool[40];
                invisTimer[i] = new int[40];
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
                for (int y = 0; y < 40; y++)
                {
                    for (int x = 0; x < 10; x++)
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
                if (areTimer > -1 || lineAreTimer > -1)
                {
                    return true;
                }
                return false;
            }
        }
        private static int leftDasTimer;
        private static int rightDasTimer;
        public static Levels level { get { return Levels.list[levelNum]; } }
        public static int lockDelayTimer;
        public static int lines;
        public static int lockDelayResets;
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
        public static int ticksThisSecond;
        public static bool btb;
        public static int combo;
        public static string lastClear;
        public static bool rotated;
        // prevent IRS and rotation at the same time
        private static bool triedCw;
        private static bool triedCcw;
        private static bool tried180;
        private static int spawnTries;
        private static bool retryHold;
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
            else if (landed)
            {
                lockDelayTimer++;
                if (lockDelayTimer > level.lockDelay)
                {
                    LockPiece(null, null);
                }
                leftoverG = 0;
            }
            else
            {
                lockDelayTimer = 0;
            }
            if (areTimer > level.are)
            {
                Spawn();
            }
            ticksThisSecond++;
            for(int x = 0; x < Matrix.invisTimer.Length; x++)
            {
                for(int y = 0; y < Matrix.invisTimer[x].Length; y++)
                {
                    Matrix.invisTimer[x][y] += 1;
                }
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
                                    Matrix.invisTimer[x][y] = 0;
                                }
                                else
                                {
                                    Matrix.state[x][y - i] = Matrix.state[x][y + 1 - i];
                                    Matrix.invisTimer[x][y - i] = Matrix.invisTimer[x][y + 1 - i];
                                }
                            }
                        }
                    }
                }
                lineAreTimer = -1;
                lined = new bool[40];
            } // blocks above cleared line fall

            if (Array.IndexOf(lined, true) > -1 && lineAreTimer == -1)
            {
                lineAreTimer = 0;
                int l = lined.Count(b => b);
                switch (l)
                {
                    case 1:
                        lastClear = "Single   ";
                        break;
                    case 2:
                        lastClear = "Double   ";
                        break;
                    case 3:
                        lastClear = "Triple   ";
                        break;
                    case 4:
                        lastClear = "Quadruple";
                        break;
                }
            }
        }
        public static void MovePiece(object o, ElapsedEventArgs _)
        {
            // Left
            if (NativeKeyboard.IsKeyDown(Config.left) && Program.paused == false)
            {
                leftDasTimer++;
                if (areTimer < 0)
                {
                    if (!Config.prevFramePresses[0])
                    {
                        rightDasTimer = 0;
                        Left();
                    }
                }
            }
            else if (Program.paused == false) { leftDasTimer = 0; }

            // Right
            if (NativeKeyboard.IsKeyDown(Config.right) && Program.paused == false)
            {
                rightDasTimer++;
                if (!Config.prevFramePresses[1])
                {
                    leftDasTimer = 0;
                    Right();
                }
            }
            else if (Program.paused == false) { rightDasTimer = 0; }

            // Hard drop
            if (NativeKeyboard.IsKeyDown(Config.hardDrop) && !Config.prevFramePresses[2] && areTimer == -1 && Program.paused == false)
            {
                LockPiece(null, null);
            }

            // Soft drop
            if (NativeKeyboard.IsKeyDown(Config.softDrop) && landed == false && Program.paused == false)
            {
                Fall(null, null);
                score += 1;
                while (!landed && Config.useSonicDrop && areTimer == -1)
                {
                    Fall(null, null);
                    score += 1;
                }
            }

            Config.SaveFramePresses1();
        }
        public static void SpinPiece(object o, ElapsedEventArgs _)
        {
            // Clockwise
            if (NativeKeyboard.IsKeyDown(Config.rotCw) && !Config.prevFramePresses[4] && areTimer == -1 && Program.paused == false)
            {
                if (rotated || rotState == 0 || triedCw)
                {
                    RotCW();
                }
                else
                {
                    triedCw = true;
                }
            }

            // Counterclockwise
            if (NativeKeyboard.IsKeyDown(Config.rotCcw) && !Config.prevFramePresses[5] && areTimer == -1 && Program.paused == false)
            {
                if (rotated || rotState == 0 || triedCcw)
                {
                    RotCCW();
                }
                else
                {
                    triedCcw = true;
                }
            }

            // 180 rotation
            if (NativeKeyboard.IsKeyDown(Config.rot180) && !Config.prevFramePresses[6] && areTimer == -1 && Program.paused == false)
            {
                if (rotated || rotState == 0 || tried180)
                {
                    Rot180();
                }
                else
                {
                    tried180 = true;
                }
            }

            // Hold
            if (NativeKeyboard.IsKeyDown(Config.hold) && !Config.prevFramePresses[7] && areTimer < 0 && Program.paused == false)
            {
                retryHold = !Hold();
            }
            else if (retryHold)
            {
                retryHold = !Hold();
            }

            if (Program.paused == false)
            {
                Config.SaveFramePresses2();
            }
        }
        public static void DasPiece(object o, ElapsedEventArgs _)
        {

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
            rotState = 0;
            leftDasTimer = Config.das;
            rightDasTimer = Config.das;
            lockDelayTimer = 0;
            score = 0;
            leftoverG = 0d;
            areTimer = -1;
            lineAreTimer = -1;
            ticksThisSecond = 0;
            Config.Setup();
            combo = -1;
            lines = 0;
            lockDelayResets = 0;
            btb = false;
            lastClear = "         ";
            rotated = false;
            triedCw = false;
            triedCcw = false;
            tried180 = false;
            spawnTries = 0;
        }
        public static void NextPiece()
        {
            bool newBag = false;
            piece = BagRandomizer.output[BagRandomizer.current][piecenum];
            if (piecenum == 6)
            {
                newBag = true;
                BagRandomizer.GetNew();
                piecenum = 0;
            }
            if (!newBag)
            {
                piecenum++;
            }
        }
        public static void Fall(object o, ElapsedEventArgs _)
        {
            if (!landed)
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
                for (int y = 0; y < 39; y++)
                {
                    for (int x = 0; x < 10; x++)
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
            for (int i = 0; i < 40 && !landed; i++)
            {
                Fall(null, null);
                score += 2;
            }
            for (int y = 0; y < 40; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (state[x][y])
                    {
                        Matrix.state[x][y] = true;
                        state[x][y] = false;
                        Matrix.invisTimer[x][y] = 0;
                    }
                }
            }
            int linesCleared = 0;
            for (int y = 0; y < 40; y++)
            {
                if (
                #region Check for line
                    Matrix.state[0][y] && Matrix.state[1][y] && Matrix.state[2][y] && Matrix.state[3][y] && Matrix.state[4][y] && Matrix.state[5][y] && Matrix.state[6][y] && Matrix.state[7][y] && Matrix.state[8][y] && Matrix.state[9][y]
                #endregion
                    )
                {
                    linesCleared++;
                    lined[y] = true;
                    for (int e = 0; e < 10; e++)
                    {
                        Matrix.state[e][y] = false;
                    }
                }
            }
            switch (linesCleared)
            {
                case 0:
                    lineAreTimer = -1;
                    combo = -1;
                    lastClear = "         ";
                    break;
                case 1:
                    lines++;
                    score += (int)Math.Floor(lines / 10d) * 100;
                    lineAreTimer = 0;
                    btb = false;
                    combo++;
                    lastClear = "Single   ";
                    Sounds.lineClear1.Play();
                    break;
                case 2:
                    lines += 2;
                    score += (int)Math.Floor(lines / 10d) * 300;
                    lineAreTimer = 0;
                    btb = false;
                    combo++;
                    lastClear = "Double   ";
                    Sounds.lineClear2.Play();
                    break;
                case 3:
                    lines += 3;
                    score += (int)Math.Floor(lines / 10d) * 500;
                    lineAreTimer = 0;
                    btb = false;
                    combo++;
                    lastClear = "Triple   ";
                    Sounds.lineClear3.Play();
                    break;
                case 4:
                    lines += 4;
                    if (btb)
                    {
                        score += (int)Math.Floor(lines / 10d) * 1200;
                    }
                    else
                    {
                        score += (int)Math.Floor(lines / 10d) * 800;
                    }
                    lineAreTimer = 0;
                    btb = true;
                    combo++;
                    lastClear = "Quadruple";
                    Sounds.lineClear4.Play();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("5+ line clear!");
            }
            if (combo > 0)
            {
                score += combo * levelNum;
            }
            areTimer = 0;
            leftoverG = 0;
        }
        public static void Spawn()
        {
            state = new bool[10][] { new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], };
            xoffset = 2;
            yoffset = 19;
            rotated = false;
            triedCw = false;
            triedCcw = false;
            tried180 = false;
            rotState = 0;
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    state[x + 2][-y + 23] = Piece.GetPiece(BagRandomizer.output[BagRandomizer.current][piecenum]).piece[rotState][y][x];
                }
            }

            if(spawnTries == 0)
            {
                NextPiece();
            }
            if (NativeKeyboard.IsKeyDown(Config.rotCcw)) { RotCCW(); Config.prevFramePresses[5] = true; }
            if (NativeKeyboard.IsKeyDown(Config.rot180)) { Rot180(); Config.prevFramePresses[6] = true; }
            if (NativeKeyboard.IsKeyDown(Config.rotCw)) { RotCW(); Config.prevFramePresses[4] = true; }


            

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 40; y++)
                {
                    if (spawnTries >= 60)
                    {
                        Sounds.topOut.Play();
                        Console.Clear();
                        Program.toppedOut = true;
                        return;
                    }
                    if (state[x][y] && Matrix.state[x][y])
                    {
                        state = new bool[10][] { new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], };
                        spawnTries++;
                        if (NativeKeyboard.IsKeyDown(Config.hold)) { if (Hold()) { areTimer = -1; } else { retryHold = true; spawnTries--; } }
                        return;
                    }
                }
                if(spawnTries >= 60)
                {
                    return;
                }
            }
            if (spawnTries >= 60)
            {
                return;
            }
            spawnTries = 0;
            areTimer = -1;

            UpdateGhost();
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    nextPieceSpawn[x + 2][-y + 23] = Piece.GetPiece(BagRandomizer.output[BagRandomizer.current][piecenum]).piece[0][y][x];
                }
            }
            lockDelayResets = 0;

            if (NativeKeyboard.IsKeyDown(Config.hold)) { if (Hold()) { areTimer = -1; } else { retryHold = true; } }
            if (NativeKeyboard.IsKeyDown(Config.hardDrop) && !Config.prevFramePresses[2]) { LockPiece(null, null); }
            leftoverG = 0;
            lockDelayTimer = 0;

        }
        public static void Left()
        {
            if (Array.IndexOf(state[0], true) == -1)
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
                        if (x == 9)
                        {
                            shifted[x][y] = false;
                        }
                        else
                        {
                            shifted[x][y] = state[x + 1][y];
                        }
                        if (shifted[x][y] && Matrix.state[x][y])
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
                if (areTimer < 0)
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
                if (areTimer < 0)
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
            Vector2D kickUsed = new Vector2D(0, 0);
            for (int i = 0; i < p.cwKicks[0].Length; i++)
            {
                kickUsed = p.cwKicks[rotState][i];
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
                if (landed)
                {
                    ResetLockDelay();
                }
                CurrentPiece.rotated = true;
                xoffset += kickUsed.x;
                yoffset += kickUsed.y;
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
            Vector2D kickUsed = new Vector2D(0, 0);
            for (int i = 0; i < p.ccwKicks[0].Length; i++)
            {
                kickUsed = p.ccwKicks[rotState][i];
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
                if (landed)
                {
                    ResetLockDelay();
                }
                CurrentPiece.rotated = true;
                xoffset += kickUsed.x;
                yoffset += kickUsed.y;
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
            Vector2D kickUsed = new Vector2D(0, 0);
            for (int i = 0; i < p.flipKicks[0].Length; i++)
            {
                kickUsed = p.flipKicks[rotState][i];
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
                if (landed)
                {
                    ResetLockDelay();
                }
                CurrentPiece.rotated = true;
                xoffset += kickUsed.x;
                yoffset += kickUsed.y;
            }
        }
        /// <summary>
        /// Hold.
        /// </summary>
        /// <returns>Success state (false = unsuccessful)</returns>
        public static bool Hold()
        {

            if (HoldPiece.current == 'N')
            {
                HoldPiece.current = piece;
                state = new bool[10][] { new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40] };

                xoffset = 2;
                yoffset = 19;
                rotState = 0;
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        state[x + 2][-y + 23] = Piece.GetPiece(BagRandomizer.output[BagRandomizer.current][piecenum]).piece[rotState][y][x];
                    }
                }

                if (spawnTries == 0)
                {
                    NextPiece();
                }
                if (NativeKeyboard.IsKeyDown(Config.rotCcw)) { RotCCW(); Config.prevFramePresses[5] = true; }
                if (NativeKeyboard.IsKeyDown(Config.rot180)) { Rot180(); Config.prevFramePresses[6] = true; }
                if (NativeKeyboard.IsKeyDown(Config.rotCw)) { RotCW(); Config.prevFramePresses[4] = true; }

                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 40; y++)
                    {
                        if (spawnTries >= 60)
                        {
                            Program.toppedOut = true;
                        }
                        if (state[x][y] && Matrix.state[x][y])
                        {
                            state = new bool[10][] { new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], };
                            spawnTries++;
                            return false;
                        }
                    }
                }
                spawnTries = 0;

                UpdateGhost();
                if (NativeKeyboard.IsKeyDown(Config.hardDrop) && !Config.prevFramePresses[2]) { LockPiece(null, null); }
            }
            else if (HoldPiece.used == false)
            {
                char heldPiece = HoldPiece.current;
                HoldPiece.used = true;
                state = new bool[10][] { new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40] };
                xoffset = 2;
                yoffset = 19;
                rotState = 0;
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        state[x + 2][-y + 23] = Piece.GetPiece(HoldPiece.current).piece[rotState][y][x];
                    }
                }
                HoldPiece.current = piece;
                piece = heldPiece;
                
            if (NativeKeyboard.IsKeyDown(Config.rotCcw)) { RotCCW(); Config.prevFramePresses[5] = true; }
            if (NativeKeyboard.IsKeyDown(Config.rot180)) { Rot180(); Config.prevFramePresses[6] = true; }
            if (NativeKeyboard.IsKeyDown(Config.rotCw)) { RotCW(); Config.prevFramePresses[4] = true; }

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 40; y++)
                {
                    if (spawnTries >= 60)
                    {
                        Program.toppedOut = true;
                    }
                    if (state[x][y] && Matrix.state[x][y])
                    {
                        state = new bool[10][] { new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], new bool[40], };
                        spawnTries++;
                        return false;
                    }
                }
            }
            spawnTries = 0;
                UpdateGhost();

                if (NativeKeyboard.IsKeyDown(Config.hardDrop) && !Config.prevFramePresses[2]) { LockPiece(null, null); }
            }
            return true;
        }
        /// <summary>
        /// Get info of a future piece, up to 7 pieces
        /// </summary>
        /// <param name="intoFuture">How many pieces into the future. 0 = next piece. Min = 0, Max = 7, inclusively.</param>
        /// <returns></returns>
        public static Piece GetFuturePiece(int intoFuture)
        {
            if (intoFuture < 0 || intoFuture > 7)
            {
                throw new ArgumentOutOfRangeException(intoFuture < 0 ? $"Minimum value is 1, got {intoFuture}" : $"Maximum value is 7, got {intoFuture}");
            }
            if (piecenum + intoFuture > 6)
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
            if (landed && lockDelayResets < 30 && areTimer < 0 && lineAreTimer < 0)
            {
                lockDelayTimer = 0;
                lockDelayResets++;
            }
        }
        public static void UpdateGhost()
        {
            for (int y = 0; y < 40; y++)
            {
                for (int x = 0; x < 10; x++)
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
        public static bool IsLanded(bool[][] _state)
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
    }
    static class Drawer
    {
        private static string[] Picture
        {
            get
            {
                string[] ycache = new string[24];
                string xcache = "";
                for (int y = 0; y < 24; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        if (Matrix.state[x][y])
                        {

                            if (CurrentPiece.level.invisibleTimer == -1 || Matrix.invisTimer[x][y] < 0.25 * CurrentPiece.level.invisibleTimer)
                            {
                                xcache += "██";
                            }
                            else if (Matrix.invisTimer[x][y] >= 1 * CurrentPiece.level.invisibleTimer)
                            {
                                if (CurrentPiece.nextPieceSpawn[x][y])
                                {
                                    xcache += "XX";
                                }
                                else { xcache += "  "; }
                            }
                            else if (Matrix.invisTimer[x][y] >= 0.75 * CurrentPiece.level.invisibleTimer)
                            {
                                xcache += "░░";
                            }
                            else if (Matrix.invisTimer[x][y] >= 0.5 * CurrentPiece.level.invisibleTimer)
                            {
                                xcache += "▒▒";
                            }
                            else if (Matrix.invisTimer[x][y] >= 0.25 * CurrentPiece.level.invisibleTimer)
                            {
                                xcache += "▓▓";
                            }
                        }
                        else if (CurrentPiece.state[x][y])
                        {
                            if (CurrentPiece.lockDelayTimer < 0.25 * CurrentPiece.level.lockDelay)
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
                        else    {xcache += "  ";}
                    }
                    ycache[y] = xcache;
                    xcache = "";
                }
                return ycache;
            }
        }
        private static bool prevFramePauseState = false;
        public static void Setup(bool doStartAnimation)
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
            for (int i = 0; i < 4; i++)
            {
                Console.SetCursorPosition(12, i);
                Console.Write("|                    |");
            }
            DrawToConsole();
            #region Starting Animation
            if (doStartAnimation)
            {

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
            }
            #endregion
        }
        public static void DrawToConsole()
        {
            Console.SetCursorPosition(0, 0);
            if (!Program.paused && !prevFramePauseState)
            {
                #region Color
                string[] Picture = Drawer.Picture;
                for (int i = 0, j = 23; i < 24; i++, j--)
                {
                    Console.SetCursorPosition(13, i);
                    if (j == 23)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else if (j == 19)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    Console.Write(Picture[j]);
                }
                #endregion
                DrawHoldPiece();
                DrawNextPieces();
                DrawProgressBar();
                DrawStats();
            }
            else if (!prevFramePauseState)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.Write($"[ ==========PAUSED========== ]\n" +
                    $"Press Esc to unpause\n" +
                    $"Press R to restart\n"
                    );
                prevFramePauseState = true;
            }
            else if(!Program.paused)
            {
                Console.Clear();
                prevFramePauseState = false;
                Setup(false);
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
        private static void DrawProgressBar()
        {

            //      * --> gravity
            //      = --> lock delay
            //      · --> are
            //      : --> line are

            Console.SetCursorPosition(12, 24);
            double p = !CurrentPiece.landed && CurrentPiece.level.g < 0.5 ? -CurrentPiece.leftoverG - 0.07 : CurrentPiece.areTimer == -1 ? -(double)CurrentPiece.lockDelayTimer / CurrentPiece.level.lockDelay : CurrentPiece.lineAreTimer == -1 ? (double)CurrentPiece.areTimer / -CurrentPiece.level.are : -(double)CurrentPiece.lineAreTimer / CurrentPiece.level.lineAre;
            int e = (int)(p * 22) + 22;
            if (!CurrentPiece.landed && CurrentPiece.level.g < 0.5 && CurrentPiece.areTimer == -1)
            {
                for (int i = 0; i < 22; i++)
                {
                    if (i > e)
                    {
                        Console.Write("=");
                    }
                    else
                    {
                        Console.Write("*");
                    }
                }

            }
            else if (CurrentPiece.areTimer == -1)
            {
                for (int i = 0; i < 22; i++)
                {
                    if (i > e)
                    {
                        Console.Write("·");
                    }
                    else
                    {
                        Console.Write("=");
                    }
                }
            }
            else if (CurrentPiece.lineAreTimer == -1 && CurrentPiece.level.g < 0.5)
            {
                for (int i = 0; i < 22; i++)
                {
                    if (i > e)
                    {
                        Console.Write("*");
                    }
                    else
                    {
                        Console.Write("·");
                    }
                }
            }
            else if (CurrentPiece.lineAreTimer == -1)
            {
                for (int i = 0; i < 22; i++)
                {
                    if (i > e)
                    {
                        Console.Write("=");
                    }
                    else
                    {
                        Console.Write("·");
                    }
                }
            }
            else
            {
                for (int i = 0; i < 22; i++)
                {
                    if (i > e)
                    {
                        Console.Write("·");
                    }
                    else
                    {
                        Console.Write(":");
                    }
                }
            }
            Console.SetCursorPosition(12, 25);
            int ldr = 30 - CurrentPiece.lockDelayResets;
            for (int i = 0; i < 15; i++)
            {
                if (ldr >= 2)
                {
                    Console.Write("″");
                }
                else if (ldr == 1)
                {
                    Console.Write("'");
                }
                else
                {
                    Console.Write(" ");
                }
                ldr -= 2;
            }
        }
        private static void DrawStats()
        {

            int l = CurrentPiece.lines - (CurrentPiece.levelNum * 10);
            string e = "";
            switch (l)
            {
                default:
                    e = "[··········]";
                    break;
                case 1:
                    e = "[=·········]";
                    break;
                case 2:
                    e = "[==········]";
                    break;
                case 3:
                    e = "[===·······]";
                    break;
                case 4:
                    e = "[====······]";
                    break;
                case 5:
                    e = "[=====·····]";
                    break;
                case 6:
                    e = "[======····]";
                    break;
                case 7:
                    e = "[=======···]";
                    break;
                case 8:
                    e = "[========··]";
                    break;
                case 9:
                    e = "[=========·]";
                    break;
            }
            Console.SetCursorPosition(0, 5);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{(CurrentPiece.btb ? "Back To Back" : "            ")}");
            Console.SetCursorPosition(0, 6);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{CurrentPiece.lastClear}");
            Console.SetCursorPosition(0, 8);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"Lvl {CurrentPiece.levelNum}\n{(int)(CurrentPiece.level.g * 100) / 100d}G   \n{CurrentPiece.level.lockDelay}LD     \n\n\n{CurrentPiece.lines}/{(CurrentPiece.levelNum + 1) * 10} lines\n{e}\n\nDelays:\n{CurrentPiece.level.are} Spawn \n{CurrentPiece.level.lineAre} Line \n\n\nScore\n{CurrentPiece.score}");
            Console.SetCursorPosition(0, 29);
            Console.Write($"{Program.fps} FPS, {Program.tps} / 60 TPS");
        }
        private static string GetMinos(bool[] minoTypes)
        {
            string _ = "";
            for (int i = 0; i < minoTypes.Length; i++)
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
            char[] a = new char[7] { 'Z','L','O','S','J','I','T' };
            int n = a.Length;
            Random r = new Random();
            while (n > 1)
            {
                int k = r.Next(n--);
                char temp = a[n];
                a[n] = a[k];
                a[k] = temp;
            }
            return new string(a);
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
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1, -1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // L => 0
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
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1, -1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // L => 0
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
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1, -1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // L => 0
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
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1, -1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // L => 0
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
                    new Vector2D[5] { new Vector2D(0, 0), new Vector2D(-1, 0), new Vector2D(-1, -1), new Vector2D(0, -2), new Vector2D( 1, -2) }, // L => 0
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
            this.das = Math.Min(Config.das, das);
            this.arr = Math.Min(Config.arr, arr);
        }
        public static void Setup()
        {
            list = new Levels[80];
            // TODO: make other levels when different mode
            for (int i = 0; i < 80; i++)
            {
                if(i < 20)
                {
                    list[i] = new Levels(Math.Min(0.01 * (1 / Math.Pow(0.8 - (i * 0.007), i)), 21), 30, -1, 10, 20, false, true, false, 20, 15);
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
                } // Blind blockstacking simulator
                // Get GM grade
            }
            // for(int i = 0; i < 80; i++)
            // {
            //     list[i] = new Levels(21,12,5,1,1,false,true,false,4,1);
            // }
        }
    }
    public static class Config
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
                return new int[8] { left, right, hardDrop, softDrop, rotCw, rotCcw, rot180, hold };
            }
        }
        public readonly static int retry = 82;
        public readonly static int pause = 27;
        #endregion
        public static int fontSize;
        /// <summary>
        /// Previous frame presses. Index (0 - 8 inclusively): L, R, HD, SD, CW, CCW, 180, Hold, Pause
        /// </summary>
        public static bool[] prevFramePresses;
        public static void Setup()
        {
            prevFramePresses = new bool[9];
        }
        public static void SaveFramePresses1()
        {
            for (int i = 0; i < 4; i++)
            {
                prevFramePresses[i] = NativeKeyboard.IsKeyDown(buttons[i]);
            }
        }
        public static void SaveFramePresses2()
        {
            for(int i = 4; i < 8; i++)
            {
                prevFramePresses[i] = NativeKeyboard.IsKeyDown(buttons[i]);
            }
        }
        public static void LoadConfig()
        {
            string dataLoc = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!File.Exists(dataLoc + "\\Retroblocks\\config.txt"))
            {
                ResetConfig();
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
                    fontSize = Convert.ToInt32(reader.ReadLine());
                    reader.Close();
                    // Checks for duplicates
                    if (buttons.Length != buttons.Distinct().Count())
                    {
                        ResetConfig();
                    }
                    #endregion
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    reader.Close();
                    ResetConfig();
                }
            }
        }
        public static void SaveConfig()
        {
            #region Check for duplicates
            if (buttons.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                ResetConfig();
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
                File.Create(dataLoc + "\\Retroblocks\\config.txt").Close();
            }
            #endregion
            #region Save
            StreamWriter writer = new StreamWriter(dataLoc + "\\Retroblocks\\config.txt");
            writer.Write($"{left}\n{right}\n{hardDrop}\n{softDrop}\n{rotCw}\n{rotCcw}\n{rot180}\n{hold}\n{das}\n{arr}\n{useSonicDrop}\n{fontSize}");
            writer.Close();
            #endregion
        }
        private static void ResetConfig()
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
            fontSize = 17;
            SaveConfig();
        }

    }
    public static class Sounds
    {
        public static SoundPlayer lineClear1;
        public static SoundPlayer lineClear2;
        public static SoundPlayer lineClear3;
        public static SoundPlayer lineClear4;
        public static SoundPlayer topOut;

        public static void Setup()
        {
            lineClear1 = new SoundPlayer(Environment.CurrentDirectory + "\\sfx\\lineclear1.wav");
            lineClear2 = new SoundPlayer(Environment.CurrentDirectory + "\\sfx\\lineclear2.wav");
            lineClear3 = new SoundPlayer(Environment.CurrentDirectory + "\\sfx\\lineclear3.wav");
            lineClear4 = new SoundPlayer(Environment.CurrentDirectory + "\\sfx\\lineclear4.wav");
            topOut     = new SoundPlayer(Environment.CurrentDirectory + "\\sfx\\topout.wav"    );
            lineClear1.LoadAsync();
            lineClear2.LoadAsync();
            lineClear3.LoadAsync();
            lineClear4.LoadAsync();
            topOut    .LoadAsync();
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
    /// <summary>
    /// Saves a score and returns the ranking.
    /// </summary>
    /// <param name="score">Score to save</param>
    /// <returns>Ranking of saved score</returns>
    public static int SaveScore(int score)
    {
        string dataLoc = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            #region Get score list
        Array.Sort(Scores);
        Array.Reverse(Scores);
        if(score > Scores[4])
        {
            int[] newScores = new int[6];
            for(int i = 0; i < 5; i++)
            {
                newScores[i] = Scores[i];
            }
            newScores[5] = score;
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
            return Array.IndexOf(Scores, score) + 1;
        }
        return 6;
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