using MelonLoader;
using BTD_Mod_Helper;
using ToInfinity;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Powers;
using Il2CppAssets.Scripts.Models.Powers.Effects;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Simulation.Bloons;
using System.Collections.Generic;

[assembly: MelonInfo(typeof(ToInfinity.ToInfinity), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace ToInfinity;

public class ToInfinity : BloonsTD6Mod
{
    private const int TIME_STOP_INTERVAL = 840;
    private const int LOTN_COOLDOWN = 8316;

    private static PowerModel? timestop;

    private static bool shouldTimeStop = true;
    private static int lastTimeStopActivation = 0;
    private int previousUpdateTime = 0;

    private AbilityToSimulation? hex;
    private AbilityToSimulation? hook;

    private readonly List<Bloon> bads = new();

    public override void OnApplicationStart()
    {
        ModHelper.Msg<ToInfinity>("ToInfinity loaded!");
    }

    public override void OnNewGameModel(GameModel result)
    {
        base.OnNewGameModel(result);

        timestop = result.GetPowerWithName("DartTime");

        if (timestop?.behaviors != null)
        {
            if (!Settings.CreateEffectOn) timestop.RemoveBehaviors<CreateEffectOnPowerModel>();
            if (!Settings.CreateSoundOn) timestop.RemoveBehaviors<CreateSoundOnPowerModel>();
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!Settings.EnableMod || InGame.instance?.bridge == null) return;

        if (Settings.ToggleAutoTimestop.JustPressed())
        {
            shouldTimeStop = !shouldTimeStop;
            MelonLogger.Msg($"Auto-time stop {(shouldTimeStop ? "enabled" : "disabled")}");
        }

        int currentUpdateTime = InGame.instance.GetSimulation().roundTime.elapsed;
        int timeToUpdate = currentUpdateTime - previousUpdateTime;
        int nextUpdateTimeEstimate = currentUpdateTime + timeToUpdate;

        if (shouldTimeStop && (nextUpdateTimeEstimate - lastTimeStopActivation >= TIME_STOP_INTERVAL) && (currentUpdateTime <= InGame.instance.GetSimulation().roundStartTime + LOTN_COOLDOWN))
        {
            ActivateTimeStop(currentUpdateTime);
        }

        previousUpdateTime = currentUpdateTime;

        HandleAbilities();
    }

    public override void OnRoundStart()
    {
        base.OnRoundStart();

        bads.Clear();
        shouldTimeStop = true;
        previousUpdateTime = InGame.instance.GetSimulation().roundStartTime;
        ActivateTimeStop(previousUpdateTime);
    }

    private static void ActivateTimeStop(int currentTime)
    {
        var inGame = InGame.instance;
        if (inGame?.bridge.AreRoundsActive() != true || timestop == null) return;

        inGame.bridge.ActivatePower(UnityEngine.Vector2.zero, timestop);
        lastTimeStopActivation = currentTime;
        // MelonLogger.Msg($"Time stop activated at: {lastTimeStopActivation}");
    }

    public override void OnMatchStart()
    {
        base.OnMatchStart();

        FindAbilities();
    }

    public override void OnBloonCreated(Bloon bloon)
    {
        base.OnBloonCreated(bloon);

        if (Settings.EnableMod && bloon.model.name.StartsWith("Bad"))
            bads.Add(bloon);
    }

    private void HandleAbilities()
    {
        if (hex != null && hex.IsReady) hex.Activate();
        if (hook != null && hook.IsReady && bads.Count > 0) hook.Activate();
    }

    public void FindAbilities()
    {
        hex = null;
        hook = null;

        var abilities = InGame.instance.GetAbilities();
        foreach (var ability in abilities)
        {
            switch (ability.model.name)
            {
                case "AbilityModel_HexAbility":
                    hex = ability;
                    break;
                case "AbilityModel_AbilityLargeBadHarpoon":
                    hook = ability;
                    break;
            }
        }
    }

    //private bool IsTimeStopActive()
    //{
    //    var bloons = InGame.instance?.GetBloons();
    //    if (bloons == null || bloons.Count == 0) return false;

    //    foreach (var bloon in bloons)
    //    {
    //        foreach (var mutator in bloon.mutators)
    //        {
    //            if (mutator.mutator.id == "TimeStopBloons")
    //                return true;
    //        }
    //    }

    //    return false;
    //}
}
