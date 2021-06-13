using System;
using Game;

namespace Menu
{
    public class Main
    {
        static int cursorPos;
        const int maxCursorPos = 11;
        static bool start;
        public static void Start()
        {
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
                    $"{(cursorPos == 5 ? "> " : "  ")}Right: {Controls.right}\n" +
                    $"{(cursorPos == 6 ? "> " : "  ")}Hard Drop: {Controls.hardDrop}\n" +
                    $"{(cursorPos == 7 ? "> " : "  ")}Soft Drop: {Controls.softDrop}\n" +
                    $"{(cursorPos == 8 ? "> " : "  ")}Rotate Clockwise: {Controls.rotCw}\n" +
                    $"{(cursorPos == 9 ? "> " : "  ")}Rotate Counterclockwise: {Controls.rotCcw}\n" +
                    $"{(cursorPos == 10 ? "> " : "  ")}Rotate 180: {Controls.rot180}\n" +
                    $"{(cursorPos == 11 ? "> " : "  ")}Hold: {Controls.hold}\n" +
                    $"Scores:\n" +
                    $"{HighScores.Scores[0]}\n" +
                    $"{HighScores.Scores[1]}\n" +
                    $"{HighScores.Scores[2]}\n" +
                    $"{HighScores.Scores[3]}\n" +
                    $"{HighScores.Scores[4]}\n"
                    );
                #endregion

            }
        }
    }
}