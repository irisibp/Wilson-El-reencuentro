using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace WilsonSubmarino
{
    // Editor only. The delivered tiles, palettes and scene do not need this builder at runtime.
    public static class SubmarinoBuild
    {
        const string Root = "Assets/WilsonSubmarino32";
        [Serializable] public class Entry { public string name, group, collider; public int x,y,width,height,column,row_from_top,mask; }
        [Serializable] public class Manifest { public int tileSize,pixelsPerUnit,width,height; public Entry[] tiles; }
        [Serializable] public class Cell { public string name; public int x,y; }
        [Serializable] public class Layout { public int width,height; public Cell[] cells,decoration; }
        static Manifest manifest;
        static Dictionary<string,Sprite> sprites;
        static Dictionary<string,Tile> tiles = new Dictionary<string,Tile>();
        static Dictionary<string,RuleTile> rules = new Dictionary<string,RuleTile>();
        static Material material;
        static readonly Vector3Int[] Positions = {
            new Vector3Int(0,1,0),new Vector3Int(1,0,0),new Vector3Int(0,-1,0),new Vector3Int(-1,0,0),
            new Vector3Int(1,1,0),new Vector3Int(1,-1,0),new Vector3Int(-1,-1,0),new Vector3Int(-1,1,0) };

        static int Normalize(int m)
        {
            int[] diag={16,32,64,128}, a={1,4,4,1}, b={2,2,8,8};
            for(int i=0;i<4;i++) if((m&a[i])==0 || (m&b[i])==0) m &= ~diag[i];
            return m;
        }
        static T Save<T>(T asset,string path) where T:Object
        {
            var old=AssetDatabase.LoadAssetAtPath<T>(path);
            if(old!=null) { EditorUtility.CopySerialized(asset,old);Object.DestroyImmediate(asset);EditorUtility.SetDirty(old);return old; }
            AssetDatabase.CreateAsset(asset,path);return asset;
        }
        static void ConfigureTexture(string path,bool multiple)
        {
            var ti=(TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType=TextureImporterType.Sprite;
            ti.spriteImportMode=multiple?SpriteImportMode.Multiple:SpriteImportMode.Single;
            ti.spritePixelsPerUnit=32;ti.filterMode=FilterMode.Point;ti.textureCompression=TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled=false;ti.wrapMode=TextureWrapMode.Clamp;ti.npotScale=TextureImporterNPOTScale.None;
            ti.alphaIsTransparency=true;ti.maxTextureSize=2048;
            var settings=new TextureImporterSettings();ti.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);ti.SaveAndReimport();
        }
        static void Slice()
        {
            string path=Root+"/Art/Submarino_32.png";ConfigureTexture(path,true);
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(AssetImporter.GetAtPath(path));provider.InitSpriteEditorDataProvider();
            var existing=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
            var rects=manifest.tiles.Select(e=>new SpriteRect {name=e.name,
                spriteID=existing.ContainsKey(e.name)?existing[e.name]:GUID.Generate(),
                rect=new Rect(e.x,e.y,32,32),alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f)}).ToArray();
            provider.SetSpriteRects(rects);
            var ids=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            ids.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            var physics=provider.GetDataProvider<ISpritePhysicsOutlineDataProvider>();
            foreach(var r in rects)
            {
                bool thin=r.name.StartsWith("plataforma_");float bottom=thin?4:-16;
                physics.SetOutlines(r.spriteID,new List<Vector2[]> {new [] {
                    new Vector2(-16,bottom),new Vector2(-16,16),new Vector2(16,16),new Vector2(16,bottom)}});
            }
            provider.Apply();((AssetImporter)provider.targetObject).SaveAndReimport();
            sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s=>s.name);
            ConfigureTexture(Root+"/Art/Fondo_demo.png",false);
        }
        static void CreateTiles()
        {
            tiles.Clear();rules.Clear();
            foreach(var e in manifest.tiles)
            {
                var tile=ScriptableObject.CreateInstance<Tile>();tile.name=e.name;tile.sprite=sprites[e.name];tile.color=Color.white;
                tile.colliderType=e.collider=="Grid"?Tile.ColliderType.Grid:e.collider=="Sprite"?Tile.ColliderType.Sprite:Tile.ColliderType.None;
                tiles[e.name]=Save(tile,Root+"/Tiles/"+e.name+".asset");
            }
            foreach(string mat in new[]{"arena","roca"})
            {
                var rt=ScriptableObject.CreateInstance<RuleTile>();rt.name="Auto_"+mat;
                rt.m_DefaultSprite=sprites[mat+"_000"];rt.m_DefaultColliderType=Tile.ColliderType.Grid;
                foreach(var e in manifest.tiles.Where(e=>e.mask>=0 && e.name.StartsWith(mat+"_")))
                {
                    var rule=new RuleTile.TilingRule();rule.m_Sprites=new[]{sprites[e.name]};rule.m_ColliderType=Tile.ColliderType.Grid;
                    rule.m_Neighbors.Clear();rule.m_NeighborPositions.Clear();
                    for(int i=0;i<8;i++)
                    {
                        // Unused diagonal conditions are DontCare, as required by the 47-mask topology.
                        int[] aa={1,4,4,1}, bb={2,2,8,8};
                        if(i>=4 && ((e.mask&aa[i-4])==0 || (e.mask&bb[i-4])==0))continue;
                        rule.m_NeighborPositions.Add(Positions[i]);
                        rule.m_Neighbors.Add((e.mask&(1<<i))!=0?RuleTile.TilingRuleOutput.Neighbor.This:RuleTile.TilingRuleOutput.Neighbor.NotThis);
                    }
                    rule.m_RuleTransform=RuleTile.TilingRuleOutput.Transform.Fixed;rt.m_TilingRules.Add(rule);
                }
                rt.UpdateNeighborPositions();rules[mat]=Save(rt,Root+"/RuleTiles/Auto_"+mat+".asset");
            }
        }
        static void Palette(string name,bool automatic)
        {
            string path=Root+"/Palettes/"+name+".prefab";
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(prefab==null)prefab=GridPaletteUtility.CreateNewPalette(Root+"/Palettes",name,GridLayout.CellLayout.Rectangle,
                GridPalette.CellSizing.Manual,Vector3.one,GridLayout.CellSwizzle.XYZ);
            var go=PrefabUtility.LoadPrefabContents(path);var map=go.GetComponentInChildren<Tilemap>();map.ClearAllTiles();
            map.GetComponent<TilemapRenderer>().sharedMaterial=material;
            if(automatic) {map.SetTile(Vector3Int.zero,rules["arena"]);map.SetTile(Vector3Int.right*2,rules["roca"]);}
            else foreach(var e in manifest.tiles)map.SetTile(new Vector3Int(e.column,-e.row_from_top,0),tiles[e.name]);
            PrefabUtility.SaveAsPrefabAsset(go,path);PrefabUtility.UnloadPrefabContents(go);
        }
        static Tilemap NewMap(string name,Transform parent,int order,bool collide=false)
        {
            var go=new GameObject(name,typeof(Tilemap),typeof(TilemapRenderer));go.transform.SetParent(parent,false);
            var r=go.GetComponent<TilemapRenderer>();r.sortingOrder=order;r.sharedMaterial=material;
            if(collide)go.AddComponent<TilemapCollider2D>();return go.GetComponent<Tilemap>();
        }
        static void Demo()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var layout=JsonUtility.FromJson<Layout>(File.ReadAllText(Root+"/Art/demo_layout.json"));
            var grid=new GameObject("Grid_Submarino",typeof(Grid));grid.GetComponent<Grid>().cellSize=Vector3.one;
            var ground=NewMap("Terreno_Arena",grid.transform,0,true);
            var platforms=NewMap("Plataformas",grid.transform,1,true);
            var decor=NewMap("Decoracion_SinColision",grid.transform,3);
            foreach(var c in layout.cells)
            {
                var map=c.name.StartsWith("plataforma_")?platforms:ground;
                map.SetTile(new Vector3Int(c.x,-c.y-1,0),tiles[c.name]);
            }
            foreach(var c in layout.decoration)decor.SetTile(new Vector3Int(c.x,-c.y-1,0),tiles[c.name]);
            // Rigidbodies are static; demo contains no character or gameplay code.
            foreach(var map in new[]{ground,platforms})map.gameObject.AddComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;
            var bg=new GameObject("Fondo_Demo",typeof(SpriteRenderer));var sr=bg.GetComponent<SpriteRenderer>();
            sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Fondo_demo.png");sr.sortingOrder=-20;sr.sharedMaterial=material;
            bg.transform.position=new Vector3(layout.width/2f,-layout.height/2f,0);
            var cam=new GameObject("Main Camera",typeof(Camera)).GetComponent<Camera>();cam.tag="MainCamera";
            cam.orthographic=true;cam.orthographicSize=layout.height/2f+.2f;cam.transform.position=new Vector3(layout.width/2f,-layout.height/2f,-10);
            cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.04f,.28f,.4f);
            EditorSceneManager.SaveScene(scene,Root+"/Demo/Submarino_Demo.unity");
        }
        static void Validate()
        {
            void Check(bool test,string msg) {if(!test)throw new Exception("VALIDACION: "+msg);}
            Check(sprites.Count==114,"114 sprites");Check(tiles.Count==114,"114 Tile assets");
            foreach(var s in sprites.Values) {Check(s.rect.size==new Vector2(32,32),s.name+" rect");Check(s.pixelsPerUnit==32,s.name+" PPU");}
            foreach(string mat in new[]{"arena","roca"})
            {
                Check(rules[mat].m_TilingRules.Count==47,mat+" 47 reglas");
                var g=new GameObject("RuleTest",typeof(Grid));var map=NewMap("Test",g.transform,0);
                for(int mask=0;mask<256;mask++)
                {
                    map.ClearAllTiles();map.SetTile(Vector3Int.zero,rules[mat]);
                    for(int i=0;i<8;i++)if((mask&(1<<i))!=0)map.SetTile(Positions[i],rules[mat]);
                    map.RefreshAllTiles();Check(map.GetSprite(Vector3Int.zero)==sprites[mat+"_"+Normalize(mask).ToString("D3")],mat+" mask "+mask);
                    Check(map.GetColliderType(Vector3Int.zero)==Tile.ColliderType.Grid,"RuleTile collider");
                }
                Object.DestroyImmediate(g);
            }
            foreach(var e in manifest.tiles)
            {
                Check(tiles[e.name].sprite==sprites[e.name],e.name+" sprite reference");
                if(e.group=="Decoracion")Check(tiles[e.name].colliderType==Tile.ColliderType.None,e.name+" no collider");
                if(e.group=="Plataformas")
                {
                    var shape=new List<Vector2>();sprites[e.name].GetPhysicsShape(0,shape);
                    Check(Mathf.Abs(shape.Min(p=>p.y)-.125f)<.001f,e.name+" collider bottom");
                    Check(Mathf.Abs(shape.Max(p=>p.y)-.5f)<.001f,e.name+" collider top");
                }
            }
            foreach(var name in new[]{"Submarino_Manual","Submarino_Automatico"})
            {
                var p=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Palettes/"+name+".prefab");
                Check(p!=null,"palette "+name);Check(p.GetComponentInChildren<Tilemap>().GetUsedTilesCount()==(name.EndsWith("Manual")?114:2),"palette tiles "+name);
            }
            string report="Unity "+Application.unityVersion+"\n114 sprites: 32x32, PPU32.\n114 Tile assets y referencias OK.\n2 RuleTiles x 47 reglas; 512 patrones de vecinos comprobados en Tilemap real.\n2 paletas comprobadas: 114 piezas manuales y 2 RuleTiles.\nDecoracion sin colision; plataformas con forma fisica de 32x12 px.\nEscena demo guardada; no se ejecutaron pruebas de movimiento de Wilson.\n";
            File.WriteAllText(Path.GetFullPath("../Wilson_Submarino_32_v1/Documentacion/VERIFICACION_UNITY.txt"),report);
            Debug.Log("SUBMARINO_VALIDATION_OK\n"+report);
        }
        [MenuItem("Tools/Wilson/Submarino/Recrear paquete y escena demo")]
        public static void Build()
        {
            // Protect an open project scene. Batch build always uses a separate disposable project.
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            foreach(var folder in new[]{"Tiles","RuleTiles","Palettes","Demo","Materials"})Directory.CreateDirectory(Root+"/"+folder);
            AssetDatabase.Refresh();manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"/Art/tiles_manifest.json"));
            Slice();
            var shader=AssetDatabase.LoadAssetAtPath<Shader>(Root+"/Materials/SubmarinoUnlit.shader");
            material=Save(new Material(shader),Root+"/Materials/SubmarinoUnlit.mat");
            CreateTiles();Palette("Submarino_Manual",false);Palette("Submarino_Automatico",true);Demo();AssetDatabase.SaveAssets();
        }
        public static void RunBatch()
        {
            try {
                Build();Validate();
                int frames=0;
                EditorApplication.CallbackFunction finish=null;
                finish=()=> { if(++frames<12)return;EditorApplication.update-=finish;FinishBatch(); };
                EditorApplication.update+=finish;
                EditorApplication.QueuePlayerLoopUpdate();
            } catch(Exception e) {Debug.LogException(e);EditorApplication.Exit(1);}
        }
        static void FinishBatch()
        {
            try {
                if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null)
                {
                    var cam=Camera.main;cam.orthographicSize=7;
                    var rt=new RenderTexture(1536,896,24,RenderTextureFormat.ARGB32);rt.antiAliasing=1;
                    cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
                    var capture=new Texture2D(1536,896,TextureFormat.RGB24,false);capture.ReadPixels(new Rect(0,0,1536,896),0,0);capture.Apply();
                    File.WriteAllBytes(Path.GetFullPath("../Wilson_Submarino_32_v1/Documentacion/Preview_UNITY_NO_importar.png"),capture.EncodeToPNG());
                    cam.targetTexture=null;RenderTexture.active=null;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(capture);
                    if(ShaderUtil.ShaderHasError(material.shader))throw new Exception("Shader submarine compilation failed");
                    File.AppendAllText(Path.GetFullPath("../Wilson_Submarino_32_v1/Documentacion/VERIFICACION_UNITY.txt"),"Render de camara 1536x896 guardado; shader compilado sin errores.\n");
                }
                string dest=Path.GetFullPath("../Wilson_Submarino_32_v1/Wilson_Submarino_32.unitypackage");
                UnityEditor.AssetPackage.Package.Export(
                    new UnityEditor.AssetPackage.ExportPackageParameters(
                        Root, dest, string.Empty, ExportPackageOptions.Recurse));
                EditorApplication.Exit(0);
            } catch(Exception e) {Debug.LogException(e);EditorApplication.Exit(1);}
        }
    }
}
