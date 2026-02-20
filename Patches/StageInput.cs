using HarmonyLib;
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
        WifeOSD.SubmitInput(inputRating, inputScore, ratingPercent, inputBeatNumber, targetBeatNumber, fmodTimeCapsule, shouldContributeToCombo, wasPlayerInput, perfectBonusScore);
    }

    [HarmonyPatch(nameof(StageInputRecord.RecordErrantInput))]
    [HarmonyPostfix]
    public static void RecordErrantInput()
    {
        WifeOSD.SubmitErrant();
    }


}
