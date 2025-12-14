using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using RhythmRift;
using Shared.RhythmEngine;
using Shared.SceneLoading.Payloads;
using UIPlugin;
using UnityEngine;

namespace WIFEPlugin;

internal static class RRStageControllerPatch
{
    public static RRStageController instance;

    public static WifeOSD wife;

    

    [HarmonyPatch(typeof(RRStageController), "UnpackScenePayload")]
    [HarmonyPostfix]
    public static void UnpackScene(RRStageController __instance, ScenePayload currentScenePayload)
    {
        instance = __instance;

        LuaManager.Reset();

        string lua_path = Path.GetDirectoryName( WIFEPlugin.instance.Info.Location ) + "\\husband.lua";
        if( File.Exists( lua_path ))
        {
            WIFEPlugin.Logger.LogInfo( "loading lua" );
            LuaManager.Load( [lua_path] );
        } else
        {
             WIFEPlugin.Logger.LogInfo( "can't find husband.lua" );
        }
       
    }

    [HarmonyPatch(typeof( RRStageController ), "UploadScoreToLeaderboardAndRefreshUi")]
    [HarmonyPrefix]
    public static bool Finished()
    {
        WifeOSD.Finished();
        return true;
    }
    

    [HarmonyPatch(typeof(RRStageController), "BeginPlay")]
    [HarmonyPostfix]
    public static void Begin()
    {
        wife = new WifeOSD(instance.transform);  
        WifeOSD.Reset();
              
    }

    [HarmonyPatch(typeof(RRStageController), "Update")]
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

            ctx.current_combo = RRStageControllerPatch.instance._stageInputRecord.CurrentComboCount;
            ctx.max_combo = RRStageControllerPatch.instance._stageInputRecord.MaxComboCount;

            ctx.vibe_activations = RRStageControllerPatch.instance._stageInputRecord.NumTimesVibePowerActivated;
            ctx.vibe_chains_hit = RRStageControllerPatch.instance._stageInputRecord.NumVibeChainsHit;
            ctx.vibe_chains_missed = RRStageControllerPatch.instance._stageInputRecord.NumVibeChainsMissed;
            ctx.vibe_duration = RRStageControllerPatch.instance._stageInputRecord.NumSecondsVibePowerWasActive;
            ctx.vibe_times = RRStageControllerPatch.instance._stageInputRecord._vibePowerActivationBeatNumbers;

            if (ctx.justCreated)
            {
                ctx.justCreated = false;
                ctx.OnPostInit.Invoke();
            }
            ctx.OnFrame.Invoke();
        }

        wife.Update(0);
    }

}
