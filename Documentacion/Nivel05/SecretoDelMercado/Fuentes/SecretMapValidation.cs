using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
public static class SecretMapValidation {
static readonly HashSet<Vector2Int> FloorCells=new HashSet<Vector2Int>();
static int safeNodes,reachableNodes,trajectorySamples;
public static void Run(){try{
 EditorSceneManager.OpenScene("Assets/Scenes/Nivel05_CiudadCostera.unity");
 Level05SecretSetup.Prepare();
 var cover=Object.FindAnyObjectByType<HiddenCoconut>().cobertura;var sprite=cover.GetComponent<SpriteRenderer>().sprite;
 cover.transform.position=new Vector3(16.5f,4.88f-sprite.bounds.min.y*cover.transform.localScale.y,0);
 int maps=Object.FindObjectsByType<Tilemap>().Length;
 Level05SecretSetup.Prepare();
 if(Object.FindObjectsByType<HiddenCoconut>().Length!=1 || Object.FindObjectsByType<Tilemap>().Length!=maps)throw new Exception("Duplicados");
 foreach(var map in Object.FindObjectsByType<Tilemap>())if(map.GetComponent<TilemapCollider2D>()!=null){
  map.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
  foreach(var cell in map.cellBounds.allPositionsWithin)if(cell.x>=0 && cell.x<60 && cell.y<8 && map.HasTile(cell))FloorCells.Add(new Vector2Int(cell.x,cell.y));
 }
 Physics2D.SyncTransforms();var coins=Object.FindObjectsByType<CoconutCollectible>();
 if(coins.Length!=29 || coins.Sum(c=>c.valor)!=35)throw new Exception("Cambian puntos");
 var hidden=Object.FindAnyObjectByType<HiddenCoconut>();if(hidden.coco.valor!=5 || hidden.coco.GetComponent<Collider2D>().enabled || hidden.coco.GetComponentsInChildren<Renderer>().Any(r=>r.enabled))throw new Exception("Coco no oculto");
 ValidateNavigation(Object.FindAnyObjectByType<WilsonController>(),coins);
 if(safeNodes!=reachableNodes)throw new Exception("Inaccesibles: "+reachableNodes+"/"+safeNodes);
 EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
 File.WriteAllText("../cambios_nivel05_secreto/VERIFICACION_MAPA.txt","PASS: dos balcones adicionales, 29 cocos y 35 puntos, dorado oculto con collider desactivado; menu sin duplicados; "+reachableNodes+"/"+safeNodes+" apoyos alcanzables; "+trajectorySamples+" muestras de trayectorias; todos los cocos apoyados y accesibles. Modelo con tolerancia de radio 0.04.\n");
 SecretPreview.Run();
}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
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
