using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Object=UnityEngine.Object;

public static class Level03Builder
{
    public const string Path03="Assets/Scenes/Nivel03_RuinasSumergidas.unity";
    const string Path02="Assets/Scenes/Nivel02_CavernaDeCoral.unity";
    const string Art="Assets/WilsonSubmarino32";
    static readonly string Output=System.IO.Path.GetFullPath("../cambios_nivel03");
    static readonly int[] Heights={1,1,1,1,1,1,2,2,2,2,2,1,1,1,1,3,3,3,3,3,1,1,1,1,2,2,2,2,2,4,4,4,4,4,2,2,2,2,3,3,3,3,2,2,2,2,2,2};
    static readonly HashSet<Vector2Int> FloorCells=new HashSet<Vector2Int>();
    static Material material;
    static Scene target;
    static int safeNodes,reachableNodes,trajectorySamples;
    static GameObject Clone(GameObject original,string name,Transform parent=null)
    {
        var go=Object.Instantiate(original);go.name=name;
        if(go.scene!=target)SceneManager.MoveGameObjectToScene(go,target);
        if(parent!=null)go.transform.SetParent(parent,false);
        return go;
    }
    static Tilemap Map(string name,Transform parent,int order,bool solid)
    {
        var go=new GameObject(name,typeof(Tilemap),typeof(TilemapRenderer));go.transform.SetParent(parent,false);
        var r=go.GetComponent<TilemapRenderer>();r.sharedMaterial=material;r.sortingOrder=order;
        if(solid){go.layer=3;go.AddComponent<TilemapCollider2D>();go.AddComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;}
        return go.GetComponent<Tilemap>();
    }
    static TMP_Text Text(Transform canvas,string name,string text,Vector2 anchor,Vector2 pos,Vector2 size,int fontSize,TextAlignmentOptions align)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(canvas,false);
        var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=anchor;r.anchoredPosition=pos;r.sizeDelta=size;
        var t=go.GetComponent<TextMeshProUGUI>();t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        t.text=text;t.fontSize=fontSize;t.color=Color.white;t.alignment=align;t.raycastTarget=false;return t;
    }
    static Tile Tile(string name)=>AssetDatabase.LoadAssetAtPath<Tile>(Art+"/Tiles/"+name+".asset");
    static void SetString(Object obj,string key,string value){var so=new SerializedObject(obj);so.FindProperty(key).stringValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    public static void Build()
    {
        if(File.Exists(Path03))throw new InvalidOperationException("El nivel 3 ya existe; no se sobrescribe.");
        var source=EditorSceneManager.OpenScene(Path02);
        var playerTemplate=Object.FindAnyObjectByType<WilsonController>();
        var pointTemplate=Object.FindAnyObjectByType<RespawnCheckpoint>();
        var exitTemplate=Object.FindAnyObjectByType<LevelExit>();
        var hazardTemplate=Object.FindAnyObjectByType<CoralHazard>();
        var coinTemplates=Object.FindObjectsByType<CoconutCollectible>().GroupBy(c=>c.valor).ToDictionary(g=>g.Key,g=>g.First());
        if(playerTemplate==null || pointTemplate==null || exitTemplate==null || hazardTemplate==null)throw new Exception("El nivel 2 debe contener jugador, punto seguro, salida y peligros guardados.");
        float radius=playerTemplate.GetComponent<Collider2D>().bounds.extents.y;
        target=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(target);
        material=AssetDatabase.LoadAssetAtPath<Material>(Art+"/Materials/SubmarinoUnlit.mat");
        var grid=new GameObject("Grid_Ruinas",typeof(Grid));grid.GetComponent<Grid>().cellSize=new Vector3(1,1,0);
        var floor=Map("TerrenoNivel03",grid.transform,0,true);var stone=Map("PiedraNivel03",grid.transform,0,true);
        var ruins=Map("ArcosDeFondo_SinColision",grid.transform,-9,false);var decor=Map("DecoracionNivel03",grid.transform,2,false);
        var arena=AssetDatabase.LoadAssetAtPath<RuleTile>(Art+"/RuleTiles/Auto_arena.asset");
        var rock=AssetDatabase.LoadAssetAtPath<RuleTile>(Art+"/RuleTiles/Auto_roca.asset");
        FloorCells.Clear();
        for(int x=0;x<48;x++)for(int y=-2;y<Heights[x];y++)FloorCells.Add(new Vector2Int(x,y));
        for(int x=11;x<=12;x++)FloorCells.Add(new Vector2Int(x,3));
        foreach(var p in FloorCells)floor.SetTile(new Vector3Int(p.x,p.y,0),arena);
        for(int y=-2;y<=9;y++){stone.SetTile(new Vector3Int(-1,y,0),rock);stone.SetTile(new Vector3Int(48,y,0),rock);}
        for(int x=0;x<48;x++)if(x<10 || x>=18 && x<27 || x>=36)for(int y=8;y<=9;y++)stone.SetTile(new Vector3Int(x,y,0),rock);
        foreach(int center in new[]{7,23,39})
        {
            for(int y=-1;y<6;y++){ruins.SetTile(new Vector3Int(center-3,y,0),rock);ruins.SetTile(new Vector3Int(center+3,y,0),rock);}
            for(int x=center-3;x<=center+3;x++)for(int y=6;y<8;y++)ruins.SetTile(new Vector3Int(x,y,0),rock);
        }
        ruins.color=new Color(.35f,.65f,.75f,.55f);
        foreach(var d in new[]{(3,"concha_abanico"),(5,"alga_verde"),(7,"coral_violeta"),(10,"esponja_alta"),(14,"alga_turquesa"),(16,"coral_rosa"),(20,"estrella_violeta"),(22,"alga_verde"),(24,"esponja_baja"),(28,"coral_violeta"),(30,"alga_corta"),(34,"estrella_naranja"),(37,"coral_rosa"),(39,"esponja_alta"),(41,"alga_turquesa"),(43,"concha_espiral"),(47,"alga_verde")})decor.SetTile(new Vector3Int(d.Item1,Heights[d.Item1],0),Tile(d.Item2));
        foreach(int x in new[]{4,14,23,32,42})decor.SetTile(new Vector3Int(x,5,0),Tile("burbujas_b"));

        var player=Clone(playerTemplate.gameObject,"Wilson");player.tag="Player";player.transform.position=new Vector3(1.5f,1+radius+.05f,0);
        var controller=player.GetComponent<WilsonController>();controller.enabled=true;controller.ReiniciarMovimiento();player.GetComponent<Rigidbody2D>().simulated=true;
        var canvas=new GameObject("Canvas",typeof(Canvas),typeof(CanvasScaler));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
        var count=Text(canvas.transform,"ContadorCocos","Cocos: 0 / 25",new Vector2(0,1),new Vector2(20,-18),new Vector2(350,44),28,TextAlignmentOptions.Left);
        Text(canvas.transform,"TituloNivel","Nivel 3: Ruinas sumergidas",Vector2.one,new Vector2(-20,-18),new Vector2(550,44),28,TextAlignmentOptions.Right);
        var hint=Text(canvas.transform,"Objetivo","Explora las ruinas. Evita los corales rojos.",Vector2.zero,new Vector2(20,18),new Vector2(1000,40),22,TextAlignmentOptions.Left);
        player.GetComponent<PlayerRespawn>().estado=hint;

        var manager=new GameObject("GameManager",typeof(CoconutManager)).GetComponent<CoconutManager>();manager.cocosNecesarios=25;manager.textoContador=count;
        var exit=Clone(exitTemplate.gameObject,"SalidaNivel03");exit.transform.position=new Vector3(46.5f,3,0);exit.SetActive(true);
        SetString(exit.GetComponent<LevelExit>(),"nombreNivel","Ruinas sumergidas");SetString(exit.GetComponent<LevelExit>(),"siguienteEscena","");exit.GetComponent<LevelExit>().alCompletarNivel=new UnityEvent();manager.salidaNivel=exit;
        var checkpoint=Clone(pointTemplate.gameObject,"CheckpointNivel03");checkpoint.transform.position=new Vector3(25.5f,3,0);
        var cp=checkpoint.GetComponent<RespawnCheckpoint>();cp.puntoReaparicion.position=new Vector3(25.5f,2+radius+.05f,0);cp.textoMarcador.text="Punto seguro";cp.textoMarcador.color=new Color(.65f,1f,1f);cp.indicador.color=new Color(.55f,.8f,.9f);
        var hazards=new GameObject("PeligrosNivel03");
        foreach(var xy in new[]{new Vector2(8.7f,2.5f),new Vector2(18,3.5f),new Vector2(35.5f,2.5f),new Vector2(44.5f,2.5f)})
        {
            var go=Clone(hazardTemplate.gameObject,"CoralPeligroso_"+(hazards.transform.childCount+1),hazards.transform);go.transform.position=xy;
        }
        var group=new GameObject("Collectibles");
        foreach(var xy in new[]{new Vector2(2.5f,1.5f),new Vector2(4.5f,1.5f),new Vector2(6.5f,2.5f),new Vector2(10,2.5f),new Vector2(13.5f,1.5f),new Vector2(15.5f,3.5f),new Vector2(19.5f,3.5f),new Vector2(21.5f,1.5f),new Vector2(23.5f,1.5f),new Vector2(24.5f,2.5f),new Vector2(27.5f,2.5f),new Vector2(29.5f,4.5f),new Vector2(33,4.5f),new Vector2(36.5f,2.5f),new Vector2(38.5f,3.5f),new Vector2(40.5f,3.5f),new Vector2(46.5f,2.5f)})Clone(coinTemplates[1].gameObject,"CocoComun",group.transform).transform.position=xy;
        Clone(coinTemplates[3].gameObject,"CocoBrillante",group.transform).transform.position=new Vector3(12.5f,4.5f,0);
        Clone(coinTemplates[5].gameObject,"CocoDorado",group.transform).transform.position=new Vector3(31.5f,4.5f,0);

        var cam=new GameObject("Main Camera",typeof(Camera),typeof(CameraFollow),typeof(AudioListener));cam.tag="MainCamera";cam.transform.position=new Vector3(8.6f,3.8f,-10);
        var camera=cam.GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=5;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.02f,.12f,.18f);
        var follow=cam.GetComponent<CameraFollow>();follow.objetivo=player.transform;follow.limiteIzquierdo=8.6f;follow.limiteDerecho=39.4f;
        var bg=new GameObject("FondoRuinas",typeof(SpriteRenderer));var sr=bg.GetComponent<SpriteRenderer>();sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"/Art/Fondo_demo.png");sr.sharedMaterial=material;sr.sortingOrder=-20;sr.color=new Color(.38f,.64f,.7f);bg.transform.position=new Vector3(24,4,0);bg.transform.localScale=new Vector3(50/sr.sprite.bounds.size.x,12/sr.sprite.bounds.size.y,1);

        EditorSceneManager.CloseScene(source,true);SceneManager.SetActiveScene(target);
        EditorSceneManager.SaveScene(target,Path03);
        var ordered=new[]{"Assets/Scenes/Nivel01_PerdidoEnElMar.unity",Path02,Path03};
        EditorBuildSettings.scenes=ordered.Select(p=>new EditorBuildSettingsScene(p,true)).Concat(EditorBuildSettings.scenes.Where(s=>!ordered.Contains(s.path))).ToArray();
        var audio=AssetDatabase.LoadAssetAtPath<GameAudioSettings>("Assets/Resources/WilsonAudioSettings.asset");audio.musicaNivel03=audio.musicaNivel02;EditorUtility.SetDirty(audio);AssetDatabase.SaveAssets();
        Validate();
    }
    static void Validate()
    {
        void Check(bool ok,string msg){if(!ok)throw new Exception("RUINAS: "+msg);}
        var player=Object.FindAnyObjectByType<WilsonController>();var manager=Object.FindAnyObjectByType<CoconutManager>();
        var coins=Object.FindObjectsByType<CoconutCollectible>();Check(coins.Length==19 && coins.Sum(c=>c.valor)==25,"19 cocos, 25 puntos");
        Check(Object.FindObjectsByType<CoralHazard>().Length==4,"4 peligros");Check(Object.FindObjectsByType<RespawnCheckpoint>().Length==1,"1 punto seguro");
        Check(manager.cocosNecesarios==25 && manager.textoContador!=null && manager.salidaNivel.GetComponent<LevelExit>().SiguienteEscena=="","objetivo y victoria final");
        Check(player.detectorSuelo.IsChildOf(player.transform) && player.GetComponent<PlayerRespawn>().estado!=null,"referencias del personaje");
        foreach(var col in Object.FindObjectsByType<TilemapCollider2D>())col.ProcessTilemapChanges();Physics2D.SyncTransforms();
        ValidateNavigation(player,coins);
        Check(safeNodes==reachableNodes,"puntos seguros alcanzables: "+reachableNodes+"/"+safeNodes);
        var cp=Object.FindAnyObjectByType<RespawnCheckpoint>();Check(Clear(cp.puntoReaparicion.position,player.GetComponent<Collider2D>().bounds.extents.y-.02f),"checkpoint despejado");
        Directory.CreateDirectory(Output);
        File.WriteAllText(Output+"/VERIFICACION_MAPA.txt","Unity "+Application.unityVersion+"\nNivel 3, 48 columnas: 19 cocos suman 25 puntos; 4 peligros; 1 checkpoint; controlador y detector conservados.\n"+reachableNodes+"/"+safeNodes+" posiciones seguras alcanzables; "+trajectorySamples+" muestras de trayectorias contra terreno y peligros.\nTodos los cocos tienen un punto de apoyo seguro accesible.\n");
    }
    static bool Clear(Vector2 position,float radius)=>!Physics2D.OverlapCircleAll(position,radius).Any(c=>c.gameObject.layer==3 || c.GetComponent<CoralHazard>()!=null);
    static void ValidateNavigation(WilsonController player,CoconutCollectible[] coins)
    {
        float radius=player.GetComponent<Collider2D>().bounds.extents.y;float testRadius=radius-.04f;
        var nodes=FloorCells.Where(p=>!FloorCells.Contains(p+Vector2Int.up)).Select(p=>new Vector2(p.x+.5f,p.y+1+radius)).Where(p=>Clear(p,testRadius)).OrderBy(p=>p.x).ThenBy(p=>p.y).ToList();
        safeNodes=nodes.Count;trajectorySamples=0;float g=Mathf.Abs(Physics2D.gravity.y)*player.GetComponent<Rigidbody2D>().gravityScale;
        bool Reach(Vector2 a,Vector2 b)
        {
            float dx=b.x-a.x,dy=b.y-a.y;
            if(Mathf.Abs(dx)<=1.01f && Mathf.Abs(dy)<.01f)
            {
                for(float k=0;k<=1;k+=.1f)if(!Clear(Vector2.Lerp(a,b,k),testRadius))return false;return true;
            }
            float disc=player.fuerzaSalto*player.fuerzaSalto-2*g*dy;if(disc<0)return false;
            float time=(player.fuerzaSalto+Mathf.Sqrt(disc))/g;if(Mathf.Abs(dx)>player.velocidad*time)return false;
            for(float t=.02f;t<time-.015f;t+=.02f)
            {
                var p=a+new Vector2(Mathf.Sign(dx)*Mathf.Min(Mathf.Abs(dx),player.velocidad*t),player.fuerzaSalto*t-.5f*g*t*t);trajectorySamples++;
                if(!Clear(p,testRadius))return false;
            }
            return true;
        }
        int start=nodes.FindIndex(n=>Mathf.Abs(n.x-1.5f)<.01f);if(start<0)throw new Exception("Inicio inseguro");
        var visited=new HashSet<int>{start};var queue=new Queue<int>();queue.Enqueue(start);
        while(queue.Count>0){int a=queue.Dequeue();for(int b=0;b<nodes.Count;b++)if(!visited.Contains(b) && Reach(nodes[a],nodes[b])){visited.Add(b);queue.Enqueue(b);}}
        reachableNodes=visited.Count;
        if(reachableNodes!=safeNodes)Debug.Log("No alcanzables: "+string.Join(",",nodes.Where((n,i)=>!visited.Contains(i))));
        foreach(var c in coins)if(!nodes.Where((n,i)=>visited.Contains(i)).Any(n=>Mathf.Abs(n.x-c.transform.position.x)<=.55f && Mathf.Abs(n.y-c.transform.position.y)<.3f))throw new Exception("Coco sin apoyo accesible: "+c.transform.position);
    }
    public static void RunBatch(){try{Build();Debug.Log("LEVEL03_BUILD_OK");EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}
