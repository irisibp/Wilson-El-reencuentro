using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using TMPro;

public static class Level02HazardsSetup
{
    const string ScenePath="Assets/Scenes/Nivel02_CavernaDeCoral.unity";
    const string Art="Assets/WilsonSubmarino32";
    static Material material;
    static TMP_FontAsset font;

    [MenuItem("Tools/Wilson/Nivel 2/Agregar peligros y punto de reaparicion")]
    public static void PrepareMenu()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Nivel 2","Sali de Play Mode antes de preparar el nivel.","Aceptar");return;
        }
        if(SceneManager.GetActiveScene().path!=ScenePath)
        {
            EditorUtility.DisplayDialog("Nivel 2","Abri Nivel02_CavernaDeCoral antes de usar esta opcion.","Aceptar");return;
        }
        PrepareActiveScene();
    }

    static GameObject Create(string name,Transform parent=null)
    {
        var go=new GameObject(name);Undo.RegisterCreatedObjectUndo(go,"Crear "+name);
        if(parent!=null)Undo.SetTransformParent(go.transform,parent,"Ubicar "+name);
        return go;
    }
    static SpriteRenderer Sprite(string name,string tile,Transform parent,Vector3 local,Color color)
    {
        var go=Create(name,parent);go.transform.localPosition=local;
        var sr=Undo.AddComponent<SpriteRenderer>(go);sr.sprite=AssetDatabase.LoadAssetAtPath<Tile>(Art+"/Tiles/"+tile+".asset").sprite;
        sr.sharedMaterial=material;sr.sortingOrder=3;sr.color=color;return sr;
    }
    static TMP_Text Label(string name,string content,Transform parent,Vector3 local,Color color,float size)
    {
        var go=Create(name,parent);go.transform.localPosition=local;
        var text=Undo.AddComponent<TextMeshPro>(go);text.font=font;text.text=content;text.fontSize=size;text.alignment=TextAlignmentOptions.Center;
        text.color=color;text.rectTransform.sizeDelta=new Vector2(3,.7f);go.GetComponent<MeshRenderer>().sortingOrder=8;return text;
    }
    static float Floor(float x)
    {
        var hit=Physics2D.Raycast(new Vector2(x,6),Vector2.down,16,1<<LayerMask.NameToLayer("Ground"));
        if(hit.collider==null)throw new InvalidOperationException("Falta suelo en x="+x+". Revisar la ubicacion de los peligros.");
        return hit.point.y;
    }
    public static bool PrepareActiveScene()
    {
        var scene=SceneManager.GetActiveScene();
        if(scene.path!=ScenePath)throw new InvalidOperationException("Esta configuracion corresponde solo al nivel 2.");
        var player=UnityEngine.Object.FindAnyObjectByType<WilsonController>();
        if(player==null)throw new InvalidOperationException("No se encontro a Wilson.");
        if(scene.GetRootGameObjects().Any(g=>g.name=="PeligrosNivel02"))
        {
            Debug.Log("El nivel 2 ya tiene peligros. No se crean duplicados.");return false;
        }
        foreach(var map in UnityEngine.Object.FindObjectsByType<TilemapCollider2D>())map.ProcessTilemapChanges();
        Physics2D.SyncTransforms();
        // Resolve every floor before changing the scene, so an edited layout fails cleanly.
        float[] xs={6f,24.5f,35.5f};float[] ys=xs.Select(Floor).ToArray();float checkpointFloor=Floor(17.5f);
        float radius=player.GetComponent<Collider2D>().bounds.extents.y;
        Vector2 spawn=new Vector2(17.5f,checkpointFloor+radius+.05f);
        if(Physics2D.OverlapCircle(spawn,radius-.02f,1<<3)!=null)throw new InvalidOperationException("El punto de reaparicion no esta despejado.");
        material=AssetDatabase.LoadAssetAtPath<Material>(Art+"/Materials/SubmarinoUnlit.mat");
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Agregar peligros y reaparicion al nivel 2");
        var respawn=player.GetComponent<PlayerRespawn>()??Undo.AddComponent<PlayerRespawn>(player.gameObject);
        var hint=UnityEngine.Object.FindObjectsByType<TMP_Text>().FirstOrDefault(t=>t.name=="Objetivo");
        Undo.RecordObject(respawn,"Vincular aviso de reaparicion");respawn.estado=hint;PrefabUtility.RecordPrefabInstancePropertyModifications(respawn);
        if(hint!=null){Undo.RecordObject(hint,"Actualizar objetivo");hint.text="Evita los corales rojos. Conservas tus cocos al reaparecer.";}
        var root=Create("PeligrosNivel02");
        for(int i=0;i<xs.Length;i++)
        {
            var go=Create("CoralPeligroso_"+(i+1),root.transform);go.transform.position=new Vector3(xs[i],ys[i]+.5f,0);
            var hazard=Undo.AddComponent<CoralHazard>(go);var box=go.GetComponent<BoxCollider2D>();box.isTrigger=true;box.size=new Vector2(.55f,.8f);box.offset=new Vector2(0,-.05f);
            Sprite("CoralRojo","coral_rojo",go.transform,Vector3.zero,new Color(1f,.28f,.32f));
            Label("AvisoPeligro","!",go.transform,new Vector3(0,.8f,0),new Color(1f,.35f,.35f),6);
        }
        var point=Create("CheckpointNivel02");point.transform.position=new Vector3(17.5f,checkpointFloor+1f,0);
        var checkpoint=Undo.AddComponent<RespawnCheckpoint>(point);
        var collider=point.GetComponent<BoxCollider2D>();collider.isTrigger=true;collider.size=new Vector2(1.4f,3.6f);collider.offset=new Vector2(0,.2f);
        var spawnPoint=Create("PuntoSeguro",point.transform);spawnPoint.transform.position=new Vector3(spawn.x,spawn.y,0);checkpoint.puntoReaparicion=spawnPoint.transform;
        Sprite("BaseRoca","roca_suelta",point.transform,new Vector3(0,-.5f,0),Color.white);
        checkpoint.indicador=Sprite("Indicador","burbujas_a",point.transform,new Vector3(0,.2f,0),new Color(.55f,.8f,.9f));
        checkpoint.textoMarcador=Label("Rotulo","Punto seguro",point.transform,new Vector3(0,1.3f,0),new Color(.65f,1f,1f),3.5f);
        EditorSceneManager.MarkSceneDirty(scene);Undo.CollapseUndoOperations(group);Selection.activeGameObject=point;SceneView.RepaintAll();
        Debug.Log("Peligros y punto seguro preparados. Guardar la escena con Ctrl+S.");return true;
    }

    public static void ValidateStatic()
    {
        void Check(bool ok,string msg){if(!ok)throw new Exception("PELIGROS: "+msg);}
        var hazards=UnityEngine.Object.FindObjectsByType<CoralHazard>();
        var point=UnityEngine.Object.FindAnyObjectByType<RespawnCheckpoint>();var player=UnityEngine.Object.FindAnyObjectByType<WilsonController>();
        Check(hazards.Length==3 && point!=null,"3 peligros y 1 checkpoint");
        Check(player.GetComponent<PlayerRespawn>().estado!=null,"aviso enlazado");
        foreach(var h in hazards)Check(h.GetComponent<BoxCollider2D>().isTrigger,"trigger de peligro");
        Check(point.GetComponent<BoxCollider2D>().isTrigger && point.puntoReaparicion!=null,"trigger y punto seguro");
        Physics2D.SyncTransforms();
        float playerRadius=player.GetComponent<Collider2D>().bounds.extents.y;
        Debug.Log("Respawn radius="+playerRadius+" point="+point.puntoReaparicion.position);
        Check(Physics2D.OverlapCircle(point.puntoReaparicion.position,Mathf.Max(.01f,playerRadius-.04f),1<<3)==null,"punto seguro despejado");
        var arcs=new[]{(new Vector2(4.5f,1+playerRadius),new Vector2(8.5f,2+playerRadius)),(new Vector2(22.5f,4+playerRadius),new Vector2(25.5f,2+playerRadius)),(new Vector2(33.5f,2+playerRadius),new Vector2(37,3+playerRadius))};
        float gravity=Mathf.Abs(Physics2D.gravity.y)*player.GetComponent<Rigidbody2D>().gravityScale;
        int samples=0;
        foreach(var arc in arcs)
        {
            float dy=arc.Item2.y-arc.Item1.y;float jump=player.fuerzaSalto;float time=(jump+Mathf.Sqrt(jump*jump-2*gravity*dy))/gravity;
            Check(arc.Item2.x-arc.Item1.x<=player.velocidad*time,"alcance horizontal");
            for(float t=.01f;t<time-.01f;t+=.01f)
            {
                var pos=arc.Item1+new Vector2(Mathf.Min(arc.Item2.x-arc.Item1.x,player.velocidad*t),jump*t-.5f*gravity*t*t);
                foreach(var c in Physics2D.OverlapCircleAll(pos,Mathf.Max(.01f,playerRadius-.04f)))
                    Check(c.gameObject.layer!=3 && c.GetComponent<CoralHazard>()==null,"salto colisiona con suelo o peligro: "+pos+" / "+c.name);
                samples++;
            }
        }
        Directory.CreateDirectory(Path.GetFullPath("../cambios_peligros"));
        File.WriteAllText(Path.GetFullPath("../cambios_peligros/VERIFICACION_MAPA.txt"),"Unity "+Application.unityVersion+"\n3 corales peligrosos, checkpoint seguro, referencias enlazadas.\n3 trayectorias de salto validas sobre los peligros; "+samples+" muestras contra colliders reales.\n");
    }
    public static void RunBatch()
    {
        try{EditorSceneManager.OpenScene(ScenePath);PrepareActiveScene();ValidateStatic();EditorSceneManager.SaveScene(SceneManager.GetActiveScene());Debug.Log("HAZARDS_SETUP_OK");EditorApplication.Exit(0);}
        catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
