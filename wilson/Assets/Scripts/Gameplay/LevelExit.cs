using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class LevelExit : MonoBehaviour
{
    [Header("Resultado")]
    [SerializeField] private string nombreNivel = "Perdido en el mar";
    [Tooltip("Nombre de la siguiente escena. Dejar vacio en el ultimo nivel.")]
    [SerializeField] private string siguienteEscena = "Nivel02_CavernaDeCoral";
    public UnityEvent alCompletarNivel = new UnityEvent();

    public bool NivelCompletado { get; private set; }
    public string SiguienteEscena => siguienteEscena;

    private string mensajeVictoria;
    private GUIStyle estiloVictoria;
    private bool cargandoEscena;

    public void ContinuarAlSiguienteNivel()
    {
        if (!NivelCompletado || cargandoEscena || string.IsNullOrWhiteSpace(siguienteEscena)) return;
        if (!Application.CanStreamedLevelBeLoaded(siguienteEscena))
        {
            Debug.LogError("No se puede abrir " + siguienteEscena + ". Agregar la escena a la lista de escenas de la build.", this);
            return;
        }
        cargandoEscena = true;
        GameAudio.Sonar(GameSound.Continuar);
        SceneManager.LoadSceneAsync(siguienteEscena, LoadSceneMode.Single);
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
        if (!NivelCompletado) return;

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
        bool haySiguiente = !string.IsNullOrWhiteSpace(siguienteEscena);
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
