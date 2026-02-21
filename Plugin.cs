using BepInEx;
using RiftOfTheNecroManager;


namespace WIFEPlugin;


[BepInPlugin(GUID, NAME, VERSION)]
[NecroManagerInfo(menuNameOverride: NAME, customEventsNameOverride: "WIFE")]
public class WIFEPlugin : RiftPlugin {
    public const string GUID = "rotn.katie.wife.wife_mod";
    public const string NAME = "WIFE Mod";
    public const string VERSION = "1.0.0";
    
    protected override void OnInit() {
        base.OnInit();
        LuaManager.InitUserdata();
    }
}
