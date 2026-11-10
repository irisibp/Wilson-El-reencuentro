using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class LifeHazard : MonoBehaviour
{
    private void Reset() => GetComponent<BoxCollider2D>().isTrigger = true;
    private void Awake() => GetComponent<BoxCollider2D>().isTrigger = true;
    private void OnTriggerEnter2D(Collider2D other) => Tocar(other);
    private void OnTriggerStay2D(Collider2D other) => Tocar(other);
    private void Tocar(Collider2D other)
    {
        var player = PlayerRespawn.DesdeCollider(other);
        if (player != null) player.IntentarReaparecer();
    }
}
