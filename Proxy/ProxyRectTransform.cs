using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace WIFEPlugin;

class ProxyRectTransform
{
    RectTransform target;
    [MoonSharpHidden]
    public ProxyRectTransform(RectTransform t)
    {
        target = t;
    }

    //
    // Create new components/gameObjects as a child of this gameObject
    //
    public RectTransform AddChild(string name)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(target.transform, false);
        return gameObject.GetComponent<RectTransform>();
    }
    public Image AddImage(string sprite)
    {
        Image img = target.gameObject.AddComponent<UnityEngine.UI.Image>();
        img.sprite = LuaManager.Sprites[sprite];
        return img;
    }
    //this gets added but no text shows up
    public TextMeshProUGUI AddTmpro() // weird capitalization is required so that it can be called as add_tmpro from lua
    {
        TextMeshProUGUI tmpro = target.gameObject.AddComponent<TextMeshProUGUI>();
        tmpro.font = RRStageControllerPatch.instance._stageUIView._scoreText.font;
        tmpro.fontSize = 24;
        return tmpro;
    }

    //
    // Get other components on this gameObject
    //
    public Image GetImage() => target.GetComponent<Image>();
    public TextMeshProUGUI GetTmpro() => target.GetComponent<TextMeshProUGUI>(); // weird capitalization is required so that it can be called as get_tmpro from lua

    //
    // Set rect transform properties
    //
    public Dictionary<string, float> GetAnchorPosition() => LuaManager.Vec2Dict(target.anchoredPosition);
    public void SetAnchorPosition(float x, float y) { target.anchoredPosition = new Vector2(x, y); }

    public Dictionary<string, float> GetAnchorMax() => LuaManager.Vec2Dict(target.anchorMax);
    public void Set_anchor_max(float x, float y) { target.anchorMax = new Vector2(x, y); }

    public Dictionary<string, float> GetAnchorMin() => LuaManager.Vec2Dict(target.anchorMin);
    public void SetAnchorMin(float x, float y) { target.anchorMin = new Vector2(x, y); }

    public Dictionary<string, float> GetOffsetMax() => LuaManager.Vec2Dict(target.offsetMax);
    public void SetOffsetMax(float x, float y) { target.offsetMax = new Vector2(x, y); }

    public Dictionary<string, float> GetOffsetMin() => LuaManager.Vec2Dict(target.offsetMin);
    public void SetOffsetMin(float x, float y) { target.offsetMin = new Vector2(x, y); }
    public Dictionary<string, float> GetSizeDelta() => LuaManager.Vec2Dict(target.sizeDelta);
    public void SetSizeDelta(float x, float y) { target.sizeDelta = new Vector2(x, y); }
    public Dictionary<string, float> GetRotation() => LuaManager.Vec3Dict(target.eulerAngles);
    public void SetRotation(float x, float y, float z) { target.eulerAngles = new Vector3(x, y, z); }
    public void SetSortOrder(int v)
    {
        target.transform.position = new Vector3(target.transform.position.x, target.transform.position.y, v);
    }

    //
    // Set gameObject properties
    //
    public void SetActive(bool v) { target.gameObject.SetActive(v); }
    public void DisableLayout()
    {
        LayoutElement le = target.gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }

}