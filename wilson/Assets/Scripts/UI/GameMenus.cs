using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

[DefaultExecutionOrder(-200)]
public class GameMenus : MonoBehaviour
{
    public const string MenuPrincipal="MenuPrincipal";
    public const string PrimerNivel="Nivel01_PerdidoEnElMar";
    public static GameMenus Instance { get; private set; }
    public static bool EstaEnPausa => Instance!=null && Instance.pausado;
    public static bool BloquearEntrada => EstaEnPausa || Instance!=null && Time.frameCount<=Instance.cambioDeMenu;
    public bool Cargando => cargando;
    public GameObject PanelPausa => pausa;
    private GameObject pausa,accionesFinal;
    private Button continuar,accesoPausa;
    private EventSystem eventos;
    private WilsonController jugador;
    private PlayerRespawn vidas;
    private LevelExit salida;
    private ReunionEnding final;
    private bool pausado,cargando;
    private int cambioDeMenu=-1;
    private float escalaAnterior=1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic(){Instance=null;Time.timeScale=1;AudioListener.pause=false;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install(){if(Instance==null)new GameObject("MenusDelJuego").AddComponent<GameMenus>();}
    private void Awake()
    {
        if(Instance!=null && Instance!=this){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);
        eventos=gameObject.AddComponent<EventSystem>();gameObject.AddComponent<StandaloneInputModule>();
        BuildPause();SceneManager.sceneLoaded+=SceneLoaded;
    }
    private void SceneLoaded(Scene scene,LoadSceneMode mode)
    {
        escalaAnterior=1;RestaurarTiempo();cargando=false;cambioDeMenu=Time.frameCount;
        jugador=FindAnyObjectByType<WilsonController>();vidas=jugador!=null?jugador.GetComponent<PlayerRespawn>():null;
        var manager=CoconutManager.Instance;salida=manager!=null && manager.salidaNivel!=null?manager.salidaNivel.GetComponent<LevelExit>():null;
        final=FindAnyObjectByType<ReunionEnding>();
        foreach(var other in FindObjectsByType<EventSystem>())if(other!=eventos){foreach(var input in other.GetComponents<BaseInputModule>())input.enabled=false;other.enabled=false;}
        if(eventos!=null)eventos.SetSelectedGameObject(null);
    }
    public bool PuedePausar => !cargando && SceneManager.GetActiveScene().name.StartsWith("Nivel") && jugador!=null && jugador.isActiveAndEnabled && jugador.GetComponent<Rigidbody2D>().simulated && (vidas==null || !vidas.Reapareciendo && !vidas.FinDePartida) && (salida==null || !salida.NivelCompletado);
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){if(pausado)Continuar();else Pausar();}
        if(pausado && !PuedePausar)RestaurarTiempo();
        accesoPausa.gameObject.SetActive(!pausado && PuedePausar);
        bool terminado=salida!=null && salida.NivelCompletado || vidas!=null && vidas.FinDePartida;
        accionesFinal.SetActive(!cargando && terminado && (final==null || !final.ReencuentroIniciado));
    }
    public void Pausar()
    {
        if(pausado || !PuedePausar)return;
        escalaAnterior=Time.timeScale>0?Time.timeScale:1;pausado=true;cambioDeMenu=Time.frameCount;Time.timeScale=0;AudioListener.pause=true;pausa.SetActive(true);
        eventos.SetSelectedGameObject(continuar.gameObject);
    }
    public void Continuar(){if(!pausado)return;RestaurarTiempo();cambioDeMenu=Time.frameCount;GameAudio.Sonar(GameSound.Continuar);eventos.SetSelectedGameObject(null);}
    private void RestaurarTiempo(){pausado=false;Time.timeScale=escalaAnterior>0?escalaAnterior:1;AudioListener.pause=false;if(pausa!=null)pausa.SetActive(false);}
    public void EmpezarJuego()=>Cargar(PrimerNivel);
    public void ReiniciarNivel(){if(SceneManager.GetActiveScene().name.StartsWith("Nivel"))Cargar(SceneManager.GetActiveScene().name);}
    public void IrAlInicio()=>Cargar(MenuPrincipal);
    private void Cargar(string scene)
    {
        if(cargando)return;
        if(!Application.CanStreamedLevelBeLoaded(scene)){Debug.LogError("La escena no esta en la build: "+scene);return;}
        RestaurarTiempo();cargando=true;cambioDeMenu=Time.frameCount;accionesFinal.SetActive(false);accesoPausa.gameObject.SetActive(false);GameAudio.Sonar(GameSound.Continuar);SceneManager.LoadSceneAsync(scene,LoadSceneMode.Single);
    }
    public static void SalirDelJuego()
    {
        if(Instance!=null){Instance.RestaurarTiempo();Instance.cargando=true;}
        else {Time.timeScale=1;AudioListener.pause=false;}
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
        #else
        Application.Quit();
        #endif
    }
    private void BuildPause()
    {
        var canvas=MenuUI.Canvas("InterfazMenus",transform,2000);
        accesoPausa=MenuUI.Button("AbrirPausa",canvas.transform,"Pausa (Esc)",Vector2.zero,new Vector2(176,36),Pausar);
        var ar=accesoPausa.GetComponent<RectTransform>();ar.anchorMin=ar.anchorMax=new Vector2(.5f,1);ar.pivot=new Vector2(.5f,1);ar.anchoredPosition=new Vector2(0,-20);
        var shade=MenuUI.Stretch("Pausa",canvas.transform);MenuUI.Fill(shade,new Color(.01f,.04f,.08f,.80f));pausa=shade.gameObject;
        var card=MenuUI.Rect("Tarjeta",shade,new Vector2(.5f,.5f),Vector2.zero,new Vector2(480,550));MenuUI.Fill(card,MenuUI.Navy);
        MenuUI.Text("Titulo",card,"PAUSA",new Vector2(38,-30),new Vector2(404,62),42,MenuUI.Gold);
        MenuUI.Text("Ayuda",card,"Tomate un respiro. Chuck te espera.",new Vector2(38,-96),new Vector2(404,54),19,MenuUI.Cream);
        continuar=MenuUI.Button("Continuar",card,"Continuar",new Vector2(38,-166),new Vector2(404,54),Continuar,true);
        MenuUI.Button("Reiniciar",card,"Reiniciar nivel",new Vector2(38,-236),new Vector2(404,50),ReiniciarNivel);
        MenuUI.Button("Inicio",card,"Volver al inicio",new Vector2(38,-302),new Vector2(404,50),IrAlInicio);
        MenuUI.Button("Salir",card,"Salir del juego",new Vector2(38,-368),new Vector2(404,50),SalirDelJuego);
        MenuUI.Text("Nota",card,"Esc: continuar\nReiniciar o volver al inicio descarta el progreso de la partida.",new Vector2(38,-444),new Vector2(404,70),17,new Color(.68f,.81f,.82f));
        pausa.SetActive(false);
        var row=MenuUI.Rect("AccionesFinal",canvas.transform,new Vector2(.5f,0),new Vector2(0,48),new Vector2(444,46));accionesFinal=row.gameObject;
        MenuUI.Button("Inicio",row,"Volver al inicio",Vector2.zero,new Vector2(214,46),IrAlInicio);
        MenuUI.Button("Salir",row,"Salir del juego",new Vector2(230,0),new Vector2(214,46),SalirDelJuego);accionesFinal.SetActive(false);
    }
    private void OnDestroy(){if(Instance!=this)return;SceneManager.sceneLoaded-=SceneLoaded;RestaurarTiempo();Instance=null;}
}
