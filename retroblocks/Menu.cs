using System;
using Game;

namespace Menu
{
    public class Main
    {
        const string credits =
            "Credits\n" +
            "\n" +
            "\n" +
            "Developer\n" +
            "\n" +
            "NOT_A_ROBOT\n" +
            "\n" +
            "\n" +
            "\n" +
            "SFX\n" +
            "SFXR by DrPetter" +
            "\n" +
            "\n" +
            "Press Enter to close";
        static int cursorPos;
        static bool start;
        static bool isInCredits;
        static bool[] prevFrameInputs;
        public static void Start()
        {
            start = false;
            prevFrameInputs = new bool[100];
            cursorPos = 0;
            HighScores.LoadScores();
            Config.LoadConfig();
            while (!start)
            {
                #region Draw screen
                Console.SetCursorPosition(0, 0);
                if (!isInCredits)
                {
                    Console.Write(
                        $"===================\n" +
                        $"||  RETROBLOCKS  ||\n" +
                        $"===================\n\n" +
                        $"{(cursorPos == 0 ? "> " : "  ")}Start!\n\n" +
                        $"Settings: \n" +
                        $"{(cursorPos == 1 ? "> " : "  ")}Font Size: {Config.fontSize} - Default: 10\n\n" +
                        $"Handling: \n" +
                        $"{(cursorPos == 2 ? "> " : "  ")}DAS: {Config.das} - Default: 10\n" +
                        $"{(cursorPos == 3 ? "> " : "  ")}ARR: {Config.arr} - Default: 1\n" +
                        $"{(cursorPos == 4 ? "> " : "  ")}Soft Drop Mode: {(Config.useSonicDrop ? "instant" : "normal")} - Default: normal\n\n" +
                        $"Controls: (Note: Setting up is glitchy :P)\n" +
                        $"{(cursorPos == 5 ? "> " : "  ")}Left: {Config.left} - Default: Left Arrow (37)\n" +
                        $"{(cursorPos == 6 ? "> " : "  ")}Right: {Config.right} - Default: Right Arrow (39)\n" +
                        $"{(cursorPos == 7 ? "> " : "  ")}Hard Drop: {Config.hardDrop} - Default: Space (32)\n" +
                        $"{(cursorPos == 8 ? "> " : "  ")}Soft Drop: {Config.softDrop} - Default: Down Arrow (40)\n" +
                        $"{(cursorPos == 9 ? "> " : "  ")}Rotate Clockwise: {Config.rotCw} - Default: Up Arrow (38)\n" +
                        $"{(cursorPos == 10 ? "> " : "  ")}Rotate Counterclockwise: {Config.rotCcw} - Default: Z (90)\n" +
                        $"{(cursorPos == 11 ? "> " : "  ")}Rotate 180: {Config.rot180} - Default: X (88)\n" +
                        $"{(cursorPos == 12 ? "> " : "  ")}Hold: {Config.hold} - Default: C (67)\n\n" +
                        $"{(cursorPos == 13 ? "> " : "  ")}Credits\n" +
                        $"Scores:\n" +
                        $"{HighScores.Scores[0]}\n" +
                        $"{HighScores.Scores[1]}\n" +
                        $"{HighScores.Scores[2]}\n" +
                        $"{HighScores.Scores[3]}\n" +
                        $"{HighScores.Scores[4]}\n"
                        );
                }
                else
                {
                    Console.Write(credits);
                }
                #endregion
                if (!isInCredits)
                {
                    switch (cursorPos)
                    {
                        case 0:
                            #region Start
                            if (NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                            {
                                cursorPos++;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(13))
                            {
                                start = true;
                                Console.Clear();
                            }
                            break;
                        #endregion
                        case 1:
                            #region Font Size
                            if (NativeKeyboard.IsKeyDown(39) && Config.fontSize < 36 && !prevFrameInputs[39])
                            {
                                Config.fontSize++;
                                ConsoleHelper.SetCurrentFont("Consolas",(short)Config.fontSize);
                                Config.SaveConfig();
                            }
                            if (NativeKeyboard.IsKeyDown(37) && Config.fontSize > 5 && !prevFrameInputs[37])
                            {
                                Config.fontSize--;
                                ConsoleHelper.SetCurrentFont("Consolas",(short)Config.fontSize);
                                Config.SaveConfig();
                            }
                            if (NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                            {
                                cursorPos++;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                            {
                                cursorPos--;
                                break;
                            }
                            break;
                            #endregion
                        case 2:
                            #region DAS
                            if (NativeKeyboard.IsKeyDown(39) && Config.das < 20 && !prevFrameInputs[39])
                            {
                                Config.das++;
                                Config.SaveConfig();
                                Console.Clear();
                            }
                            if (NativeKeyboard.IsKeyDown(37) && Config.das > 1 && !prevFrameInputs[37])
                            {
                                Config.das--;
                                Config.SaveConfig();
                                Console.Clear();
                            }
                            if (NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                            {
                                cursorPos++;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                            {
                                cursorPos--;
                                break;
                            }
                            break;
                        #endregion
                        case 3:
                            #region ARR
                            if (NativeKeyboard.IsKeyDown(39) && Config.arr < 15 && !prevFrameInputs[39])
                            {
                                Config.arr++;
                                Config.SaveConfig();
                                Console.Clear();
                            }
                            if (NativeKeyboard.IsKeyDown(37) && Config.arr > 0 && !prevFrameInputs[37])
                            {
                                Config.arr--;
                                Config.SaveConfig();
                                Console.Clear();
                            }
                            if (NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                            {
                                cursorPos++;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                            {
                                cursorPos--;
                                break;
                            }
                            break;
                        #endregion
                        case 4:
                            #region Sonic Drop
                            if (NativeKeyboard.IsKeyDown(39) && !prevFrameInputs[39])
                            {
                                Config.useSonicDrop = !Config.useSonicDrop;
                                Config.SaveConfig();
                                Console.Clear();
                            }
                            if (NativeKeyboard.IsKeyDown(37) && !prevFrameInputs[37])
                            {
                                Config.useSonicDrop = !Config.useSonicDrop;
                                Config.SaveConfig();
                                Console.Clear();
                            }
                            if (NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                            {
                                cursorPos++;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                            {
                                cursorPos--;
                                break;
                            }
                            break;
                        #endregion
                        case 5:
                            #region Left
                            if (NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                            {
                                cursorPos++;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                            {
                                cursorPos--;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(13))
                            {
                                Console.SetCursorPosition(8, 9);
                                Console.Write("Awaiting input, press Esc to cancel - Default: Left Arrow (37)               ");
                                System.Threading.Thread.Sleep(250);
                                ConsoleKeyInfo key = Console.ReadKey();
                                while (key.Key == ConsoleKey.Enter)
                                {
                                    key = Console.ReadKey();
                                }
                                if (key.Key != ConsoleKey.Escape)
                                {
                                    Config.left = (int)key.Key;
                                    Config.SaveConfig();
                                }
                                Console.Clear();
                            }
                            break;
                        #endregion
                        case 6:
                            #region Right
                            if (NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                            {
                                cursorPos++;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                            {
                                cursorPos--;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(13))
                            {
                                Console.SetCursorPosition(9, 10);
                                Console.Write("Awaiting input, press Esc to cancel - Default: Right Arrow (39)               ");
                                System.Threading.Thread.Sleep(250);
                                ConsoleKeyInfo key = Console.ReadKey();
                                while (key.Key == ConsoleKey.Enter)
                                {
                                    key = Console.ReadKey();
                                }
                                if (key.Key != ConsoleKey.Escape)
                                {
                                    Config.right = (int)key.Key;
                                    Config.SaveConfig();
                                }
                                Console.Clear();
                            }
                            break;
                        #endregion
                        case 7:
                            #region Hard Drop
                            if (NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                            {
                                cursorPos++;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                            {
                                cursorPos--;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(13))
                            {
                                Console.SetCursorPosition(13, 11);
                                Console.Write("Awaiting input, press Esc to cancel - Default: Space (32)               ");
                                System.Threading.Thread.Sleep(250);
                                ConsoleKeyInfo key = Console.ReadKey();
                                while (key.Key == ConsoleKey.Enter)
                                {
                                    key = Console.ReadKey();
                                }
                                if (key.Key != ConsoleKey.Escape)
                                {
                                    Config.hardDrop = (int)key.Key;
                                    Config.SaveConfig();
                                }
                                Console.Clear();
                            }
                            break;
                        #endregion
                        case 8:
                            #region Soft Drop
                            if (NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                            {
                                cursorPos++;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                            {
                                cursorPos--;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(13))
                            {
                                Console.SetCursorPosition(13, 12);
                                Console.Write("Awaiting input, press Esc to cancel - Default: Down Arrow (40)               ");
                                System.Threading.Thread.Sleep(250);
                                ConsoleKeyInfo key = Console.ReadKey();
                                while (key.Key == ConsoleKey.Enter)
                                {
                                    key = Console.ReadKey();
                                }
                                if (key.Key != ConsoleKey.Escape)
                                {
                                    Config.softDrop = (int)key.Key;
                                    Config.SaveConfig();
                                }
                                Console.Clear();
                            }
                            break;
                        #endregion
                        case 9:
                            #region Rot cw
                            if (NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                            {
                                cursorPos++;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                            {
                                cursorPos--;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(13))
                            {
                                Console.SetCursorPosition(19, 13);
                                Console.Write("Awaiting input, press Esc to cancel - Default: Up Arrow (38)               ");
                                System.Threading.Thread.Sleep(250);
                                ConsoleKeyInfo key = Console.ReadKey();
                                while (key.Key == ConsoleKey.Enter)
                                {
                                    key = Console.ReadKey();
                                }
                                if (key.Key != ConsoleKey.Escape)
                                {
                                    Config.rotCw = (int)key.Key;
                                    Config.SaveConfig();
                                }
                                Console.Clear();
                            }
                            break;
                        #endregion
                        case 10:
                            #region Rot ccw
                            if (NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                            {
                                cursorPos++;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                            {
                                cursorPos--;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(13))
                            {
                                Console.SetCursorPosition(26, 14);
                                Console.Write("Awaiting input, press Esc to cancel - Default: Z (90)               ");
                                System.Threading.Thread.Sleep(250);
                                ConsoleKeyInfo key = Console.ReadKey();
                                while (key.Key == ConsoleKey.Enter)
                                {
                                    key = Console.ReadKey();
                                }
                                if (key.Key != ConsoleKey.Escape)
                                {
                                    Config.rotCcw = (int)key.Key;
                                    Config.SaveConfig();
                                }
                                Console.Clear();
                            }
                            break;
                        #endregion
                        case 11:
                            #region Rot 180
                            if (NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                            {
                                cursorPos++;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                            {
                                cursorPos--;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(13))
                            {
                                Console.SetCursorPosition(14, 15);
                                Console.Write("Awaiting input, press Esc to cancel - Default: X (88)               ");
                                System.Threading.Thread.Sleep(250);
                                ConsoleKeyInfo key = Console.ReadKey();
                                while (key.Key == ConsoleKey.Enter)
                                {
                                    key = Console.ReadKey();
                                }
                                if (key.Key != ConsoleKey.Escape)
                                {
                                    Config.rot180 = (int)key.Key;
                                    Config.SaveConfig();
                                }
                                Console.Clear();
                            }
                            break;
                        #endregion
                        case 12:
                            #region Hold
                            if (NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                            {
                                cursorPos++;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                            {
                                cursorPos--;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(13))
                            {
                                Console.SetCursorPosition(8, 16);
                                Console.Write("Awaiting input, press Esc to cancel - Default: C (67)               ");
                                System.Threading.Thread.Sleep(250);
                                ConsoleKeyInfo key = Console.ReadKey();
                                while (key.Key == ConsoleKey.Enter)
                                {
                                    key = Console.ReadKey();
                                }
                                if (key.Key != ConsoleKey.Escape)
                                {
                                    Config.rot180 = (int)key.Key;
                                    Config.SaveConfig();
                                }
                                Console.Clear();
                            }
                            break;
                        #endregion
                        case 13:
                            #region Credits
                            if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                            {
                                cursorPos--;
                                break;
                            }
                            if (NativeKeyboard.IsKeyDown(13) && !prevFrameInputs[13])
                            {
                                isInCredits = true;
                                Console.Clear();
                            }
                            break;
                            #endregion
                    }
                }
                else
                {
                    if(NativeKeyboard.IsKeyDown(13) && !prevFrameInputs[13])
                    {
                        isInCredits = false;
                        Console.Clear();
                    }
                }
                
                for (int i = 0; i < prevFrameInputs.Length; i++)
                {
                    prevFrameInputs[i] = NativeKeyboard.IsKeyDown(i);
                }
            }
            start = false;
        }
    }
}