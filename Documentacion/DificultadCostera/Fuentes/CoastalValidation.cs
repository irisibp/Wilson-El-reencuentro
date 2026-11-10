using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object=UnityEngine.Object;

public static class CoastalValidation
{
    struct Node {public Vector2 p;public MovingPlatform2D moving;public Node(Vector2 p,MovingPlatform2D m=null){this.p=p;moving=m;}}
    static void Check(bool ok,string text){if(!ok)throw new Exception(text);}
    public static void Run()
    {
        try
        {
            CoastalDifficultySetup.ApplyBoth();
            Directory.CreateDirectory("../cambios_dificultad_costera");
            string report="Unity "+Application.unityVersion+"\n";
            foreach(string path in new[]{CoastalDifficultySetup.Beach,CoastalDifficultySetup.City})
            {
                EditorSceneManager.OpenScene(path);
                int count=Object.FindObjectsByType<MovingPlatform2D>().Length;
                CoastalDifficultySetup.PrepareActive();
                Check(count==2 && Object.FindObjectsByType<MovingPlatform2D>().Length==2,"Dos plataformas e idempotencia");
                bool city=path==CoastalDifficultySetup.City;
                Check(Object.FindObjectsByType<LifeHazard>().Length==(city?5:4),"Numero de trampas");
                var coins=Object.FindObjectsByType<CoconutCollectible>();Check(coins.Length==(city?29:24) && coins.Sum(c=>c.valor)==(city?35:30),"Conservar cocos y puntos");
                if(city)Check(Object.FindObjectsByType<HiddenCoconut>().Length==1 && Object.FindAnyObjectByType<ReunionEnding>()!=null,"Secreto y final conservados");
                report+=ValidatePath(city,coins);
                EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
            File.WriteAllText("../cambios_dificultad_costera/VERIFICACION_MAPA.txt",report);
            Debug.Log("COASTAL_MAP_OK");EditorApplication.Exit(0);
        }
        catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
    static string ValidatePath(bool city,CoconutCollectible[] coins)
    {
        foreach(var c in Object.FindObjectsByType<TilemapCollider2D>())c.ProcessTilemapChanges();Physics2D.SyncTransforms();
        var player=Object.FindAnyObjectByType<WilsonController>();float radius=player.GetComponent<Collider2D>().bounds.extents.y;float tolerance=radius-.035f;
        var platforms=Object.FindObjectsByType<MovingPlatform2D>();
        foreach(var p in platforms)p.GetComponent<Collider2D>().enabled=false;
        var cells=new HashSet<Vector2Int>();
        foreach(var map in Object.FindObjectsByType<Tilemap>().Where(t=>t.GetComponent<TilemapCollider2D>()!=null))foreach(var c in map.cellBounds.allPositionsWithin)if(c.x>=0 && c.x<(city?60:52) && c.y<8 && map.HasTile(c))cells.Add(new Vector2Int(c.x,c.y));
        bool Clear(Vector2 p)=>!Physics2D.OverlapCircleAll(p,tolerance).Any(c=>c.gameObject.layer==3 || c.GetComponent<LifeHazard>()!=null);
        var nodes=cells.Where(c=>!cells.Contains(c+Vector2Int.up)).Select(c=>new Node(new Vector2(c.x+.5f,c.y+1+radius))).Where(n=>Clear(n.p)).ToList();int fixedNodes=nodes.Count;
        foreach(var platform in platforms)
        {
            float h=platform.GetComponent<BoxCollider2D>().size.y/2;
            foreach(float phase in new[]{0f,.5f,1f})foreach(float offset in new[]{-.5f,0,.5f})
            {
                Vector2 center=Vector2.Lerp(platform.puntoA,platform.puntoB,phase);
                var point=center+new Vector2(offset,h+radius);Check(Clear(point),"Recorrido movil obstruido "+platform.name+" "+point);
                nodes.Add(new Node(point,platform));
            }
        }
        float g=Mathf.Abs(Physics2D.gravity.y)*player.GetComponent<Rigidbody2D>().gravityScale;int samples=0;
        bool Reach(Node a,Node b)
        {
            if(a.moving!=null && a.moving==b.moving)return true; // Board, wait, and travel on the same platform.
            float dx=b.p.x-a.p.x,dy=b.p.y-a.p.y;
            if(a.moving==null && b.moving==null && Mathf.Abs(dx)<=1.01f && Mathf.Abs(dy)<.01f){for(float k=0;k<=1;k+=.1f)if(!Clear(Vector2.Lerp(a.p,b.p,k)))return false;return true;}
            float disc=player.fuerzaSalto*player.fuerzaSalto-2*g*dy;if(disc<0)return false;
            float time=(player.fuerzaSalto+Mathf.Sqrt(disc))/g;
            foreach(float delay in new[]{0f,.08f,.16f,.24f})
            {
                if(Mathf.Abs(dx)>player.velocidad*(time-delay))continue;bool clear=true;
                for(float t=.01f;t<time-.01f;t+=.01f){samples++;Vector2 p=a.p+new Vector2(Mathf.Sign(dx)*Mathf.Min(Mathf.Abs(dx),player.velocidad*Mathf.Max(0,t-delay)),player.fuerzaSalto*t-.5f*g*t*t);if(!Clear(p)){clear=false;break;}}
                if(clear)return true;
            }
            return false;
        }
        int start=nodes.FindIndex(n=>Mathf.Abs(n.p.x-1.5f)<.01f && n.moving==null);Check(start>=0,"Inicio seguro");
        var seen=new HashSet<int>{start};var queue=new Queue<int>();queue.Enqueue(start);
        while(queue.Count>0){int a=queue.Dequeue();for(int b=0;b<nodes.Count;b++)if(!seen.Contains(b) && Reach(nodes[a],nodes[b])){seen.Add(b);queue.Enqueue(b);}}
        Check(seen.Count==nodes.Count,"Apoyos no alcanzables: "+string.Join(",",nodes.Where((n,i)=>!seen.Contains(i)).Select(n=>n.p.ToString())));
        foreach(var c in coins)Check(nodes.Where((n,i)=>seen.Contains(i)).Any(n=>Mathf.Abs(n.p.x-c.transform.position.x)<.56f && Mathf.Abs(n.p.y-c.transform.position.y)<.42f),"Coco sin apoyo accesible "+c.transform.position);
        var checkpoint=Object.FindAnyObjectByType<RespawnCheckpoint>();Check(Clear(checkpoint.puntoReaparicion.position),"Checkpoint seguro");Check(Clear(player.transform.position),"Inicio despejado");
        foreach(var p in platforms)
        {
            if(Mathf.Abs(p.puntoA.x-p.puntoB.x)<.01f){float bottom=Mathf.Min(p.puntoA.y,p.puntoB.y)-.15f;Check(bottom-2>radius*2+.1f,"Espacio seguro bajo elevador");}
            p.GetComponent<Collider2D>().enabled=true;
        }
        return (city?"Nivel 5":"Nivel 4")+": "+fixedNodes+" apoyos fijos y "+(nodes.Count-fixedNodes)+" estados de plataformas accesibles; "+samples+" muestras balisticas; cocos y checkpoint accesibles. Plataformas comprobadas en extremos y punto medio, con espera para abordarlas; radio menos 0.035 de tolerancia.\n";
    }
}
