using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class LevelExit : MonoBehaviour
{
    [Header("Resultado")]
    [SerializeField] private string nombreNivel = "Perdido en el mar";
    public UnityEvent alCompletarNivel = new UnityEvent();

    public bool NivelCompletado { get; private set; }

    private string mensajeVictoria;
    private GUIStyle estiloVictoria;

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
        if (wilson == null) return;

        CoconutManager manager = CoconutManager.Instance;
        if (manager == null || !manager.SalidaDesbloqueada) return;

        // Set first so multiple colliders and event callbacks cannot finish twice.
        NivelCompletado = true;
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
            estiloVictoria = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                wordWrap = true
            };
            estiloVictoria.normal.textColor = Color.white;
        }

        float ancho = Mathf.Min(460f, Screen.width - 32f);
        Rect rect = new Rect((Screen.width - ancho) / 2f, (Screen.height - 160f) / 2f, ancho, 160f);
        int profundidadAnterior = GUI.depth;
        GUI.depth = -100;
        GUI.Box(rect, mensajeVictoria, estiloVictoria);
        GUI.depth = profundidadAnterior;
    }
}
