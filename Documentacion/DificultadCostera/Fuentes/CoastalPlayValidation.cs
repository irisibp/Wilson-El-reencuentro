using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CoastalPlayValidation
{
    const string Pending="WilsonCoastalPending";
    static int stage, level=4, points;
    static float stamp, maxX, maxY, offsetX;
    static MovingPlatform2D platform;
    static CoastalInputDriver driver;
    static PlayerRespawn defeated;
    static string report="";
    static CoastalPlayValidation(){if(SessionState.GetBool(Pending,false))EditorApplication.update+=Tick;}
    static void Check(bool ok,string text){if(!ok)throw new Exception("NIVEL "+level+" ETAPA "+stage+": "+text);}
    public static void Run()
    {
        EditorSceneManager.OpenScene(CoastalDifficultySetup.Beach);
        SessionState.SetBool(Pending,true);SessionState.SetFloat("CoastalDeadline",(float)EditorApplication.timeSinceStartup+190);
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();
    }
    static void Place(WilsonController p,Vector2 position)
    {
        var body=p.GetComponent<Rigidbody2D>();body.position=position;p.transform.position=position;body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();stamp=Time.time;
    }
    static void Board(WilsonController p,MovingPlatform2D moving)
    {
        platform=moving;var c=p.GetComponent<Collider2D>();Vector2 offset=(Vector2)c.bounds.center-p.GetComponent<Rigidbody2D>().position;
        Place(p,moving.GetComponent<Rigidbody2D>().position+Vector2.up*(.15f+c.bounds.extents.y+.025f)-offset);
        offsetX=0;maxX=maxY=0;driver.horizontal=0;
    }
    static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>SessionState.GetFloat("CoastalDeadline",0))throw new TimeoutException("Etapa "+stage);
            if(!EditorApplication.isPlaying)return;
            var p=UnityEngine.Object.FindAnyObjectByType<WilsonController>();var manager=CoconutManager.Instance;
            if(p==null || manager==null)return;
            var r=p.GetComponent<PlayerRespawn>();float elapsed=Time.time-stamp;
            if(stage==0 && Time.time>.3f)
            {
                Check(manager.cocosNecesarios==(level==4?30:35) && r.VidasRestantes==3,"contador y vidas iniciales");
                Check(UnityEngine.Object.FindObjectsByType<LifeHazard>().Length==(level==4?4:5),"trampas persistentes");
                driver=new GameObject("InputDePrueba",typeof(CoastalInputDriver)).GetComponent<CoastalInputDriver>();driver.player=p;
                Board(p,UnityEngine.Object.FindObjectsByType<MovingPlatform2D>().Single(m=>m.name=="MuelleMovil"));stage=1;
            }
            else if(stage==1 || stage==3)
            {
                if(elapsed>.35f)
                {
                    var c=p.GetComponent<Collider2D>();float dx=Mathf.Abs(c.bounds.center.x-platform.GetComponent<Rigidbody2D>().position.x-offsetX);float dy=Mathf.Abs(c.bounds.min.y-platform.GetComponent<Collider2D>().bounds.max.y);
                    maxX=Mathf.Max(maxX,dx);maxY=Mathf.Max(maxY,dy);
                    Check(maxX<.15f && maxY<.12f,"pasajero estable: dx="+maxX+" dy="+maxY+" apoyo="+(p.PlataformaActual!=null));
                    Check(r.VidasRestantes==3,"plataforma sin daño");
                }
                if(elapsed>6.3f)
                {
                    report+="Nivel "+level+", "+platform.name+": 6.3 s de viaje, pausas y cambios de sentido; deriva X="+maxX.ToString("F4")+", separacion Y="+maxY.ToString("F4")+".\n";
                    if(stage==1){driver.jump=true;driver.horizontal=1;stamp=Time.time;stage=2;}
                    else
                    {
                        points=manager.CocosRecolectados;
                        var danger=UnityEngine.Object.FindObjectsByType<LifeHazard>().OrderBy(h=>h.transform.position.x).First();
                        Place(p,danger.transform.position);stage=4;
                    }
                }
            }
            else if(stage==2 && elapsed>.18f)
            {
                Check(p.PlataformaActual==null && p.GetComponent<Rigidbody2D>().linearVelocity.y>3,"saltar libera el apoyo");
                Check(p.transform.parent==null,"sin emparentar al pasajero");
                driver.horizontal=0;Board(p,UnityEngine.Object.FindObjectsByType<MovingPlatform2D>().Single(m=>m.name.StartsWith("Elevador")));stage=3;
            }
            else if(stage==4 && r.VidasRestantes==2 && !r.Reapareciendo)
            {
                Check(manager.CocosRecolectados==points && p.PlataformaActual==null,"daño conserva puntos y limpia apoyo");
                Check(Vector2.Distance(p.GetComponent<Rigidbody2D>().position,r.PosicionReaparicion)<.2f,"reaparece en suelo seguro");
                var danger=UnityEngine.Object.FindObjectsByType<LifeHazard>().OrderBy(h=>h.transform.position.x).First();Place(p,danger.transform.position);stage=5;
            }
            else if(stage==5 && elapsed>.25f)
            {
                Check(r.VidasRestantes==2 && !r.Reapareciendo,"proteccion evita vidas duplicadas por permanecer en la trampa");
                Place(p,new Vector2(9.5f,1.5f));stage=6;
            }
            else if(stage==6 && r.VidasRestantes==1 && !r.Reapareciendo){Place(p,new Vector2(9.5f,1.5f));stage=7;}
            else if(stage==7 && r.FinDePartida)
            {
                var settings=Resources.Load<GameAudioSettings>("WilsonAudioSettings");Check(GameAudio.Instance.GetComponents<AudioSource>().Any(a=>a.clip==settings.musicaGameOver && a.isPlaying),"OMEN");
                defeated=r;stage=8;r.ReintentarNivel();
            }
            else if(stage==8 && r!=defeated && r.VidasRestantes==3)
            {
                Check(manager.CocosRecolectados==0 && UnityEngine.Object.FindObjectsByType<MovingPlatform2D>().Length==2,"reinicio completo con plataformas");
                if(level==5){Check(!UnityEngine.Object.FindAnyObjectByType<HiddenCoconut>().Descubierto,"secreto reiniciado");Place(p,new Vector2(16.5f,5.366f));stage=9;}
                else CollectAndWin(p,manager);
            }
            else if(stage==9 && elapsed>.35f)
            {
                var hidden=UnityEngine.Object.FindAnyObjectByType<HiddenCoconut>();Check(hidden.Descubierto && hidden.coco==null && manager.CocosRecolectados==5,"coco oculto accesible y puntuando");CollectAndWin(p,manager);
            }
            else if(stage==10 && SceneManager.GetActiveScene().name=="Nivel05_CiudadCostera"){level=5;stage=0;}
            else if(stage==11 && elapsed>1.6f)
            {
                Check(UnityEngine.Object.FindAnyObjectByType<ReunionEnding>().FinalVisible,"reencuentro final");
                Check(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(a=>a.enabled)==1,"un listener");
                File.WriteAllText("../cambios_dificultad_costera/VERIFICACION_PLAYMODE.txt",report+"PASS ambos niveles: pasajeros horizontales y verticales, pausas y reversas, salto sin emparentar, daño de trampas, inmunidad, reaparicion sin arrastre, dos caidas fisicas, Game Over con OMEN, reintento con plataformas y cocos restaurados; 30 puntos y transicion al 5; coco oculto, 35 puntos y final con Chuck.\n");
                SessionState.SetBool(Pending,false);Debug.Log("COASTAL_PLAYMODE_OK");EditorApplication.Exit(0);
            }
        }
        catch(Exception e){SessionState.SetBool(Pending,false);Debug.LogException(e);EditorApplication.Exit(1);}
    }
    static void CollectAndWin(WilsonController p,CoconutManager manager)
    {
        foreach(var c in UnityEngine.Object.FindObjectsByType<CoconutCollectible>())c.SendMessage("OnTriggerEnter2D",p.GetComponent<Collider2D>());
        Check(manager.CocosRecolectados==manager.cocosNecesarios && manager.salidaNivel.activeSelf,"todos los puntos y salida");
        var exit=manager.salidaNivel.GetComponent<LevelExit>();
        if(level==5)Place(p,new Vector2(57.1f,2.366f));
        exit.SendMessage("OnTriggerEnter2D",p.GetComponent<Collider2D>());Check(exit.NivelCompletado,"victoria");
        if(level==4){Check(exit.SiguienteEscena=="Nivel05_CiudadCostera","conexion 4 a 5");stage=10;exit.ContinuarAlSiguienteNivel();}
        else{Check(exit.EsFinalDelJuego && exit.SiguienteEscena=="","ultimo nivel");stamp=Time.time;stage=11;}
    }
}
