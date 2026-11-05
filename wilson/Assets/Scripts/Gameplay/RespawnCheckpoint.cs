using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class RespawnCheckpoint : MonoBehaviour
{
    public Transform puntoReaparicion;
    public SpriteRenderer indicador;
    public TMP_Text textoMarcador;
    public bool Activado { get; private set; }

    private void Reset() { GetComponent<BoxCollider2D>().isTrigger = true; }
    private void Awake() { GetComponent<BoxCollider2D>().isTrigger = true; }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Activado) return;
        var player = PlayerRespawn.DesdeCollider(other);
        if (player == null || !player.ActivarCheckpoint(puntoReaparicion)) return;
        Activado = true;
        if (indicador != null) indicador.color = new Color(.4f, 1f, .65f, 1f);
        if (textoMarcador != null)
        {
            textoMarcador.text = "Activado";
            textoMarcador.color = new Color(.4f, 1f, .65f, 1f);
        }
    }
}
