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

public static class Level04Builder
{
    public const string Path04="Assets/Scenes/Nivel04_LaPlaya.unity";
    const string Path03="Assets/Scenes/Nivel03_RuinasSumergidas.unity";
    const string Art="Assets/WilsonSubmarino32";
    const string Local="Assets/Art/Nivel04";
    static readonly string Output=System.IO.Path.GetFullPath("../cambios_nivel04");
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
    static Sprite Tropical(int n)=>AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Tiles/Nivel01/Set de sprites pixel art tropical.png").OfType<Sprite>().Single(s=>s.name.EndsWith("_"+n));
    static void Prop(string name,Sprite sprite,Vector2 foot,float height,int order)
    {
        var go=new GameObject(name,typeof(SpriteRenderer));var r=go.GetComponent<SpriteRenderer>();
        r.sprite=sprite;r.sharedMaterial=material;r.sortingOrder=order;
        float scale=height/sprite.bounds.size.y;go.transform.localScale=Vector3.one*scale;
        go.transform.position=new Vector3(foot.x,foot.y-sprite.bounds.min.y*scale,0);
    }
    static int Height(int x)
    {
        if(x<0 || x>=52)return 0;
        if(x>=8 && x<=9 || x>=17 && x<=18 || x>=26 && x<=27 || x>=35 && x<=37 || x>=44 && x<=45)return 0;
        if(x<8)return 1;if(x<28)return 2;if(x<35)return 3;return 2;
    }
    public static void AddPanels(Canvas canvas=null)
    {
        if(canvas==null)canvas=Object.FindAnyObjectByType<Canvas>();
        foreach(bool top in new[]{true,false}){
            string name=top?"BandaHUD":"BandaObjetivo";
            if(canvas.transform.Find(name)!=null)continue;
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(canvas.transform,false);go.transform.SetAsFirstSibling();
            var r=go.GetComponent<RectTransform>();r.anchorMin=new Vector2(0,top?1:0);r.anchorMax=new Vector2(1,top?1:0);r.pivot=new Vector2(.5f,top?1:0);r.anchoredPosition=Vector2.zero;r.sizeDelta=new Vector2(0,top?105:60);
            var image=go.GetComponent<Image>();image.color=new Color(.025f,.13f,.17f,.65f);image.raycastTarget=false;
        }
    }
    public static void PolishBatch(){EditorSceneManager.OpenScene(Path04);AddPanels();EditorSceneManager.SaveScene(SceneManager.GetActiveScene());Level04Preview.Run();}
    public static void Build()
    {
        if(File.Exists(Path04))throw new Exception("El nivel 4 ya existe; no se sobrescribe.");
        Directory.CreateDirectory(Local);AssetDatabase.Refresh();
        var source=EditorSceneManager.OpenScene(Path03);
        var playerTemplate=Object.FindAnyObjectByType<WilsonController>();
        var pointTemplate=Object.FindAnyObjectByType<RespawnCheckpoint>();
        var exitTemplate=Object.FindAnyObjectByType<LevelExit>();
        var coinTemplates=Object.FindObjectsByType<CoconutCollectible>().GroupBy(c=>c.valor).ToDictionary(g=>g.Key,g=>g.First());
        if(playerTemplate==null || pointTemplate==null || exitTemplate==null)throw new Exception("Faltan objetos del nivel 3.");
        float radius=playerTemplate.GetComponent<Collider2D>().bounds.extents.y;
        target=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(target);
        material=AssetDatabase.LoadAssetAtPath<Material>(Art+"/Materials/SubmarinoUnlit.mat");
        var shader=AssetDatabase.LoadAssetAtPath<Shader>(Local+"/BeachSand.shader");
        if(shader==null || UnityEditor.ShaderUtil.ShaderHasError(shader))throw new Exception("Shader de arena invalido.");
        var beach=new Material(shader);AssetDatabase.CreateAsset(beach,Local+"/ArenaPlaya.mat");
        var grid=new GameObject("Grid_Playa",typeof(Grid));grid.GetComponent<Grid>().cellSize=new Vector3(1,1,0);
        var floor=Map("ArenaNivel04",grid.transform,0,true);floor.GetComponent<TilemapRenderer>().sharedMaterial=beach;
        var planks=Map("MuelleDeMadera",grid.transform,0,true);
        var decor=Map("ConchasDePlaya",grid.transform,2,false);
        var arena=AssetDatabase.LoadAssetAtPath<RuleTile>(Art+"/RuleTiles/Auto_arena.asset");
        FloorCells.Clear();
        for(int x=0;x<52;x++)if(Height(x)>0)for(int y=-2;y<Height(x);y++){floor.SetTile(new Vector3Int(x,y,0),arena);FloorCells.Add(new Vector2Int(x,y));}
        // A separate upper jetty offers the bright coconut without closing the gap below it.
        var woodSprite=Tropical(57);
        var wood=ScriptableObject.CreateInstance<Tile>();wood.name="Muelle";wood.sprite=woodSprite;wood.colliderType=UnityEngine.Tilemaps.Tile.ColliderType.Grid;
        AssetDatabase.CreateAsset(wood,Local+"/Muelle.asset");
        // Collider cells support a fitted visual made from the existing wooden pier sprite.
        for(int x=21;x<=23;x++){planks.SetTile(new Vector3Int(x,3,0),wood);FloorCells.Add(new Vector2Int(x,3));}
        planks.GetComponent<TilemapRenderer>().enabled=false;
        var pier=new GameObject("VisualMuelle",typeof(SpriteRenderer));var ps=pier.GetComponent<SpriteRenderer>();ps.sprite=woodSprite;ps.sharedMaterial=material;ps.sortingOrder=1;
        pier.transform.localScale=new Vector3(3/woodSprite.bounds.size.x,1/woodSprite.bounds.size.y,1);pier.transform.position=new Vector3(22.5f,3.5f,0);
        foreach(var d in new[]{(3,"concha_abanico"),(6,"estrella_naranja"),(11,"concha_espiral"),(15,"concha_abanico"),(20,"estrella_naranja"),(25,"concha_espiral"),(29,"concha_abanico"),(33,"estrella_naranja"),(39,"concha_espiral"),(42,"concha_abanico"),(47,"estrella_naranja")})decor.SetTile(new Vector3Int(d.Item1,Height(d.Item1),0),Tile(d.Item2));
        foreach(int x in new[]{4,14,24,31,41,48})Prop("PalmeraDecorativa_SinColision",Tropical(x%2==0?65:67),new Vector2(x+.3f,Height(x)-.05f),3.4f,-2);
        foreach(int x in new[]{1,12,19,30,40,49})Prop("VegetacionCostera_SinColision",Tropical(63),new Vector2(x+.25f,Height(x)-.02f),.6f,-1);
        var wall=Map("LimitesLaterales",grid.transform,0,true);wall.GetComponent<TilemapRenderer>().enabled=false;
        var boundary=AssetDatabase.LoadAssetAtPath<Tile>(Art+"/Tiles/roca_255.asset");
        for(int y=-2;y<10;y++){wall.SetTile(new Vector3Int(-1,y,0),boundary);wall.SetTile(new Vector3Int(52,y,0),boundary);}
        var zone=new GameObject("ZonaCaidaNivel04",typeof(BoxCollider2D),typeof(FallZone));zone.transform.position=new Vector3(26,-6,0);zone.GetComponent<BoxCollider2D>().isTrigger=true;zone.GetComponent<BoxCollider2D>().size=new Vector2(58,6);
        var player=Clone(playerTemplate.gameObject,"Wilson");player.tag="Player";player.transform.position=new Vector3(1.5f,1+radius+.05f,0);
        var controller=player.GetComponent<WilsonController>();controller.enabled=true;controller.ReiniciarMovimiento();player.GetComponent<Rigidbody2D>().simulated=true;
        var canvas=new GameObject("Canvas",typeof(Canvas),typeof(CanvasScaler));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
        var count=Text(canvas.transform,"ContadorCocos","Cocos: 0 / 30",new Vector2(0,1),new Vector2(20,-18),new Vector2(350,44),28,TextAlignmentOptions.Left);
        Text(canvas.transform,"TituloNivel","Nivel 4: La playa",Vector2.one,new Vector2(-20,-18),new Vector2(550,44),28,TextAlignmentOptions.Right);
        var hint=Text(canvas.transform,"Objetivo","¡Tierra firme! Salta los huecos y reúne 30 puntos.",Vector2.zero,new Vector2(20,18),new Vector2(1100,40),22,TextAlignmentOptions.Left);
        player.GetComponent<PlayerRespawn>().estado=hint;AddPanels(canvas.GetComponent<Canvas>());
        var manager=new GameObject("GameManager",typeof(CoconutManager)).GetComponent<CoconutManager>();manager.cocosNecesarios=30;manager.textoContador=count;
        var exit=Clone(exitTemplate.gameObject,"SalidaNivel04");exit.transform.position=new Vector3(50.5f,3,0);exit.SetActive(true);
        foreach(Transform child in exit.transform.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
        foreach(var renderer in exit.GetComponents<SpriteRenderer>())renderer.enabled=false;
        SetString(exit.GetComponent<LevelExit>(),"nombreNivel","La playa");SetString(exit.GetComponent<LevelExit>(),"siguienteEscena","");exit.GetComponent<LevelExit>().alCompletarNivel=new UnityEvent();manager.salidaNivel=exit;
        // A wooden sign replaces the underwater cave at the beach exit.
        Prop("CartelSalida",Tropical(66),new Vector2(50.5f,2),1.7f,3);
        var sign=GameObject.Find("CartelSalida");sign.transform.SetParent(exit.transform,true);
        var checkpoint=Clone(pointTemplate.gameObject,"CheckpointNivel04");checkpoint.transform.position=new Vector3(24.5f,3,0);
        var cp=checkpoint.GetComponent<RespawnCheckpoint>();cp.puntoReaparicion.position=new Vector3(24.5f,2+radius+.05f,0);cp.textoMarcador.text="Punto seguro";cp.textoMarcador.color=new Color(1,.97f,.7f);cp.indicador.color=new Color(.55f,.9f,.7f);
        var indicator=cp.indicador;indicator.sprite=AssetDatabase.LoadAssetAtPath<Tile>(Art+"/Tiles/concha_abanico.asset").sprite;
        var group=new GameObject("Collectibles");
        foreach(var xy in new[]{new Vector2(2.5f,1.5f),new Vector2(4.5f,1.5f),new Vector2(6.5f,1.5f),new Vector2(7.5f,1.5f),new Vector2(10.5f,2.5f),new Vector2(12.5f,2.5f),new Vector2(14.5f,2.5f),new Vector2(16.5f,2.5f),new Vector2(19.5f,2.5f),new Vector2(20.5f,2.5f),new Vector2(24.5f,2.5f),new Vector2(25.5f,2.5f),new Vector2(28.5f,3.5f),new Vector2(30.5f,3.5f),new Vector2(34.5f,3.5f),new Vector2(38.5f,2.5f),new Vector2(40.5f,2.5f),new Vector2(42.5f,2.5f),new Vector2(43.5f,2.5f),new Vector2(46.5f,2.5f),new Vector2(48.5f,2.5f),new Vector2(50.5f,2.5f)})Clone(coinTemplates[1].gameObject,"CocoComun",group.transform).transform.position=xy;
        Clone(coinTemplates[3].gameObject,"CocoBrillante",group.transform).transform.position=new Vector3(22.5f,4.5f,0);
        Clone(coinTemplates[5].gameObject,"CocoDorado",group.transform).transform.position=new Vector3(32.5f,3.5f,0);
        var cam=new GameObject("Main Camera",typeof(Camera),typeof(CameraFollow),typeof(AudioListener));cam.tag="MainCamera";cam.transform.position=new Vector3(8.6f,3.8f,-10);
        var camera=cam.GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=5;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.15f,.75f,1);
        var follow=cam.GetComponent<CameraFollow>();follow.objetivo=player.transform;follow.limiteIzquierdo=8.6f;follow.limiteDerecho=43.4f;
        // Repeat the same horizon without stretching it across the entire level.
        var bgSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Nivel01/Horizonte tropical pixelado.png");
        for(int i=0;i<3;i++){
            var bg=new GameObject("HorizontePlaya_"+i,typeof(SpriteRenderer));var r=bg.GetComponent<SpriteRenderer>();r.sprite=bgSprite;r.sharedMaterial=material;r.sortingOrder=-20;
            float height=12,width=height*bgSprite.bounds.size.x/bgSprite.bounds.size.y;
            bg.transform.localScale=Vector3.one*(height/bgSprite.bounds.size.y);bg.transform.position=new Vector3(width*(i-.0f),4,0);
        }
        EditorSceneManager.CloseScene(source,true);SceneManager.SetActiveScene(target);
        EditorSceneManager.SaveScene(target,Path04);
        var ordered=new[]{"Assets/Scenes/Nivel01_PerdidoEnElMar.unity","Assets/Scenes/Nivel02_CavernaDeCoral.unity",Path03,Path04};
        EditorBuildSettings.scenes=ordered.Select(p=>new EditorBuildSettingsScene(p,true)).Concat(EditorBuildSettings.scenes.Where(s=>!ordered.Contains(s.path))).ToArray();
        var audio=AssetDatabase.LoadAssetAtPath<GameAudioSettings>("Assets/Resources/WilsonAudioSettings.asset");audio.musicaNivel04=audio.musicaNivel01;EditorUtility.SetDirty(audio);AssetDatabase.SaveAssets();
        Validate();
    }
    static void Validate()
    {
        var coins=Object.FindObjectsByType<CoconutCollectible>();if(coins.Length!=24 || coins.Sum(c=>c.valor)!=30)throw new Exception("Cocos incorrectos");
        foreach(var c in Object.FindObjectsByType<TilemapCollider2D>())c.ProcessTilemapChanges();Physics2D.SyncTransforms();
        ValidateNavigation(Object.FindAnyObjectByType<WilsonController>(),coins);
        if(safeNodes!=reachableNodes)throw new Exception("Apoyos inaccesibles: "+reachableNodes+"/"+safeNodes);
        Directory.CreateDirectory(Output);File.WriteAllText(Output+"/VERIFICACION_MAPA.txt","Unity "+Application.unityVersion+"\n52 columnas, cinco huecos, 24 cocos que suman 30 puntos.\n"+reachableNodes+"/"+safeNodes+" apoyos seguros accesibles; "+trajectorySamples+" muestras de trayectorias. Todos los cocos tienen un apoyo accesible. Modelo balistico con tolerancia de radio de 0.04.\n");
    }
    public static void RunBatch(){try{Build();Debug.Log("LEVEL04_BUILD_OK");EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
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
            foreach(float delay in new[]{0f,.08f,.16f,.24f})
            {
                if(Mathf.Abs(dx)>player.velocidad*(time-delay))continue;
                bool clear=true;
                for(float t=.01f;t<time-.01f;t+=.01f)
                {
                    var p=a+new Vector2(Mathf.Sign(dx)*Mathf.Min(Mathf.Abs(dx),player.velocidad*Mathf.Max(0,t-delay)),player.fuerzaSalto*t-.5f*g*t*t);trajectorySamples++;
                    if(!Clear(p,testRadius)){clear=false;break;}
                }
                if(clear)return true;
            }
            return false;
        }
        int start=nodes.FindIndex(n=>Mathf.Abs(n.x-1.5f)<.01f);if(start<0)throw new Exception("Inicio inseguro");
        var visited=new HashSet<int>{start};var queue=new Queue<int>();queue.Enqueue(start);
        while(queue.Count>0){int a=queue.Dequeue();for(int b=0;b<nodes.Count;b++)if(!visited.Contains(b) && Reach(nodes[a],nodes[b])){visited.Add(b);queue.Enqueue(b);}}
        reachableNodes=visited.Count;
        if(reachableNodes!=safeNodes)Debug.Log("No alcanzables: "+string.Join(",",nodes.Where((n,i)=>!visited.Contains(i))));
        foreach(var c in coins)if(!nodes.Where((n,i)=>visited.Contains(i)).Any(n=>Mathf.Abs(n.x-c.transform.position.x)<=.55f && Mathf.Abs(n.y-c.transform.position.y)<.3f))throw new Exception("Coco sin apoyo accesible: "+c.transform.position);
    }
}
