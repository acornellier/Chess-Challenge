using System;
using System.IO;
using System.Numerics;
using Raylib_cs;

namespace ChessChallenge.Application;

public static class MenuUI
{
    public static void DrawButtons(ChallengeController controller)
    {
        var buttonPos = UIHelper.Scale(new Vector2(260, 210));
        var buttonSize = UIHelper.Scale(new Vector2(260, 55));
        var spacing = buttonSize.Y * 1.2f;
        var breakSpacing = spacing * 0.6f;

        // Game Buttons
        if (NextButtonInRow("Human vs MyBot", ref buttonPos, spacing, buttonSize))
        {
            var whiteType = controller.HumanWasWhiteLastGame ? ChallengeController.PlayerType.MyBot : ChallengeController.PlayerType.Human;
            var blackType = !controller.HumanWasWhiteLastGame ? ChallengeController.PlayerType.MyBot : ChallengeController.PlayerType.Human;
            controller.StartNewGame(whiteType, blackType);
        }

        if (NextButtonInRow("MyBot vs MyBot", ref buttonPos, spacing, buttonSize))
            controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.MyBot);
        if (NextButtonInRow("MyBot vs EvilBot", ref buttonPos, spacing, buttonSize))
            controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.EvilBot);
        if (NextButtonInRow("MyBot vs ReeledWarrior114Bot", ref buttonPos, spacing, buttonSize))
            controller.StartNewBotMatch(ChallengeController.PlayerType.MyBot, ChallengeController.PlayerType.ReeledWarrior114Bot);

        // Page buttons
        buttonPos.Y += breakSpacing;

        if (NextButtonInRow("Save Games", ref buttonPos, spacing, buttonSize))
        {
            var pgns = controller.AllPGNs;
            var directoryPath = Path.Combine(FileHelper.AppDataPath, "Games");
            Directory.CreateDirectory(directoryPath);
            var fileName = FileHelper.GetUniqueFileName(directoryPath, "games", ".txt");
            var fullPath = Path.Combine(directoryPath, fileName);
            File.WriteAllText(fullPath, pgns);
            ConsoleHelper.Log("Saved games to " + fullPath, false, ConsoleColor.Blue);
        }

        if (NextButtonInRow("Rules & Help", ref buttonPos, spacing, buttonSize))
            FileHelper.OpenUrl("https://github.com/SebLague/Chess-Challenge");
        if (NextButtonInRow("Documentation", ref buttonPos, spacing, buttonSize))
            FileHelper.OpenUrl("https://seblague.github.io/chess-coding-challenge/documentation/");
        if (NextButtonInRow("Submission Page", ref buttonPos, spacing, buttonSize))
            FileHelper.OpenUrl("https://forms.gle/6jjj8jxNQ5Ln53ie6");

        // Window and quit buttons
        buttonPos.Y += breakSpacing;

        var isBigWindow = Raylib.GetScreenWidth() > Settings.ScreenSizeSmall.X;
        var windowButtonName = isBigWindow ? "Smaller Window" : "Bigger Window";
        if (NextButtonInRow(windowButtonName, ref buttonPos, spacing, buttonSize))
            Program.SetWindowSize(isBigWindow ? Settings.ScreenSizeSmall : Settings.ScreenSizeBig);
        if (NextButtonInRow("Exit (ESC)", ref buttonPos, spacing, buttonSize))
            Environment.Exit(0);

        bool NextButtonInRow(string name, ref Vector2 pos, float spacingY, Vector2 size)
        {
            var pressed = UIHelper.Button(name, pos, size);
            pos.Y += spacingY;
            return pressed;
        }
    }
}