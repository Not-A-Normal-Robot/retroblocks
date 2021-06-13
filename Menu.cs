﻿using System;
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
                $"{(cursorPos == 2 ? "> " : "  ")}Soft Drop Mode: {(Controls.useSonicDrop ? "instant" : "normal")}\n" +
                $"{(cursorPos == 2 ? "> " : "  ")}ARR\n" +
                $"{(cursorPos == 2 ? "> " : "  ")}ARR\n" +
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