using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class Level04Validation
{
    const string Pending="WilsonBeachPending";
    static GameAudio previousAudio;
    static PlayerRespawn defeated;
    static int stage;
    static float stamp;
    static Level04Validation(){if(SessionState.GetBool(Pending,false))EditorApplication.update+=Tick;}
    static void Check(bool ok,string message){if(!ok)throw new Exception("PLAYA: "+message);}
    public static void Run()
    {
        try
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Nivel03_RuinasSumergidas.unity");
            Check(UnityEngine.Object.FindAnyObjectByType<LevelExit>().SiguienteEscena=="Nivel04_LaPlaya","conexion de ruinas a playa");
            SessionState.SetBool(Pending,true);
            SessionState.SetFloat("WilsonBeachDeadline",(float)EditorApplication.timeSinceStartup+90);
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;
            EditorApplication.EnterPlaymode();
        }
        catch(Exception e){Fail(e);}
    }
    static void Drop(WilsonController player)
    {
        var body=player.GetComponent<Rigidbody2D>();body.position=new Vector2(36.5f,3.5f);
        player.transform.position=body.position;body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();stamp=Time.time;
    }
    static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>SessionState.GetFloat("WilsonBeachDeadline",0))throw new TimeoutException("Etapa "+stage);
            if(!EditorApplication.isPlaying)return;
            var player=UnityEngine.Object.FindAnyObjectByType<WilsonController>();var manager=CoconutManager.Instance;var audio=GameAudio.Instance;
            if(player==null || manager==null || audio==null)return;
            var r=player.GetComponent<PlayerRespawn>();
            var settings=Resources.Load<GameAudioSettings>("WilsonAudioSettings");
            if(stage==0 && Time.time>.3f)
            {
                previousAudio=audio;manager.RecolectarCoco(manager.cocosNecesarios-manager.CocosRecolectados);
                var exit=manager.salidaNivel.GetComponent<LevelExit>();exit.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());
                Check(exit.NivelCompletado,"completar ruinas");stage=1;exit.ContinuarAlSiguienteNivel();
            }
            else if(stage==1 && SceneManager.GetActiveScene().name=="Nivel04_LaPlaya")
            {
                Check(manager.cocosNecesarios==30 && manager.CocosRecolectados==0,"contador nuevo 0/30");
                var coins=UnityEngine.Object.FindObjectsByType<CoconutCollectible>();Check(coins.Length==24 && coins.Sum(c=>c.valor)==30,"24 cocos, 30 puntos");
                Check(r.VidasRestantes==3 && !r.TieneCheckpoint,"tres vidas nuevas");
                Check(audio==previousAudio && audio.GetComponents<AudioSource>().Any(s=>s.clip==settings.musicaNivel04 && s.isPlaying),"Star Way y audio persistente");
                Check(UnityEngine.Object.FindObjectsByType<CoralHazard>().Length==0,"playa sin corales submarinos peligrosos");
                Check(UnityEngine.Object.FindObjectsByType<FallZone>().Length==1,"una zona de caida");
                var point=UnityEngine.Object.FindAnyObjectByType<RespawnCheckpoint>();point.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());
                Check(point.Activado,"punto seguro activo");
                coins.First(c=>c.valor==1).SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());
                Drop(player);stage=2;
            }
            else if(stage==2 && r.VidasRestantes==2 && !r.Reapareciendo)
            {
                Check(r.TieneCheckpoint && Vector2.Distance(player.transform.position,r.PosicionReaparicion)<.2f,"reaparicion segura");
                Check(manager.CocosRecolectados>=1,"conservar puntos al caer");Drop(player);stage=3;
            }
            else if(stage==3 && r.VidasRestantes==1 && !r.Reapareciendo){Drop(player);stage=4;}
            else if(stage==4 && r.FinDePartida)
            {
                Check(r.VidasRestantes==0 && r.Fallos==3,"tercera caida Game Over");
                Check(audio.GetComponents<AudioSource>().Any(s=>s.clip==settings.musicaGameOver && s.isPlaying),"OMEN");
                defeated=r;stage=5;r.ReintentarNivel();
            }
            else if(stage==5 && r!=defeated && r.VidasRestantes==3)
            {
                Check(!r.TieneCheckpoint && manager.CocosRecolectados==0,"reinicio limpio");
                var coins=UnityEngine.Object.FindObjectsByType<CoconutCollectible>();Check(coins.Length==24,"recuperar cocos");
                foreach(var coin in coins)coin.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());
                Check(manager.CocosRecolectados==30 && manager.salidaNivel.activeSelf,"desbloquear salida");
                var exit=manager.salidaNivel.GetComponent<LevelExit>();Check(exit.SiguienteEscena=="","no cargar SampleScene como nivel 5");
                exit.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());Check(exit.NivelCompletado,"victoria de playa");
                Check(!r.IntentarReaparecerPorCaida(),"no revivir tras victoria");
                Check(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(a=>a.enabled)==1,"un listener");
                File.WriteAllText("../cambios_nivel04/VERIFICACION_PLAYMODE.txt","Unity "+Application.unityVersion+"\nPASS: completar ruinas y continuar a playa; contador nuevo; 24 cocos/30 puntos; Star Way; checkpoint independiente; caidas fisicas en hueco de tres celdas; conservar puntos; tres vidas; Game Over y OMEN; reintento limpio; salida con 30 puntos; victoria final estable; SampleScene excluida; un listener.\n");
                SessionState.SetBool(Pending,false);Debug.Log("LEVEL04_PLAYMODE_OK");EditorApplication.Exit(0);
            }
        }
        catch(Exception e){Fail(e);}
    }
    static void Fail(Exception e){SessionState.SetBool(Pending,false);Debug.LogException(e);EditorApplication.Exit(1);}
}
