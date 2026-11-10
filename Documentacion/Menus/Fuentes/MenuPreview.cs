using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class MenuPreview
{
    public static void Run()
    {
        EditorSceneManager.OpenScene(MenuSetup.MenuPath);
        int frames=0;EditorApplication.CallbackFunction callback=null;callback=()=>{if(++frames<16)return;EditorApplication.update-=callback;Capture();};EditorApplication.update+=callback;EditorApplication.QueuePlayerLoopUpdate();
    }
    static void Capture()
    {
        try
        {
            var camera=Camera.main;var canvas=UnityEngine.Object.FindAnyObjectByType<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            foreach(var t in UnityEngine.Object.FindObjectsByType<TMP_Text>())t.ForceMeshUpdate(true,true);Canvas.ForceUpdateCanvases();
            var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();
            Directory.CreateDirectory("../cambios_menus");File.WriteAllBytes("../cambios_menus/Inicio.png",tex.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);Debug.Log("MENU_SETUP_OK");EditorApplication.Exit(0);
        }
        catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
