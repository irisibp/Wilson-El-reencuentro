using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

// Editable scene objects and native meshes: no additional raster atlas or colliders.
public static class CityGeometry
{
    const string Root="Assets/Art/Nivel05";
    struct Piece { public Rect rect; public Color color; public Piece(float x,float y,float w,float h,Color c){rect=new Rect(x,y,w,h);color=c;} }
    static Color Ink=new Color(.12f,.23f,.28f), Cream=new Color(1,.91f,.70f), Teal=new Color(.12f,.55f,.58f);
    static Material material;
    static void Add(List<Piece> p,float x,float y,float w,float h,Color c)=>p.Add(new Piece(x,y,w,h,c));
    static GameObject Draw(string name,List<Piece> pieces,Vector2 origin,int order)
    {
        var verts=new List<Vector3>();var colors=new List<Color>();var tris=new List<int>();var uv=new List<Vector2>();
        foreach(var p in pieces){int i=verts.Count;Rect r=p.rect;verts.Add(new Vector3(r.xMin,r.yMin));verts.Add(new Vector3(r.xMax,r.yMin));verts.Add(new Vector3(r.xMax,r.yMax));verts.Add(new Vector3(r.xMin,r.yMax));for(int k=0;k<4;k++){colors.Add(p.color);uv.Add(Vector2.zero);}tris.AddRange(new[]{i,i+2,i+1,i,i+3,i+2});}
        var mesh=new Mesh{name=name};mesh.SetVertices(verts);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Root+"/Geometria/"+name+".asset");
        var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.position=origin;go.GetComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=order;return go;
    }
    static void Label(string text,Vector2 center,float width,float size,int order)
    {
        var go=new GameObject("Letrero_"+text,typeof(TextMeshPro));go.transform.position=center;var t=go.GetComponent<TextMeshPro>();
        t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");t.text=text;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.color=Cream;t.rectTransform.sizeDelta=new Vector2(width,.45f);t.GetComponent<MeshRenderer>().sortingOrder=order;
    }
    static void Building(int n,float x,float y,float w,float h,Color paint,string shop)
    {
        var p=new List<Piece>();Add(p,0,0,w,h,Ink);Add(p,.1f,0,w-.2f,h-.18f,paint);Add(p,w-.28f,0,.18f,h-.18f,paint*.78f);Add(p,-.1f,h-.24f,w+.2f,.18f,new Color(.76f,.36f,.23f));
        int columns=Mathf.Max(2,Mathf.FloorToInt(w/.9f));
        for(int row=0;row<Mathf.FloorToInt((h-1.3f)/.95f);row++)for(int c=0;c<columns;c++){
            float wx=.3f+c*(w-.6f)/columns,wy=1.6f+row*.95f;
            Add(p,wx-.04f,wy-.04f,.50f,.66f,Ink);Add(p,wx,wy,.42f,.58f,(row+c+n)%3==0?Cream:new Color(.35f,.65f,.72f));Add(p,wx+.18f,wy,.04f,.58f,Ink);Add(p,wx-.09f,wy-.12f,.60f,.07f,Cream);
        }
        Add(p,w*.5f-.32f,0,.64f,1.1f,Ink);Add(p,w*.5f+.16f,.49f,.06f,.06f,Cream);
        Add(p,.12f,1.10f,w-.24f,.4f,Teal);
        for(int stripe=0;stripe<Mathf.CeilToInt(w/.32f);stripe++)Add(p,.1f+stripe*.32f,1.03f,.16f,.18f,Cream);
        Draw("EdificioCostero_"+n,p,new Vector2(x,y),-8);
        Label(shop,new Vector2(x+w/2,y+1.31f),w-.3f,2.0f,-7);
    }
    static void Resident(int n,float x,float y,Color shirt)
    {
        var p=new List<Piece>();Color skin=new Color(.80f,.50f,.30f),pants=new Color(.16f,.28f,.38f);
        Add(p,.06f,0,.13f,.42f,pants);Add(p,.26f,0,.13f,.42f,pants);Add(p,.02f,0,.19f,.08f,Ink);Add(p,.25f,0,.19f,.08f,Ink);
        Add(p,.05f,.37f,.35f,.44f,shirt);Add(p,0,.40f,.08f,.33f,skin);Add(p,.39f,.40f,.08f,.33f,skin);
        Add(p,.12f,.78f,.23f,.27f,skin);Add(p,.1f,1.02f,.27f,.08f,Ink);Add(p,.28f,.91f,.035f,.035f,Ink);
        Draw("VecinoDecorativo_"+n,p,new Vector2(x,y),-2);
    }
    static void Lamp(int n,float x,float y)
    {
        var p=new List<Piece>();Add(p,-.1f,0,.2f,.12f,Ink);Add(p,-.035f,0,.07f,2.25f,Ink);Add(p,-.21f,2.15f,.42f,.08f,Ink);Add(p,-.14f,2.23f,.28f,.37f,Ink);Add(p,-.08f,2.28f,.16f,.25f,Cream);Add(p,-.22f,2.60f,.44f,.07f,Ink);Draw("FarolCostero_"+n,p,new Vector2(x,y),-1);
    }
    public static void Build(System.Func<int,int> height)
    {
        System.IO.Directory.CreateDirectory(Root+"/Geometria");AssetDatabase.Refresh();
        material=new Material(AssetDatabase.LoadAssetAtPath<Shader>("Assets/WilsonSubmarino32/Materials/SubmarinoUnlit.shader"));AssetDatabase.CreateAsset(material,Root+"/CiudadUnlit.mat");
        var skyline=new List<Piece>();
        for(int i=0;i<30;i++){float h=.7f+(i*7%9)*.11f;Add(skyline,i*2,0,1.7f,h,new Color(.35f,.61f,.63f));Add(skyline,i*2+.15f,h,.8f,.15f,new Color(.29f,.52f,.56f));}
        Draw("SiluetaCiudadAlFondo",skyline,new Vector2(0,2.5f),-16);
        Building(1,.6f,1,3.8f,4.4f,new Color(.96f,.72f,.47f),"COSTA");
        Building(2,10.3f,2,3.0f,4.4f,new Color(.55f,.79f,.77f),"CAFÉ");
        Building(3,14.2f,2,3.3f,3.8f,new Color(.94f,.61f,.49f),"MERCADO");
        Building(4,20.2f,2,3.0f,3.6f,new Color(.93f,.83f,.59f),"HELADOS");
        Building(5,30.4f,3,3.4f,4.1f,new Color(.62f,.78f,.88f),"PASEO");
        Building(6,41.3f,2,3.7f,4.8f,new Color(.90f,.67f,.57f),"HOTEL");
        Building(7,50.3f,2,3.3f,4.2f,new Color(.63f,.79f,.70f),"FLORERÍA");
        var promenade=new List<Piece>();
        for(int x=0;x<60;x++)if(height(x)>0){Add(promenade,x,height(x)-.12f,.98f,.12f,new Color(.87f,.76f,.62f));Add(promenade,x,height(x)-.28f,1,.08f,new Color(.56f,.36f,.27f));}
        Draw("BaldosasDelPaseo",promenade,Vector2.zero,1);
        int n=0;foreach(int x in new[]{6,15,25,36,46,54})Lamp(++n,x+.3f,height(x));
        n=0;foreach(int x in new[]{3,12,16,21,31,44,51,55})Resident(++n,x+.15f,height(x),n%2==0?new Color(.9f,.43f,.28f):Teal);
        var plaza=new List<Piece>();Add(plaza,0,0,.22f,3.3f,Ink);Add(plaza,4.45f,0,.22f,3.3f,Ink);Add(plaza,-.15f,3.1f,4.97f,.55f,Teal);Add(plaza,-.2f,3.65f,5.07f,.1f,Cream);
        Draw("PortalPlazaFinal",plaza,new Vector2(54.9f,2),-6);Label("PLAZA DEL REENCUENTRO",new Vector2(57.25f,5.37f),4.5f,1.8f,-5);
    }
}
