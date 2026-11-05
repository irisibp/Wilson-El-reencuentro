using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(WilsonController), typeof(Rigidbody2D))]
public class PlayerRespawn : MonoBehaviour
{
    [Header("Reaparicion")]
    [SerializeField, Min(0f)] private float pausa = 0.35f;
    [SerializeField, Min(0f)] private float proteccion = 1f;
    public TMP_Text estado;
    [Header("Vidas")]
    [SerializeField, Min(1)] private int vidasIniciales = 3;

    public Vector2 PosicionReaparicion { get; private set; }
    public bool TieneCheckpoint { get; private set; }
    public bool Reapareciendo { get; private set; }
    public int Fallos { get; private set; }
    public int VidasRestantes { get; private set; }
    public bool FinDePartida { get; private set; }

    private WilsonController controlador;
    private Rigidbody2D cuerpo;
    private SpriteRenderer[] sprites;
    private Color[] colores;
    private float proteccionHasta;
    private string mensajeInicial;
    private GUIStyle estiloVidas;
    private GUIStyle estiloFin;
    private bool reiniciando;

    private void Awake()
    {
        controlador = GetComponent<WilsonController>();
        cuerpo = GetComponent<Rigidbody2D>();
        PosicionReaparicion = cuerpo.position;
        VidasRestantes = Mathf.Max(1, vidasIniciales);
        sprites = GetComponentsInChildren<SpriteRenderer>();
        colores = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) colores[i] = sprites[i].color;
    }

    private void Start()
    {
        mensajeInicial = estado != null ? estado.text : "";
    }

    public static PlayerRespawn DesdeCollider(Collider2D other)
    {
        PlayerRespawn player = other.attachedRigidbody != null
            ? other.attachedRigidbody.GetComponent<PlayerRespawn>() : null;
        return player != null ? player : other.GetComponentInParent<PlayerRespawn>();
    }

    public bool ActivarCheckpoint(Transform punto)
    {
        if (punto == null || FinDePartida || !controlador.isActiveAndEnabled || Reapareciendo) return false;
        PosicionReaparicion = punto.position;
        TieneCheckpoint = true;
        if (estado != null) estado.text = "Punto de reaparición activado.";
        return true;
    }

    public bool IntentarReaparecer() => ProcesarFallo(false);

    // Falling must also work during the brief protection against corals.
    public bool IntentarReaparecerPorCaida() => ProcesarFallo(true);

    private bool ProcesarFallo(bool porCaida)
    {
        // A completed level disables movement/simulation; never revive a winner.
        if (!isActiveAndEnabled || FinDePartida || Reapareciendo || (!porCaida && Time.time < proteccionHasta) ||
            !controlador.isActiveAndEnabled || !cuerpo.simulated) return false;
        Reapareciendo = true;
        Fallos++;
        VidasRestantes = Mathf.Max(0, VidasRestantes - 1);
        GameAudio.Sonar(GameSound.Fallo);
        controlador.enabled = false;
        controlador.ReiniciarMovimiento();
        cuerpo.simulated = false;
        if (VidasRestantes == 0)
        {
            Reapareciendo = false;
            FinDePartida = true;
            RestaurarColores();
            if (estado != null) estado.text = "Game Over. Te quedaste sin vidas.";
            GameAudio.GameOver();
            return true;
        }
        for (int i = 0; i < sprites.Length; i++) sprites[i].color = new Color(1f, .3f, .3f, colores[i].a);
        if (estado != null) estado.text = "Reapareciendo...";
        StartCoroutine(Reaparecer());
        return true;
    }

    private IEnumerator Reaparecer()
    {
        yield return new WaitForSeconds(pausa);
        cuerpo.position = PosicionReaparicion;
        transform.position = new Vector3(PosicionReaparicion.x, PosicionReaparicion.y, transform.position.z);
        controlador.ReiniciarMovimiento();
        cuerpo.simulated = true;
        Physics2D.SyncTransforms();
        var follow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
        if (follow != null && follow.objetivo == transform) follow.Reencuadrar();
        proteccionHasta = Time.time + proteccion;
        controlador.enabled = true;
        Reapareciendo = false;
        if (estado != null) estado.text = TieneCheckpoint ? "Punto de reaparición activado." : mensajeInicial;

        bool visible = false;
        while (Time.time < proteccionHasta)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                Color c = colores[i];
                if (!visible) c.a *= .3f;
                sprites[i].color = c;
            }
            visible = !visible;
            yield return new WaitForSeconds(.09f);
        }
        RestaurarColores();
    }

    private void RestaurarColores()
    {
        if (sprites == null) return;
        for (int i = 0; i < sprites.Length; i++) if (sprites[i] != null) sprites[i].color = colores[i];
    }

    public void ReintentarNivel()
    {
        if (!FinDePartida || reiniciando) return;
        string scene = SceneManager.GetActiveScene().name;
        if (!Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogError("No se puede reiniciar la escena " + scene + ". Revisar la lista de escenas de la build.", this);
            return;
        }
        reiniciando = true;
        GameAudio.Sonar(GameSound.Continuar);
        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
    }

    private void OnGUI()
    {
        if (estiloVidas == null)
        {
            estiloVidas = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleLeft };
            estiloVidas.normal.textColor = Color.white;
            estiloFin = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            estiloFin.normal.textColor = Color.white;
        }
        GUI.Label(new Rect(20, 64, 200, 34), "Vidas: " + VidasRestantes, estiloVidas);
        if (!FinDePartida) return;
        float width = Mathf.Min(460, Screen.width - 32);
        Rect box = new Rect((Screen.width - width) / 2, (Screen.height - 220) / 2, width, 220);
        int previousDepth = GUI.depth;
        GUI.depth = -110;
        GUI.Box(box, GUIContent.none);
        GUI.Label(new Rect(box.x + 12, box.y + 16, width - 24, 132), "Game Over\nTe quedaste sin vidas.", estiloFin);
        bool previousEnabled = GUI.enabled;
        GUI.enabled = !reiniciando;
        if (GUI.Button(new Rect(box.x + 30, box.y + 164, width - 60, 36), reiniciando ? "Cargando..." : "Reintentar nivel"))
            ReintentarNivel();
        GUI.enabled = previousEnabled;
        GUI.depth = previousDepth;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (Reapareciendo && gameObject.activeInHierarchy && cuerpo != null)
        {
            controlador.ReiniciarMovimiento();
            cuerpo.simulated = true;
            controlador.enabled = true;
        }
        Reapareciendo = false;
        proteccionHasta = 0;
        RestaurarColores();
    }
}
