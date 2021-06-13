using System;
using System.Timers;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;

namespace Game
{
    // Starter
    public class Program
    {
        public static bool paused = false;
        private static Timer frameTimer;
        const uint ENABLE_QUICK_EDIT = 0x0040;
        const int STD_INPUT_HANDLE = -10;
        static void Main()
        {
            #region Setup console
            DisableQuickEdit(null, null);
            Console.CursorVisible = false;
            #endregion
            #region Main Menu
            Menu.Main.Start();
            #endregion
            #region Setup game
            BagRandomizer.Setup();
            Matrix.Setup();
            CurrentPiece.Setup();
            CurrentPiece.LockPiece(null, null);
            CurrentPiece.UpdatePiece();
            HoldPiece.Setup();
            Drawer.Setup();

            #endregion
            #region Run game
            frameTimer = new Timer(16);
            frameTimer.Elapsed += CurrentPiece.UpdatePiece;
            frameTimer.AutoReset = true;
            frameTimer.Enabled = true;
            while (true)
            {
                Drawer.DrawToConsole();
            }
            #endregion
        }
        #region Disable Selecting Text
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
    }
    // Inactive, just stores data
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
    // Stores data and logic for current piece and level
    public static class CurrentPiece
    {
        public static bool[][] state; // board state = state[x][y]
        // 0x = left
        // 0y = bottom
        public static bool[][] ghost;
        public static bool[][] nextPieceSpawn;
        public static char piece;
        public static int piecenum;
        public static int xoffset;
        public static int yoffset;
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
        private static Timer gravTimer;
        private static Timer lockDelayTimer;
        public static int lines;
        public static int lockDelayResets;
        public static bool useSonicDrop;
        public static int levelNum {
            get
            {
                return (int)Math.Floor(lines / 10d);
            }
        }
        public static void UpdatePiece(object o, ElapsedEventArgs _)
        {
            UpdatePiece();
        }
        public static void UpdatePiece()
        {
            if (landed && (lockDelayTimer == null || !lockDelayTimer.Enabled))
            {
                lockDelayTimer = new Timer(level.lockDelay * 16.6666666666666);
                lockDelayTimer.Elapsed += LockPiece;
                lockDelayTimer.Enabled = true;
            }
        }
        public static void Setup()
        {
            state = new bool[10][];
            for (int i = 0; i < state.Length; i++)
            {
                state[i] = new bool[40];
            }
            ghost = new bool[10][];
            for (int i = 0; i < ghost.Length; i++)
            {
                ghost[i] = new bool[40];
            }
            nextPieceSpawn = new bool[10][];
            for (int i = 0; i < nextPieceSpawn.Length; i++)
            {
                nextPieceSpawn[i] = new bool[40];
            }
            piece = BagRandomizer.output[BagRandomizer.current][0];
            piecenum = 0;
            Controls.das = 8;
            Controls.arr = 0;
            leftDasTimer = Controls.das;
            rightDasTimer = Controls.das;
            Spawn();
            gravTimer = new Timer(1 / level.g * 16.6666);
            gravTimer.AutoReset = true;
            gravTimer.Elapsed += Fall;
        }
        public static void NextPiece()
        {
            bool newBag = false;
            if (piecenum == 6)
            {
                newBag = false;
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
            }
        }
        public static void LockPiece(object o, ElapsedEventArgs _)
        {
            while(!landed)
            {
                Fall(null, null);
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
            Spawn();
        }
        public static void Spawn()
        {
            for(int x = 0; x < 5; x++)
            {
                for(int y = 0; y < 5; y++)
                {
                    state[x + 2][-y + 23] = Piece.GetPiece(BagRandomizer.output[BagRandomizer.current][piecenum])[y][x];
                }
            }
            NextPiece();
            UpdateGhost();
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    nextPieceSpawn[x + 2][-y + 23] = Piece.GetPiece(BagRandomizer.output[BagRandomizer.current][piecenum])[y][x];
                }
            }
        }
        public static void Left()
        {
            UpdateGhost();
        }
        public static void Right()
        {
            UpdateGhost();
        }
        public static void RotCW()
        {
            UpdateGhost();
            // TODO
        }
        public static void RotCCW()
        {
            UpdateGhost();
            // TODO
        }
        public static void Rot180()
        {
            UpdateGhost();
            // TODO
        }
        public static void DasLeft(object o, ElapsedEventArgs _)
        {
            if(Controls.arr == 0)
            {
                Left();
                Left();
                Left();
                Left();
                Left();
                Left();
                Left();
                Left();
            }
            else
            {
                Left();
                leftDasTimer = Controls.arr;
            }
        }
        public static void DasRight(object o, ElapsedEventArgs _)
        {
            if (Controls.arr == 0)
            {
                Right();
                Right();
                Right();
                Right();
                Right();
                Right();
                Right();
                Right();
            }
            else
            {
                Right();
                rightDasTimer = Controls.arr;
            }
        }
        public static void CancelLeftDas()
        {
            leftDasTimer = Controls.das;
        }
        public static void CancelRightDas()
        {
            rightDasTimer = Controls.das;
        }
        /// <summary>
        /// Get info of a future piece, up to 7 pieces
        /// </summary>
        /// <param name="intoFuture">How many pieces into the future. 1 = next piece. Min = 1, Max = 7, inclusively.</param>
        /// <returns></returns>
        public static bool[][] GetFuturePiece(int intoFuture)
        {
            if(intoFuture < 1 || intoFuture > 7)
            {
                throw new ArgumentOutOfRangeException(intoFuture < 1 ? $"Minimum value is 1, got {intoFuture}" : $"Maximum value is 7, got {intoFuture}");
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
            if(landed)
            {
                lockDelayTimer = new Timer(level.lockDelay * 16.6666666666666);
                lockDelayTimer.Elapsed += LockPiece;
                lockDelayTimer.Enabled = true;
                lockDelayResets++;
                if (lockDelayResets > 75)
                {
                    LockPiece(null, null);
                }
            }
            else
            {
                lockDelayTimer = new Timer(level.lockDelay * 16.6666666666666);
                lockDelayTimer.Elapsed += LockPiece;
                lockDelayTimer.Enabled = false;
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
            while (!IsLanded(ghost))
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

                for (int y = 0; y < 40; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        newState[x][Math.Max(y - 1, 0)] = ghost[x][y];
                    }
                }
                ghost = newState;
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
        public static void Unpause()
        {
            CancelLeftDas();
            CancelRightDas();
        }
        public static void ClearLine()
        {
            if (level.g < 1)
            {
                gravTimer.Interval = 1 / level.g * 16.6666;
            }
            else
            {
                gravTimer.Enabled = false;
            }
        }
    }
    // Draws to window
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
                        if (Matrix.state[x][y] || CurrentPiece.state[x][y])
                        {
                            xcache += "[]";
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
               $"HOLD        |                    |    NEXT\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n" +
               $"            |                    |\n"
                );
            Console.ForegroundColor = ConsoleColor.Red;
            for(int i = 0; i < 4; i++)
            {
                Console.SetCursorPosition(12, i);
                Console.Write("|                    |");
            }
            DrawToConsole();
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
            bool[][] _ = Piece.GetPiece(HoldPiece.current);
            Console.Write(GetMinos(_[1]));
            Console.SetCursorPosition(0, 2);
            Console.Write(GetMinos(_[2]));
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
    // Produces a random sequence of tetriminos using the 7bag method.
    static class BagRandomizer
    {
        public static string[] output = new string[2];
        public static int current = 0;
        public static int next = 1;
        /// <returns>Index of output that is supposed to be the current one</returns>
        public static int GetNew()
        {
            output[next] = GetSeq();
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
    // Stores data about hold piece
    static class HoldPiece
    {
        public static char current;
        public static void Setup()
        {
            current = 'N';
        }
    }
    static class Piece
    {
        public readonly static bool[][] Z = new bool[5][]
        {
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, true,  true,  false, false }, //  [][]# 
            new bool[5] { false, false, true,  true,  false }, //  # [][]# 
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, false, false, false, false }, //
        };
        public readonly static bool[][] L = new bool[5][]
        {
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, false, false, true,  false }, //  # # []
            new bool[5] { false, true,  true,  true,  false }, //  [][][]#
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, false, false, false, false }, //
        };
        public readonly static bool[][] O = new bool[5][]
        {
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, false, true,  true,  false }, //  # [][]
            new bool[5] { false, false, true,  true,  false }, //  # [][]#
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, false, false, false, false }, //
        };
        public readonly static bool[][] S = new bool[5][]
        {
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, false, true,  true,  false }, //   #[][]
            new bool[5] { false, true,  true,  false, false }, //  [][]# #
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, false, false, false, false }, //
        };
        public readonly static bool[][] I = new bool[5][]
        {
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, false, false, false, false }, //  # # #
            new bool[5] { false, true,  true,  true,  true  }, //  [][][][]
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, false, false, false, false }, //
        };
        public readonly static bool[][] J = new bool[5][]
        {
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, true,  false, false, false }, //  []# #
            new bool[5] { false, true,  true,  true,  false }, //  [][][]#
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, false, false, false, false }, //
        };
        public readonly static bool[][] T = new bool[5][]
        {
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, false, true,  false, false }, //  # []#
            new bool[5] { false, true,  true,  true,  false }, //  [][][]#
            new bool[5] { false, false, false, false, false }, //
            new bool[5] { false, false, false, false, false }, //
        };
        public readonly static bool[][] None = new bool[5][]
        {
            new bool[5] { false, false, false, false, false },
            new bool[5] { false, false, false, false, false },
            new bool[5] { false, false, false, false, false },
            new bool[5] { false, false, false, false, false },
            new bool[5] { false, false, false, false, false },
        };
        public static bool[][] GetPiece(char pieceChar)
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
                case 'N':
                    return None;
                default:
                    return None;
            }
        }
    }
    public class Levels
    {
        public double g;
        public int lockDelay;
        public static Levels[] list = new Levels[70]
        {
            // Initially start with D grade
            new Levels(0.01, 30), // 1
            new Levels(0.02, 30),
            new Levels(0.05, 30),
            new Levels(0.08, 30),
            new Levels(0.1 , 30),
            new Levels(0.15, 30),
            new Levels(0.2 , 30), // Grant C- grade
            new Levels(0.3 , 30),
            new Levels(0.4 , 30),
            new Levels(0.5 , 30), // 10 - 666ms + 500ms <Grant C grade>
            new Levels(0.75, 30),
            new Levels(0.9 , 30),
            new Levels(1   , 30),
            new Levels(2   , 30), // Grant C+ grade
            new Levels(4   , 30),
            new Levels(7   , 30),
            new Levels(10  , 30), // Grant B- grade
            new Levels(14  , 30),
            new Levels(19  , 30),
            new Levels(22  , 30), // 20 - 500ms (2/s) // Grant B grade
            new Levels(22  , 29),
            new Levels(22  , 28), // Grant B+ grade
            new Levels(22  , 27),
            new Levels(22  , 26), // Grant A- grade
            new Levels(22  , 25),
            new Levels(22  , 24), // Grant A grade
            new Levels(22  , 23),
            new Levels(22  , 22), // Grant A+ grade
            new Levels(22  , 21),
            new Levels(22  , 20), // M10 (30) 333ms (3/s) <Grant S- grade>
            new Levels(22  , 20),
            new Levels(22  , 19), // Grant S grade
            new Levels(22  , 19),
            new Levels(22  , 18), // Grant S+ grade
            new Levels(22  , 18),
            new Levels(22  , 17), // Grant SS- grade
            new Levels(22  , 17),
            new Levels(22  , 16), // Grant SS grade
            new Levels(22  , 16),
            new Levels(22  , 15), // M20 (40) 250ms (4/s) <Grant SS+ grade>
            new Levels(22  , 15),
            new Levels(22  , 15), // Grant SSS- grade
            new Levels(22  , 14),
            new Levels(22  , 14), // Grant SSS grade
            new Levels(22  , 13),
            new Levels(22  , 13), // Grant SSS+ grade
            new Levels(22  , 12),
            new Levels(22  , 12),
            new Levels(22  , 11),
            new Levels(22  , 11), // M30 (50) 176ms (5.4/s) <Grant U- grade>
            new Levels(22  , 11),
            new Levels(22  , 10),
            new Levels(22  , 10),
            new Levels(22  , 10),
            new Levels(22  , 10),
            new Levels(22  ,  9), // Grant U grade
            new Levels(22  ,  9),
            new Levels(22  ,  9),
            new Levels(22  ,  9),
            new Levels(22  ,  8), // M40 (60) 128ms (7.8/s) <Grant U+ grade>
            new Levels(22  ,  8),
            new Levels(22  ,  7),
            new Levels(22  ,  7),
            new Levels(22  ,  7),
            new Levels(22  ,  6),
            new Levels(22  ,  6),
            new Levels(22  ,  6),
            new Levels(22  ,  5), // Grant Master grade
            new Levels(22  ,  5),
            new Levels(22  ,  5), // M50 (70)
            // If time > 15 minutes, start invisible board challenge <less than ~1.9pps> <Master-A> <Master-S if complete>
            // Else if time > 12 minutes, start invisible holdless challenge <~1.9pps> <Master-S grade> <Master-SS if complete>
            // Else if time < 12 minutes, start previewless invisible holdless challenge <~2.4pps> <Master-X grade> <Master-M if complete> <Grand Master if done 5+ tetrises>
        };
        public Levels(double gravity, int lockdelay)
        {
            g = gravity;
            lockDelay = lockdelay;
        }
    }
    public static class Controls
    {
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

        public static void LoadControls()
        {
            if (!File.Exists("%appdata%\\Retroblocks\\config.txt"))
            {
                ResetControls();
            }
            else
            {
               
                try
                {
                    #region Get controls
                    StreamReader reader = new StreamReader("%appdata%\\Retroblocks\\config.txt");
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

                    // Checks for duplicates
                    if (buttons.GroupBy(x => x).Any(g => g.Count() > 1))
                    {
                        ResetControls();
                    }
                    #endregion
                }
                catch
                {
                    ResetControls();
                }
            }
        }
        public static void SaveControls()
        {
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
            StreamWriter writer = new StreamWriter("%appdata%\\Retroblocks\\config.txt");
            writer.Write($"{left}\n{right}\n{hardDrop}\n{softDrop}\n{rotCw}\n{rotCcw}\n{rot180}\n{das}\n{arr}");
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
                Array.Sort(Scores); // lowest to highest
                Array.Reverse(Scores); // highest to lowest
            }
            catch
            {
                ResetScores();
            }
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