using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Temporary bridge for the open editor. Runs only with the explicit task marker.
[InitializeOnLoad]
public static class ApplyCoastalDifficultyOnce
{
    const string Request="Temp/WilsonDifficulty.request";
    const string Done="Temp/WilsonDifficulty.done";
    static ApplyCoastalDifficultyOnce(){EditorApplication.update+=Tick;}
    static void Tick()
    {
        if(!File.Exists(Request) || File.ReadAllText(Request)!="coastal-v1")return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating)return;
        if(EditorApplication.isPlaying){EditorApplication.ExitPlaymode();return;}
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        EditorApplication.update-=Tick;
        try{CoastalDifficultySetup.ApplyBoth();File.WriteAllText(Done,"OK: niveles 4 y 5 aplicados y guardados. "+DateTime.UtcNow.ToString("O"));}
        catch(Exception e){File.WriteAllText(Done,"ERROR: "+e);Debug.LogException(e);}
        finally{File.Delete(Request);}
    }
}
