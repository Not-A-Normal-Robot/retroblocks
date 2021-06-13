using System;

namespace Menu
{
    public class Main
    {
        public static void Start()
        {
            HighScores.LoadScores();
            Console.Write(
                $"===================\n" +
                $"||  RETROBLOCKS  ||\n" +
                $"===================\n" +
                $"Press Enter to begin\n" +
                $"\n" +
                $"\n" +
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