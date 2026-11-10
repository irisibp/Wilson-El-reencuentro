using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class LevelExit : MonoBehaviour
{
    [Header("Resultado")]
    [SerializeField] private string nombreNivel = "Perdido en el mar";
    [Tooltip("Nombre de la siguiente escena. Si queda vacio, se usa la siguiente entrada de la build cuando su nombre empieza por Nivel.")]
    [SerializeField] private string siguienteEscena = "Nivel02_CavernaDeCoral";
    [SerializeField] private bool finalDelJuego;
    public bool EsFinalDelJuego => finalDelJuego;
    public UnityEvent alCompletarNivel = new UnityEvent();

    public bool NivelCompletado { get; private set; }
    public string SiguienteEscena
    {
        get
        {
            if (finalDelJuego) return "";
            if (!string.IsNullOrWhiteSpace(siguienteEscena)) return siguienteEscena;
            int current = gameObject.scene.buildIndex;
            int next = current + 1;
            if (current < 0 || next >= SceneManager.sceneCountInBuildSettings) return "";
            string path = SceneUtility.GetScenePathByBuildIndex(next);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            return name.StartsWith("Nivel", System.StringComparison.Ordinal) ? name : "";
        }
    }

    private string mensajeVictoria;
    private GUIStyle estiloVictoria;
    private bool cargandoEscena;

    public void ContinuarAlSiguienteNivel()
    {
        string destino = SiguienteEscena;
        if (!NivelCompletado || cargandoEscena || string.IsNullOrWhiteSpace(destino)) return;
        if (!Application.CanStreamedLevelBeLoaded(destino))
        {
            Debug.LogError("No se puede abrir " + destino + ". Agregar la escena a la lista de escenas de la build.", this);
            return;
        }
        cargandoEscena = true;
        GameAudio.Sonar(GameSound.Continuar);
        SceneManager.LoadSceneAsync(destino, LoadSceneMode.Single);
    }

    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (NivelCompletado) return;

        // Also supports a collider on a child of Wilson's rigidbody.
        WilsonController wilson = other.attachedRigidbody != null
            ? other.attachedRigidbody.GetComponent<WilsonController>()
            : null;
        if (wilson == null) wilson = other.GetComponentInParent<WilsonController>();
        if (wilson == null || !wilson.isActiveAndEnabled) return;

        CoconutManager manager = CoconutManager.Instance;
        if (manager == null || !manager.SalidaDesbloqueada) return;

        // Set first so multiple colliders and event callbacks cannot finish twice.
        NivelCompletado = true;
        GameAudio.Sonar(GameSound.Victoria);
        mensajeVictoria = "¡Nivel completado!\n" + nombreNivel +
            "\nCocos: " + manager.CocosRecolectados + " / " + manager.cocosNecesarios;

        wilson.enabled = false;
        Rigidbody2D cuerpo = wilson.GetComponent<Rigidbody2D>();
        if (cuerpo != null)
        {
            cuerpo.linearVelocity = Vector2.zero;
            cuerpo.angularVelocity = 0f;
            cuerpo.simulated = false;
        }

        Debug.Log(mensajeVictoria, this);
        alCompletarNivel.Invoke();
    }

    private void OnGUI()
    {
        if (!NivelCompletado || finalDelJuego) return;

        if (estiloVictoria == null)
        {
            estiloVictoria = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                wordWrap = true
            };
            estiloVictoria.normal.textColor = Color.white;
        }

        float ancho = Mathf.Min(460f, Screen.width - 32f);
        bool haySiguiente = !string.IsNullOrWhiteSpace(SiguienteEscena);
        float alto = haySiguiente ? 212f : 160f;
        Rect rect = new Rect((Screen.width - ancho) / 2f, (Screen.height - alto) / 2f, ancho, alto);
        int profundidadAnterior = GUI.depth;
        GUI.depth = -100;
        GUI.Box(rect, GUIContent.none);
        GUI.Label(new Rect(rect.x + 12f, rect.y + 8f, ancho - 24f, 144f), mensajeVictoria, estiloVictoria);
        if (haySiguiente)
        {
            bool habilitadoAnterior = GUI.enabled;
            GUI.enabled = !cargandoEscena;
            if (GUI.Button(new Rect(rect.x + 30f, rect.y + 160f, ancho - 60f, 36f), cargandoEscena ? "Cargando..." : "Continuar"))
                ContinuarAlSiguienteNivel();
            GUI.enabled = habilitadoAnterior;
        }
        GUI.depth = profundidadAnterior;
    }
}
