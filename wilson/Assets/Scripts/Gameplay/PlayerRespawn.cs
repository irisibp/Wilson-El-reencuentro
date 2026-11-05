using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(WilsonController), typeof(Rigidbody2D))]
public class PlayerRespawn : MonoBehaviour
{
    [Header("Reaparicion")]
    [SerializeField, Min(0f)] private float pausa = 0.35f;
    [SerializeField, Min(0f)] private float proteccion = 1f;
    public TMP_Text estado;

    public Vector2 PosicionReaparicion { get; private set; }
    public bool TieneCheckpoint { get; private set; }
    public bool Reapareciendo { get; private set; }
    public int Fallos { get; private set; }

    private WilsonController controlador;
    private Rigidbody2D cuerpo;
    private SpriteRenderer[] sprites;
    private Color[] colores;
    private float proteccionHasta;
    private string mensajeInicial;

    private void Awake()
    {
        controlador = GetComponent<WilsonController>();
        cuerpo = GetComponent<Rigidbody2D>();
        PosicionReaparicion = cuerpo.position;
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
        if (punto == null || !controlador.isActiveAndEnabled || Reapareciendo) return false;
        PosicionReaparicion = punto.position;
        TieneCheckpoint = true;
        if (estado != null) estado.text = "Punto de reaparición activado.";
        return true;
    }

    public bool IntentarReaparecer()
    {
        // A completed level disables movement/simulation; never revive a winner.
        if (!isActiveAndEnabled || Reapareciendo || Time.time < proteccionHasta ||
            !controlador.isActiveAndEnabled || !cuerpo.simulated) return false;
        Reapareciendo = true;
        Fallos++;
        controlador.enabled = false;
        controlador.ReiniciarMovimiento();
        cuerpo.simulated = false;
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
