using UnityEngine;

public class ShooterGameInfo
{
    public const int PLAYER_MAX_LIVES = 1;

    public const string PLAYER_LIVES = "PlayerLives";
    public const string PLAYER_READY = "IsPlayerReady";
    public const string PLAYER_LOADED_LEVEL = "PlayerLoadedLevel";
    public const string PLAYER_SKIN = "PlayerSkin";
    public const string PLAYER_NUMBER = "PlayerNumber";

    public static Color GetColor(int colorChoice)
    {
        switch (colorChoice)
        {
            case 0: return Color.yellow;
            case 1: return new Color(1, 0.65f, 0); //orange
            case 2: return Color.red;
            case 3: return new Color(1, 0.4f, 0.7f); //pink
            case 4: return new Color(0.2f, 0.8f, 0.2f); //lime green
            case 5: return Color.cyan;
            case 6: return Color.blue;
            case 7: return new Color(0.7f, 0, 0.9f); //purple
        }

        return Color.black;
    }
}