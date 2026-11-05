using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class LevelFlowValidation
{
    const string Pending="WilsonLevelFlowPending";
    const string Stage="WilsonLevelFlowStage";
    static LevelFlowValidation()
    {
        if(SessionState.GetBool(Pending,false))EditorApplication.update+=Tick;
    }
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        try
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Nivel01_PerdidoEnElMar.unity");
            var exit=UnityEngine.Object.FindAnyObjectByType<LevelExit>();
            Check(exit!=null && exit.SiguienteEscena=="Nivel02_CavernaDeCoral","La salida existente adopta la nueva escena por defecto");
            SessionState.SetBool(Pending,true);SessionState.SetString(Stage,"first");
            SessionState.SetFloat("WilsonLevelFlowDeadline",(float)EditorApplication.timeSinceStartup+90f);
            EditorApplication.EnterPlaymode();
        }
        catch(Exception e){Fail(e);}
    }
    static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>SessionState.GetFloat("WilsonLevelFlowDeadline",0))throw new TimeoutException("Transicion de escenas");
            if(!EditorApplication.isPlaying || EditorApplication.isCompiling)return;
            var manager=CoconutManager.Instance;
            if(manager==null)return;
            var player=UnityEngine.Object.FindAnyObjectByType<WilsonController>();
            if(player==null)return;
            var collider=player.GetComponent<Collider2D>();
            string stage=SessionState.GetString(Stage,"");
            if(stage=="first")
            {
                Check(SceneManager.GetActiveScene().name=="Nivel01_PerdidoEnElMar","Comenzar en nivel 1");
                Check(manager.CocosRecolectados==0,"Contador inicial nivel 1");
                var exit=manager.salidaNivel.GetComponent<LevelExit>();
                exit.ContinuarAlSiguienteNivel();Check(!exit.NivelCompletado,"No continuar antes de ganar");
                manager.RecolectarCoco(15);Check(manager.salidaNivel.activeSelf,"Desbloqueo de primera cueva");
                exit.SendMessage("OnTriggerEnter2D",collider);
                Check(exit.NivelCompletado,"Victoria del nivel 1");
                SessionState.SetString(Stage,"second");exit.ContinuarAlSiguienteNivel();
            }
            else if(stage=="second" && SceneManager.GetActiveScene().name=="Nivel02_CavernaDeCoral")
            {
                Check(manager.cocosNecesarios==20 && manager.CocosRecolectados==0,"Contador nuevo 0 / 20");
                Check(player.enabled && player.GetComponent<Rigidbody2D>().simulated,"Wilson recupera el movimiento");
                Check(player.detectorSuelo!=null && player.detectorSuelo.IsChildOf(player.transform),"Detector en jugador nuevo");
                var coins=UnityEngine.Object.FindObjectsByType<CoconutCollectible>(FindObjectsSortMode.None);
                Check(coins.Length==14,"14 objetos coleccionables");
                foreach(var coin in coins)coin.SendMessage("OnTriggerEnter2D",collider);
                Check(manager.CocosRecolectados==20 && manager.salidaNivel.activeSelf,"Todos los cocos desbloquean la segunda salida");
                var exit=manager.salidaNivel.GetComponent<LevelExit>();exit.SendMessage("OnTriggerEnter2D",collider);
                Check(exit.NivelCompletado && exit.SiguienteEscena=="","Victoria final sin escena inexistente");
                exit.ContinuarAlSiguienteNivel();Check(SceneManager.GetActiveScene().name=="Nivel02_CavernaDeCoral","Ultimo nivel permanece en victoria");
                string report="Unity "+Application.unityVersion+"\nPASS en Play Mode: completar nivel 1 con 15 puntos; cargar nivel 2 mediante ContinuarAlSiguienteNivel; nuevo contador 0/20; movimiento activo; recolectar 14 cocos con valor total 20; desbloquear salida; completar nivel 2 sin intentar cargar un nivel 3.\n";
                File.WriteAllText(Path.GetFullPath("../cambios_nivel02/VERIFICACION_FLUJO.txt"),report);
                SessionState.SetBool(Pending,false);Debug.Log("LEVEL_FLOW_VALIDATION_OK\n"+report);EditorApplication.Exit(0);
            }
        }
        catch(Exception e){Fail(e);}
    }
    static void Fail(Exception e){SessionState.SetBool(Pending,false);Debug.LogException(e);EditorApplication.Exit(1);}
}
