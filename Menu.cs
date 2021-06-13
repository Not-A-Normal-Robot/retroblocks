using System;
using Game;

namespace Menu
{
    public class Main
    {
        public static int cursorPos;
        public static void Start()
        {
            cursorPos = 0;
            HighScores.LoadScores();
            Controls.LoadControls();
            Console.Write(
                $"===================\n" +
                $"||  RETROBLOCKS  ||\n" +
                $"===================\n" +
                $"{(cursorPos == 0 ? "> " : "  ")}Start\n" +
                $"{(cursorPos == 1 ? "> " : "  ")}DAS: {Controls.das}\n" +
                $"{(cursorPos == 2 ? "> " : "  ")}ARR: {Controls.arr}\n" +
                $"{(cursorPos == 3 ? "> " : "  ")}Soft Drop Mode: {(Controls.useSonicDrop ? "instant" : "normal")}\n" +
                $"Controls: \n"+
                $"{(cursorPos == 4 ? "> " : "  ")}Left: {Controls.left}\n" +
                $"{(cursorPos == 5 ? "> " : "  ")}Left: {Controls.right}\n" +
                $"{(cursorPos == 6 ? "> " : "  ")}Left: {Controls.hardDrop}\n" +
                $"{(cursorPos == 7 ? "> " : "  ")}Left: {Controls.softDrop}\n" +
                $"{(cursorPos == 8 ? "> " : "  ")}Left: {Controls.rotCw}\n" +
                $"{(cursorPos == 9 ? "> " : "  ")}Left: {Controls.rotCcw}\n" +
                $"{(cursorPos == 10? "> " : "  ")}Left: {Controls.rot180}\n" +
                $"{(cursorPos == 11? "> " : "  ")}Left: {Controls.hold}\n" +
                $"Scores:\n" +
                $"{HighScores.Scores[0]}\n" +
                $"{HighScores.Scores[1]}\n" +
                $"{HighScores.Scores[2]}\n" +
                $"{HighScores.Scores[3]}\n" +
                $"{HighScores.Scores[4]}\n"
                );
        }
    }
}