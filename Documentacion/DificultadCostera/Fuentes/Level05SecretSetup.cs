using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using TMPro;

public static class Level05SecretSetup
{
    const string ScenePath="Assets/Scenes/Nivel05_CiudadCostera.unity";
    const string Art="Assets/Art/Tiles/Nivel01/Set de sprites pixel art tropical.png";

    static T[] InScene<T>() where T:Component => SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();

    [MenuItem("Tools/Wilson/Nivel 5/Agregar coco oculto y balcones")]
    public static void Prepare()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path!=ScenePath)
        {
            EditorUtility.DisplayDialog("Nivel 5","Abri Nivel05_CiudadCostera y sali de Play antes de aplicar el cambio.","Aceptar");return;
        }
        if(InScene<HiddenCoconut>().Length!=0){Debug.Log("El secreto ya esta preparado; no se duplicaron objetos.");return;}
        var coin=InScene<CoconutCollectible>().SingleOrDefault(c=>c.valor==5);
        var grid=InScene<Grid>().SingleOrDefault(g=>g.name=="Grid_CiudadCostera");
        var wood=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Nivel05/Muelle.asset");
        var sprites=AssetDatabase.LoadAllAssetsAtPath(Art).OfType<Sprite>().ToArray();
        var plank=sprites.Single(s=>s.name.EndsWith("_57"));var crate=sprites.Single(s=>s.name.EndsWith("_41"));
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/WilsonSubmarino32/Materials/SubmarinoUnlit.mat");
        if(coin==null || grid==null || wood==null){Debug.LogError("Falta el coco dorado, la grilla o el tile del muelle. No se cambio la escena.");return;}
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Agregar secreto del mercado");
        var mapGo=new GameObject("BalconesDelMercado",typeof(Tilemap),typeof(TilemapRenderer),typeof(TilemapCollider2D),typeof(Rigidbody2D));
        Undo.RegisterCreatedObjectUndo(mapGo,"Crear balcones");mapGo.transform.SetParent(grid.transform,false);mapGo.layer=3;
        mapGo.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;mapGo.GetComponent<TilemapRenderer>().enabled=false;
        var map=mapGo.GetComponent<Tilemap>();
        for(int x=12;x<=14;x++)map.SetTile(new Vector3Int(x,3,0),wood);
        for(int x=16;x<=17;x++)map.SetTile(new Vector3Int(x,4,0),wood);
        AddPlank("PrimerBalcon",plank,material,mapGo.transform,13.5f,3.5f,3);
        AddPlank("BalconSecreto",plank,material,mapGo.transform,17,4.5f,2);
        Undo.RecordObject(coin.transform,"Mover coco dorado al balcon");coin.transform.position=new Vector3(16.5f,5.5f,coin.transform.position.z);
        PrefabUtility.RecordPrefabInstancePropertyModifications(coin.transform);
        foreach(var r in coin.GetComponentsInChildren<Renderer>(true)){Undo.RecordObject(r,"Ocultar coco");r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
        foreach(var c in coin.GetComponentsInChildren<Collider2D>(true)){Undo.RecordObject(c,"Ocultar contacto del coco");c.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(c);}
        var secret=new GameObject("SecretoDelMercado",typeof(BoxCollider2D),typeof(HiddenCoconut));Undo.RegisterCreatedObjectUndo(secret,"Crear secreto");secret.transform.position=new Vector3(16.5f,5.65f,0);
        var box=secret.GetComponent<BoxCollider2D>();box.isTrigger=true;box.size=new Vector2(1.5f,1.6f);
        var cover=new GameObject("CajaDecorativa_SinColision",typeof(SpriteRenderer));cover.transform.SetParent(secret.transform,false);
        var sr=cover.GetComponent<SpriteRenderer>();sr.sprite=crate;sr.sharedMaterial=material;sr.sortingOrder=10;
        float scale=1.05f/crate.bounds.size.y;cover.transform.localScale=Vector3.one*scale;
        cover.transform.position=new Vector3(16.5f,4.88f-crate.bounds.min.y*scale,0);
        var hidden=secret.GetComponent<HiddenCoconut>();hidden.coco=coin;hidden.cobertura=cover;
        var objective=InScene<TMP_Text>().SingleOrDefault(t=>t.name=="Objetivo");
        if(objective!=null){Undo.RecordObject(objective,"Agregar pista");objective.text="Reúne 35 puntos. Busca un coco oculto en los balcones del mercado.";}
        mapGo.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();Physics2D.SyncTransforms();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());Undo.CollapseUndoOperations(group);
        Debug.Log("Balcones y coco oculto preparados. Se mantienen los 29 cocos y 35 puntos. Guarda con Ctrl+S; podes deshacer con Ctrl+Z.");
    }

    static void AddPlank(string name,Sprite sprite,Material material,Transform parent,float x,float y,float width)
    {
        var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(parent,false);go.transform.position=new Vector3(x,y,0);
        go.transform.localScale=new Vector3(width/sprite.bounds.size.x,1/sprite.bounds.size.y,1);
        var r=go.GetComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=material;r.sortingOrder=2;
    }
}
