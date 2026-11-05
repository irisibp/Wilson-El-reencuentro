using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class FallZone : MonoBehaviour
{
    private void Reset() => GetComponent<BoxCollider2D>().isTrigger = true;
    private void Awake() => GetComponent<BoxCollider2D>().isTrigger = true;
    private void OnTriggerEnter2D(Collider2D other) => Caer(other);
    private void OnTriggerStay2D(Collider2D other) => Caer(other);

    private void Caer(Collider2D other)
    {
        var player = PlayerRespawn.DesdeCollider(other);
        if (player != null) player.IntentarReaparecerPorCaida();
    }
}
