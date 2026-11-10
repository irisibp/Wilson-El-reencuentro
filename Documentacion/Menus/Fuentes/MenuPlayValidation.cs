using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[InitializeOnLoad]
public static class MenuPlayValidation
{
    const string Pending="WilsonMenusPending",QuitExpected="WilsonMenusQuit",Report="WilsonMenusReport";
    static int stage,level=2,hits;
    static float stamp,frozenTime;
    static Vector2 frozenPlayer;
    static Vector2[] frozenPlatforms;
    static int samples;
    static GameAudio originalAudio;
    static WilsonController previousPlayer;
    static MenuPlayValidation(){if(SessionState.GetBool(Pending,false))EditorApplication.update+=Tick;}
    static void Check(bool ok,string text){if(!ok)throw new Exception("MENU etapa "+stage+": "+text);}
    static void Note(string text)=>SessionState.SetString(Report,SessionState.GetString(Report,"")+"PASS: "+text+"\n");
    static void Next(int n){stage=n;stamp=Time.unscaledTime;}
    public static void Run()
    {
        EditorSceneManager.OpenScene(MenuSetup.MenuPath);SessionState.SetBool(Pending,true);SessionState.SetBool(QuitExpected,false);SessionState.SetString(Report,"");SessionState.SetFloat("MenusDeadline",(float)EditorApplication.timeSinceStartup+180);
        EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();
    }
    static Button PauseButton(string name)=>GameMenus.Instance.PanelPausa.GetComponentsInChildren<Button>(true).Single(b=>b.name==name);
    static void Tick()
    {
        try
        {
            if(!EditorApplication.isPlaying)
            {
                if(SessionState.GetBool(QuitExpected,false) && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    Check(Mathf.Abs(Time.timeScale-1)<.001f && !AudioListener.pause,"salir desde pausa restaura tiempo y audio");
                    Note("Salir del juego desde pausa detiene Play sin cerrar el editor; tiempo y audio restaurados.");
                    Directory.CreateDirectory("../cambios_menus");File.WriteAllText("../cambios_menus/VERIFICACION_PLAYMODE.txt","Unity "+Application.unityVersion+"\n"+SessionState.GetString(Report,""));SessionState.SetBool(Pending,false);SessionState.SetBool(QuitExpected,false);Debug.Log("MENUS_PLAYMODE_OK");EditorApplication.Exit(0);
                }
                return;
            }
            if(EditorApplication.timeSinceStartup>SessionState.GetFloat("MenusDeadline",0))throw new TimeoutException("Etapa "+stage);
            var menu=GameMenus.Instance;if(menu==null)return;float elapsed=Time.unscaledTime-stamp;
            var view=UnityEngine.Object.FindAnyObjectByType<MainMenuView>();var player=UnityEngine.Object.FindAnyObjectByType<WilsonController>();var manager=CoconutManager.Instance;
            if(stage==0 && Time.unscaledTime>.4f)
            {
                Check(SceneManager.GetActiveScene().name==GameMenus.MenuPrincipal && view!=null,"inicio primero");
                Check(UnityEngine.Object.FindObjectsByType<GameMenus>().Length==1 && UnityEngine.Object.FindObjectsByType<EventSystem>().Count(e=>e.enabled)==1,"un menu y un EventSystem");
                Check(GameAudio.Instance!=null && GameAudio.Instance.GetComponents<AudioSource>().Any(a=>a.isPlaying),"musica en inicio");originalAudio=GameAudio.Instance;
                view.controles.onClick.Invoke();Check(view.ayuda.activeSelf && !view.opciones.activeSelf,"controles");view.volver.onClick.Invoke();Check(!view.ayuda.activeSelf && view.opciones.activeSelf,"volver de controles");Note("Inicio con musica; controles y volver; un EventSystem.");view.jugar.onClick.Invoke();Next(1);
            }
            else if(stage==1 && player!=null && SceneManager.GetActiveScene().name==GameMenus.PrimerNivel && elapsed>.3f)
            {
                Check(manager.CocosRecolectados==0 && menu.PuedePausar && !AudioListener.pause && Time.timeScale==1,"jugar desde inicio");Check(GameAudio.Instance==originalAudio,"audio persistente");
                menu.Pausar();Check(GameMenus.EstaEnPausa && AudioListener.pause && Time.timeScale==0,"estado pausa");frozenPlayer=player.GetComponent<Rigidbody2D>().position;frozenTime=Time.time;Next(2);
            }
            else if(stage==2 && elapsed>.2f)
            {
                Check(Vector2.Distance(player.GetComponent<Rigidbody2D>().position,frozenPlayer)<.001f && Mathf.Abs(Time.time-frozenTime)<.001f,"tiempo y fisica congelados");
                var jump=typeof(WilsonController).GetField("solicitarSalto",BindingFlags.Instance|BindingFlags.NonPublic);jump.SetValue(player,true);player.SendMessage("Update");Check(!(bool)jump.GetValue(player),"no encolar salto en pausa");
                samples=GameAudio.Instance.GetComponents<AudioSource>().Single(a=>a.loop).timeSamples;Next(3);
            }
            else if(stage==3 && elapsed>.25f)
            {
                Check(Mathf.Abs(GameAudio.Instance.GetComponents<AudioSource>().Single(a=>a.loop).timeSamples-samples)<1024,"musica pausada");SavePause();PauseButton("Continuar").onClick.Invoke();Check(!GameMenus.EstaEnPausa && !AudioListener.pause && Time.timeScale==1,"continuar");Note("Pausa congela fisica, reloj y musica; limpia salto pendiente; Continuar restaura todo.");Next(4);
            }
            else if(stage==4 && elapsed>.15f)
            {
                previousPlayer=player;menu.Pausar();PauseButton("Reiniciar").onClick.Invoke();Check(Time.timeScale==1 && !AudioListener.pause,"reiniciar desde pausa libera tiempo");Next(5);
            }
            else if(stage==5 && player!=null && player!=previousPlayer && elapsed>.3f)
            {
                Check(manager.CocosRecolectados==0 && menu.PuedePausar,"nivel reiniciado");menu.Pausar();PauseButton("Inicio").onClick.Invoke();Next(6);
            }
            else if(stage==6 && view!=null && elapsed>.3f)
            {
                Check(Time.timeScale==1 && !AudioListener.pause && !GameMenus.EstaEnPausa,"inicio sin pausa residual");Check(UnityEngine.Object.FindObjectsByType<GameMenus>().Length==1,"sin duplicar menus");Note("Reiniciar nivel y volver al inicio desde pausa sin bloqueo ni duplicados.");Next(7);SceneManager.LoadSceneAsync("Nivel02_CavernaDeCoral");
            }
            else if(stage==7 && player!=null && SceneManager.GetActiveScene().name.StartsWith("Nivel0"+level) && elapsed>1.1f)
            {
                Check(menu.PuedePausar,"pausa disponible en nivel "+level);menu.Pausar();frozenPlayer=player.GetComponent<Rigidbody2D>().position;frozenPlatforms=UnityEngine.Object.FindObjectsByType<MovingPlatform2D>().Select(p=>p.GetComponent<Rigidbody2D>().position).ToArray();Next(8);
            }
            else if(stage==8 && elapsed>.2f)
            {
                Check(Time.timeScale==0 && Vector2.Distance(frozenPlayer,player.GetComponent<Rigidbody2D>().position)<.001f,"fisica nivel "+level);
                var platforms=UnityEngine.Object.FindObjectsByType<MovingPlatform2D>();Check(platforms.Length==frozenPlatforms.Length,"plataformas persistentes");for(int i=0;i<platforms.Length;i++)Check(Vector2.Distance(platforms[i].GetComponent<Rigidbody2D>().position,frozenPlatforms[i])<.001f,"plataforma congelada");
                menu.Continuar();Note("Pausa y continuacion en nivel "+level+", plataformas congeladas donde corresponde.");
                if(level<5){level++;Next(7);string[] names={"","","","Nivel03_RuinasSumergidas","Nivel04_LaPlaya","Nivel05_CiudadCostera"};SceneManager.LoadSceneAsync(names[level]);}
                else {hits=1;player.GetComponent<PlayerRespawn>().IntentarReaparecerPorCaida();Next(9);}
            }
            else if(stage==9 && elapsed>.45f)
            {
                var r=player.GetComponent<PlayerRespawn>();
                if(hits<3){Check(!r.Reapareciendo,"reaparicion");r.IntentarReaparecerPorCaida();hits++;stamp=Time.unscaledTime;}
                else
                {
                    Check(r.FinDePartida && !menu.PuedePausar,"Game Over sin superponer pausa");menu.Pausar();Check(!GameMenus.EstaEnPausa && Time.timeScale==1,"no congelar Game Over");menu.IrAlInicio();Next(10);
                }
            }
            else if(stage==10 && view!=null && elapsed>.2f){Note("Game Over bloquea la pausa y permite volver al inicio.");Next(11);SceneManager.LoadSceneAsync("Nivel05_CiudadCostera");}
            else if(stage==11 && player!=null && elapsed>.35f)
            {
                manager.RecolectarCoco(manager.cocosNecesarios);var body=player.GetComponent<Rigidbody2D>();body.position=new Vector2(57.1f,2.366f);player.transform.position=body.position;Physics2D.SyncTransforms();manager.salidaNivel.SendMessage("OnTriggerEnter2D",player.GetComponent<Collider2D>());Next(12);
            }
            else if(stage==12 && elapsed>1.6f)
            {
                var ending=UnityEngine.Object.FindAnyObjectByType<ReunionEnding>();Check(ending.FinalVisible && !menu.PuedePausar,"final sin pausa superpuesta");ending.VolverAlInicio();Next(13);
            }
            else if(stage==13 && view!=null && elapsed>.3f)
            {
                Check(Time.timeScale==1 && !AudioListener.pause,"final regresa al titulo");Note("El reencuentro con Chuck vuelve al nuevo inicio; sin pausa sobre la cinematica.");view.jugar.onClick.Invoke();Next(14);
            }
            else if(stage==14 && player!=null && elapsed>.3f)
            {
                menu.Pausar();SessionState.SetBool(QuitExpected,true);PauseButton("Salir").onClick.Invoke();
            }
        }
        catch(Exception e){SessionState.SetBool(Pending,false);SessionState.SetBool(QuitExpected,false);Debug.LogException(e);EditorApplication.Exit(1);}
    }
    static void SavePause()
    {
        var camera=Camera.main;var canvases=UnityEngine.Object.FindObjectsByType<Canvas>();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
        foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}Canvas.ForceUpdateCanvases();
        var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes("../cambios_menus/Pausa.png",tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);
        for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}
    }
}
