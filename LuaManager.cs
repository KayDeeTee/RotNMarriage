using System;
using System.Collections.Generic;
using System.IO;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using RhythmRift.Enemies;
using RhythmRift;

namespace WIFEPlugin;

public static class LuaManager
{
    //
    //  Create all the non-primitives you are allowed to send to a lua context
    //  RegisterAssembly does every class marked with [MoonSharpUserData]
    //  RegisterProxyType makes a proxy that replaces any attempt to return a class to lua, replaces it with a proxy that references it instead
    //
    public static void InitUserdata()
    {
        UserData.RegisterAssembly();
        UserData.RegisterProxyType<ProxyTestMeshProUGUI, TextMeshProUGUI>(r => new ProxyTestMeshProUGUI(r));
        UserData.RegisterProxyType<ProxyImage, Image>(r => new ProxyImage(r));
        UserData.RegisterProxyType<ProxyRectTransform, RectTransform>(r => new ProxyRectTransform(r));
    }

    //
    //   
    //
    public static Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
    public static List<Script> scripts = new List<Script>();
    public static List<LuaContext> luaContexts = new List<LuaContext>();

    //
    //  Reset and load all lua files found at song folder 
    //
    public static void Reset()
    {
        scripts.Clear();
        luaContexts.Clear();
    }
    public static void LoadFile(string path)
    {
        Log(String.Format("Attempting to load lua at {0}", path));

        Script lua = new Script(MoonSharp.Interpreter.CoreModules.Preset_HardSandbox);
        lua.Options.ScriptLoader = new FileSystemScriptLoader();
        //Create Vars / Functions
        Log("Creating Globals...");
        lua.Globals["log"] = (System.Object)Log;
        Log("Created global: log");
        LuaContext ctx = new LuaContext(lua);
        Log("Created ctx object");
        ctx.stageController = RRStageControllerPatch.instance;
        lua.Globals["ctx"] = ctx;
        Log("Created global: ctx");
        luaContexts.Add(ctx);

        Log("Added luaContext to list");

        try
        {
            Log("Doing lua file");
            lua.DoFile(path);

            DynValue v = lua.Globals.Get("Init");
            if (!v.IsNil())
            {
                if (v.Type == DataType.Function)
                {
                    lua.Call(v);
                }
            }
            scripts.Add(lua);
        }
        catch (ScriptRuntimeException ex)
        {
            string errorMessage = string.Format("LUA ScriptRuntimeEx: {0}", ex.DecoratedMessage);
            WIFEPlugin.Logger.LogError(errorMessage);
        }
        catch (SyntaxErrorException ex)
        {
            string errorMessage = string.Format("LUA SyntaxErrorEx: {0}", ex.DecoratedMessage);
            WIFEPlugin.Logger.LogError(errorMessage);
        }
    }
    public static void Load(string[] paths)
    {
        foreach (string path in paths)
        {
            LoadFile(path);
        }
    }

    //
    // Utils for making vectors into something you can send to lua as a table
    // Vec3 becomes a table with an x, y, and z entry so you can access it the same way you would in c#
    //
    
    public static Dictionary<string, float> QuaternionDict(Quaternion v)
    {
        Dictionary<string, float> d = new Dictionary<string, float>();
        d["x"] = v.x;
        d["y"] = v.y;
        d["z"] = v.z;
        d["w"] = v.w;
        return d;
    }
    public static Dictionary<string, float> Vec4Dict(Vector4 v)
    {
        Dictionary<string, float> d = new Dictionary<string, float>();
        d["x"] = v.x;
        d["y"] = v.y;
        d["z"] = v.z;
        d["w"] = v.w;
        return d;
    }
    public static Dictionary<string, float> Vec3Dict(Vector3 v)
    {
        Dictionary<string, float> d = new Dictionary<string, float>();
        d["x"] = v.x;
        d["y"] = v.y;
        d["z"] = v.z;
        return d;
    }

    public static Dictionary<string, float> Vec2Dict(Vector2 v)
    {
        Dictionary<string, float> d = new Dictionary<string, float>();
        d["x"] = v.x;
        d["y"] = v.y;
        return d;
    }

    //
    //  Lua callbacks
    //
    private static void Log(string message)
    {
        WIFEPlugin.Logger.LogInfo(message);
    }
}
