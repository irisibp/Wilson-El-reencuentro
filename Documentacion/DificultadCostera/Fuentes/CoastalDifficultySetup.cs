using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Object=UnityEngine.Object;

public static class CoastalDifficultySetup
{
    const string Art="Assets/Art/DificultadCostera";
    public const string Beach="Assets/Scenes/Nivel04_LaPlaya.unity";
    public const string City="Assets/Scenes/Nivel05_CiudadCostera.unity";
    static Material material;
    static PhysicsMaterial2D frictionless;
    static Sprite wood;
    static T[] InScene<T>() where T:Component => SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();

    [MenuItem("Tools/Wilson/Niveles 4 y 5/Aumentar dificultad de ambos niveles")]
    public static void ApplyBoth()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode){Debug.LogError("Sali de Play para preparar la dificultad.");return;}
        Scene initial=SceneManager.GetActiveScene();
        Directory.CreateDirectory("Temp/WilsonAntesDeDificultad");
        try
        {
            foreach(string path in new[]{Beach,City})
            {
                Scene scene=SceneManager.GetSceneByPath(path);bool loaded=scene.IsValid() && scene.isLoaded;
                if(!loaded)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
                SceneManager.SetActiveScene(scene);
                EditorSceneManager.SaveScene(scene,"Temp/WilsonAntesDeDificultad/"+Path.GetFileName(path),true);
                PrepareActive();EditorSceneManager.SaveScene(scene);
                if(!loaded)EditorSceneManager.CloseScene(scene,true);
            }
        }
        finally {if(initial.IsValid() && initial.isLoaded)SceneManager.SetActiveScene(initial);}
        Debug.Log("Niveles 4 y 5 guardados: trampas, saltos largos y dos plataformas moviles por nivel.");
    }

    [MenuItem("Tools/Wilson/Niveles 4 y 5/Preparar solo la escena abierta")]
    public static void PrepareActive()
    {
        Scene scene=SceneManager.GetActiveScene();bool city=scene.path==City;
        if(EditorApplication.isPlayingOrWillChangePlaymode || scene.path!=Beach && !city){Debug.LogError("Abri el nivel 4 o 5 fuera de Play.");return;}
        if(scene.GetRootGameObjects().Any(g=>g.name=="DesafioCostero")){Debug.Log("Esta escena ya tiene la dificultad aplicada.");return;}
        var floor=InScene<Tilemap>().Single(m=>m.name==(city?"ArenaNivel05":"ArenaNivel04"));
        if(city && InScene<HiddenCoconut>().Length==0)Level05SecretSetup.Prepare();
        EnsureAssets();
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Aumentar dificultad costera");
        var root=new GameObject("DesafioCostero");Undo.RegisterCreatedObjectUndo(root,"Crear desafio");
        int[] holes=city?new[]{10,37,41,42}:new[]{10,34,38,39};
        Undo.RegisterCompleteObjectUndo(floor,"Ampliar vacios");
        var decor=InScene<Tilemap>().FirstOrDefault(m=>m.name=="ConchasDePlaya");if(decor!=null)Undo.RegisterCompleteObjectUndo(decor,"Quitar adornos sin suelo");
        foreach(int x in holes)for(int y=-3;y<=4;y++){floor.SetTile(new Vector3Int(x,y,0),null);if(decor!=null)decor.SetTile(new Vector3Int(x,y,0),null);}
        MoveCoin(new Vector2(10.5f,2.5f),new Vector2(11.5f,2.5f));
        if(city)
        {
            MoveCoin(new Vector2(37.5f,3.5f),new Vector2(35.5f,3.5f));
            MoveCoin(new Vector2(41.5f,2.5f),new Vector2(46.5f,2.5f));
            ClearOldPlatform("BalconesDelMercado",12,14,3,"PrimerBalcon");
            Shift("EdificioCostero_2",.8f);Shift("Letrero_CAFÉ",.8f);
            Shift("EdificioCostero_6",2);Shift("Letrero_HOTEL",2);
            ReplacePromenade(floor);
            Platform(root.transform,"MuelleMovil",new Vector2(38,2.5f),new Vector2(41,2.5f),2,1.8f,.55f);
            Platform(root.transform,"ElevadorDelMercado",new Vector2(13.5f,3.1f),new Vector2(13.5f,4.15f),2.2f,1.1f,.6f);
            foreach(var xy in new[]{new Vector2(5.5f,1),new Vector2(24,2),new Vector2(31.5f,3),new Vector2(44.5f,2),new Vector2(53.5f,2)})Hazard(root.transform,xy,true);
        }
        else
        {
            MoveCoin(new Vector2(34.5f,3.5f),new Vector2(33.5f,3.5f));
            MoveCoin(new Vector2(38.5f,2.5f),new Vector2(41.5f,2.5f));
            ClearOldPlatform("MuelleDeMadera",21,23,3,"VisualMuelle");
            Platform(root.transform,"MuelleMovil",new Vector2(35,2.5f),new Vector2(38,2.5f),2,1.4f,.6f);
            Platform(root.transform,"ElevadorDeLaPlaya",new Vector2(22.5f,3.1f),new Vector2(22.5f,4.15f),2.2f,.9f,.6f);
            foreach(var xy in new[]{new Vector2(5.5f,1),new Vector2(13.5f,2),new Vector2(31.5f,3),new Vector2(47.5f,2)})Hazard(root.transform,xy,false);
        }
        var objective=InScene<TMP_Text>().FirstOrDefault(t=>t.name=="Objetivo");
        if(objective!=null){Undo.RecordObject(objective,"Actualizar instrucciones");objective.text=city?"Evita los pinchos, usa las plataformas móviles y busca el coco del mercado.":"Evita los erizos y cruza en el muelle móvil. Reúne 30 puntos.";}
        floor.RefreshAllTiles();foreach(var c in InScene<TilemapCollider2D>())c.ProcessTilemapChanges();Physics2D.SyncTransforms();
        EditorSceneManager.MarkSceneDirty(scene);Undo.CollapseUndoOperations(group);
    }

    static void MoveCoin(Vector2 from,Vector2 to)
    {
        var c=InScene<CoconutCollectible>().FirstOrDefault(c=>c.valor==1 && Vector2.Distance(c.transform.position,from)<.1f);if(c==null)return;
        Undo.RecordObject(c.transform,"Reubicar coco fuera del vacio");c.transform.position=new Vector3(to.x,to.y,c.transform.position.z);PrefabUtility.RecordPrefabInstancePropertyModifications(c.transform);
    }
    static void Shift(string name,float x)
    {
        var t=InScene<Transform>().FirstOrDefault(t=>t.name==name);if(t==null)return;Undo.RecordObject(t,"Apoyar edificio en la nueva orilla");t.position+=Vector3.right*x;
    }
    static void ClearOldPlatform(string name,int left,int right,int y,string visual)
    {
        var map=InScene<Tilemap>().Single(m=>m.name==name);Undo.RegisterCompleteObjectUndo(map,"Reemplazar plataforma fija");
        for(int x=left;x<=right;x++)map.SetTile(new Vector3Int(x,y,0),null);
        var t=InScene<Transform>().FirstOrDefault(t=>t.name==visual);if(t!=null){Undo.RecordObject(t.gameObject,"Ocultar madera fija");t.gameObject.SetActive(false);}
    }
    static void Platform(Transform parent,string name,Vector2 a,Vector2 b,float width,float speed,float pause)
    {
        var go=new GameObject(name,typeof(Rigidbody2D),typeof(BoxCollider2D),typeof(MovingPlatform2D));go.transform.SetParent(parent,false);go.transform.position=a;go.layer=3;
        var rb=go.GetComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;rb.gravityScale=0;rb.constraints=RigidbodyConstraints2D.FreezeRotation;rb.interpolation=RigidbodyInterpolation2D.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
        var box=go.GetComponent<BoxCollider2D>();box.size=new Vector2(width,.3f);box.sharedMaterial=frictionless;
        var moving=go.GetComponent<MovingPlatform2D>();moving.puntoA=a;moving.puntoB=b;moving.velocidad=speed;moving.pausa=pause;
        var visual=new GameObject("Madera",typeof(SpriteRenderer));visual.transform.SetParent(go.transform,false);
        visual.transform.localScale=new Vector3(width/wood.bounds.size.x,.65f/wood.bounds.size.y,1);visual.transform.localPosition=new Vector3(0,.15f-wood.bounds.max.y*visual.transform.localScale.y,0);
        var sr=visual.GetComponent<SpriteRenderer>();sr.sprite=wood;sr.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/WilsonSubmarino32/Materials/SubmarinoUnlit.mat");sr.sortingOrder=3;
    }
    static void Hazard(Transform parent,Vector2 foot,bool city)
    {
        var go=new GameObject((city?"PinchosObra_":"Erizo_")+foot.x,typeof(BoxCollider2D),typeof(LifeHazard));go.transform.SetParent(parent,false);go.transform.position=foot+Vector2.up*.32f;
        var box=go.GetComponent<BoxCollider2D>();box.isTrigger=true;box.size=city?new Vector2(.9f,.56f):new Vector2(.68f,.64f);
        var visual=new GameObject("PeligroVisible",typeof(MeshFilter),typeof(MeshRenderer));visual.transform.SetParent(go.transform,false);visual.GetComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+(city?"/Pinchos.asset":"/Erizo.asset"));
        var renderer=visual.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=5;
        var label=new GameObject("Advertencia",typeof(TextMeshPro));label.transform.SetParent(go.transform,false);label.transform.localPosition=new Vector3(0,.8f,0);
        var text=label.GetComponent<TextMeshPro>();text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");text.text="!";text.fontSize=3;text.color=new Color(1,.23f,.12f);text.alignment=TextAlignmentOptions.Center;text.rectTransform.sizeDelta=new Vector2(.5f,.5f);text.GetComponent<MeshRenderer>().sortingOrder=8;
    }
    static void EnsureAssets()
    {
        Directory.CreateDirectory(Art);AssetDatabase.Refresh();
        material=AssetDatabase.LoadAssetAtPath<Material>(Art+"/TrampasUnlit.mat");if(material==null){material=new Material(AssetDatabase.LoadAssetAtPath<Shader>("Assets/WilsonSubmarino32/Materials/SubmarinoUnlit.shader"));AssetDatabase.CreateAsset(material,Art+"/TrampasUnlit.mat");}
        frictionless=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(Art+"/SinFriccion.physicsMaterial2D");if(frictionless==null){frictionless=new PhysicsMaterial2D("SinFriccion"){friction=0,bounciness=0};AssetDatabase.CreateAsset(frictionless,Art+"/SinFriccion.physicsMaterial2D");}
        wood=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Tiles/Nivel01/Set de sprites pixel art tropical.png").OfType<Sprite>().Single(s=>s.name.EndsWith("_57"));
        foreach(bool spikes in new[]{false,true})
        {
            string path=Art+(spikes?"/Pinchos.asset":"/Erizo.asset");if(AssetDatabase.LoadAssetAtPath<Mesh>(path)!=null)continue;
            var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
            void Tri(Vector3 a,Vector3 b,Vector3 c,Color color){int n=vertices.Count;vertices.AddRange(new[]{a,b,c});colors.AddRange(new[]{color,color,new Color(1,.27f,.12f)});triangles.AddRange(new[]{n,n+1,n+2});}
            if(spikes)for(int i=0;i<5;i++)Tri(new Vector3(-.5f+i*.2f,-.3f),new Vector3(-.3f+i*.2f,-.3f),new Vector3(-.4f+i*.2f,.3f),new Color(.20f,.25f,.29f));
            else for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Tri(Vector3.zero,new Vector3(Mathf.Cos(a-.17f),Mathf.Sin(a-.17f))*.22f,new Vector3(Mathf.Cos(a),Mathf.Sin(a))*.4f,new Color(.36f,.13f,.12f));Tri(Vector3.zero,new Vector3(Mathf.Cos(a),Mathf.Sin(a))*.4f,new Vector3(Mathf.Cos(a+.17f),Mathf.Sin(a+.17f))*.22f,new Color(.36f,.13f,.12f));}
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetUVs(0,vertices.Select(v=>Vector2.zero).ToList());mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);
        }
        AssetDatabase.SaveAssets();
    }
    static void ReplacePromenade(Tilemap floor)
    {
        var filter=InScene<MeshFilter>().FirstOrDefault(f=>f.name=="BaldosasDelPaseo");if(filter==null)return;
        var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
        void Quad(float x,float y,float w,float h,Color color){int i=vertices.Count;vertices.AddRange(new[]{new Vector3(x,y),new Vector3(x+w,y),new Vector3(x+w,y+h),new Vector3(x,y+h)});for(int j=0;j<4;j++)colors.Add(color);triangles.AddRange(new[]{i,i+2,i+1,i,i+3,i+2});}
        for(int x=0;x<60;x++)for(int y=6;y>=-2;y--)if(floor.HasTile(new Vector3Int(x,y,0))){Quad(x,y+1-.12f,.98f,.12f,new Color(.87f,.76f,.62f));Quad(x,y+1-.28f,1,.08f,new Color(.56f,.36f,.27f));break;}
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/BaldosasDificiles.asset");if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,Art+"/BaldosasDificiles.asset");}
        mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetUVs(0,vertices.Select(v=>Vector2.zero).ToList());mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
        Undo.RecordObject(filter,"Ajustar baldosas al nuevo suelo");filter.sharedMesh=mesh;AssetDatabase.SaveAssets();
    }
}
