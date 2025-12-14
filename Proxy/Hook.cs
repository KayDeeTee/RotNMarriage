using MoonSharp.Interpreter;
using System.Collections.Generic;

namespace WIFEPlugin;


[MoonSharpUserData]
public abstract class BaseHook {
    private readonly HashSet<Closure> hooks = [];
    private readonly HashSet<Closure> copy = []; // used to prevent iteration issues

    public void Add(Closure hook) {
        if(hook != null) hooks.Add(hook);
    }

    public bool Remove(Closure hook) {
        if(hook != null) return hooks.Remove(hook);
        return false;
    }

    protected void Invoke(params object[] args) {
        copy.Clear();
        copy.UnionWith(hooks);
        foreach(Closure hook in copy) {
            try {
                hook.Call(args);
            }
            catch (ScriptRuntimeException ex) {
                string errorMessage = string.Format("LUA ScriptRuntimeEx: {0}", ex.DecoratedMessage);
                WIFEPlugin.Logger.LogError(errorMessage);
                //LuaManager.logOSD.AddMessage(LuaOSDMessage.MessageLevel.Error, errorMessage, -1);
            }
            catch (SyntaxErrorException ex) {
                string errorMessage = string.Format("LUA SyntaxErrorEx: {0}", ex.DecoratedMessage);
                WIFEPlugin.Logger.LogError(errorMessage);
                //LuaManager.logOSD.AddMessage(LuaOSDMessage.MessageLevel.Fatal, errorMessage, -1);
            }
        }
    }
}

public class Hook : BaseHook {
    public void Invoke() {
        base.Invoke();
    }
}

public class Hook<T> : BaseHook {
    public void Invoke(T arg) {
        base.Invoke(arg);
    }
}

public class Hook<T1, T2> : BaseHook {
    public void Invoke(T1 arg1, T2 arg2) {
        base.Invoke(arg1, arg2);
    }
}

public class Hook<T1, T2, T3> : BaseHook {
    public void Invoke(T1 arg1, T2 arg2, T3 arg3) {
        base.Invoke(arg1, arg2, arg3);
    }
}

public class Hook<T1, T2, T3, T4> : BaseHook {
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
        base.Invoke(arg1, arg2, arg3, arg4);
    }
}

public class Hook<T1, T2, T3, T4, T5> : BaseHook {
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
        base.Invoke(arg1, arg2, arg3, arg4, arg5);
    }
}

public class Hook<T1, T2, T3, T4, T5, T6> : BaseHook {
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
        base.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
    }
}

public class Hook<T1, T2, T3, T4, T5, T6, T7> : BaseHook {
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
        base.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    }
}

public class Hook<T1, T2, T3, T4, T5, T6, T7, T8> : BaseHook {
    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
        base.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
    }
}