using System.Collections;
using System.IO;
using System.Linq;
using HarmonyLib;
using RhythmRift;
using Shared;
using Shared.RhythmEngine;
using Shared.SceneLoading.Payloads;
using UnityEngine;

namespace WIFEPlugin;

internal static class StageInputPatch
{
    [HarmonyPatch(typeof(StageInputRecord), "RecordInput")]
    [HarmonyPostfix]
    public static void RecordInput(InputRating inputRating, int inputScore, float ratingPercent, float inputBeatNumber, float targetBeatNumber, FmodTimeCapsule fmodTimeCapsule, bool shouldContributeToCombo = true, bool wasPlayerInput = true, int perfectBonusScore = 0)
    {
        WifeOSD.SubmitInput(inputRating, inputScore, ratingPercent, inputBeatNumber, targetBeatNumber, fmodTimeCapsule, shouldContributeToCombo, wasPlayerInput, perfectBonusScore);
    }

    [HarmonyPatch(typeof(StageInputRecord), "RecordErrantInput")]
    [HarmonyPostfix]
    public static void RecordErrantInput()
    {
        WifeOSD.SubmitErrant();
    }


}
