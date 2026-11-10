using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;

public static class CoastalPreview
{
    static int level;
    public static void Run()
    {
        try
        {
            string report="";
            foreach(string name in new[]{"Nivel01_PerdidoEnElMar","Nivel02_CavernaDeCoral","Nivel03_RuinasSumergidas"})
            {
                EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
                var p=UnityEngine.Object.FindAnyObjectByType<WilsonController>();p.SendMessage("Awake");var rb=p.GetComponent<Rigidbody2D>();
                typeof(WilsonController).GetField("movimiento",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(p,1f);rb.linearVelocity=new Vector2(0,-2);p.SendMessage("FixedUpdate");
                if(Mathf.Abs(rb.linearVelocity.x-p.velocidad)>.01f || Mathf.Abs(rb.linearVelocity.y+2)>.01f)throw new Exception("Movimiento normal alterado: "+name);
                typeof(WilsonController).GetField("solicitarSalto",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(p,true);p.SendMessage("FixedUpdate");
                if(Mathf.Abs(rb.linearVelocity.y-p.fuerzaSalto)>.01f)throw new Exception("Salto normal alterado: "+name);
                p.ReiniciarMovimiento();if(rb.linearVelocity!=Vector2.zero || p.PlataformaActual!=null)throw new Exception("Reinicio de movimiento");
                report+="PASS "+name+": velocidad horizontal, velocidad vertical preservada, salto y limpieza del movimiento.\n";
            }
            File.WriteAllText("../cambios_dificultad_costera/VERIFICACION_REGRESION.txt",report);
            Open(4);
        }
        catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
    static void Open(int n)
    {
        level=n;EditorSceneManager.OpenScene(n==4?CoastalDifficultySetup.Beach:CoastalDifficultySetup.City);
        int frames=0;EditorApplication.CallbackFunction callback=null;
        callback=()=>{if(++frames<12)return;EditorApplication.update-=callback;Capture();};EditorApplication.update+=callback;EditorApplication.QueuePlayerLoopUpdate();
    }
    static void Capture()
    {
        try
        {
            var cam=Camera.main;cam.GetComponent<CameraFollow>().enabled=false;
            var canvas=UnityEngine.Object.FindAnyObjectByType<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=1;
            foreach(var text in UnityEngine.Object.FindObjectsByType<TMP_Text>())text.ForceMeshUpdate(true,true);Canvas.ForceUpdateCanvases();
            Save(cam,"Nivel0"+level+"_inicio.png");
            cam.transform.position=new Vector3(level==4?36:39.5f,3.8f,-10);Save(cam,"Nivel0"+level+"_muelle_movil.png");
            if(level==5){cam.transform.position=new Vector3(14.5f,3.8f,-10);Save(cam,"Nivel05_elevador_mercado.png");Debug.Log("COASTAL_PREVIEW_OK");EditorApplication.Exit(0);}
            else Open(5);
        }
        catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
    static void Save(Camera cam,string name)
    {
        var rt=new RenderTexture(1280,720,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
        var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes("../cambios_dificultad_costera/"+name,tex.EncodeToPNG());
        cam.targetTexture=null;RenderTexture.active=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);
    }
}
