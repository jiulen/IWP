using UnityEngine;

public class ShooterGameInfo
{
    //lobby
    public const string PLAYER_READY = "IsPlayerReady";
    public const string PLAYER_SKIN = "PlayerSkin";
    public const string PLAYER_NUMBER = "PlayerNumber";

    //game
    public const string PLAYER_LOADED_LEVEL = "PlayerLoadedLevel";
    public const string PLAYER_SELECTED_ACTION = "PlayerSelectedAction";
    public const string PLAYER_FLIP = "PlayerFlip";
    public const string PLAYER_SHOW_CONTROLS = "PlayerShowControls";
    public const string PLAYER_GROUNDED = "PlayerGrounded";

    public static Color GetColor(int colorChoice)
    {
        switch (colorChoice)
        {
            case 0: return NormalizeRGB(253, 183, 62); //mustard yellow
            case 1: return NormalizeRGB(247, 83, 6); //orange
            case 2: return Color.red;
            case 3: return NormalizeRGB(255, 0, 169); //pink
            case 4: return NormalizeRGB(0, 153, 0); //green
            case 5: return NormalizeRGB(0, 150, 180); //turqoise
            case 6: return NormalizeRGB(0, 0, 225); //blue
            case 7: return NormalizeRGB(179, 0, 230); //purple
        }

        return Color.black;
    }

    static Color NormalizeRGB(int r, int g, int b)
    {
        return new Color(r / 255f, g / 255f, b / 255f);
    }
}