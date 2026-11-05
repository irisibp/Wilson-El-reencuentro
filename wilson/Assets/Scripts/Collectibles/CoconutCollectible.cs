
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CoconutCollectible : MonoBehaviour
{
    [Header("Valor del coco")]
    public int valor = 1;

    private bool recolectado = false;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (recolectado) return;

        if (other.CompareTag("Player"))
        {
            if (CoconutManager.Instance == null) return;

            recolectado = true;
            CoconutManager.Instance.RecolectarCoco(valor);
            Destroy(gameObject);
        }
    }
}
