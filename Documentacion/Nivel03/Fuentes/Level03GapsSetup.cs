using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class Level03GapsSetup
{
    const string ScenePath = "Assets/Scenes/Nivel03_RuinasSumergidas.unity";

    [MenuItem("Tools/Wilson/Nivel 3/Agregar vacios y caidas")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path != ScenePath)
        {
            EditorUtility.DisplayDialog("Nivel 3", "Abri Nivel03_RuinasSumergidas y sali de Play Mode antes de aplicar los cambios.", "Aceptar");
            return;
        }
        if (UnityEngine.Object.FindAnyObjectByType<FallZone>() != null)
        {
            Debug.Log("Los vacios ya estan preparados. No se duplicaron objetos.");
            return;
        }
        var maps = UnityEngine.Object.FindObjectsByType<Tilemap>();
        var ground = maps.Single(m => m.name == "TerrenoNivel03");
        var decor = maps.Single(m => m.name == "DecoracionNivel03");
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Agregar vacios al nivel 3");
        Undo.RegisterCompleteObjectUndo(ground, "Abrir vacios");
        Undo.RegisterCompleteObjectUndo(decor, "Quitar decoracion sin apoyo");
        foreach (int x in new[] {11, 12, 20, 21, 42, 43})
        {
            int top = x >= 42 ? 2 : 1;
            for (int y = -2; y < top; y++) ground.SetTile(new Vector3Int(x, y, 0), null);
            decor.SetTile(new Vector3Int(x, top, 0), null);
        }
        // Keep the bonus platform above the first gap and every collectible.
        foreach (var coin in UnityEngine.Object.FindObjectsByType<CoconutCollectible>())
        {
            if (coin.valor != 1 || Vector2.Distance(coin.transform.position, new Vector2(21.5f, 1.5f)) > .1f) continue;
            Undo.RecordObject(coin.transform, "Mover coco a la orilla segura");
            coin.transform.position = new Vector3(22.5f, 1.5f, coin.transform.position.z);
            PrefabUtility.RecordPrefabInstancePropertyModifications(coin.transform);
        }
        var zone = new GameObject("ZonaCaidaNivel03");
        Undo.RegisterCreatedObjectUndo(zone, "Crear zona de caida");
        zone.transform.position = new Vector3(24, -6, 0);
        var box = Undo.AddComponent<BoxCollider2D>(zone);
        box.isTrigger = true;
        box.size = new Vector2(54, 6);
        Undo.AddComponent<FallZone>(zone);
        foreach (var collider in ground.GetComponents<TilemapCollider2D>()) collider.ProcessTilemapChanges();
        Physics2D.SyncTransforms();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Undo.CollapseUndoOperations(group);
        Debug.Log("Nivel 3: tres vacios de dos celdas, 25 puntos conservados y caidas con perdida de vida. Guarda con Ctrl+S. Podes deshacer con Ctrl+Z.");
    }
}
