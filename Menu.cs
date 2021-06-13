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
                    $"Controls: \n" +
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
                    case 1:
                        if (NativeKeyboard.IsKeyDown(39) && Controls.das < 20 && !prevFrameInputs[39])
                        {
                            Controls.das++;
                        }
                        if(NativeKeyboard.IsKeyDown(37) && Controls.das > 1 && !prevFrameInputs[37])
                        {
                            Controls.das--;
                        }
                        if(NativeKeyboard.IsKeyDown(40) && !prevFrameInputs[40])
                        {
                            cursorPos++;
                            break;
                        }
                        if(NativeKeyboard.IsKeyDown(38) && !prevFrameInputs[38])
                        {
                            cursorPos--;
                            break;
                        }
                }
                for(int i = 0; i < prevFrameInputs.Length; i++)
                {
                    prevFrameInputs[i] = NativeKeyboard.IsKeyDown(i);
                }
            }
            start = false;
        }
    }
}