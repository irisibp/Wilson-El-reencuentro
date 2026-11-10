
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WilsonController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 6f;
    public float fuerzaSalto = 12f;

    [Header("Detección del suelo")]
    public Transform detectorSuelo;
    public float radioDeteccion = 0.2f;
    public LayerMask capaSuelo;

    private Rigidbody2D rb;
    private float movimiento;
    private bool enSuelo;
    private bool solicitarSalto;
    private Collider2D colision;
    private readonly RaycastHit2D[] apoyos = new RaycastHit2D[8];
    public MovingPlatform2D PlataformaActual { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        colision = GetComponent<Collider2D>();
    }

    void Update()
    {
        movimiento = Input.GetAxisRaw("Horizontal");

        enSuelo = detectorSuelo != null &&
            Physics2D.OverlapCircle(
                detectorSuelo.position,
                radioDeteccion,
                capaSuelo
            );

        if (Input.GetButtonDown("Jump") && enSuelo)
        {
            solicitarSalto = true;
        }
    }

    void FixedUpdate()
    {
        PlataformaActual = BuscarPlataforma();
        Vector2 soporte = PlataformaActual != null ? PlataformaActual.VelocidadActual : Vector2.zero;
        float vertical = rb.linearVelocity.y;
        if (PlataformaActual != null) vertical = soporte.y;
        rb.linearVelocity = new Vector2(movimiento * velocidad + soporte.x, vertical);
        if (solicitarSalto)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto + Mathf.Max(0, soporte.y));
            solicitarSalto = false;
            PlataformaActual = null;
        }
    }

    private MovingPlatform2D BuscarPlataforma()
    {
        if (colision == null || !rb.simulated) return null;
        var filtro = new ContactFilter2D();
        filtro.SetLayerMask(capaSuelo);
        filtro.useTriggers = false;
        int count = colision.Cast(Vector2.down, filtro, apoyos, .06f);
        for (int i = 0; i < count; i++)
        {
            if (apoyos[i].normal.y < .55f) continue;
            float distanciaSuelo = colision.bounds.min.y - apoyos[i].collider.bounds.max.y;
            if (distanciaSuelo < -.08f || distanciaSuelo > .08f) continue;
            var plataforma = apoyos[i].collider.GetComponent<MovingPlatform2D>();
            if (plataforma != null && rb.linearVelocity.y <= plataforma.VelocidadActual.y + .5f)
                return plataforma;
        }
        return null;
    }

    public void ReiniciarMovimiento()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        PlataformaActual = null;
        movimiento = 0f;
        enSuelo = false;
        solicitarSalto = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void OnDrawGizmosSelected()
    {
        if (detectorSuelo == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            detectorSuelo.position,
            radioDeteccion
        );
    }
}
