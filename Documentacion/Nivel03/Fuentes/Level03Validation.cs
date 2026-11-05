using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class Level03Validation
{
    const string Pending="WilsonLevel03Pending";
    const string Stage="WilsonLevel03Stage";
    static GameAudio originalAudio;
    static PlayerRespawn defeated;
    static Level03Validation(){if(SessionState.GetBool(Pending,false))EditorApplication.update+=Tick;}
    static void Check(bool ok,string msg){if(!ok)throw new Exception("NIVEL03: "+msg);}
    static void SetStage(string stage){SessionState.SetString(Stage,stage);SessionState.SetFloat("WilsonLevel03Time",Time.time);}
    public static void Run()
    {
        try
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Nivel02_CavernaDeCoral.unity");
            Check(UnityEngine.Object.FindAnyObjectByType<LevelExit>().SiguienteEscena=="Nivel03_RuinasSumergidas","salida del nivel 2 conecta al 3 sin editar la escena");
            SessionState.SetBool(Pending,true);SessionState.SetString(Stage,"second");
            SessionState.SetFloat("WilsonLevel03Deadline",(float)EditorApplication.timeSinceStartup+90);
            EditorApplication.EnterPlaymode();
        }
        catch(Exception e){Fail(e);}
    }
    static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>SessionState.GetFloat("WilsonLevel03Deadline",0))throw new TimeoutException(SessionState.GetString(Stage,""));
            if(!EditorApplication.isPlaying)return;
            var manager=CoconutManager.Instance;var audio=GameAudio.Instance;var player=UnityEngine.Object.FindAnyObjectByType<WilsonController>();
            if(manager==null || audio==null || player==null)return;
            var settings=Resources.Load<GameAudioSettings>("WilsonAudioSettings");var music=audio.GetComponents<AudioSource>().Single(s=>s.clip!=null);
            string stage=SessionState.GetString(Stage,"");float elapsed=Time.time-SessionState.GetFloat("WilsonLevel03Time",0);
            if(stage=="second" && Time.time>.25f)
            {
                originalAudio=audio;Check(SceneManager.GetActiveScene().name=="Nivel02_CavernaDeCoral","empezar en nivel 2");
                manager.RecolectarCoco(manager.cocosNecesarios-manager.CocosRecolectados);
                var exit=manager.salidaNivel.GetComponent<LevelExit>();exit.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());Check(exit.NivelCompletado,"victoria del nivel 2");
                SetStage("third");exit.ContinuarAlSiguienteNivel();
            }
            else if(stage=="third" && SceneManager.GetActiveScene().name=="Nivel03_RuinasSumergidas" && elapsed>.25f)
            {
                Check(manager.cocosNecesarios==25 && manager.CocosRecolectados==0,"contador 0/25");
                var coins=UnityEngine.Object.FindObjectsByType<CoconutCollectible>();Check(coins.Length==19 && coins.Sum(c=>c.valor)==25,"19 cocos suman 25");
                var respawn=player.GetComponent<PlayerRespawn>();Check(respawn.VidasRestantes==3 && !respawn.TieneCheckpoint,"tres vidas, checkpoint nuevo");
                Check(player.detectorSuelo.IsChildOf(player.transform) && player.enabled && player.GetComponent<Rigidbody2D>().simulated,"jugador independiente y activo");
                Check(audio==originalAudio && music.clip==settings.musicaNivel03 && music.isPlaying,"audio del nivel 3");
                Check(UnityEngine.Object.FindObjectsByType<CoralHazard>().Length==4,"cuatro peligros");
                var point=UnityEngine.Object.FindAnyObjectByType<RespawnCheckpoint>();point.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());Check(point.Activado,"checkpoint activable");
                coins.First(c=>c.valor==1).SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());
                Check(respawn.IntentarReaparecer() && respawn.VidasRestantes==2,"primer fallo");SetStage("death1");
            }
            else if(stage=="death1" && elapsed>1.55f)
            {
                var respawn=player.GetComponent<PlayerRespawn>();Check(!respawn.Reapareciendo && respawn.TieneCheckpoint && manager.CocosRecolectados>=1,"conservar checkpoint y cocos");
                Check(respawn.IntentarReaparecer() && respawn.VidasRestantes==1,"segundo fallo");SetStage("death2");
            }
            else if(stage=="death2" && elapsed>1.55f)
            {
                var respawn=player.GetComponent<PlayerRespawn>();Check(respawn.IntentarReaparecer() && respawn.FinDePartida && respawn.VidasRestantes==0,"tercer fallo Game Over");defeated=respawn;SetStage("gameover");
            }
            else if(stage=="gameover" && elapsed>.5f)
            {
                Check(music.clip==settings.musicaGameOver && music.isPlaying,"OMEN al perder");SetStage("retry");player.GetComponent<PlayerRespawn>().ReintentarNivel();
            }
            else if(stage=="retry" && elapsed>.3f)
            {
                var respawn=player.GetComponent<PlayerRespawn>();if(respawn==defeated)return;
                Check(respawn.VidasRestantes==3 && !respawn.TieneCheckpoint && manager.CocosRecolectados==0,"reinicio completo del nivel 3");
                var coins=UnityEngine.Object.FindObjectsByType<CoconutCollectible>();Check(coins.Length==19,"recuperar 19 cocos");
                foreach(var coin in coins)coin.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());
                Check(manager.CocosRecolectados==25 && manager.salidaNivel.activeSelf,"desbloquear salida con 25 puntos");
                var exit=manager.salidaNivel.GetComponent<LevelExit>();Check(exit.SiguienteEscena=="","no cargar SampleScene como nivel 4");
                exit.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());Check(exit.NivelCompletado,"completar ruinas");exit.ContinuarAlSiguienteNivel();
                Check(SceneManager.GetActiveScene().name=="Nivel03_RuinasSumergidas" && !respawn.IntentarReaparecer(),"victoria final estable");
                Check(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(l=>l.enabled)==1,"listener unico");
                File.WriteAllText(Path.GetFullPath("../cambios_nivel03/VERIFICACION_PLAYMODE.txt"),"Unity "+Application.unityVersion+"\nPASS: completar nivel 2 y continuar al 3; contador nuevo 0/25; 19 cocos; 4 peligros; checkpoint independiente; jugador activo; musica asignada; 3 vidas; conservar cocos en primeros fallos; Game Over con OMEN; reintentar restaura 3 vidas, 19 cocos y checkpoint; victoria con 25 puntos; no se carga SampleScene como nivel 4; un listener.\n");
                SessionState.SetBool(Pending,false);Debug.Log("LEVEL03_PLAYMODE_OK");EditorApplication.Exit(0);
            }
        }
        catch(Exception e){Fail(e);}
    }
    static void Fail(Exception e){SessionState.SetBool(Pending,false);Debug.LogException(e);EditorApplication.Exit(1);}
}
