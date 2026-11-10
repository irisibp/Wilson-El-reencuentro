using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class SecretPreview
{
    public static void Run()
    {
        EditorSceneManager.OpenScene(Level05Builder.Path05);
        int frames=0;EditorApplication.CallbackFunction tick=null;
        tick=()=> {if(++frames<12)return;EditorApplication.update-=tick;Capture();};EditorApplication.update+=tick;
        EditorApplication.QueuePlayerLoopUpdate();
    }
    static void Capture()
    {
        try
        {
            var cam=Camera.main;cam.GetComponent<CameraFollow>().enabled=false;
            var canvas=UnityEngine.Object.FindAnyObjectByType<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=1;
            foreach(var text in canvas.GetComponentsInChildren<TMP_Text>())text.ForceMeshUpdate(true,true);
            Canvas.ForceUpdateCanvases();
            Save(cam,1280,720,"Preview_jugable.png");
            cam.transform.position=new Vector3(15,3.8f,-10);Save(cam,1280,720,"Balcones_del_mercado.png");
            cam.transform.position=new Vector3(30,4,-10);cam.orthographicSize=6;
            Save(cam,2880,576,"Mapa_completo.png");
            cam.transform.position=new Vector3(51.4f,3.8f,-10);cam.orthographicSize=5;Save(cam,1280,720,"Plaza_final.png");
            Debug.Log("LEVEL05_PREVIEW_OK");EditorApplication.Exit(0);
        }
        catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
    static void Save(Camera cam,int w,int h,string name)
    {
        var rt=new RenderTexture(w,h,24);rt.antiAliasing=1;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
        var tex=new Texture2D(w,h,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();
        File.WriteAllBytes(Path.GetFullPath("../cambios_nivel05_secreto/"+name),tex.EncodeToPNG());
        cam.targetTexture=null;RenderTexture.active=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);
    }
}
