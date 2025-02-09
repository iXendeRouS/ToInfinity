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
    // time is measured in 60hz intervals
    private const int TIME_STOP_INTERVAL = 840; // 14 seconds, as a timestop with longer timestops mk is 15s
    private const int LOTN_COOLDOWN = 8316;     // 138.6 seconds, assumes energiser and shorter cooldown mk works like 180 * (1 - 0.2 - 0.03)

    private static PowerModel? timestop;

    private static int lastTimeStopActivation = 0;
    private int previousUpdateTime = 0;

    // abilities to automate that cant simply be techbotted
    private AbilityToSimulation? hex;   // techbotting ezili would trigger totem which loses lives. Not a big deal but eh
    private AbilityToSimulation? hook;  // ensure navarch only hooks in BADs to not waste ability
    
    private readonly List<Bloon> bads = new();  // bads for navarch to hook

    public override void OnApplicationStart()
    {
        ModHelper.Msg<ToInfinity>("ToInfinity loaded!");
    }

    public override void OnNewGameModel(GameModel result)
    {
        base.OnNewGameModel(result);

        // get and modify the timestop PowerModel
        timestop = result.GetPowerWithName("DartTime");

        if (timestop?.behaviors != null)
        {
            // remove the flash / sound on activation given settings
            if (!Settings.CreateEffectOn) timestop.RemoveBehaviors<CreateEffectOnPowerModel>();
            if (!Settings.CreateSoundOn) timestop.RemoveBehaviors<CreateSoundOnPowerModel>();
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (!Settings.EnableMod || InGame.instance?.bridge == null) return;

        // calculate the estimated time of the next update to ensure 100% timestop uptime
        int currentUpdateTime = InGame.instance.GetSimulation().roundTime.elapsed;
        int timeToUpdate = currentUpdateTime - previousUpdateTime;
        int nextUpdateTimeEstimate = currentUpdateTime + timeToUpdate;

        // if next update is beyond TIME_STOP_INTERVAL AND LOTN blackhole is not available
        if ((nextUpdateTimeEstimate - lastTimeStopActivation >= TIME_STOP_INTERVAL) && (currentUpdateTime <= InGame.instance.GetSimulation().roundStartTime + LOTN_COOLDOWN))
        {
            ActivateTimeStop(currentUpdateTime);
        }

        previousUpdateTime = currentUpdateTime;

        HandleAbilities();
    }

    private void HandleAbilities()
    {
        if (hex != null && hex.IsReady) hex.Activate();
        if (hook != null && hook.IsReady && bads.Count > 0) hook.Activate();
    }

    public override void OnRoundStart()
    {
        FindAbilities();

        base.OnRoundStart();
        
        previousUpdateTime = InGame.instance.GetSimulation().roundStartTime;
        ActivateTimeStop(previousUpdateTime);   // have to timestop here as OnUpdate only timestops when the TIME_STOP_INTERVAL is exceeded
    }

    // activates a timestop and updates lastTimeStopActivation
    private static void ActivateTimeStop(int currentTime)
    {
        var inGame = InGame.instance;
        if (inGame?.bridge.AreRoundsActive() != true || timestop == null) return;

        inGame.bridge.ActivatePower(UnityEngine.Vector2.zero, timestop);
        lastTimeStopActivation = currentTime;
        // MelonLogger.Msg($"Time stop activated at: {lastTimeStopActivation}");
    }

    public override void OnBloonCreated(Bloon bloon)
    {
        base.OnBloonCreated(bloon);

        // Add new bads to the bads list
        if (Settings.EnableMod && bloon.model.name.StartsWith("Bad"))
            bads.Add(bloon);
    }

    // find the hex and hook abilities to automate.
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
