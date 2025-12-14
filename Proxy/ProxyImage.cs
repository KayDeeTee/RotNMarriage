using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace WIFEPlugin;

class ProxyImage
{
    Image target;
    [MoonSharpHidden]
    public ProxyImage(Image t)
    {
        target = t;
    }
    public void SetSprite(string v)
    {
        //if (LuaManager.Sprites.ContainsKey(v))
        //{
        //    target.sprite = LuaManager.Sprites[v];
        //}
    }
    public Transform GetTransform() { return target.GetComponent<Transform>(); }
    public void SetColor(float r, float g, float b, float a)
    {
        target.color = new Color(r, g, b, a);
    }
    public void SetColour(float r, float g, float b, float a)
    {
        SetColor(r, g, b, a);    
    }
    public void SetFill(float amt)
    {
        target.fillAmount = amt;
    }
    public void SetSliced()
    {
        target.type = Image.Type.Sliced;
    }
    public void SetMethod(int method)
    {
        //hori
        //vert
        //radial 90
        //radial 180
        //radial 360
        target.fillMethod = (Image.FillMethod)method;
    }
}