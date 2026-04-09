using HarmonyLib;
using RiftOfTheNecroManager;
using Shared;
using Shared.RhythmEngine;

namespace WIFEPlugin;

[HarmonyPatch(typeof(StageInputRecord))]
internal static class StageInputPatch
{
    [HarmonyPatch(nameof(StageInputRecord.RecordInput))]
    [HarmonyPostfix]
    public static void RecordInput(InputRating inputRating, int inputScore, float ratingPercent, float inputBeatNumber, float targetBeatNumber, FmodTimeCapsule fmodTimeCapsule, bool shouldContributeToCombo = true, bool wasPlayerInput = true, int perfectBonusScore = 0)
    {
        try
        {
            WifeOSD.SubmitInput(inputRating, inputScore, ratingPercent, inputBeatNumber, targetBeatNumber, fmodTimeCapsule, shouldContributeToCombo, wasPlayerInput, perfectBonusScore);
        } catch
        {
            Log.Error("error submitting inputs to lua");
        }
        
    }

    [HarmonyPatch(nameof(StageInputRecord.RecordErrantInput))]
    [HarmonyPostfix]
    public static void RecordErrantInput()
    {
        try {
            WifeOSD.SubmitErrant();
        } catch
        {
            Log.Error("error submitting errants to lua");
        }
    }


}
