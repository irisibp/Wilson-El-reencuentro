using UnityEngine;

[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class MovingPlatform2D : MonoBehaviour
{
    public Vector2 puntoA;
    public Vector2 puntoB;
    [Min(.1f)] public float velocidad = 1.4f;
    [Min(0)] public float pausa = .6f;
    public Vector2 VelocidadActual { get; private set; }
    private Rigidbody2D cuerpo;
    private bool haciaB = true;
    private float espera;

    private void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        cuerpo.bodyType = RigidbodyType2D.Kinematic;
        cuerpo.gravityScale = 0;
        cuerpo.constraints = RigidbodyConstraints2D.FreezeRotation;
        cuerpo.interpolation = RigidbodyInterpolation2D.Interpolate;
        cuerpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        cuerpo.position = puntoA;
        espera = pausa;
    }

    private void FixedUpdate()
    {
        if (espera > 0)
        {
            espera = Mathf.Max(0, espera - Time.fixedDeltaTime);
            VelocidadActual = Vector2.zero;
            cuerpo.MovePosition(cuerpo.position);
            return;
        }
        Vector2 destino = haciaB ? puntoB : puntoA;
        Vector2 siguiente = Vector2.MoveTowards(cuerpo.position, destino, velocidad * Time.fixedDeltaTime);
        VelocidadActual = (siguiente - cuerpo.position) / Time.fixedDeltaTime;
        cuerpo.MovePosition(siguiente);
        if ((siguiente - destino).sqrMagnitude < .000001f)
        {
            haciaB = !haciaB;
            espera = pausa;
        }
    }

    private void OnDisable() => VelocidadActual = Vector2.zero;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(puntoA, puntoB);
        Gizmos.DrawWireSphere(puntoA, .15f);
        Gizmos.DrawWireSphere(puntoB, .15f);
    }
}
