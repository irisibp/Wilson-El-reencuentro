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

public static class Level05Builder
{
    public const string Path05="Assets/Scenes/Nivel05_CiudadCostera.unity";
    const string Path04="Assets/Scenes/Nivel04_LaPlaya.unity";
    const string Art="Assets/WilsonSubmarino32";
    const string Local="Assets/Art/Nivel05";
    static readonly string Output=System.IO.Path.GetFullPath("../cambios_nivel05");
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
        if(x<0 || x>=60)return 0;
        if(x>=8 && x<=9 || x>=18 && x<=19 || x>=28 && x<=29 || x>=38 && x<=40 || x>=48 && x<=49)return 0;
        if(x<8)return 1;if(x<30)return 2;if(x<38)return 3;return 2;
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
    public static void Build()
    {
        if(File.Exists(Path05))throw new Exception("El nivel 5 ya existe; no se sobrescribe.");
        Directory.CreateDirectory(Local);AssetDatabase.Refresh();
        ImportChuck();
        var source=EditorSceneManager.OpenScene(Path04);
        var playerTemplate=Object.FindAnyObjectByType<WilsonController>();
        var pointTemplate=Object.FindAnyObjectByType<RespawnCheckpoint>();
        var exitTemplate=Object.FindAnyObjectByType<LevelExit>();
        var coinTemplates=Object.FindObjectsByType<CoconutCollectible>().GroupBy(c=>c.valor).ToDictionary(g=>g.Key,g=>g.First());
        if(playerTemplate==null || pointTemplate==null || exitTemplate==null)throw new Exception("Faltan objetos del nivel 4.");
        float radius=playerTemplate.GetComponent<Collider2D>().bounds.extents.y;
        target=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(target);
        material=AssetDatabase.LoadAssetAtPath<Material>(Art+"/Materials/SubmarinoUnlit.mat");
        var beach=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Nivel04/ArenaPlaya.mat");
        if(beach==null)throw new Exception("Falta la arena del nivel 4.");
        var grid=new GameObject("Grid_CiudadCostera",typeof(Grid));grid.GetComponent<Grid>().cellSize=new Vector3(1,1,0);
        var floor=Map("ArenaNivel05",grid.transform,0,true);floor.GetComponent<TilemapRenderer>().sharedMaterial=beach;
        var planks=Map("MuelleDeMadera",grid.transform,0,true);
        var decor=Map("ConchasDePlaya",grid.transform,2,false);
        var arena=AssetDatabase.LoadAssetAtPath<RuleTile>(Art+"/RuleTiles/Auto_arena.asset");
        FloorCells.Clear();
        for(int x=0;x<60;x++)if(Height(x)>0)for(int y=-2;y<Height(x);y++){floor.SetTile(new Vector3Int(x,y,0),arena);FloorCells.Add(new Vector2Int(x,y));}
        // A separate upper jetty offers the bright coconut without closing the gap below it.
        var woodSprite=Tropical(57);
        var wood=ScriptableObject.CreateInstance<Tile>();wood.name="Muelle";wood.sprite=woodSprite;wood.colliderType=UnityEngine.Tilemaps.Tile.ColliderType.Grid;
        AssetDatabase.CreateAsset(wood,Local+"/Muelle.asset");
        // Collider cells support a fitted visual made from the existing wooden pier sprite.
        for(int x=23;x<=25;x++){planks.SetTile(new Vector3Int(x,3,0),wood);FloorCells.Add(new Vector2Int(x,3));}
        planks.GetComponent<TilemapRenderer>().enabled=false;
        var pier=new GameObject("VisualMuelle",typeof(SpriteRenderer));var ps=pier.GetComponent<SpriteRenderer>();ps.sprite=woodSprite;ps.sharedMaterial=material;ps.sortingOrder=1;
        pier.transform.localScale=new Vector3(3/woodSprite.bounds.size.x,1/woodSprite.bounds.size.y,1);pier.transform.position=new Vector3(24.5f,3.5f,0);
        foreach(var d in new[]{(5,"concha_abanico"),(11,"concha_espiral"),(17,"estrella_naranja"),(27,"concha_abanico"),(35,"concha_espiral"),(46,"estrella_naranja"),(53,"concha_abanico")})decor.SetTile(new Vector3Int(d.Item1,Height(d.Item1),0),Tile(d.Item2));
        foreach(int x in new[]{5,16,26,36,46,54})Prop("PalmeraCostera_SinColision",Tropical(x%2==0?65:67),new Vector2(x+.3f,Height(x)-.05f),3.2f,-3);
        CityGeometry.Build(Height);
        var wall=Map("LimitesLaterales",grid.transform,0,true);wall.GetComponent<TilemapRenderer>().enabled=false;
        var boundary=AssetDatabase.LoadAssetAtPath<Tile>(Art+"/Tiles/roca_255.asset");
        for(int y=-2;y<10;y++){wall.SetTile(new Vector3Int(-1,y,0),boundary);wall.SetTile(new Vector3Int(60,y,0),boundary);}
        var zone=new GameObject("ZonaCaidaNivel05",typeof(BoxCollider2D),typeof(FallZone));zone.transform.position=new Vector3(30,-6,0);zone.GetComponent<BoxCollider2D>().isTrigger=true;zone.GetComponent<BoxCollider2D>().size=new Vector2(66,6);
        var player=Clone(playerTemplate.gameObject,"Wilson");player.tag="Player";player.transform.position=new Vector3(1.5f,1+radius+.05f,0);
        var controller=player.GetComponent<WilsonController>();controller.enabled=true;controller.ReiniciarMovimiento();player.GetComponent<Rigidbody2D>().simulated=true;
        var canvas=new GameObject("Canvas",typeof(Canvas),typeof(CanvasScaler));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
        var count=Text(canvas.transform,"ContadorCocos","Cocos: 0 / 35",new Vector2(0,1),new Vector2(20,-18),new Vector2(350,44),28,TextAlignmentOptions.Left);
        Text(canvas.transform,"TituloNivel","Nivel 5: Ciudad costera",Vector2.one,new Vector2(-20,-18),new Vector2(550,44),28,TextAlignmentOptions.Right);
        var hint=Text(canvas.transform,"Objetivo","Reúne 35 puntos y encuentra a Chuck al final del paseo.",Vector2.zero,new Vector2(20,18),new Vector2(1100,40),22,TextAlignmentOptions.Left);
        player.GetComponent<PlayerRespawn>().estado=hint;AddPanels(canvas.GetComponent<Canvas>());
        var manager=new GameObject("GameManager",typeof(CoconutManager)).GetComponent<CoconutManager>();manager.cocosNecesarios=35;manager.textoContador=count;
        var exit=Clone(exitTemplate.gameObject,"SalidaNivel05");exit.transform.position=new Vector3(57.1f,3,0);exit.SetActive(true);
        foreach(Transform child in exit.transform.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
        foreach(var renderer in exit.GetComponents<SpriteRenderer>())renderer.enabled=false;
        SetString(exit.GetComponent<LevelExit>(),"nombreNivel","El reencuentro");SetString(exit.GetComponent<LevelExit>(),"siguienteEscena","");exit.GetComponent<LevelExit>().alCompletarNivel=new UnityEvent();manager.salidaNivel=exit;
        var exitSO=new SerializedObject(exit.GetComponent<LevelExit>());exitSO.FindProperty("finalDelJuego").boolValue=true;exitSO.ApplyModifiedPropertiesWithoutUndo();
        var chuck=new GameObject("Chuck");chuck.transform.position=new Vector3(58,2,0);
        var visual=new GameObject("ChuckVisual",typeof(SpriteRenderer));visual.transform.SetParent(chuck.transform,false);
        var chuckSprite=AssetDatabase.LoadAssetAtPath<Sprite>(Local+"/Chuck_Idle.png");var cr=visual.GetComponent<SpriteRenderer>();cr.sprite=chuckSprite;cr.sharedMaterial=material;cr.sortingOrder=4;
        visual.transform.localPosition=new Vector3(0.0490869f,-0.0666180f,0);
        var chuckLabel=new GameObject("NombreChuck",typeof(TextMeshPro));chuckLabel.transform.SetParent(chuck.transform,false);chuckLabel.transform.localPosition=new Vector3(0,2.75f,0);var ct=chuckLabel.GetComponent<TextMeshPro>();ct.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");ct.text="Chuck";ct.fontSize=3;ct.alignment=TextAlignmentOptions.Center;ct.color=new Color(1,.94f,.7f);ct.rectTransform.sizeDelta=new Vector2(2,.5f);ct.GetComponent<MeshRenderer>().sortingOrder=8;
        PrefabUtility.SaveAsPrefabAssetAndConnect(chuck,Local+"/Chuck.prefab",InteractionMode.AutomatedAction);
        var meet=new GameObject("WilsonJuntoAChuck");meet.transform.position=new Vector3(56.95f,2+radius,0);
        var ending=new GameObject("FinalDelJuego",typeof(ReunionEnding)).GetComponent<ReunionEnding>();ending.salida=exit.GetComponent<LevelExit>();ending.wilson=controller;ending.puntoEncuentro=meet.transform;ending.interfazNivel=canvas.GetComponent<Canvas>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(ending.salida.alCompletarNivel,ending.MostrarFinal);
        var checkpoint=Clone(pointTemplate.gameObject,"CheckpointNivel05");checkpoint.transform.position=new Vector3(26.5f,3,0);
        var cp=checkpoint.GetComponent<RespawnCheckpoint>();cp.puntoReaparicion.position=new Vector3(26.5f,2+radius+.05f,0);cp.textoMarcador.text="Punto seguro";cp.textoMarcador.color=new Color(1,.97f,.7f);cp.indicador.color=new Color(.55f,.9f,.7f);
        var indicator=cp.indicador;indicator.sprite=AssetDatabase.LoadAssetAtPath<Tile>(Art+"/Tiles/concha_abanico.asset").sprite;
        var group=new GameObject("Collectibles");
        foreach(var xy in new[]{new Vector2(2.5f,1.5f),new Vector2(4.5f,1.5f),new Vector2(6.5f,1.5f),new Vector2(7.5f,1.5f),new Vector2(10.5f,2.5f),new Vector2(12.5f,2.5f),new Vector2(14.5f,2.5f),new Vector2(16.5f,2.5f),new Vector2(17.5f,2.5f),new Vector2(20.5f,2.5f),new Vector2(21.5f,2.5f),new Vector2(22.5f,2.5f),new Vector2(26.5f,2.5f),new Vector2(27.5f,2.5f),new Vector2(30.5f,3.5f),new Vector2(32.5f,3.5f),new Vector2(36.5f,3.5f),new Vector2(37.5f,3.5f),new Vector2(41.5f,2.5f),new Vector2(43.5f,2.5f),new Vector2(45.5f,2.5f),new Vector2(47.5f,2.5f),new Vector2(50.5f,2.5f),new Vector2(52.5f,2.5f),new Vector2(54.5f,2.5f),new Vector2(55.5f,2.5f),new Vector2(56.5f,2.5f)})Clone(coinTemplates[1].gameObject,"CocoComun",group.transform).transform.position=xy;
        Clone(coinTemplates[3].gameObject,"CocoBrillante",group.transform).transform.position=new Vector3(24.5f,4.5f,0);
        Clone(coinTemplates[5].gameObject,"CocoDorado",group.transform).transform.position=new Vector3(34.5f,3.5f,0);
        var cam=new GameObject("Main Camera",typeof(Camera),typeof(CameraFollow),typeof(AudioListener));cam.tag="MainCamera";cam.transform.position=new Vector3(8.6f,3.8f,-10);
        var camera=cam.GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=5;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.15f,.75f,1);
        var follow=cam.GetComponent<CameraFollow>();follow.objetivo=player.transform;follow.limiteIzquierdo=8.6f;follow.limiteDerecho=51.4f;
        // Repeat the same horizon without stretching it across the entire level.
        var bgSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Nivel01/Horizonte tropical pixelado.png");
        for(int i=0;i<3;i++){
            var bg=new GameObject("HorizontePlaya_"+i,typeof(SpriteRenderer));var r=bg.GetComponent<SpriteRenderer>();r.sprite=bgSprite;r.sharedMaterial=material;r.sortingOrder=-20;
            float height=12,width=height*bgSprite.bounds.size.x/bgSprite.bounds.size.y;
            bg.transform.localScale=Vector3.one*(height/bgSprite.bounds.size.y);bg.transform.position=new Vector3(width*(i-.0f),4,0);
        }
        EditorSceneManager.CloseScene(source,true);SceneManager.SetActiveScene(target);
        EditorSceneManager.SaveScene(target,Path05);
        var ordered=new[]{"Assets/Scenes/Nivel01_PerdidoEnElMar.unity","Assets/Scenes/Nivel02_CavernaDeCoral.unity","Assets/Scenes/Nivel03_RuinasSumergidas.unity",Path04,Path05};
        EditorBuildSettings.scenes=ordered.Select(p=>new EditorBuildSettingsScene(p,true)).Concat(EditorBuildSettings.scenes.Where(s=>!ordered.Contains(s.path))).ToArray();
        var audio=AssetDatabase.LoadAssetAtPath<GameAudioSettings>("Assets/Resources/WilsonAudioSettings.asset");audio.musicaNivel05=audio.musicaNivel01;EditorUtility.SetDirty(audio);AssetDatabase.SaveAssets();
        Validate();
    }
    static void ImportChuck()
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(Local+"/Chuck_Idle.png");importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.spritePixelsPerUnit=570.4166667f;importer.maxTextureSize=2048;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=9;settings.spritePivot=new Vector2(.5f,0);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();
    }
    static void Validate()
    {
        var coins=Object.FindObjectsByType<CoconutCollectible>();if(coins.Length!=29 || coins.Sum(c=>c.valor)!=35)throw new Exception("Cocos incorrectos");
        foreach(var c in Object.FindObjectsByType<TilemapCollider2D>())c.ProcessTilemapChanges();Physics2D.SyncTransforms();
        ValidateNavigation(Object.FindAnyObjectByType<WilsonController>(),coins);
        if(safeNodes!=reachableNodes)throw new Exception("Apoyos inaccesibles: "+reachableNodes+"/"+safeNodes);
        Directory.CreateDirectory(Output);File.WriteAllText(Output+"/VERIFICACION_MAPA.txt","Unity "+Application.unityVersion+"\n60 columnas, cinco huecos, 29 cocos que suman 35 puntos.\n"+reachableNodes+"/"+safeNodes+" apoyos seguros accesibles; "+trajectorySamples+" muestras de trayectorias. Todos los cocos tienen un apoyo accesible. Modelo balistico con tolerancia de radio de 0.04.\n");
    }
    public static void RunBatch(){try{Build();Debug.Log("LEVEL05_BUILD_OK");EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
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
