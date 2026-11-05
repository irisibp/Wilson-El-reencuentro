
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform objetivo;
    public float suavizado = 5f;
    public float limiteIzquierdo = 0f;
    public float limiteDerecho = 19f;

    private Vector3 velocidad = Vector3.zero;

    public void Reencuadrar()
    {
        if (objetivo == null) return;
        velocidad = Vector3.zero;
        transform.position = new Vector3(
            Mathf.Clamp(objetivo.position.x, limiteIzquierdo, limiteDerecho),
            transform.position.y,
            -10f
        );
    }

    void LateUpdate()
    {
        if (objetivo == null) return;

        float x = Mathf.Clamp(
            objetivo.position.x,
            limiteIzquierdo,
            limiteDerecho
        );

        Vector3 posicionDeseada = new Vector3(
            x,
            transform.position.y,
            -10f
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            posicionDeseada,
            ref velocidad,
            1f / suavizado
        );
    }
}
