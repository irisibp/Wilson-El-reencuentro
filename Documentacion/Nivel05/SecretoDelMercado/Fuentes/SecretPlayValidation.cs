using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class SecretPlayValidation
{
    const string Pending="WilsonSecretTest";
    static int stage, points;
    static float stamp;
    static PlayerRespawn defeated;
    static SecretPlayValidation(){if(SessionState.GetBool(Pending,false))EditorApplication.update+=Tick;}
    static void Check(bool ok,string text){if(!ok)throw new Exception("SECRETO: "+text);}
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Nivel05_CiudadCostera.unity");
        SessionState.SetBool(Pending,true);SessionState.SetFloat("SecretDeadline",(float)EditorApplication.timeSinceStartup+90);
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();
    }
    static void Place(WilsonController p,float x,float y)
    {
        var b=p.GetComponent<Rigidbody2D>();b.position=new Vector2(x,y);p.transform.position=b.position;b.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();stamp=Time.time;
    }
    static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>SessionState.GetFloat("SecretDeadline",0))throw new TimeoutException("Etapa "+stage);
            if(!EditorApplication.isPlaying)return;
            var p=UnityEngine.Object.FindAnyObjectByType<WilsonController>();var m=CoconutManager.Instance;
            if(p==null || m==null)return;
            var r=p.GetComponent<PlayerRespawn>();var secret=UnityEngine.Object.FindAnyObjectByType<HiddenCoconut>();float elapsed=Time.time-stamp;
            if(stage==0 && Time.time>.3f)
            {
                Check(secret!=null && !secret.Descubierto && secret.cobertura.activeSelf,"caja inicial");
                Check(!secret.coco.GetComponent<Collider2D>().enabled && secret.coco.GetComponentsInChildren<Renderer>().All(a=>!a.enabled),"coco invisible y sin contacto");
                Check(UnityEngine.Object.FindObjectsByType<CoconutCollectible>().Length==29,"mantener los objetos");
                Place(p,16.5f,2.366f);stage=1;
            }
            else if(stage==1 && elapsed>.35f)
            {
                Check(!secret.Descubierto && secret.coco!=null && secret.cobertura.activeSelf,"no encontrar desde el suelo");
                points=m.CocosRecolectados;Place(p,15.55f,5.366f);stage=2;
            }
            else if(stage==2 && secret.Descubierto)
            {
                Check(!secret.cobertura.activeSelf && secret.coco.GetComponent<Collider2D>().enabled && secret.coco.GetComponentsInChildren<Renderer>().All(a=>a.enabled),"revelar con contacto real del jugador");
                Check(m.CocosRecolectados==points,"descubrir no otorga puntos gratis");
                Place(p,16.5f,5.366f);stage=3;
            }
            else if(stage==3 && elapsed>.35f)
            {
                Check(secret.coco==null && m.CocosRecolectados==points+5,"recoger cinco puntos una vez");points=m.CocosRecolectados;
                Place(p,39.5f,3.5f);stage=4;
            }
            else if(stage==4 && r.VidasRestantes==2 && !r.Reapareciendo)
            {
                Check(secret.Descubierto && !secret.cobertura.activeSelf && secret.coco==null && m.CocosRecolectados==points,"conservar hallazgo al caer");
                Place(p,39.5f,3.5f);stage=5;
            }
            else if(stage==5 && r.VidasRestantes==1 && !r.Reapareciendo){Place(p,39.5f,3.5f);stage=6;}
            else if(stage==6 && r.FinDePartida){defeated=r;stage=7;r.ReintentarNivel();}
            else if(stage==7 && r!=defeated && r.VidasRestantes==3)
            {
                Check(!secret.Descubierto && secret.cobertura.activeSelf && secret.coco!=null && !secret.coco.GetComponent<Collider2D>().enabled,"secreto restaurado al reintentar");
                Check(m.CocosRecolectados==0 && UnityEngine.Object.FindObjectsByType<CoconutCollectible>().Length==29,"reinicio completo");
                Place(p,16.5f,5.366f);stage=8;
            }
            else if(stage==8 && elapsed>.35f)
            {
                Check(secret.Descubierto && secret.coco==null && m.CocosRecolectados==5,"redescubrir y recoger al reintentar");
                foreach(var c in UnityEngine.Object.FindObjectsByType<CoconutCollectible>())c.SendMessage("OnTriggerEnter2D",p.GetComponent<Collider2D>());
                Check(m.CocosRecolectados==35 && m.salidaNivel.activeSelf,"35 puntos desbloquean a Chuck");
                Place(p,57.1f,2.366f);m.salidaNivel.SendMessage("OnTriggerEnter2D",p.GetComponent<Collider2D>());stage=9;
            }
            else if(stage==9 && elapsed>1.6f)
            {
                var ending=UnityEngine.Object.FindAnyObjectByType<ReunionEnding>();Check(ending.FinalVisible && ending.salida.SiguienteEscena=="","final con Chuck intacto");
                File.WriteAllText("../cambios_nivel05_secreto/VERIFICACION_PLAYMODE.txt","PASS: coco oculto sin render ni collider; no se descubre desde el suelo; contacto fisico del jugador revela caja; descubrir no regala puntos; recoger da exactamente 5; una caida conserva hallazgo y puntos; Game Over y reintento restauran el secreto; segunda recoleccion valida; total 35 desbloquea a Chuck y el final sigue funcionando.\n");
                SessionState.SetBool(Pending,false);Debug.Log("SECRET_PLAYMODE_OK");EditorApplication.Exit(0);
            }
        }
        catch(Exception e){SessionState.SetBool(Pending,false);Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
