using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class Level05Validation
{
    const string Pending="WilsonCityPending";
    static int stage;
    static GameAudio originalAudio;
    static PlayerRespawn defeated;
    static float endingStarted;
    static Level05Validation(){if(SessionState.GetBool(Pending,false))EditorApplication.update+=Tick;}
    static void Check(bool ok,string message){if(!ok)throw new Exception("CIUDAD: "+message);}
    public static void Run()
    {
        try
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Nivel04_LaPlaya.unity");
            var exit=UnityEngine.Object.FindAnyObjectByType<LevelExit>();
            Check(!exit.EsFinalDelJuego && exit.SiguienteEscena=="Nivel05_CiudadCostera","conexion playa-ciudad");
            SessionState.SetBool(Pending,true);SessionState.SetFloat("CityDeadline",(float)EditorApplication.timeSinceStartup+100);
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();
        }
        catch(Exception e){Fail(e);}
    }
    static void Drop(WilsonController p)
    {
        var body=p.GetComponent<Rigidbody2D>();body.position=new Vector2(39.5f,3.5f);p.transform.position=body.position;body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
    }
    static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>SessionState.GetFloat("CityDeadline",0))throw new TimeoutException("Etapa "+stage);
            if(!EditorApplication.isPlaying)return;
            var p=UnityEngine.Object.FindAnyObjectByType<WilsonController>();var manager=CoconutManager.Instance;var audio=GameAudio.Instance;
            if(p==null || manager==null || audio==null)return;
            var r=p.GetComponent<PlayerRespawn>();var settings=Resources.Load<GameAudioSettings>("WilsonAudioSettings");
            if(stage==0 && Time.time>.3f)
            {
                originalAudio=audio;manager.RecolectarCoco(manager.cocosNecesarios-manager.CocosRecolectados);
                var exit=manager.salidaNivel.GetComponent<LevelExit>();exit.SendMessage("OnTriggerEnter2D",p.GetComponent<Collider2D>());
                Check(exit.NivelCompletado,"victoria de playa");stage=1;exit.ContinuarAlSiguienteNivel();
            }
            else if(stage==1 && SceneManager.GetActiveScene().name=="Nivel05_CiudadCostera")
            {
                var coins=UnityEngine.Object.FindObjectsByType<CoconutCollectible>();Check(coins.Length==29 && coins.Sum(c=>c.valor)==35,"29 cocos, 35 puntos");
                Check(manager.CocosRecolectados==0 && manager.cocosNecesarios==35 && r.VidasRestantes==3,"nuevo nivel independiente");
                Check(GameObject.Find("Chuck")!=null && GameObject.Find("Chuck").activeInHierarchy,"Chuck visible antes de desbloquear el final");
                var end=UnityEngine.Object.FindAnyObjectByType<ReunionEnding>();
                Check(end!=null && end.salida.EsFinalDelJuego && !end.ReencuentroIniciado,"final conectado y pendiente");
                end.salida.SendMessage("OnTriggerEnter2D",p.GetComponent<Collider2D>());
                Check(!end.salida.NivelCompletado && !end.ReencuentroIniciado,"contacto prematuro no termina el juego");
                Check(audio==originalAudio && audio.GetComponents<AudioSource>().Any(a=>a.clip==settings.musicaNivel05 && a.isPlaying),"musica por nivel y singleton");
                Check(UnityEngine.Object.FindObjectsByType<MeshRenderer>().Count(m=>m.name.StartsWith("EdificioCostero_"))==7,"siete edificios");
                Check(UnityEngine.Object.FindObjectsByType<MeshRenderer>().Count(m=>m.name.StartsWith("VecinoDecorativo_"))==8,"ocho vecinos");
                var cp=UnityEngine.Object.FindAnyObjectByType<RespawnCheckpoint>();cp.SendMessage("OnTriggerEnter2D",p.GetComponent<Collider2D>());Check(cp.Activado,"checkpoint activo");
                coins.First(c=>c.valor==1).SendMessage("OnTriggerEnter2D",p.GetComponent<Collider2D>());
                Drop(p);stage=2;
            }
            else if(stage==2 && r.VidasRestantes==2 && !r.Reapareciendo)
            {
                Check(r.TieneCheckpoint && Vector2.Distance(p.transform.position,r.PosicionReaparicion)<.2f && manager.CocosRecolectados>=1,"checkpoint y puntos tras caer");Drop(p);stage=3;
            }
            else if(stage==3 && r.VidasRestantes==1 && !r.Reapareciendo){Drop(p);stage=4;}
            else if(stage==4 && r.FinDePartida)
            {
                Check(r.Fallos==3 && r.VidasRestantes==0,"tres caidas Game Over");Check(audio.GetComponents<AudioSource>().Any(a=>a.clip==settings.musicaGameOver && a.isPlaying),"OMEN");
                defeated=r;stage=5;r.ReintentarNivel();
            }
            else if(stage==5 && r!=defeated && r.VidasRestantes==3)
            {
                var end=UnityEngine.Object.FindAnyObjectByType<ReunionEnding>();var coins=UnityEngine.Object.FindObjectsByType<CoconutCollectible>();
                Check(!r.TieneCheckpoint && manager.CocosRecolectados==0 && coins.Length==29 && !end.ReencuentroIniciado,"reintento limpio");
                foreach(var c in coins)c.SendMessage("OnTriggerEnter2D",p.GetComponent<Collider2D>());
                Check(manager.CocosRecolectados==35 && manager.salidaNivel.activeSelf,"final desbloqueado con 35");
                // A final exit stays final even if an explicit next-scene name is accidentally set.
                var so=new SerializedObject(end.salida);so.FindProperty("siguienteEscena").stringValue="Nivel01_PerdidoEnElMar";so.ApplyModifiedPropertiesWithoutUndo();
                Check(end.salida.SiguienteEscena=="","final sin continuacion");
                var body=p.GetComponent<Rigidbody2D>();body.position=new Vector2(57.1f,r.PosicionReaparicion.y-.05f);p.transform.position=body.position;Physics2D.SyncTransforms();
                end.salida.SendMessage("OnTriggerEnter2D",p.GetComponent<Collider2D>());
                Check(end.salida.NivelCompletado && end.ReencuentroIniciado && !p.enabled && !body.simulated,"evento persistente y jugador detenido");
                Check(!r.IntentarReaparecerPorCaida(),"no revivir tras ganar");end.MostrarFinal();endingStarted=Time.time;stage=6;
            }
            else if(stage==6 && Time.time-endingStarted>1.6f)
            {
                var end=UnityEngine.Object.FindAnyObjectByType<ReunionEnding>();
                Check(end.FinalVisible && Vector2.Distance(p.transform.position,end.puntoEncuentro.position)<.02f,"reencuentro animado terminado");
                Check(!end.interfazNivel.gameObject.activeSelf && !r.enabled,"HUD y vidas ocultos en final");
                Check(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(a=>a.enabled)==1,"un listener");
                stage=7;end.VolverAlInicio();
            }
            else if(stage==7 && SceneManager.GetActiveScene().name=="Nivel01_PerdidoEnElMar" && manager.CocosRecolectados==0)
            {
                Check(p.enabled && p.GetComponent<Rigidbody2D>().simulated,"reinicio del juego jugable");
                Check(UnityEngine.Object.FindAnyObjectByType<ReunionEnding>()==null && !manager.salidaNivel.GetComponent<LevelExit>().NivelCompletado,"sin estado final filtrado");
                File.WriteAllText("../cambios_nivel05/VERIFICACION_PLAYMODE.txt","Unity "+Application.unityVersion+"\nPASS: playa -> ciudad; contador nuevo 0/35; 29 cocos; Chuck visible; final bloqueado antes de tiempo; 7 edificios y 8 vecinos; musica y singleton; checkpoint; tres caidas fisicas; vidas y puntos conservados; OMEN; reintento limpio; 35 puntos desbloquean reencuentro; evento persistente; jugador detenido y no resucitable; final sin continuacion incluso con destino explicito; acercamiento a Chuck; pantalla final activa y HUD oculto; volver al nivel 1 jugable; un listener.\n");
                SessionState.SetBool(Pending,false);Debug.Log("LEVEL05_PLAYMODE_OK");EditorApplication.Exit(0);
            }
        }
        catch(Exception e){Fail(e);}
    }
    static void Fail(Exception e){SessionState.SetBool(Pending,false);Debug.LogException(e);EditorApplication.Exit(1);}
}
