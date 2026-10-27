using UnityEngine;

public static class UpgradeData
{

    public static bool HasSmokeBomb = false;
    public static float BonusTime = 0f;
    public static float BonusStamina = 0f;


    public static void ResetUpgrades()
    {
        HasSmokeBomb = false;
        BonusTime = 0f;
        BonusStamina = 0f;
    }
}