
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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        rb.linearVelocity = new Vector2(
            movimiento * velocidad,
            rb.linearVelocity.y
        );

        if (solicitarSalto)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                fuerzaSalto
            );

            solicitarSalto = false;
        }
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
