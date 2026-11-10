using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ApplyMenusOnce
{
    const string Request="Temp/WilsonMenus.request",Done="Temp/WilsonMenus.done";
    static ApplyMenusOnce(){EditorApplication.update+=Tick;}
    static void Tick()
    {
        if(!File.Exists(Request) || File.ReadAllText(Request)!="menus-v1")return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating)return;
        if(EditorApplication.isPlaying){EditorApplication.ExitPlaymode();return;}
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        EditorApplication.update-=Tick;
        try
        {
            Directory.CreateDirectory("Temp/WilsonAntesDeMenus");
            for(int i=0;i<SceneManager.sceneCount;i++)
            {
                Scene s=SceneManager.GetSceneAt(i);
                if(s.isDirty && s.path.StartsWith("Assets/Scenes/"))
                {
                    EditorSceneManager.SaveScene(s,"Temp/WilsonAntesDeMenus/"+Path.GetFileName(s.path),true);
                    EditorSceneManager.SaveScene(s);
                }
            }
            MenuSetup.Prepare();MenuBuild.Export();EditorSceneManager.OpenScene(MenuSetup.MenuPath);
            File.WriteAllText(Done,"OK: inicio configurado, Windows exportado y MenuPrincipal abierto.\n"+MenuBuild.Output);
        }
        catch(Exception e){File.WriteAllText(Done,"ERROR: "+e);Debug.LogException(e);}
        finally{File.Delete(Request);}
    }
}
