using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.ModOptions;
using UnityEngine;

namespace ToInfinity
{
    internal class Settings : ModSettings
    {
        public static readonly ModSettingBool EnableMod = new(true)
        {
            displayName = "Enable Mod"
        };

        public static readonly ModSettingBool CreateEffectOn = new(true)
        {
            displayName = "CreateEffectOn"
        };

        public static readonly ModSettingBool CreateSoundOn = new(true)
        {
            displayName = "CreateSoundOn"
        };

        public static readonly ModSettingHotkey ToggleAutoTimestop = new(KeyCode.Backslash)
        {
            description = "ToggleAutoTimestop",
        };
    }
}

