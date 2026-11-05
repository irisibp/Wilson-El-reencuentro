using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object=UnityEngine.Object;
[InitializeOnLoad]
public static class GapsValidation {
static HashSet<Vector2Int> FloorCells=new HashSet<Vector2Int>();
static int safeNodes,reachableNodes,trajectorySamples;
const string Pending="WilsonGapsPending";
static float stamp;
static int stage;
static GapsValidation(){if(SessionState.GetBool(Pending,false))EditorApplication.update+=Tick;}
static void Check(bool ok,string msg){if(!ok)throw new Exception(msg);}
public static void Run(){try{
 EditorSceneManager.OpenScene("Assets/Scenes/Nivel03_RuinasSumergidas.unity");
 Level03GapsSetup.Prepare();
 var map=Object.FindObjectsByType<Tilemap>().Single(m=>m.name=="TerrenoNivel03");
 foreach(var p in map.cellBounds.allPositionsWithin)if(map.HasTile(p))FloorCells.Add(new Vector2Int(p.x,p.y));
 foreach(var col in Object.FindObjectsByType<TilemapCollider2D>())col.ProcessTilemapChanges();Physics2D.SyncTransforms();
 ValidateNavigation(Object.FindAnyObjectByType<WilsonController>(),Object.FindObjectsByType<CoconutCollectible>());
 Check(safeNodes==reachableNodes,"Inaccesibles: "+reachableNodes+"/"+safeNodes);
 foreach(int x in new[]{11,12,20,21,42,43})for(int y=-2;y<1;y++)Check(!map.HasTile(new Vector3Int(x,y,0)),"Hueco bloqueado");
 EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
 File.WriteAllText("../cambios_nivel03/VERIFICACION_VACIOS.txt","PASS geometria: "+reachableNodes+"/"+safeNodes+" apoyos seguros accesibles; "+trajectorySamples+" muestras de trayectorias, 19 cocos accesibles. Modelo balistico con pulsos de movimiento horizontal, radio de colision menos 0.04 de tolerancia.\n");
 SessionState.SetBool(Pending,true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();
}catch(Exception e){Fail(e);}}
static void Drop(WilsonController p){var rb=p.GetComponent<Rigidbody2D>();rb.position=new Vector2(20.7f,1.5f);p.transform.position=rb.position;rb.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();stamp=Time.time;}
static void Tick(){try{
 if(!EditorApplication.isPlaying)return;
 var p=Object.FindAnyObjectByType<WilsonController>();if(p==null)return;var r=p.GetComponent<PlayerRespawn>();
 if(stage==0 && Time.time>.3f){Drop(p);stage=1;}
 else if(stage==1 && r.VidasRestantes==2 && !r.Reapareciendo){
 Check(Vector2.Distance(p.transform.position,r.PosicionReaparicion)<.2f,"Volver al inicio");
 var cp=Object.FindAnyObjectByType<RespawnCheckpoint>();Check(r.ActivarCheckpoint(cp.puntoReaparicion),"Activar checkpoint");Drop(p);stage=2;
 }
 else if(stage==2 && r.VidasRestantes==1 && !r.Reapareciendo){
 Check(r.TieneCheckpoint && Vector2.Distance(p.transform.position,r.PosicionReaparicion)<.2f,"Volver al checkpoint");
 Check(!r.IntentarReaparecer(),"Corales respetan proteccion");Drop(p);stage=3;
 }
 else if(stage==3 && r.FinDePartida){
 Check(r.VidasRestantes==0 && r.Fallos==3,"Una vida por caida");
 var settings=Resources.Load<GameAudioSettings>("WilsonAudioSettings");Check(GameAudio.Instance.GetComponents<AudioSource>().Any(a=>a.clip==settings.musicaGameOver),"OMEN");
 stage=4;stamp=Time.time;r.ReintentarNivel();
 }
 else if(stage==4 && r.VidasRestantes==3){
 Check(!r.TieneCheckpoint && CoconutManager.Instance.CocosRecolectados==0,"Reinicio limpio");
 Check(Object.FindAnyObjectByType<FallZone>()!=null,"Zona guardada en reinicio");
 foreach(var c in Object.FindObjectsByType<CoconutCollectible>())c.SendMessage("OnTriggerEnter2D",p.GetComponent<Collider2D>());
 Check(CoconutManager.Instance.CocosRecolectados==25 && CoconutManager.Instance.salidaNivel.activeSelf,"25 puntos y salida");
 File.AppendAllText("../cambios_nivel03/VERIFICACION_VACIOS.txt","PASS Play Mode: caidas fisicas, reaparicion inicial y checkpoint, caida durante proteccion, una vida por caida, tres caidas Game Over y OMEN, reintento con huecos persistentes y 3 vidas, 25 puntos y salida.\n");
 SessionState.SetBool(Pending,false);Debug.Log("GAPS_OK");EditorApplication.Exit(0);
 }
 if(stage<4 && Time.time-stamp>8)throw new Exception("Timeout caida etapa "+stage);
}catch(Exception e){Fail(e);}}
static void Fail(Exception e){SessionState.SetBool(Pending,false);Debug.LogException(e);EditorApplication.Exit(1);}
    static bool Clear(Vector2 position,float radius)=>!Physics2D.OverlapCircleAll(position,radius).Any(c=>c.gameObject.layer==3 || c.GetComponent<CoralHazard>()!=null);
    static void ValidateNavigation(WilsonController player,CoconutCollectible[] coins)
    {
        float radius=player.GetComponent<Collider2D>().bounds.extents.y;float testRadius=radius-.04f;
        var nodes=FloorCells.Where(p=>!FloorCells.Contains(p+Vector2Int.up)).Select(p=>new Vector2(p.x+.5f,p.y+1+radius)).Where(p=>Clear(p,testRadius)).OrderBy(p=>p.x).ThenBy(p=>p.y).ToList();
        safeNodes=nodes.Count;trajectorySamples=0;float g=Mathf.Abs(Physics2D.gravity.y)*player.GetComponent<Rigidbody2D>().gravityScale;
        bool Reach(Vector2 a,Vector2 b)
        {
            float dx=b.x-a.x,dy=b.y-a.y;
            if(Mathf.Abs(dx)<=1.01f && Mathf.Abs(dy)<.01f)
            {
                for(float k=0;k<=1;k+=.1f)if(!Clear(Vector2.Lerp(a,b,k),testRadius))return false;return true;
            }
            float disc=player.fuerzaSalto*player.fuerzaSalto-2*g*dy;if(disc<0)return false;
            float time=(player.fuerzaSalto+Mathf.Sqrt(disc))/g;if(Mathf.Abs(dx)>player.velocidad*time)return false;
            foreach(float delay in new[]{0f,.08f,.16f,.24f})
            {
                if(Mathf.Abs(dx)>player.velocidad*(time-delay))continue;
                bool clear=true;
                for(float t=.01f;t<time-.01f;t+=.01f)
                {
                    var p=a+new Vector2(Mathf.Sign(dx)*Mathf.Min(Mathf.Abs(dx),player.velocidad*Mathf.Max(0,t-delay)),player.fuerzaSalto*t-.5f*g*t*t);trajectorySamples++;
                    if(!Clear(p,testRadius)){clear=false;break;}
                }
                if(clear)return true;
            }
            return false;
        }
        int start=nodes.FindIndex(n=>Mathf.Abs(n.x-1.5f)<.01f);if(start<0)throw new Exception("Inicio inseguro");
        var visited=new HashSet<int>{start};var queue=new Queue<int>();queue.Enqueue(start);
        while(queue.Count>0){int a=queue.Dequeue();for(int b=0;b<nodes.Count;b++)if(!visited.Contains(b) && Reach(nodes[a],nodes[b])){visited.Add(b);queue.Enqueue(b);}}
        reachableNodes=visited.Count;
        if(reachableNodes!=safeNodes)Debug.Log("No alcanzables: "+string.Join(",",nodes.Where((n,i)=>!visited.Contains(i))));
        foreach(var c in coins)if(!nodes.Where((n,i)=>visited.Contains(i)).Any(n=>Mathf.Abs(n.x-c.transform.position.x)<=.55f && Mathf.Abs(n.y-c.transform.position.y)<.3f))throw new Exception("Coco sin apoyo accesible: "+c.transform.position);
    }
}
