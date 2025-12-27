using MoonSharp.Interpreter;
using RhythmRift;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using RhythmRift.Enemies;
using System.Collections.Generic;
using RhythmRift.Traps;
using static RhythmRift.Traps.RRTrapController;

namespace WIFEPlugin;


//should probably move sprite handling / caching into the lua context itself so 2 separate scripts loading images can't conflict
[MoonSharpUserData]
public class LuaContext
{
    [MoonSharpHidden]
    public Script script;
    [MoonSharpHidden]
    public RRStageController stageController;
    [MoonSharpHidden]
    public RREnemyController enemyController;
    [MoonSharpHidden]
    public RRTrapController trapController;
    [MoonSharpHidden]
    public LuaContext(Script lua)
    {
        script = lua;
    }
    //
    //  Lua hooks so you can do ctx.on_frame.add(func) to have func() be called every frame
    //
    public Hook OnPostInit { get; } = new();
    public Hook OnFrame { get; } = new();
    public Hook<int> OnBeat { get; } = new(); // args: beat
    public Hook OnChange { get; } = new();
    //
    //  Game data to be passed to lua via ctx
    //
    public float currentTime;
    public float previousTime;
    public float deltaTime;
    public float currentBeat;
    public float currentVibe;
    public int currentHealth;
    public bool inVibe = false;
    public bool justCreated = true;

    public TextMeshProUGUI text_obj;
    public bool text_init = true;

    public int early_crit = 0;
    public int late_crit = 0;
    public int early_perfects = 0;
    public int late_perfects = 0;
    public int early_early_perfects = 0;
    public int late_late_perfects = 0;
    public int early_greats = 0;
    public int late_greats = 0;
    public int early_goods = 0;
    public int late_goods = 0;
    public int early_okays = 0;
    public int late_okays = 0;
    public int misses = 0;
    public int overtaps = 0;
    public float average = 0;
    public float wife_score = 0;
    public float wife_max = 0;

    public int current_combo = 0;
    public int max_combo = 0;

    public int current_score = 0;
    public int prev_current_score = 0;

    public int vibe_chains_hit = 0;
    public int vibe_chains_missed = 0;
    public int vibe_activations = 0;
    public float vibe_duration = 0;
    public List<float> vibe_times = new List<float>();

    // Get Rating / Mult
    public int GetRatingMultCount(int rating, int mult) //0 = miss, 1 = ok, 4 =perfect
    {
        return RRStageControllerPatch.instance._stageInputRecord.GetNumInputsForRatingAndMultiplier( (Shared.InputRating)rating, mult );
    }

    //
    //  Functions for getting references to unity components
    //
    public Transform GetTransform(string path)
    {
        return stageController.transform.Find(path);
    }
    public TextMeshProUGUI GetTmpro(string path) // weird capitalization is required so that it can be called as get_tmpro from lua
    {
        return GetTransform(path)?.GetComponent<TextMeshProUGUI>();
    }
    public Image GetImage(string path)
    {
        return GetTransform(path)?.GetComponent<Image>();
    }


}
