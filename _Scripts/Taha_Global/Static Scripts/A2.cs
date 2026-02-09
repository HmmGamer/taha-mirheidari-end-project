using UnityEngine;

/// <summary>
/// this class is used to store values that cause hardcoding
/// </summary>
public static class A
{
    public static class Tags
    {
        public const string player = "Player";
    }
    public static class LayerMasks
    {
        public static LayerMask player = LayerMask.GetMask("player");
    }
    public static class Layers
    {
        public const int player = 8;
    }
    public static class Anim
    {
        // optional naming : trigger => t, Bool => b, int => i, float => f
        public const string t_collectAirdrop = "collect";
    }
    public static class DataKey
    {
        public const int True = 331;
        public const int False = 23;

        public const string hasShownRateUs = "sd3";  // taha global reference
        public const string totalLoadTimes = "dde";  // taha global reference
        public const string areAdsRemoved = "0chg";  // taha global reference
        public const string timersData = "1i115";    // taha global reference
        public const string lastLanguage = "dd293";  // taha global reference

        public const string currentLevelIndex = "ji385";
        public const string lastUnfinishedLevel = "ff3da";
        public const string savedData = "sce3";
        public const string areMinesPurchased = "s4rw";
        public const string isVibrateActive = "bh43";

        public static bool _IsTrue(string iKey, bool iReturnOnNotExists = false)
        {
            if (PlayerPrefs.GetInt(iKey, False) == True)
                return true;
            return iReturnOnNotExists;    
        }
        public static void _SetTrue(string iKey)
        {
            PlayerPrefs.SetInt (iKey, True);
        }
    }
}
