using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class HiddenCoconut : MonoBehaviour
{
    public CoconutCollectible coco;
    public GameObject cobertura;
    public bool Descubierto { get; private set; }

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
        MostrarCoco(false);
    }

    private void MostrarCoco(bool visible)
    {
        if (coco == null) return;
        foreach (var r in coco.GetComponentsInChildren<Renderer>(true)) r.enabled = visible;
        foreach (var c in coco.GetComponentsInChildren<Collider2D>(true)) c.enabled = visible;
    }

    private void OnTriggerEnter2D(Collider2D other) => Descubrir(other);
    private void OnTriggerStay2D(Collider2D other) => Descubrir(other);

    private void Descubrir(Collider2D other)
    {
        if (Descubierto || coco == null) return;
        var player = other.attachedRigidbody != null
            ? other.attachedRigidbody.GetComponent<WilsonController>()
            : other.GetComponentInParent<WilsonController>();
        if (player == null || !player.isActiveAndEnabled || !player.CompareTag("Player")) return;
        Descubierto = true;
        if (cobertura != null) cobertura.SetActive(false);
        MostrarCoco(true);
        GameAudio.Sonar(GameSound.Desbloqueo);
    }
}
