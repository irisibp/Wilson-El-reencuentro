using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class MenuUI
{
    public static readonly Color Navy=new Color(.025f,.10f,.16f,.96f);
    public static readonly Color Gold=new Color(1,.81f,.40f);
    public static readonly Color Cream=new Color(1,.96f,.85f);
    public static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size,Vector2? pivot=null)
    {
        var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);
        var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.pivot=pivot??new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;return r;
    }
    public static RectTransform Stretch(string name,Transform parent)
    {
        var r=Rect(name,parent,Vector2.zero,Vector2.zero,Vector2.zero);r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;
    }
    public static Canvas Canvas(string name,Transform parent,int order)
    {
        var go=new GameObject(name,typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));if(parent!=null)go.transform.SetParent(parent,false);
        var c=go.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=order;
        var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;return c;
    }
    public static Image Fill(RectTransform rect,Color color)
    {
        var image=rect.gameObject.AddComponent<Image>();image.color=color;return image;
    }
    public static TMP_Text Text(string name,Transform parent,string content,Vector2 position,Vector2 size,int font,Color color,TextAlignmentOptions align=TextAlignmentOptions.TopLeft)
    {
        var r=Rect(name,parent,new Vector2(0,1),position,size,new Vector2(0,1));var text=r.gameObject.AddComponent<TextMeshProUGUI>();
        text.font=Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");text.text=content;text.fontSize=font;text.color=color;text.alignment=align;text.raycastTarget=false;return text;
    }
    public static Button Button(string name,Transform parent,string caption,Vector2 position,Vector2 size,UnityAction action=null,bool primary=false)
    {
        var rect=Rect(name,parent,new Vector2(0,1),position,size,new Vector2(0,1));var image=Fill(rect,primary?Gold:new Color(.10f,.27f,.33f));
        var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;
        var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,1,.83f);colors.pressedColor=new Color(.69f,.78f,.77f);colors.selectedColor=new Color(1,.94f,.73f);button.colors=colors;
        var label=Text("Texto",rect,caption,Vector2.zero,size,23,primary?Navy:Cream,TextAlignmentOptions.Center);label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;
        if(action!=null)button.onClick.AddListener(action);return button;
    }
}
