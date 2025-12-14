using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shared;
using Shared.RhythmEngine;
using Shared.Utilities;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

namespace WIFEPlugin;

public struct ScoreHistory
{
    public ScoreHistory(float beat, int total_score)
    {
        TrueBeatNumber = beat;
        Score = total_score;
    }
    public float TrueBeatNumber;
    public int Score;
}

public class WifeOSD
{
    public static Dictionary<int, int> inputCounts;

    public static Dictionary<int, int> inputCountsEarly;

    public static Dictionary<int, int> inputCountsLate;
    public static float wife_score;
    public static float wife_max;

    public static float rolling_average = 0.0f;
    public static int rolling_count = 0;

    public static List<ScoreHistory> score;
    public static List<ScoreHistory> prevScore; 

    public static int prev_index = -1;

    public static int current_score = 0;
    public static int prev_current_score = 0;

    public static string level_id;
    public static string diff;

    public static void Reset()
    {
        inputCounts = new System.Collections.Generic.Dictionary<int, int>();
        inputCountsEarly = new System.Collections.Generic.Dictionary<int, int>();
        inputCountsLate = new System.Collections.Generic.Dictionary<int, int>();
        wife_max = 0;
        wife_score = 0;
        rolling_average = 0.0f;
        rolling_count = 0;

        prev_index = -1;
        current_score = 0;
        prev_current_score = 0;

        score = new List<ScoreHistory>();
        prevScore = new List<ScoreHistory>();

        WIFEPlugin.Logger.LogInfo("Getting level info...");

        level_id = RRStageControllerPatch.instance._levelId;
        diff = ((int)RRStageControllerPatch.instance._stageScenePayload.GetLevelDifficulty()).ToString();

        WIFEPlugin.Logger.LogInfo(level_id);
        WIFEPlugin.Logger.LogInfo(diff);

        string scores_path = Path.GetDirectoryName( WIFEPlugin.instance.Info.Location ) + "/scores/";
        string path = level_id + "_" + diff;
        string full_path = scores_path + path;

        if( !Directory.Exists(scores_path) ) Directory.CreateDirectory(scores_path);

        if( File.Exists(full_path))
        {
            WIFEPlugin.Logger.LogInfo("Reading file for previous score");
            string prev = File.ReadAllText( full_path );
            string[] lines = prev.Split("\n");
            for( int i = 0; i < lines.Length; i++)
            {
                string[] sub_line = lines[i].Split("\t");
                float beat = float.Parse(sub_line[0]);
                int score = int.Parse(sub_line[1]);
                WIFEPlugin.Logger.LogInfo($"{beat} -> {score}");
                prevScore.Add( new ScoreHistory( beat, score ) );
            }
        }
        UpdateLua();
    }

    public static void Finished()
    {
        WIFEPlugin.Logger.LogInfo("Finished song.");
        int total_score = RRStageControllerPatch.instance._stageInputRecord.TotalScore;
        int prev_score = 0;
        WIFEPlugin.Logger.LogInfo("Getting previous score...");
        foreach( ScoreHistory sh in prevScore)
        {
            prev_score = sh.Score;
        }
        WIFEPlugin.Logger.LogInfo($"Previous scores : {prev_score}");
        if( total_score > prev_score)
        {
            WIFEPlugin.Logger.LogInfo("Sorting score before saving.");
            score.Sort( delegate(ScoreHistory a, ScoreHistory b) { 
                int cmp = a.TrueBeatNumber.CompareTo(b.TrueBeatNumber);
                if( cmp == 0 ){ return a.Score.CompareTo(b.Score); }
                return cmp; 
            } );
            WIFEPlugin.Logger.LogInfo("Finished song with higher score, saving data to /scores/");
            string text_output = "";
            foreach( ScoreHistory s in score )
            {
                float beat = s.TrueBeatNumber;
                int score = s.Score;
                text_output += $"{beat}\t{score}\n";
            }
            string scores_path = Path.GetDirectoryName( WIFEPlugin.instance.Info.Location ) + "/scores/";
            string path = level_id + "_" + diff;
            string full_path = scores_path + path;
            WIFEPlugin.Logger.LogInfo("Checking folder exists...");
            if( !Directory.Exists(scores_path) ) Directory.CreateDirectory(scores_path);
            WIFEPlugin.Logger.LogInfo("Saving data...");
            File.WriteAllText( full_path, text_output );
            WIFEPlugin.Logger.LogInfo("Done!");
        }
    }

    public WifeOSD(Transform parent)
    {
        RectTransform screen = (RectTransform)parent.Find("RhythmRiftCanvas/ScreenContainer");

        GameObject cont = new GameObject("WifeOSD", typeof(RectTransform));
        cont.transform.SetParent(screen, false);

        RectTransform cont_transform = (RectTransform)cont.transform;
        cont_transform.anchorMin = new Vector2(0, 0);
        cont_transform.anchorMax = new Vector2(1, 1);
        cont_transform.offsetMax = new Vector2(0, 0);
        cont_transform.offsetMin = new Vector2(0, 0);
        cont_transform.sizeDelta = new Vector2(0, 0);

        //create text
        GameObject text = new GameObject("WifeOSDText", typeof(RectTransform), typeof(CanvasRenderer));
        text.transform.SetParent(cont.transform, false);
        TextObj = text.gameObject.AddComponent<TextMeshProUGUI>();
        TextObj.font = RRStageControllerPatch.instance._stageUIView._scoreText.font;
        TextObj.fontSize = 32;

        TextObj.richText = true;

        RectTransform text_transform = (RectTransform)text.transform;
        text_transform.anchorMin = new Vector2(0.05f, 0.15f);
        text_transform.anchorMax = new Vector2(0.35f, 0.85f);
        text_transform.offsetMax = new Vector2(0, 0);
        text_transform.offsetMin = new Vector2(0, 0);
        text_transform.sizeDelta = new Vector2(0, 0);

        TextObj.enableWordWrapping = true;
        TextObj.overflowMode = TextOverflowModes.Overflow;
        TextObj.outlineColor = Color.black;
        TextObj.outlineWidth = 0.5f;
        TextObj.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0.5f);

        TextObj.text = "";
        WIFEPlugin.Logger.LogInfo($"Created Text Obj for WIFE plugin");

        foreach (LuaContext ctx in LuaManager.luaContexts)
        {
            ctx.text_obj = TextObj;
        }

        InitUpdateLua();
    }

    public static TextMeshProUGUI TextObj;
    public bool requiresReordering = false;

    public void Update(float deltaTime)
    {
        UpdateScoreProgress( RRStageControllerPatch.instance.BeatmapPlayer.FmodTimeCapsule.TrueBeatNumber - 0.5f  );
        return;
    }
    
    public static float NextScoreBeat()
    {
        if( prev_index + 1 < prevScore.Count ) return prevScore[prev_index+1].TrueBeatNumber;
        return -1;
    }

    public static void UpdateScoreProgress(float targetBeatNumber)
    {
        float next_beat = NextScoreBeat();
        while( next_beat != -1 && next_beat < targetBeatNumber)
        {
            prev_current_score = prevScore[prev_index+1].Score;
            prev_index += 1;
            next_beat = NextScoreBeat();
        }
    }

    public static void SubmitInput(InputRating inputRating, int inputScore, float ratingPercent, float inputBeatNumber, float targetBeatNumber, FmodTimeCapsule fmodTimeCapsule, bool shouldContributeToCombo = true, bool wasPlayerInput = true, int perfectBonusScore = 0)
    {
        if( inputRating == InputRating.Miss)
        {
            wife_score += wife3_miss_weight;
            wife_max += 2;
            if( !inputCounts.ContainsKey(windows.Length) ) inputCounts.Add(windows.Length, 0);
            inputCounts[windows.Length] += 1;

            UpdateLua();
            return;
        }

        float secs = (inputBeatNumber-targetBeatNumber) * fmodTimeCapsule.BeatLengthInSeconds;
        float usecs = secs * usec;
        wife_score += wife3( secs, 1 );
        wife_max += 2;

        AddRating( usecs );

        rolling_average *= rolling_count;
        rolling_average += secs;
        rolling_count += 1;
        rolling_average /= rolling_count;

        
        float target_beat = targetBeatNumber;
        int total_score = RRStageControllerPatch.instance._stageInputRecord.TotalScore;
        ScoreHistory s = new ScoreHistory(target_beat, total_score);

        score.Add( s );
        
        current_score = total_score;
        UpdateScoreProgress(targetBeatNumber);
        UpdateLua();
    }
    
    public static void SubmitErrant()
    {
        //if( RRStageControllerPatch.instance._stageInputRecord.CurrentComboCount == 0 ) return;
        wife_score += wife3_miss_weight;
        wife_max += 2;

        if( !inputCounts.ContainsKey(windows.Length+1) ) inputCounts.Add(windows.Length+1, 0);
        inputCounts[windows.Length+1] += 1;

        UpdateLua();
    }

    public static void AddRating(float usecs)
    {
        int idx = RatingIndex( math.abs(usecs) );
        if( !inputCounts.ContainsKey(idx) ) inputCounts.Add(idx, 0);
        inputCounts[idx] += 1;

        if( usecs > 0)
        {
            if( !inputCountsLate.ContainsKey(idx) ) inputCountsLate.Add(idx, 0);
            inputCountsLate[idx] += 1;
        } else
        {
            if( !inputCountsEarly.ContainsKey(idx) ) inputCountsEarly.Add(idx, 0);
            inputCountsEarly[idx] += 1;
        }
    }

    public static int RatingIndex( float usecs )
    {
        //emulate the inaccuracies the game has due to rounding
        float hit_window = RRStageControllerPatch.instance.BeatmapPlayer.ActiveInputRatingsDefinition.AfterBeatHitWindow;
        int percentage = (int)((1.0 -  math.abs(usecs / usec) / hit_window) * 100);
        if( percentage >= 98 ) return 0;
        if( percentage >= 90 ) return 1;
        if( percentage >= 80) return 2;
        if( percentage >= 50 ) return 3;
        if( percentage >= 30 ) return 4;
        if( percentage >= 0 ) return 5;
        return 6;

        for( int i = 0; i < windows.Length; i++)
        {
            if( usecs < windows[i] ) return i;
        }
        return windows.Length;
    }
    public static string[] names = { "CRIT", "FLAW", "PERF", "GREAT", "GOOD", "OKAY", "MISS", "OVER" };

    public static float[] windows = { 3500, 17500, 35000, 87500, 122500, 175000 };
    public static float miss_window = 180000;
    public const float usec = 1000000.0f;
    public const float wife3_miss_weight = -5.5f;
    public static float wife3(float maxms, float ts)
    {
        float j_pow = 0.75f;
        float max_points = 2.0f;
        float ridic = 5.0f*ts;
        float max_boo_weight = 180.0f * ts;

        maxms = math.abs(maxms * 1000.0f);

        // case optimizations
        if (maxms <= ridic) {
            return max_points;
        }

        float zero = 65.0f * math.pow(ts, j_pow);
	    float dev = 22.7f * math.pow(ts, j_pow);

        if (maxms <= zero) {
            return max_points * werwerwerwerf((zero - maxms) / dev);
        }
        if (maxms <= max_boo_weight) {
            return (maxms - zero) * wife3_miss_weight / (max_boo_weight - zero);
        }
        return wife3_miss_weight;
    }

    public static float werwerwerwerf( float v )
    {
        float a1 = 0.254829592f;
        float a2 = -0.284496736f;
        float a3 = 1.421413741f;
        float a4 = -1.453152027f;
        float a5 = 1.061405429f;
        float p = 0.3275911f;

        float s = math.sign(v);
        v = math.abs(v);

        var t = 1 / (1 + p * v);
        var y = 1 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * math.exp(-v * v);

        return s * y;
    }

    public static int GetEarlyCount( int idx )
    {
        if( inputCountsEarly.ContainsKey(idx) ) return inputCountsEarly[idx];
        return 0;
    }
    public static int GetLateCount( int idx )
    {
        if( inputCountsLate.ContainsKey(idx) ) return inputCountsLate[idx];
        return 0;
    }
    public static int GetTotalCount( int idx )
    {
        if( inputCounts.ContainsKey(idx) ) return inputCounts[idx];
        return 0;
    }

    public static void InitUpdateLua()
    {
        foreach (LuaContext ctx in LuaManager.luaContexts)
        {
            ctx.early_crit = 0;
            ctx.late_crit = 0;
            ctx.early_perfects = 0;
            ctx.late_perfects = 0;
            ctx.early_early_perfects = 0;
            ctx.late_late_perfects = 0;
            ctx.early_greats = 0;
            ctx.late_greats = 0;
            ctx.early_goods = 0;
            ctx.late_goods = 0;
            ctx.early_okays = 0;
            ctx.late_okays = 0;
            ctx.misses = 0;
            ctx.overtaps = 0;
            ctx.average = 0;
            ctx.wife_score = 0;
            ctx.wife_max = 0;

            ctx.current_combo = 0;
            ctx.max_combo = 0;

            ctx.current_score = 0;
            ctx.prev_current_score = 0;

            ctx.vibe_activations = 0;
            ctx.vibe_chains_hit = 0;
            ctx.vibe_chains_missed = 0;
            ctx.vibe_duration = 0;
            ctx.vibe_times =[];

            //ctx.text_obj = TextObj;

            ctx.OnChange.Invoke();

            ctx.text_init = false;
        }
    }

    public static void UpdateLua()
    {
        foreach (LuaContext ctx in LuaManager.luaContexts)
        {
            ctx.early_crit = GetEarlyCount(0);
            ctx.late_crit = GetLateCount(0);
            ctx.early_perfects = GetEarlyCount(1);
            ctx.late_perfects = GetLateCount(1);
            ctx.early_early_perfects = GetEarlyCount(2);
            ctx.late_late_perfects = GetLateCount(2);
            ctx.early_greats = GetEarlyCount(3);
            ctx.late_greats = GetLateCount(3);
            ctx.early_goods = GetEarlyCount(4);
            ctx.late_goods = GetLateCount(4);
            ctx.early_okays = GetEarlyCount(5);
            ctx.late_okays = GetLateCount(5);
            ctx.misses = GetTotalCount(windows.Length);
            ctx.overtaps = GetTotalCount(windows.Length+1);
            ctx.average = rolling_average;
            ctx.wife_score = wife_score;
            ctx.wife_max = wife_max;

            ctx.current_combo = RRStageControllerPatch.instance._stageInputRecord.CurrentComboCount;
            ctx.max_combo = RRStageControllerPatch.instance._stageInputRecord.MaxComboCount;

            ctx.current_score = current_score;
            ctx.prev_current_score = prev_current_score;

            ctx.vibe_activations = RRStageControllerPatch.instance._stageInputRecord.NumTimesVibePowerActivated;
            ctx.vibe_chains_hit = RRStageControllerPatch.instance._stageInputRecord.NumVibeChainsHit;
            ctx.vibe_chains_missed = RRStageControllerPatch.instance._stageInputRecord.NumVibeChainsMissed;
            ctx.vibe_duration = RRStageControllerPatch.instance._stageInputRecord.NumSecondsVibePowerWasActive;
            ctx.vibe_times = RRStageControllerPatch.instance._stageInputRecord._vibePowerActivationBeatNumbers;

            //ctx.text_obj = TextObj;

            ctx.OnChange.Invoke();
        }
    }

}