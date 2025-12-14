//
//  c# will compain about snake_case usage, but functions / vars in here are for lua which does usually use snake_case
//
using MoonSharp.Interpreter;
using UnityEngine;
using TMPro;

namespace WIFEPlugin;

class ProxyTestMeshProUGUI
{
    TextMeshProUGUI target;
    [MoonSharpHidden]
    public ProxyTestMeshProUGUI(TextMeshProUGUI t)
    {
        target = t;
    }

    //
    // Get transform
    //
    public Transform GetTransform() => target.GetComponent<Transform>();

    //
    //  Set TMPro properties
    //
    public string GetText() => target.text;
    public void SetText(string v) { target.text = v; }
    public void SetFontSize(float v) { target.fontSize = v; }
    public void SetOutline(float r, float g, float b, float a, float thickness, float dilation)
    {
        target.outlineColor = new Color(r, g, b, a);
        target.outlineWidth = thickness;
        target.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, dilation);
    }
    public void SetColor(float r, float g, float b, float a)
    {
        target.color = new Color(r, g, b, a);
    }
    public void SetColour(float r, float g, float b, float a)
    {
        SetColor(r, g, b, a);
    }
}
