using System.IO;
using HarmonyLib;
using RhythmRift;
using RiftOfTheNecroManager;
using Shared.RhythmEngine;
using Shared.SceneLoading.Payloads;

namespace WIFEPlugin;

[HarmonyPatch(typeof(RRStageController))]
internal static class RRStageControllerPatch
{
    public static RRStageController instance = null!; // TODO: fix this null error

    public static WifeOSD? wife = null;

    

    [HarmonyPatch(nameof(RRStageController.UnpackScenePayload))]
    [HarmonyPostfix]
    public static void UnpackScene(RRStageController __instance, ScenePayload currentScenePayload)
    {
        instance = __instance;

        LuaManager.Reset();

        string lua_path = Path.Combine(Path.GetDirectoryName(PluginData.Info.Location), "husband.lua");
        if( File.Exists( lua_path ))
        {
            Log.Info( "loading lua" );
            LuaManager.Load( [lua_path] );
        } else
        {
            Log.Info( "can't find husband.lua" );
        }
       
    }

    [HarmonyPatch(nameof(RRStageController.UploadScoreToLeaderboardAndRefreshUi))]
    [HarmonyPrefix]
    public static bool Finished()
    {
        WifeOSD.Finished();
        return true;
    }
    

    [HarmonyPatch(nameof(RRStageController.BeginPlay))]
    [HarmonyPostfix]
    public static void Begin()
    {
        wife = new WifeOSD(instance.transform);
        WifeOSD.Reset();
              
    }

    [HarmonyPatch(nameof(RRStageController.Update))]
    [HarmonyPostfix]
    public static void OnUpdate(RRStageController __instance)
    {
        bool paused = __instance._isPaused;
        FmodTimeCapsule fmod = __instance.BeatmapPlayer.FmodTimeCapsule;
        foreach (LuaContext ctx in LuaManager.luaContexts)
        {
            ctx.previousTime = ctx.currentTime;
            ctx.currentTime = fmod.Time;
            ctx.deltaTime = fmod.DeltaTime;
            ctx.currentBeat = fmod.TrueBeatNumber;
            ctx.inVibe = __instance._isVibePowerActive;
            ctx.currentHealth = __instance.PlayerHealth;
            ctx.currentVibe = __instance._currentVibePower;

            ctx.current_combo = instance._stageInputRecord.CurrentComboCount;
            ctx.max_combo = instance._stageInputRecord.MaxComboCount;

            ctx.vibe_activations = instance._stageInputRecord.NumTimesVibePowerActivated;
            ctx.vibe_chains_hit = instance._stageInputRecord.NumVibeChainsHit;
            ctx.vibe_chains_missed = instance._stageInputRecord.NumVibeChainsMissed;
            ctx.vibe_duration = instance._stageInputRecord.NumSecondsVibePowerWasActive;
            ctx.vibe_times = instance._stageInputRecord._vibePowerActivationBeatNumbers;

            if (ctx.justCreated)
            {
                ctx.justCreated = false;
                ctx.OnPostInit.Invoke();
            }
            ctx.OnFrame.Invoke();
        }

        wife?.Update(0);
    }
}
