using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HazardsFullValidation
{
    public static void Run()
    {
        try
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Nivel02_CavernaDeCoral.unity");
            Level02HazardsSetup.PrepareActiveScene();Level02HazardsSetup.ValidateStatic();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            HazardsPlayValidation.Run();
        }
        catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
