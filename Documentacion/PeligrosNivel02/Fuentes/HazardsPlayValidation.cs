using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class HazardsPlayValidation
{
    const string Pending="WilsonHazardTestPending";
    const string Stage="WilsonHazardTestStage";
    static GameObject collected;
    static HazardsPlayValidation(){if(SessionState.GetBool(Pending,false))EditorApplication.update+=Tick;}
    static void Check(bool ok,string msg){if(!ok)throw new Exception("REAPARICION: "+msg);}
    public static void Run()
    {
        try
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Nivel02_CavernaDeCoral.unity");
            SessionState.SetBool(Pending,true);SessionState.SetString(Stage,"start");
            SessionState.SetFloat("WilsonHazardDeadline",(float)EditorApplication.timeSinceStartup+75f);
            EditorApplication.EnterPlaymode();
        }
        catch(Exception e){Fail(e);}
    }
    static void Move(WilsonController player,Vector2 position)
    {
        player.ReiniciarMovimiento();player.GetComponent<Rigidbody2D>().position=position;
        player.transform.position=new Vector3(position.x,position.y,0);Physics2D.SyncTransforms();
    }
    static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>SessionState.GetFloat("WilsonHazardDeadline",0))throw new TimeoutException("Fase "+SessionState.GetString(Stage,""));
            if(!EditorApplication.isPlaying)return;
            var manager=CoconutManager.Instance;var player=UnityEngine.Object.FindAnyObjectByType<WilsonController>();
            if(manager==null || player==null)return;
            var respawn=player.GetComponent<PlayerRespawn>();var point=UnityEngine.Object.FindAnyObjectByType<RespawnCheckpoint>();
            if(respawn==null || point==null)return;
            var body=player.GetComponent<Rigidbody2D>();var collider=player.GetComponent<Collider2D>();
            var hazards=UnityEngine.Object.FindObjectsByType<CoralHazard>(FindObjectsSortMode.None).OrderBy(h=>h.transform.position.x).ToArray();
            string stage=SessionState.GetString(Stage,"");
            if(stage=="start")
            {
                Check(manager.CocosRecolectados==0 && !respawn.TieneCheckpoint,"estado inicial");
                var coin=UnityEngine.Object.FindObjectsByType<CoconutCollectible>(FindObjectsSortMode.None).First(c=>c.valor==1);collected=coin.gameObject;coin.SendMessage("OnTriggerEnter2D",collider);
                Check(manager.CocosRecolectados==1,"coco recogido antes de fallar");
                var field=typeof(WilsonController).GetField("solicitarSalto",BindingFlags.Instance|BindingFlags.NonPublic);field.SetValue(player,true);
                player.ReiniciarMovimiento();Check(!(bool)field.GetValue(player),"limpiar salto pendiente");
                SessionState.SetString(Stage,"firstDeath");Move(player,hazards[0].transform.position);
            }
            else if(stage=="firstDeath" && respawn.Reapareciendo)
            {
                Check(respawn.Fallos==1 && !player.enabled && !body.simulated,"contacto fisico inicia reaparicion");
                Check(!respawn.IntentarReaparecer() && !respawn.IntentarReaparecer() && respawn.Fallos==1,"contactos repetidos no duplican fallos");
                // A queued exit callback cannot win while Wilson is in a respawn pause.
                manager.cocosNecesarios=1;manager.salidaNivel.SetActive(true);var exit=manager.salidaNivel.GetComponent<LevelExit>();exit.SendMessage("OnTriggerEnter2D",collider);
                Check(!exit.NivelCompletado,"no completar nivel mientras esta reapareciendo");manager.cocosNecesarios=20;manager.salidaNivel.SetActive(false);
                SessionState.SetString(Stage,"firstReturn");
            }
            else if(stage=="firstReturn" && !respawn.Reapareciendo)
            {
                Check(Vector2.Distance(body.position,respawn.PosicionReaparicion)<.12f && respawn.PosicionReaparicion.x<2,"volver al inicio antes del checkpoint");
                Check(manager.CocosRecolectados==1 && collected==null,"mantener puntaje y coco destruido");
                Check(!respawn.IntentarReaparecer(),"proteccion temporal");
                Check(Mathf.Abs(Camera.main.transform.position.x-8.6f)<.02f,"camara vuelve al inicio");
                SessionState.SetFloat("WilsonHazardFirstReturn",Time.time);SessionState.SetString(Stage,"checkpoint");Move(player,point.puntoReaparicion.position);
            }
            else if(stage=="checkpoint" && point.Activado)
            {
                Check(respawn.TieneCheckpoint && Vector2.Distance(respawn.PosicionReaparicion,point.puntoReaparicion.position)<.001f,"contacto fisico activa checkpoint");
                Check(point.textoMarcador.text=="Activado","aviso del checkpoint");
                if(Time.time-SessionState.GetFloat("WilsonHazardFirstReturn",0)<1.2f)return;
                SessionState.SetInt("WilsonCheckpointScore",manager.CocosRecolectados);
                SessionState.SetString(Stage,"secondDeath");Move(player,hazards[1].transform.position);
            }
            else if(stage=="secondDeath" && respawn.Reapareciendo)
            {
                Check(respawn.Fallos==2,"segundo contacto despues de la proteccion");SessionState.SetString(Stage,"secondReturn");
            }
            else if(stage=="secondReturn" && !respawn.Reapareciendo)
            {
                Check(Vector2.Distance(body.position,point.puntoReaparicion.position)<.12f,"volver al checkpoint");
                Check(manager.CocosRecolectados>=SessionState.GetInt("WilsonCheckpointScore",0),"puntaje conservado: actual="+manager.CocosRecolectados+" anterior="+SessionState.GetInt("WilsonCheckpointScore",0));
                Check(player.enabled && body.simulated,"movimiento reactivado: controlador="+player.enabled+" cuerpo="+body.simulated+" fallos="+respawn.Fallos);
                Check(Mathf.Abs(Camera.main.transform.position.x-17.5f)<.02f,"camara en el checkpoint");
                foreach(var coin in UnityEngine.Object.FindObjectsByType<CoconutCollectible>(FindObjectsSortMode.None))coin.SendMessage("OnTriggerEnter2D",collider);
                Check(manager.CocosRecolectados==20 && manager.salidaNivel.activeSelf,"desbloqueo de la salida tras reaparecer");
                manager.salidaNivel.GetComponent<LevelExit>().SendMessage("OnTriggerEnter2D",collider);
                Check(manager.salidaNivel.GetComponent<LevelExit>().NivelCompletado,"victoria sigue funcionando");
                SessionState.SetFloat("WilsonHazardVictory",Time.time);SessionState.SetString(Stage,"victory");
            }
            else if(stage=="victory" && Time.time-SessionState.GetFloat("WilsonHazardVictory",0)>1.2f)
            {
                Check(!respawn.IntentarReaparecer() && !player.enabled && !body.simulated,"no revivir despues de completar el nivel");
                string report="Unity "+Application.unityVersion+"\nPASS en Play Mode: contactos fisicos con coral; una sola reaparicion por fallo; retorno al inicio; salto pendiente limpiado; cocos y puntaje conservados; proteccion temporal; activacion fisica del checkpoint; segundo fallo vuelve al checkpoint; camara reencuadrada; salida desbloqueada y victoria; no revivir despues de ganar.\n";
                File.WriteAllText(Path.GetFullPath("../cambios_peligros/VERIFICACION_PLAYMODE.txt"),report);
                SessionState.SetBool(Pending,false);Debug.Log("HAZARDS_PLAYMODE_OK\n"+report);EditorApplication.Exit(0);
            }
        }
        catch(Exception e){Fail(e);}
    }
    static void Fail(Exception e){SessionState.SetBool(Pending,false);Debug.LogException(e);EditorApplication.Exit(1);}
}
