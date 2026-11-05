using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Object=UnityEngine.Object;

public static class Level02Builder
{
    public const string ScenePath="Assets/Scenes/Nivel02_CavernaDeCoral.unity";
    const string FirstPath="Assets/Scenes/Nivel01_PerdidoEnElMar.unity";
    const string Art="Assets/WilsonSubmarino32";
    static readonly string Output=Path.GetFullPath("../cambios_nivel02");
    static readonly int[] Heights={1,1,1,1,1,1,1,2,2,2,2,1,1,1,1,3,3,3,3,1,1,1,1,2,2,2,2,4,4,4,4,4,2,2,2,2,3,3,3,3};
    static Material unlit;
    static HashSet<Vector2Int> floorCells=new HashSet<Vector2Int>();
    static List<Vector2> coinPositions=new List<Vector2>();
    static List<Vector2> navigationNodes=new List<Vector2>();
    static int reachableNodes;

    static Tilemap Map(string name,Transform parent,int order,bool solid)
    {
        var go=new GameObject(name,typeof(Tilemap),typeof(TilemapRenderer));go.transform.SetParent(parent,false);
        var sr=go.GetComponent<TilemapRenderer>();sr.sortingOrder=order;sr.sharedMaterial=unlit;
        if(solid){go.layer=LayerMask.NameToLayer("Ground");go.AddComponent<TilemapCollider2D>();go.AddComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;}
        return go.GetComponent<Tilemap>();
    }
    static TMP_Text Text(Transform canvas,string name,string content,Vector2 anchor,Vector2 position,Vector2 size,int fontSize,TextAlignmentOptions align)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(canvas,false);
        var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.pivot=anchor;r.anchoredPosition=position;r.sizeDelta=size;
        var t=go.GetComponent<TextMeshProUGUI>();t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        t.text=content;t.fontSize=fontSize;t.alignment=align;t.color=Color.white;t.raycastTarget=false;return t;
    }
    static Tile Tile(string name)=>AssetDatabase.LoadAssetAtPath<Tile>(Art+"/Tiles/"+name+".asset");
    static void Collectible(string prefab,Vector2 position,Transform parent)
    {
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Collectibles/"+prefab+".prefab");
        var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.transform.SetParent(parent);go.transform.position=position;
        foreach(var sr in go.GetComponentsInChildren<SpriteRenderer>()){sr.sharedMaterial=unlit;sr.sortingOrder=4;}
        coinPositions.Add(position);
    }
    static void SetString(Object target,string property,string value)
    {
        var so=new SerializedObject(target);so.FindProperty(property).stringValue=value;so.ApplyModifiedPropertiesWithoutUndo();
    }
    public static void Build()
    {
        if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        // Never silently overwrite an authored level.
        if(File.Exists(ScenePath))throw new InvalidOperationException("El nivel 2 ya existe. Editarlo directamente; el generador no lo sobrescribe.");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        floorCells.Clear();coinPositions.Clear();
        unlit=AssetDatabase.LoadAssetAtPath<Material>(Art+"/Materials/SubmarinoUnlit.mat");
        var grid=new GameObject("Grid_Caverna",typeof(Grid));grid.GetComponent<Grid>().cellSize=new Vector3(1,1,0);
        var terrain=Map("TerrenoNivel02",grid.transform,0,true);
        var ceiling=Map("TechoNivel02",grid.transform,0,true);
        var decor=Map("DecoracionNivel02",grid.transform,2,false);
        var arena=AssetDatabase.LoadAssetAtPath<RuleTile>(Art+"/RuleTiles/Auto_arena.asset");
        var rock=AssetDatabase.LoadAssetAtPath<RuleTile>(Art+"/RuleTiles/Auto_roca.asset");
        for(int x=0;x<40;x++)for(int y=-2;y<Heights[x];y++)floorCells.Add(new Vector2Int(x,y));
        // Upper branches remain reachable from the two adjacent steps.
        for(int x=11;x<=13;x++)floorCells.Add(new Vector2Int(x,3));
        for(int x=20;x<=22;x++)floorCells.Add(new Vector2Int(x,3));
        foreach(var p in floorCells)terrain.SetTile(new Vector3Int(p.x,p.y,0),arena);
        for(int x=-1;x<=40;x++)for(int y=8;y<=9;y++)ceiling.SetTile(new Vector3Int(x,y,0),rock);
        for(int y=-2;y<8;y++){ceiling.SetTile(new Vector3Int(-1,y,0),rock);ceiling.SetTile(new Vector3Int(40,y,0),rock);}
        // Ceiling rock teeth have no separate invisible colliders.
        foreach(int x in new[]{3,4,16,17,34,35})ceiling.SetTile(new Vector3Int(x,7,0),rock);
        foreach(var d in new[]{(3,"coral_rosa"),(5,"alga_turquesa"),(8,"esponja_alta"),(13,"alga_verde"),(16,"coral_violeta"),(18,"alga_corta"),(24,"coral_rosa"),(26,"esponja_baja"),(28,"alga_turquesa"),(31,"coral_violeta"),(34,"estrella_naranja"),(36,"alga_verde")})
            decor.SetTile(new Vector3Int(d.Item1,Heights[d.Item1],0),Tile(d.Item2));
        foreach(int x in new[]{6,14,19,25,32})decor.SetTile(new Vector3Int(x,5,0),Tile("burbujas_a"));
        terrain.RefreshAllTiles();ceiling.RefreshAllTiles();

        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Wilson.prefab");
        var player=(GameObject)PrefabUtility.InstantiatePrefab(prefab);player.name="Wilson";player.tag="Player";
        player.transform.position=new Vector3(1.5f,1.55f,0);
        var controller=player.GetComponent<WilsonController>()??player.AddComponent<WilsonController>();controller.enabled=true;
        controller.velocidad=6;controller.fuerzaSalto=12;controller.capaSuelo=1<<LayerMask.NameToLayer("Ground");controller.radioDeteccion=.15f;
        var detector=new GameObject("DetectorSuelo");detector.transform.SetParent(player.transform,false);detector.transform.localPosition=new Vector3(0,-.45f,0);controller.detectorSuelo=detector.transform;
        var body=player.GetComponent<Rigidbody2D>();body.gravityScale=3;body.simulated=true;body.constraints=RigidbodyConstraints2D.FreezeRotation;
        foreach(var sr in player.GetComponentsInChildren<SpriteRenderer>()){sr.sharedMaterial=unlit;sr.sortingOrder=5;}

        var camera=new GameObject("Main Camera",typeof(Camera),typeof(CameraFollow));camera.tag="MainCamera";
        camera.transform.position=new Vector3(8.6f,3.8f,-10);var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=5f;cam.backgroundColor=new Color(.025f,.09f,.17f);cam.clearFlags=CameraClearFlags.SolidColor;
        var follow=camera.GetComponent<CameraFollow>();follow.objetivo=player.transform;follow.limiteIzquierdo=8.6f;follow.limiteDerecho=31.4f;
        var bg=new GameObject("FondoCaverna",typeof(SpriteRenderer));var bgRenderer=bg.GetComponent<SpriteRenderer>();
        bgRenderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"/Art/Fondo_demo.png");bgRenderer.color=new Color(.36f,.48f,.7f);bgRenderer.sortingOrder=-20;bgRenderer.sharedMaterial=unlit;
        bg.transform.position=new Vector3(20,4,0);bg.transform.localScale=new Vector3(42f/bgRenderer.sprite.bounds.size.x,12f/bgRenderer.sprite.bounds.size.y,1);

        var canvas=new GameObject("Canvas",typeof(Canvas),typeof(CanvasScaler));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
        var counter=Text(canvas.transform,"ContadorCocos","Cocos: 0 / 20",new Vector2(0,1),new Vector2(20,-18),new Vector2(350,44),28,TextAlignmentOptions.Left);
        Text(canvas.transform,"TituloNivel","Nivel 2: Caverna de coral",Vector2.one,new Vector2(-20,-18),new Vector2(550,44),28,TextAlignmentOptions.Right);
        Text(canvas.transform,"Objetivo","Reúne 20 puntos de cocos y encuentra la salida.",Vector2.zero,new Vector2(20,18),new Vector2(1000,40),22,TextAlignmentOptions.Left);
        var manager=new GameObject("GameManager",typeof(CoconutManager)).GetComponent<CoconutManager>();manager.cocosNecesarios=20;manager.textoContador=counter;

        var exit=new GameObject("SalidaNivel02",typeof(LevelExit));exit.transform.position=new Vector3(38.5f,4f,0);
        var trigger=exit.GetComponent<BoxCollider2D>();trigger.isTrigger=true;trigger.size=new Vector2(1,1.7f);trigger.offset=new Vector2(0,-.1f);
        var levelExit=exit.GetComponent<LevelExit>();SetString(levelExit,"nombreNivel","Caverna de coral");SetString(levelExit,"siguienteEscena","");
        var visual=new GameObject("VisualCueva",typeof(SpriteRenderer));visual.transform.SetParent(exit.transform,false);visual.transform.localPosition=new Vector3(0,-1,0);
        var cave=visual.GetComponent<SpriteRenderer>();cave.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(LevelExitCaveImporter.SpritePath);cave.sharedMaterial=unlit;cave.sortingOrder=1;
        manager.salidaNivel=exit;

        var collectibles=new GameObject("Collectibles");
        foreach(var p in new[]{new Vector2(2.5f,1.65f),new Vector2(4.5f,1.65f),new Vector2(8.5f,2.65f),new Vector2(10,2.65f),new Vector2(13.5f,1.65f),new Vector2(16,3.65f),new Vector2(18,3.65f),new Vector2(20.5f,4.65f),new Vector2(24.5f,2.65f),new Vector2(27.5f,4.65f),new Vector2(33.5f,2.65f),new Vector2(37,3.65f)})Collectible("CocoComun",p,collectibles.transform);
        Collectible("CocoBrillante",new Vector2(12,4.65f),collectibles.transform);
        Collectible("CocoDorado",new Vector2(30,4.65f),collectibles.transform);
        EditorSceneManager.SaveScene(scene,ScenePath);
        var original=EditorBuildSettings.scenes.Where(s=>s.path!=FirstPath && s.path!=ScenePath);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(FirstPath,true),new EditorBuildSettingsScene(ScenePath,true)}.Concat(original).ToArray();
        AssetDatabase.SaveAssets();Validate(scene);
    }

    static void Validate(Scene scene)
    {
        void Check(bool ok,string msg){if(!ok)throw new Exception("NIVEL02: "+msg);}
        var player=Object.FindAnyObjectByType<WilsonController>();var manager=Object.FindAnyObjectByType<CoconutManager>();
        var coins=Object.FindObjectsByType<CoconutCollectible>(FindObjectsSortMode.None);
        Check(coins.Length==14 && coins.Sum(c=>c.valor)==20,"14 cocos con valor total 20");
        Check(manager.cocosNecesarios==20 && manager.textoContador!=null && manager.salidaNivel!=null,"referencias del manager");
        Check(player.CompareTag("Player") && player.detectorSuelo.IsChildOf(player.transform),"jugador y detector");
        Check(player.capaSuelo.value==(1<<3),"Ground");Check(manager.salidaNivel.GetComponent<LevelExit>().SiguienteEscena=="","nivel final sin siguiente escena inexistente");
        foreach(var c in coins)Check(c.GetComponent<Collider2D>().isTrigger,"collider del coco");
        Physics2D.SyncTransforms();foreach(var map in Object.FindObjectsByType<TilemapCollider2D>(FindObjectsSortMode.None))map.ProcessTilemapChanges();Physics2D.SyncTransforms();
        ValidateReachability(player);
        Check(reachableNodes==navigationNodes.Count,"todos los puntos de apoyo alcanzables");
        foreach(var p in coinPositions)
            Check(navigationNodes.Any(n=>Mathf.Abs(n.x-p.x)<=.55f && Mathf.Abs(n.y-p.y)<.3f),"coco sobre una plataforma alcanzable: "+p);
        Check(navigationNodes.Any(n=>n.x>37 && n.y>3),"salida alcanzable");
        Directory.CreateDirectory(Output);
        string report="Unity "+Application.unityVersion+"\nEscena: "+ScenePath+"\n14 cocos: 12 comunes (1), 1 brillante (3), 1 dorado (5): total 20.\nWilson: velocidad 6, salto 12, gravedad 3; Ground y detector enlazados.\n"+reachableNodes+" / "+navigationNodes.Count+" puntos de apoyo alcanzables en el grafo de salto; arcos muestreados contra colliders reales.\nReferencias de contador, salida, sprites, camara y lista de escenas configuradas.\n";
        File.WriteAllText(Output+"/VERIFICACION_NIVEL02.txt",report);Debug.Log("LEVEL02_STATIC_VALIDATION_OK\n"+report);
    }
    static void ValidateReachability(WilsonController player)
    {
        navigationNodes.Clear();
        foreach(var p in floorCells)if(!floorCells.Contains(p+Vector2Int.up))navigationNodes.Add(new Vector2(p.x+.5f,p.y+1.5f));
        navigationNodes=navigationNodes.OrderBy(n=>n.x).ThenBy(n=>n.y).ToList();
        float gravity=Mathf.Abs(Physics2D.gravity.y)*player.GetComponent<Rigidbody2D>().gravityScale;
        float jump=player.fuerzaSalto,speed=player.velocidad;
        bool Clear(Vector2 p)=>Physics2D.OverlapCircle(p,.46f,1<<3)==null;
        bool CanReach(Vector2 a,Vector2 b)
        {
            float dx=b.x-a.x,dy=b.y-a.y;
            if(Mathf.Abs(dx)<1.01f && Mathf.Abs(dy)<.01f)return true;
            float discr=jump*jump-2*gravity*dy;if(discr<0)return false;
            float time=(jump+Mathf.Sqrt(discr))/gravity;
            if(Mathf.Abs(dx)>speed*time)return false;
            // Horizontal input at full speed, then released to land on the target.
            for(float t=.025f;t<time-.015f;t+=.025f)
            {
                float x=a.x+Mathf.Sign(dx)*Mathf.Min(Mathf.Abs(dx),speed*t);
                float y=a.y+jump*t-.5f*gravity*t*t;
                if(!Clear(new Vector2(x,y)))return false;
            }
            return true;
        }
        int start=navigationNodes.FindIndex(n=>Mathf.Abs(n.x-1.5f)<.01f && Mathf.Abs(n.y-1.5f)<.01f);
        var visited=new HashSet<int>{start};var queue=new Queue<int>();queue.Enqueue(start);
        while(queue.Count>0){int a=queue.Dequeue();for(int b=0;b<navigationNodes.Count;b++)if(!visited.Contains(b) && CanReach(navigationNodes[a],navigationNodes[b])){visited.Add(b);queue.Enqueue(b);}}
        reachableNodes=visited.Count;
        if(visited.Count!=navigationNodes.Count)Debug.Log("Unreachable: "+string.Join(", ",navigationNodes.Where((n,i)=>!visited.Contains(i))));
    }
    public static void RunBatch()
    {
        try{Build();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
