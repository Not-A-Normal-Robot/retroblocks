using System;
using Game;

namespace Menu
{
    public class Main
    {
        static int cursorPos;
        const int maxCursorPos = 11;
        static bool start;
        static bool[] prevFrameInputs;
        public static void Start()
        {
            start = false;
            prevFrameInputs = new bool[100];
            cursorPos = 0;
            HighScores.LoadScores();
            Controls.LoadControls();
            while (!start)
            {
                #region Draw screen
                Console.SetCursorPosition(0, 0);
                Console.Write(
                    $"===================\n" +
                    $"||  RETROBLOCKS  ||\n" +
                    $"===================\n" +
                    $"{(cursorPos == 0 ? "> " : "  ")}Start\n" +
                    $"Handling: \n"+
                    $"{(cursorPos == 1 ? "> " : "  ")}DAS: {Controls.das} - Default: 10\n" +
                    $"{(cursorPos == 2 ? "> " : "  ")}ARR: {Controls.arr} - Default: 1\n" +
                    $"{(cursorPos == 3 ? "> " : "  ")}Soft Drop Mode: {(Controls.useSonicDrop ? "instant" : "normal")} - Default: normal\n" +
                    $"Controls: \n" +  //  from left: 234567890123456789012345678
                    $"{(cursorPos == 4 ? "> " : "  ")}Left: {Controls.left} - Default: Left Arrow (37)\n" +
                    $"{(cursorPos == 5 ? "> " : "  ")}Right: {Controls.right} - Default: Right Arrow (39)\n" +
                    $"{(cursorPos == 6 ? "> " : "  ")}Hard Drop: {Controls.hardDrop} - Default: Space (32)\n" +
                    $"{(cursorPos == 7 ? "> " : "  ")}Soft Drop: {Controls.softDrop} - Default: Down Arrow (40)\n" +
                    $"{(cursorPos == 8 ? "> " : "  ")}Rotate Clockwise: {Controls.rotCw} - Default: Up Arrow (38)\n" +
                    $"{(cursorPos == 9 ? "> " : "  ")}Rotate Counterclockwise: {Controls.rotCcw} - Default: Z (90)\n" +
                    $"{(cursorPos == 10 ? "> " : "  ")}Rotate 180: {Controls.rot180} - Default: X (88)\n" +
                    $"{(cursorPos == 11 ? "> " : "  ")}Hold: {Controls.hold} - Default: C (67)\n" +
                    $"Scores:\n" +
                    $"{HighScores.Scores[0]}\n" +
                    $"{HighScores.Scores[1]}\n" +
                    $"{HighScores.Scores[2]}\n" +
                    $"{HighScores.Scores[3]}\n" +
                    $"{HighScores.Scores[4]}\n"
                    );
                #endregion
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
                        }
                        break;
                    #endregion
                    case 1:
                        #region DAS
                        if (NativeKeyboard.IsKeyDown(39) && Controls.das < 20 && !prevFrameInputs[39])
                        {
                            Controls.das++;
                            Controls.SaveControls();
                            Console.Clear();
                        }
                        if (NativeKeyboard.IsKeyDown(37) && Controls.das > 1 && !prevFrameInputs[37])
                        {
                            Controls.das--;
                            Controls.SaveControls();
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
                    case 2:
                        #region ARR
                        if (NativeKeyboard.IsKeyDown(39) && Controls.arr < 15 && !prevFrameInputs[39])
                        {
                            Controls.arr++;
                            Controls.SaveControls();
                            Console.Clear();
                        }
                        if (NativeKeyboard.IsKeyDown(37) && Controls.arr > 0 && !prevFrameInputs[37])
                        {
                            Controls.arr--;
                            Controls.SaveControls();
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
                        #region Sonic Drop
                        if (NativeKeyboard.IsKeyDown(39) && !prevFrameInputs[39])
                        {
                            Controls.useSonicDrop = !Controls.useSonicDrop;
                            Controls.SaveControls();
                            Console.Clear();
                        }
                        if (NativeKeyboard.IsKeyDown(37) && !prevFrameInputs[37])
                        {
                            Controls.useSonicDrop = !Controls.useSonicDrop;
                            Controls.SaveControls();
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
                            Console.SetCursorPosition(9, 9);
                            Console.Write("Awaiting input, press Esc to cancel - Default: Left Arrow (37)               ");
                            ConsoleKeyInfo key = Console.ReadKey();
                            if (key.Key != ConsoleKey.Escape)
                            {
                                Controls.left = (int)key.Key;
                                Controls.SaveControls();
                            }
                            Console.Clear();
                        }
                        break;
                    #endregion
                    case 5:
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
                            Console.SetCursorPosition(10, 10);
                            Console.Write("Awaiting input, press Esc to cancel - Default: Right Arrow (39)               ");
                            ConsoleKeyInfo key = Console.ReadKey();
                            if (key.Key != ConsoleKey.Escape)
                            {
                                Controls.right = (int)key.Key;
                                Controls.SaveControls();
                            }
                            Console.Clear();
                        }
                        break;
                    #endregion
                    case 6:
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
                            ConsoleKeyInfo key = Console.ReadKey();
                            if (key.Key != ConsoleKey.Escape)
                            {
                                Controls.hardDrop = (int)key.Key;
                                Controls.SaveControls();
                            }
                            Console.Clear();
                        }
                        break;
                    #endregion
                    case 7:
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
                            ConsoleKeyInfo key = Console.ReadKey();
                            if (key.Key != ConsoleKey.Escape)
                            {
                                Controls.softDrop = (int)key.Key;
                                Controls.SaveControls();
                            }
                            Console.Clear();
                        }
                        break;
                    #endregion
                    case 8:
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
                            Console.SetCursorPosition(20, 13);
                            Console.Write("Awaiting input, press Esc to cancel - Default: Up Arrow (38)               ");
                            ConsoleKeyInfo key = Console.ReadKey();
                            if (key.Key != ConsoleKey.Escape)
                            {
                                Controls.rotCw = (int)key.Key;
                                Controls.SaveControls();
                            }
                            Console.Clear();
                        }
                        break;
                    #endregion
                    case 9:
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
                            Console.SetCursorPosition(27, 14);
                            Console.Write("Awaiting input, press Esc to cancel - Default: Z (90)               ");
                            ConsoleKeyInfo key = Console.ReadKey();
                            if (key.Key != ConsoleKey.Escape)
                            {
                                Controls.rotCcw = (int)key.Key;
                                Controls.SaveControls();
                            }
                            Console.Clear();
                        }
                        break;
                    #endregion
                    case 10:
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
                            Console.SetCursorPosition(15, 15);
                            Console.Write("Awaiting input, press Esc to cancel - Default: X (88)               ");
                            ConsoleKeyInfo key = Console.ReadKey();
                            if (key.Key != ConsoleKey.Escape)
                            {
                                Controls.rot180 = (int)key.Key;
                                Controls.SaveControls();
                            }
                            Console.Clear();
                        }
                        break;
                    #endregion
                    case 11:
                        #region Hold
                        if (NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                        {
                            cursorPos--;
                            break;
                        }
                        if (NativeKeyboard.IsKeyDown(13))
                        {
                            Console.SetCursorPosition(9, 16);
                            Console.Write("Awaiting input, press Esc to cancel - Default: C (67)               ");
                            ConsoleKeyInfo key = Console.ReadKey();
                            if (key.Key != ConsoleKey.Escape)
                            {
                                Controls.rot180 = (int)key.Key;
                                Controls.SaveControls();
                            }
                            Console.Clear();
                        }
                        break;
                        #endregion
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