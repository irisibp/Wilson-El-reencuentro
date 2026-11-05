using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class GameOverValidation
{
    const string Pending="WilsonGameOverTestPending";
    const string Stage="WilsonGameOverTestStage";
    static GameAudio originalAudio;
    static PlayerRespawn defeated;
    static GameOverValidation(){if(SessionState.GetBool(Pending,false))EditorApplication.update+=Tick;}
    static void Check(bool ok,string msg){if(!ok)throw new Exception("GAMEOVER: "+msg);}
    static void SetStage(string value){SessionState.SetString(Stage,value);SessionState.SetFloat("WilsonGameOverStageTime",Time.time);}
    public static void Run()
    {
        try
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Nivel02_CavernaDeCoral.unity");
            if(UnityEngine.Object.FindAnyObjectByType<RespawnCheckpoint>()==null)
            {
                Level02HazardsSetup.PrepareActiveScene();EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
            EditorSceneManager.OpenScene("Assets/Scenes/Nivel01_PerdidoEnElMar.unity");
            SessionState.SetBool(Pending,true);SessionState.SetString(Stage,"level1");
            SessionState.SetFloat("WilsonGameOverDeadline",(float)EditorApplication.timeSinceStartup+75);
            EditorApplication.EnterPlaymode();
        }
        catch(Exception e){Fail(e);}
    }
    static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>SessionState.GetFloat("WilsonGameOverDeadline",0))throw new TimeoutException(SessionState.GetString(Stage,""));
            if(!EditorApplication.isPlaying)return;
            var audio=GameAudio.Instance;var manager=CoconutManager.Instance;var player=UnityEngine.Object.FindAnyObjectByType<WilsonController>();
            if(audio==null || manager==null || player==null)return;
            var settings=Resources.Load<GameAudioSettings>("WilsonAudioSettings");
            var music=audio.GetComponents<AudioSource>().Single(s=>s.clip!=null);
            string stage=SessionState.GetString(Stage,"");float elapsed=Time.time-SessionState.GetFloat("WilsonGameOverStageTime",0);
            if(stage=="level1" && Time.time>.25f)
            {
                Check(music.isPlaying && music.clip==settings.musicaNivel01 && music.loop,"Star Way en nivel 1");originalAudio=audio;
                manager.RecolectarCoco(manager.cocosNecesarios-manager.CocosRecolectados);
                var exit=manager.salidaNivel.GetComponent<LevelExit>();exit.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());
                Check(exit.NivelCompletado,"victoria del nivel 1");SetStage("level2");exit.ContinuarAlSiguienteNivel();
            }
            else if(stage=="level2" && SceneManager.GetActiveScene().name=="Nivel02_CavernaDeCoral" && elapsed>.25f)
            {
                Check(audio==originalAudio && music.clip==settings.musicaNivel02 && music.loop && music.isPlaying,"Test Your Skills al continuar");
                var respawn=player.GetComponent<PlayerRespawn>();Check(respawn.VidasRestantes==3 && !respawn.FinDePartida,"tres vidas iniciales");
                respawn.ReintentarNivel();Check(!respawn.FinDePartida && respawn.VidasRestantes==3,"no reiniciar antes de Game Over");
                var coin=UnityEngine.Object.FindObjectsByType<CoconutCollectible>().First(c=>c.valor==1);coin.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());
                Check(manager.CocosRecolectados>=1,"coco recogido");
                Check(respawn.IntentarReaparecer() && respawn.VidasRestantes==2,"primer fallo consume una vida");
                Check(!respawn.IntentarReaparecer() && respawn.VidasRestantes==2,"sin doble consumo");SetStage("return1");
            }
            else if(stage=="return1")
            {
                var respawn=player.GetComponent<PlayerRespawn>();if(respawn.Reapareciendo || elapsed<1.55f)return;
                Check(respawn.VidasRestantes==2 && !respawn.FinDePartida && manager.CocosRecolectados>=1,"primer retorno conserva cocos");
                var point=UnityEngine.Object.FindAnyObjectByType<RespawnCheckpoint>();point.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());Check(point.Activado && respawn.TieneCheckpoint,"checkpoint activado");
                Check(respawn.IntentarReaparecer() && respawn.VidasRestantes==1,"segundo fallo");SetStage("return2");
            }
            else if(stage=="return2")
            {
                var respawn=player.GetComponent<PlayerRespawn>();if(respawn.Reapareciendo || elapsed<1.55f)return;
                Check(respawn.VidasRestantes==1 && respawn.TieneCheckpoint && manager.CocosRecolectados>=1,"segundo retorno conserva progreso");
                Check(Vector2.Distance(player.GetComponent<Rigidbody2D>().position,respawn.PosicionReaparicion)<.15f,"retorno al checkpoint");
                Check(respawn.IntentarReaparecer() && respawn.VidasRestantes==0 && respawn.FinDePartida,"tercer fallo es Game Over");
                defeated=respawn;SetStage("gameover");
            }
            else if(stage=="gameover" && elapsed>.75f)
            {
                var respawn=player.GetComponent<PlayerRespawn>();
                Check(respawn.FinDePartida && !respawn.Reapareciendo && !player.enabled && !player.GetComponent<Rigidbody2D>().simulated,"sin reaparicion automatica ni movimiento");
                Check(music.clip==settings.musicaGameOver && music.isPlaying && !music.loop,"OMEN en Game Over sin forzar bucle");
                Check(!respawn.IntentarReaparecer() && respawn.VidasRestantes==0,"vidas nunca negativas");
                var point=UnityEngine.Object.FindAnyObjectByType<RespawnCheckpoint>();Check(!respawn.ActivarCheckpoint(point.puntoReaparicion),"no activar checkpoint derrotado");
                SetStage("retry");respawn.ReintentarNivel();respawn.ReintentarNivel();
            }
            else if(stage=="retry" && elapsed>.3f)
            {
                var respawn=player.GetComponent<PlayerRespawn>();if(respawn==defeated)return;
                Check(respawn.VidasRestantes==3 && !respawn.FinDePartida && !respawn.TieneCheckpoint && respawn.Fallos==0,"reintentar restaura tres vidas y checkpoint");
                Check(manager.CocosRecolectados==0 && UnityEngine.Object.FindObjectsByType<CoconutCollectible>().Length==14,"reintentar restaura todos los cocos");
                Check(player.enabled && player.GetComponent<Rigidbody2D>().simulated,"movimiento restaurado");
                Check(audio==originalAudio && music.clip==settings.musicaNivel02 && music.loop && music.isPlaying,"reintentar vuelve a Test Your Skills sin duplicar reproductor");
                Check(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(l=>l.enabled)==1,"un listener");
                File.WriteAllText(Path.GetFullPath("../cambios_gameover/VERIFICACION_PLAYMODE.txt"),"Unity "+Application.unityVersion+"\nPASS: Star Way en nivel 1; victoria y Continuar cambian a Test Your Skills; 3 vidas; primeros dos fallos conservan cocos y checkpoint; tercer fallo bloquea movimiento y reproduce OMEN; sin reaparicion automatica ni vidas negativas; reintentar restaura 3 vidas, 14 cocos, contador 0 y checkpoint; regresa la musica del nivel 2; mismo servicio de audio y un listener.\n");
                SessionState.SetBool(Pending,false);Debug.Log("GAMEOVER_VALIDATION_OK");EditorApplication.Exit(0);
            }
        }
        catch(Exception e){Fail(e);}
    }
    static void Fail(Exception e){SessionState.SetBool(Pending,false);Debug.LogException(e);EditorApplication.Exit(1);}
}
