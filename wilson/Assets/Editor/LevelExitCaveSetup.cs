using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Applies only to this standalone decorative sprite, never to the terrain atlas.
public class LevelExitCaveImporter : AssetPostprocessor
{
    public const string SpritePath = "Assets/Art/Props/Nivel01/CuevaSalida.png";
    void OnPreprocessTexture()
    {
        if (assetPath != SpritePath) return;
        var importer = (TextureImporter)assetImporter;
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = width / 2.4f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 4096;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = new Vector2(.5f, 0f);
        settings.spriteGenerateFallbackPhysicsShape = false;
        importer.SetTextureSettings(settings);
    }
}

public static class LevelExitCaveSetup
{
    [MenuItem("Tools/Wilson/Salida/Aplicar cueva a la salida seleccionada")]
    static void ApplySelected()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Salida", "Sali de Play Mode antes de cambiar la apariencia.", "Aceptar");
            return;
        }
        var root = Selection.activeGameObject;
        if (root == null || root.GetComponent<LevelExit>() == null)
        {
            EditorUtility.DisplayDialog("Salida", "Selecciona SalidaNivel, el objeto que contiene Level Exit.", "Aceptar");
            return;
        }
        if (EditorUtility.IsPersistent(root))
        {
            EditorUtility.DisplayDialog("Salida", "Selecciona la salida de la escena en Hierarchy.", "Aceptar");
            return;
        }
        Apply(root);
        Selection.activeGameObject = root;
    }

    public static void Apply(GameObject root)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(LevelExitCaveImporter.SpritePath);
        if (sprite == null) throw new System.InvalidOperationException("Todavia no se importo CuevaSalida.png.");
        var scale = root.transform.lossyScale;
        if (Mathf.Abs(scale.x) < .0001f || Mathf.Abs(scale.y) < .0001f || Mathf.Abs(scale.z) < .0001f)
            throw new System.InvalidOperationException("La escala de la salida no puede ser cero.");

        Undo.SetCurrentGroupName("Aplicar cueva a la salida");
        int undoGroup = Undo.GetCurrentGroup();
        var original = root.GetComponent<SpriteRenderer>();
        var existing = root.transform.Find("VisualCueva");
        // On repeated use preserve the established ground baseline.
        float baseY = existing != null ? existing.position.y :
            original != null && original.sprite != null ? original.bounds.min.y : root.transform.position.y - 1f;
        GameObject visual;
        if (existing == null)
        {
            visual = new GameObject("VisualCueva");
            Undo.RegisterCreatedObjectUndo(visual, "Crear visual de cueva");
            Undo.SetTransformParent(visual.transform, root.transform, "Ubicar visual de cueva");
        }
        else visual = existing.gameObject;

        Undo.RecordObject(visual.transform, "Ajustar cueva");
        visual.transform.position = new Vector3(root.transform.position.x, baseY, root.transform.position.z);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
        var renderer = visual.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = Undo.AddComponent<SpriteRenderer>(visual);
        Undo.RecordObject(renderer, "Configurar sprite de cueva");
        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.enabled = true;
        renderer.sortingLayerID = original != null ? original.sortingLayerID : 0;
        renderer.sortingOrder = original != null ? original.sortingOrder + 1 : 1;
        var unlit = AssetDatabase.LoadAssetAtPath<Material>("Assets/WilsonSubmarino32/Materials/SubmarinoUnlit.mat");
        if (unlit != null) renderer.sharedMaterial = unlit;
        if (original != null)
        {
            Undo.RecordObject(original, "Ocultar marcador rosa");
            original.enabled = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(original);
        }
        PrefabUtility.RecordPrefabInstancePropertyModifications(visual.transform);
        PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        EditorSceneManager.MarkSceneDirty(root.scene);
        Undo.CollapseUndoOperations(undoGroup);
        SceneView.RepaintAll();
    }
}
