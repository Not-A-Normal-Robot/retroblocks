using System;
using static Game.Config;
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
            "SFX\n" +
            "SFXR by DrPetter" +
            "\n" +
            "\n" +
            "Bug testers\n" +
            "User670 (testing on Chinese CMD and Windows Terminal)\n" +
            "\n" +
            "\n" +
            "Press Enter to close";
        static int cursorPos;
        static bool start;
        private static int m;
        /// <summary>
        /// 0: main menu, 1: control settings, 2: display settings, 3: credits, 4: mode selection menu
        /// </summary>
        static int menu
        {
            get
            {
                return m;
            }
            set
            {
                Console.Clear();
                cursorPos = 0;
                m = value;
            }
        }
        /// <summary>
        /// cursorPos shouldn't go above this[menu]
        /// </summary>
        static readonly int[] cursorLimits = new int[5] { 3, 12, 1, 0, 0 };
        /// <summary>
        /// left, right, up, down, enter, escape
        /// </summary> 
        static readonly int[] acceptedInputs = new int[6] { 37, 39, 38, 40, 13, 27 };
        static int[] prevFrameInputs = new int[6];
        public static void Start()
        {
            start = false;
            cursorPos = 0;
            HighScores.LoadScores();
            LoadConfig();
            while (!start)
            {
                #region Draw screen
                Program.DisableQuickEdit(null, null);
                Console.SetCursorPosition(0, 0);
                switch (menu)
                {
                    case 0:
                        Console.Write(
                        $"===================\n" +
                        $"||  RETROBLOCKS  ||\n" +
                        $"===================\n\n" +
                        $"{(cursorPos == 0 ? "> " : "  ")}Play\n" +
                        $"\n" +
                        $"\n" +
                        $"Settings:\n" +
                        $"{(cursorPos == 1 ? "> " : "  ")}Controls\n" +
                        $"{(cursorPos == 2 ? "> " : "  ")}Display\n" +
                        $"{(cursorPos == 3 ? "> " : "  ")}Credits\n" +
                        $"\n" +
                        $"\n" +
                        $"Scores:\n" +
                        $"{HighScores.Scores[0]}\n" +
                        $"{HighScores.Scores[1]}\n" +
                        $"{HighScores.Scores[2]}\n" +
                        $"{HighScores.Scores[3]}\n" +
                        $"{HighScores.Scores[4]}\n"
                        );
                        break;
                    case 1:
                        Console.Write(
                            $"Controls\n" +
                            $"========\n" +
                            $"\n" +
                            $"{(cursorPos == 0 ? "> " : "  ")}DAS: {(cursorPos == 0 && das > 1 ? "<< " : "   ")}{das}{(cursorPos == 0 && das < 20 ? " >>" : "     ")}\n" +
                            $"{(cursorPos == 1 ? "> " : "  ")}ARR: {(cursorPos == 1 && arr > 0 ? "<< " : "   ")}{arr}{(cursorPos == 1 && arr < 10 ? " >>" : "     ")}\n" +
                            $"{(cursorPos == 2 ? "> " : "  ")}Soft Drop Mode: {(cursorPos == 2 ? "<< " : "   ")}{(useSonicDrop ? "instant" : "normal")}{(cursorPos == 2 ? " >> " : "    ")}\n" +
                            $"\n" +
                            $"Keybinds\n" +
                            $"========\n" +
                            $"{(cursorPos == 3 ? "> " : "  ")}Left: {left} (Default: 37 Left Arrow)\n" +
                            $"{(cursorPos == 4 ? "> " : "  ")}Right: {right} (Default: 39 Right Arrow)\n" +
                            $"{(cursorPos == 5 ? "> " : "  ")}Hard Drop: {hardDrop} (Default: 32 Space)\n" +
                            $"{(cursorPos == 6 ? "> " : "  ")}Soft Drop: {softDrop} (Default: 40 Down Arrow)\n" +
                            $"{(cursorPos == 7 ? "> " : "  ")}Rotate Clockwise: {rotCw} (Default: 38 Up Arrow)\n" +
                            $"{(cursorPos == 8 ? "> " : "  ")}Rotate Counterclockwise: {rotCcw} (Default: 90 Z key)\n" +
                            $"{(cursorPos == 9 ? "> " : "  ")}Rotate 180 degrees: {rot180} (Default: 88 X key)\n" +
                            $"{(cursorPos ==10 ? "> " : "  ")}Swap Hold Piece: {hold} (Default: 67 C key)\n" +
                            $"\n" +
                            $"{(cursorPos ==11 ? "> " : "  ")}Reset Controls\n" +
                            $"\n" +
                            $"{(cursorPos ==12 ? "> " : "  ")}Exit"
                            );
                        break;
                    case 2:
                        Console.Write(
                            $"Display\n" +
                            $"=======\n" +
                            $"\n" +
                            $"{(cursorPos == 0 ? "> " : "  ")}ASCII compatibility mode: {(asciiMode ? "ON " : "OFF")}\n" +
                            $"Skin customization: coming soon!\n" +
                            $"\n" +
                            $"{(cursorPos == 1 ? "> " : "  ")}Exit"
                            );
                        break;
                    case 3:
                        Console.Write(credits);
                        break;
                    case 4:
                        Console.Write(
                            $"Mode Selection\n" +
                            $"==============\n" +
                            $"\n" +
                            $""
                            // TO DO
                            );
                        break;
                }

                #endregion
                #region Generic navigation code
                for (int i = 0; i < prevFrameInputs.Length; i++)
                {
                    if (NativeKeyboard.IsKeyDown(acceptedInputs[i]))
                    {
                        prevFrameInputs[i]++;
                    }
                    else
                    {
                        prevFrameInputs[i] = 0;
                    }
                    if(prevFrameInputs[i] > 1000)
                    {
                        prevFrameInputs[i] = 100;
                    }
                }
                if (prevFrameInputs[0] == 1 || prevFrameInputs[0] > 15)
                {
                    if (prevFrameInputs[0] > 15)
                    {
                        prevFrameInputs[0] = 12;
                    }
                    Left();
                }
                if (prevFrameInputs[1] == 1 || prevFrameInputs[1] > 15)
                {
                    if (prevFrameInputs[1] > 15)
                    {
                        prevFrameInputs[1] = 12;
                    }
                    Right();
                }
                if (prevFrameInputs[2] == 1 || prevFrameInputs[2] > 15)
                {
                    if (prevFrameInputs[2] > 15)
                    {
                        prevFrameInputs[2] = 12;
                    }
                    if (cursorPos == 0 && cursorLimits[menu] != 0)
                    {
                        cursorPos = cursorLimits[menu];
                    }
                    else if (cursorLimits[menu] != 0)
                    {
                        cursorPos--;
                    }
                }
                if (prevFrameInputs[3] == 1 || prevFrameInputs[3] > 15)
                {
                    if (prevFrameInputs[3] > 15)
                    {
                        prevFrameInputs[3] = 12;
                    }
                    if (cursorPos == cursorLimits[menu])
                    {
                        cursorPos = 0;
                    }
                    else
                    {
                        cursorPos++;
                    }
                }
                if (prevFrameInputs[4] == 1 || prevFrameInputs[4] > 15)
                {
                    if (prevFrameInputs[4] > 15)
                    {
                        prevFrameInputs[4] = 12;
                    }
                    Enter();
                }
                if (prevFrameInputs[5] == 1 || prevFrameInputs[5] > 15)
                {
                    if (prevFrameInputs[5] > 15)
                    {
                        prevFrameInputs[5] = 12;
                    }
                    Escape();
                }
                #endregion
                System.Threading.Thread.Sleep(15);
            }
            start = false;
        }
        private static void Left()
        {
            switch (menu)
            {
                case 1:
                    switch (cursorPos)
                    {
                        case 0:
                            if(das > 1)
                            {
                                das--;
                                SaveConfig();
                            }
                            return;
                        case 1:
                            if (arr > 0)
                            {
                                arr--;
                                SaveConfig();
                            }
                            return;
                        case 2:
                            useSonicDrop = !useSonicDrop;
                            SaveConfig();
                            return;
                    }
                    return;
                case 2:
                    if (cursorPos == 0)
                    {
                        asciiMode = !asciiMode;
                        SaveConfig();
                    }
                    return;
            }
        }
        private static void Right()
        {

            switch (menu)
            {
                case 1:
                    switch (cursorPos)
                    {
                        case 0:
                            if (das < 20)
                            {
                                das++;
                                SaveConfig();
                            }
                            return;
                        case 1:
                            if (arr < 10)
                            {
                                arr++;
                                SaveConfig();
                            }
                            return;
                        case 2:
                            useSonicDrop = !useSonicDrop;
                            SaveConfig();
                            return;
                    }
                    return;
                case 2:
                    if (cursorPos == 0)
                    {
                        asciiMode = !asciiMode;
                        SaveConfig();
                    }
                    break;
                default:
                    return;
            }
        }
        private static void Enter()
        {

            switch (menu)
            {
                case 0:
                    switch (cursorPos)
                    {
                        case 0:
                            menu = 4;
                            return;
                        default:
                            menu = cursorPos;
                            return;
                    }
                case 1:
                    switch (cursorPos)
                    {
                        case < 3:
                            return;
                        case 11:
                            int f = fontSize;
                            ResetConfig();
                            fontSize = f;
                            return;
                        case 12:
                            menu = 0;
                            return;
                        default:
                            Console.Clear();
                            Console.Write($"Awaiting input for \"{GetButtonName(cursorPos - 3)}\".\n" +
                                $"Press Escape to cancel.\n" +
                                $"The enter/return key is not a valid key for the input.\n" +
                                $"Default keybind: {GetDefaultButtonKeybind(cursorPos - 3)}");
                            int key = -1;
                            while (true) 
                            {
                                for(int i = 0; i < 255; i++)
                                {
                                    if (NativeKeyboard.IsKeyDown(i))
                                    {
                                        if(i == 27)
                                        {
                                            key = -2;
                                        }
                                        if(i == 13)
                                        {
                                            continue;
                                        }
                                        key = i;
                                        break;
                                    }
                                }
                                if(key != -1)
                                {
                                    break;
                                }
                            }
                            if(key != -2)
                            {
                                GetButtonRef(cursorPos - 3) = key;
                                SaveConfig();
                            }
                            Console.Clear();
                            return;
                    }
                case 2:
                    if (cursorPos == 1)
                    {
                        menu = 0;
                    }
                    return;
                case 3:
                    menu = 0;
                    return;
            }
        }
        private static void Escape()
        {
            menu = 0;
        }
    }
}