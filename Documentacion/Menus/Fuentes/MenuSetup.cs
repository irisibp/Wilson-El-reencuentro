using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Object=UnityEngine.Object;

public static class MenuSetup
{
    public const string MenuPath="Assets/Scenes/MenuPrincipal.unity";
    static Sprite Tropical(int index)=>AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Tiles/Nivel01/Set de sprites pixel art tropical.png").OfType<Sprite>().Single(s=>s.name.EndsWith("_"+index));
    static GameObject Sprite(string name,Sprite sprite,Material material,Vector2 foot,float height,int order)
    {
        var go=new GameObject(name,typeof(SpriteRenderer));var r=go.GetComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=material;r.sortingOrder=order;
        float scale=height/sprite.bounds.size.y;go.transform.localScale=Vector3.one*scale;go.transform.position=new Vector3(foot.x,foot.y-sprite.bounds.min.y*scale,0);return go;
    }
    [MenuItem("Tools/Wilson/Menus/Crear inicio y configurar exportacion")]
    public static void Prepare()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode){Debug.LogError("Sali de Play antes de crear el inicio.");return;}
        Scene initial=SceneManager.GetActiveScene();Scene menu=SceneManager.GetSceneByPath(MenuPath);bool loaded=menu.IsValid() && menu.isLoaded;bool created=false,opened=false;
        try
        {
            if(!File.Exists(MenuPath))
            {
                menu=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(menu);created=true;
                var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/WilsonSubmarino32/Materials/SubmarinoUnlit.mat");
                var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=5;cam.backgroundColor=new Color(.04f,.46f,.67f);
                var bgSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Backgrounds/Nivel01/Horizonte tropical pixelado.png");var bg=new GameObject("HorizonteDelInicio",typeof(SpriteRenderer));var b=bg.GetComponent<SpriteRenderer>();b.sprite=bgSprite;b.sharedMaterial=mat;b.sortingOrder=-20;bg.transform.localScale=Vector3.one*(11/bgSprite.bounds.size.y);
                Sprite("IslaDelReencuentro",Tropical(10),mat,new Vector2(4.7f,-3.7f),6,-5);
                var chuck=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Nivel05/Chuck.prefab"));chuck.name="ChuckDelInicio";chuck.transform.position=new Vector3(5.6f,-2.35f,0);var label=chuck.transform.Find("NombreChuck");if(label!=null)label.gameObject.SetActive(false);
                var wilson=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/Wilson/Idle_00.png");Sprite("WilsonDelInicio",wilson,mat,new Vector2(3.7f,-2.35f),.95f,4);
                var canvas=MenuUI.Canvas("PantallaDeInicio",null,100);MenuUI.Fill(MenuUI.Stretch("Sombra",canvas.transform),new Color(.01f,.07f,.12f,.26f));
                var card=MenuUI.Rect("TarjetaDeInicio",canvas.transform,new Vector2(0,.5f),new Vector2(64,0),new Vector2(456,604),new Vector2(0,.5f));MenuUI.Fill(card,MenuUI.Navy);
                var border=MenuUI.Rect("BordeDorado",card,new Vector2(0,1),Vector2.zero,new Vector2(456,5),new Vector2(0,1));MenuUI.Fill(border,MenuUI.Gold).raycastTarget=false;
                MenuUI.Text("Antetitulo",card,"CINCO NIVELES. UN REENCUENTRO.",new Vector2(38,-31),new Vector2(380,34),18,new Color(.63f,.81f,.80f));
                MenuUI.Text("Titulo",card,"WILSON",new Vector2(34,-77),new Vector2(386,84),66,MenuUI.Gold);
                MenuUI.Text("Subtitulo",card,"EL REENCUENTRO",new Vector2(38,-164),new Vector2(380,44),29,MenuUI.Cream);
                var view=canvas.gameObject.AddComponent<MainMenuView>();view.opciones=MenuUI.Stretch("Opciones",card).gameObject;view.ayuda=MenuUI.Stretch("Controles",card).gameObject;
                MenuUI.Text("Descripcion",view.opciones.transform,"Atravesá el mar, la playa y la ciudad.\nChuck te espera al final del camino.",new Vector2(38,-234),new Vector2(380,72),20,MenuUI.Cream);
                view.jugar=MenuUI.Button("Jugar",view.opciones.transform,"Jugar",new Vector2(38,-332),new Vector2(380,56),null,true);
                view.controles=MenuUI.Button("Controles",view.opciones.transform,"Controles",new Vector2(38,-406),new Vector2(380,50));
                view.salir=MenuUI.Button("Salir",view.opciones.transform,"Salir del juego",new Vector2(38,-474),new Vector2(380,50));
                MenuUI.Text("Ayuda",view.ayuda.transform,"A / D o flechas: moverse\nEspacio: saltar\nEsc: pausar o continuar\n\nReúne cocos y evita las trampas.\nUsa las plataformas móviles para\nllegar hasta Chuck.",new Vector2(38,-236),new Vector2(380,246),20,MenuUI.Cream);
                view.volver=MenuUI.Button("Volver",view.ayuda.transform,"Volver",new Vector2(38,-504),new Vector2(380,50));view.ayuda.SetActive(false);
                MenuUI.Text("Nota",view.opciones.transform,"Una nueva partida comienza en el nivel 1.",new Vector2(38,-548),new Vector2(380,32),16,new Color(.63f,.81f,.80f));
                EditorSceneManager.SaveScene(menu,MenuPath);
            }
            if(!menu.IsValid() || !menu.isLoaded){menu=EditorSceneManager.OpenScene(MenuPath,OpenSceneMode.Additive);opened=true;}
            Directory.CreateDirectory("Assets/Art/Menus");AssetDatabase.Refresh();
            foreach(var renderer in menu.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)))
            {
                if(renderer.sprite==null)continue;
                string materialPath="Assets/Art/Menus/"+renderer.sprite.texture.name+".mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if(material==null){material=new Material(AssetDatabase.LoadAssetAtPath<Shader>("Assets/WilsonSubmarino32/Materials/SubmarinoUnlit.shader"));material.mainTexture=renderer.sprite.texture;AssetDatabase.CreateAsset(material,materialPath);}
                renderer.sharedMaterial=material;PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }
            EditorSceneManager.SaveScene(menu,MenuPath);
            string[] order={MenuPath,"Assets/Scenes/Nivel01_PerdidoEnElMar.unity","Assets/Scenes/Nivel02_CavernaDeCoral.unity","Assets/Scenes/Nivel03_RuinasSumergidas.unity","Assets/Scenes/Nivel04_LaPlaya.unity","Assets/Scenes/Nivel05_CiudadCostera.unity"};
            EditorBuildSettings.scenes=Ordered(EditorBuildSettings.scenes,order);
            foreach(string id in AssetDatabase.FindAssets("t:BuildProfile"))
            {
                var profile=AssetDatabase.LoadAssetAtPath<BuildProfile>(AssetDatabase.GUIDToAssetPath(id));var serialized=new SerializedObject(profile);var overrides=serialized.FindProperty("m_OverrideGlobalSceneList");
                if(overrides!=null && overrides.boolValue){profile.scenes=Ordered(profile.scenes,order);EditorUtility.SetDirty(profile);}
            }
            AssetDatabase.SaveAssets();Debug.Log("Inicio creado y primero en la build. Pausa y salida se instalan automaticamente al jugar cualquier nivel.");
        }
        finally
        {
            if((created || opened) && !loaded && menu.IsValid() && menu.isLoaded)EditorSceneManager.CloseScene(menu,true);
            if(initial.IsValid() && initial.isLoaded)SceneManager.SetActiveScene(initial);
        }
    }
    static EditorBuildSettingsScene[] Ordered(EditorBuildSettingsScene[] existing,string[] order)=>order.Select(p=>new EditorBuildSettingsScene(p,true)).Concat(existing.Where(s=>!order.Contains(s.path))).ToArray();
    public static void RunBatch(){try{EditorSceneManager.OpenScene("Assets/Scenes/Nivel01_PerdidoEnElMar.unity");Prepare();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}
