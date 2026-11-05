using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AudioPlayValidation
{
    const string Pending="WilsonAudioTestPending";
    const string Stage="WilsonAudioTestStage";
    static GameAudio originalAudio;
    static AudioSource originalMusic;
    static AudioPlayValidation(){if(SessionState.GetBool(Pending,false))EditorApplication.update+=Tick;}
    static void Check(bool ok,string msg){if(!ok)throw new Exception("AUDIO: "+msg);}
    public static void Run()
    {
        try
        {
            // The authored file may predate the unsaved setup currently open in the user's editor.
            // Prepare the isolated fixture only; never rewrite the user's open scene.
            EditorSceneManager.OpenScene("Assets/Scenes/Nivel02_CavernaDeCoral.unity");
            if(UnityEngine.Object.FindAnyObjectByType<RespawnCheckpoint>()==null)
            {
                Level02HazardsSetup.PrepareActiveScene();
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
            EditorSceneManager.OpenScene("Assets/Scenes/Nivel01_PerdidoEnElMar.unity");
            SessionState.SetBool(Pending,true);SessionState.SetString(Stage,"start");
            SessionState.SetFloat("WilsonAudioDeadline",(float)EditorApplication.timeSinceStartup+75);
            EditorApplication.EnterPlaymode();
        }
        catch(Exception e){Fail(e);}
    }
    static void SetStage(string value){SessionState.SetString(Stage,value);SessionState.SetFloat("WilsonAudioStageTime",Time.time);}
    static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>SessionState.GetFloat("WilsonAudioDeadline",0))throw new TimeoutException("Audio en fase "+SessionState.GetString(Stage,""));
            if(!EditorApplication.isPlaying)return;
            var audio=GameAudio.Instance;var manager=CoconutManager.Instance;
            if(audio==null || manager==null)return;
            var settings=Resources.Load<GameAudioSettings>("WilsonAudioSettings");
            var sources=audio.GetComponents<AudioSource>();Check(sources.Length==2,"dos fuentes separadas");
            var music=sources.Single(s=>s.loop);var fx=sources.Single(s=>!s.loop);
            string stage=SessionState.GetString(Stage,"");float elapsed=Time.time-SessionState.GetFloat("WilsonAudioStageTime",0);
            if(stage=="start" && Time.time>.25f)
            {
                Check(music.isPlaying && music.clip==settings.musica,"musica en reproduccion");
                Check(music.spatialBlend==0 && fx.spatialBlend==0,"audio 2D");
                Check(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(l=>l.enabled)==1,"un listener");
                originalAudio=audio;originalMusic=music;
                new GameObject("DuplicateAudioTest").AddComponent<GameAudio>();
                SessionState.SetInt("WilsonAudioCue",0);SetStage("cue");GameAudio.Sonar(GameSound.Coco);
            }
            else if(stage=="cue" && elapsed>.03f)
            {
                Check(fx.isPlaying,"efecto en reproduccion "+SessionState.GetInt("WilsonAudioCue",0));
                Check(UnityEngine.Object.FindObjectsByType<GameAudio>().Length==1,"singleton sin duplicados");SetStage("cueEnd");
            }
            else if(stage=="cueEnd" && elapsed>.7f)
            {
                int cue=SessionState.GetInt("WilsonAudioCue",0)+1;SessionState.SetInt("WilsonAudioCue",cue);
                if(cue<6){SetStage("cue");GameAudio.Sonar((GameSound)cue);}
                else
                {
                    var player=UnityEngine.Object.FindAnyObjectByType<WilsonController>();
                    var coin=UnityEngine.Object.FindObjectsByType<CoconutCollectible>().First(c=>c.valor==1);
                    int score=manager.CocosRecolectados;coin.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());
                    Check(manager.CocosRecolectados==score+1 && fx.isPlaying,"evento de coco");
                    manager.RecolectarCoco(manager.cocosNecesarios-manager.CocosRecolectados);
                    manager.salidaNivel.GetComponent<LevelExit>().SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());
                    Check(manager.salidaNivel.GetComponent<LevelExit>().NivelCompletado,"victoria");SetStage("victory");
                }
            }
            else if(stage=="victory" && elapsed>.3f)
            {
                Check(music.volume<settings.volumenMusica*.6f,"musica atenuada en victoria");
                SessionState.SetFloat("WilsonMusicTime",music.time);SetStage("second");
                manager.salidaNivel.GetComponent<LevelExit>().ContinuarAlSiguienteNivel();
            }
            else if(stage=="second" && SceneManager.GetActiveScene().name=="Nivel02_CavernaDeCoral" && elapsed>.25f)
            {
                Check(audio==originalAudio && music==originalMusic,"misma musica entre escenas");
                Check(music.isPlaying,"musica sigue sonando");
                float delta=music.time-SessionState.GetFloat("WilsonMusicTime",0);if(delta<0)delta+=music.clip.length;
                Check(delta>0 && delta<3,"no reiniciar la pista al cambiar de nivel");
                Check(UnityEngine.Object.FindObjectsByType<AudioListener>().Count(l=>l.enabled)==1 && Camera.main.GetComponent<AudioListener>()!=null,"listener creado en la segunda camara");
                Check(music.volume>settings.volumenMusica*.8f,"restaurar volumen tras victoria");
                var player=UnityEngine.Object.FindAnyObjectByType<WilsonController>();var point=UnityEngine.Object.FindAnyObjectByType<RespawnCheckpoint>();
                point.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());Check(point.Activado && fx.isPlaying,"efecto del checkpoint");
                SetStage("checkpoint");
            }
            else if(stage=="checkpoint" && elapsed>.6f)
            {
                var respawn=UnityEngine.Object.FindAnyObjectByType<PlayerRespawn>();Check(respawn.IntentarReaparecer() && fx.isPlaying,"efecto de fallo");SetStage("fall");
            }
            else if(stage=="fall" && elapsed>.6f)
            {
                Check(music.isPlaying,"musica continua al reaparecer");
                string report="Unity "+Application.unityVersion+"\nPASS en Play Mode: musica en bucle en AudioSource 2D; seis efectos en reproduccion; dos fuentes separadas; singleton sin duplicados; un listener habilitado por escena; eventos de coco, victoria, checkpoint y fallo; volumen atenuado al ganar y restaurado al continuar; misma fuente y avance de pista entre niveles; musica sigue durante la reaparicion.\n";
                File.WriteAllText(Path.GetFullPath("../cambios_audio/VERIFICACION_PLAYMODE.txt"),report);
                SessionState.SetBool(Pending,false);Debug.Log("AUDIO_PLAYMODE_OK\n"+report);EditorApplication.Exit(0);
            }
        }
        catch(Exception e){Fail(e);}
    }
    static void Fail(Exception e){SessionState.SetBool(Pending,false);Debug.LogException(e);EditorApplication.Exit(1);}
}
