
using UnityEngine;
using TMPro;

public class CoconutManager : MonoBehaviour
{
    public static CoconutManager Instance { get; private set; }

    [Header("Configuración")]
    public int cocosNecesarios = 15;
    public TMP_Text textoContador;
    public GameObject salidaNivel;

    private int cocosRecolectados = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        ActualizarContador();

        if (salidaNivel != null)
            salidaNivel.SetActive(false);
    }

    public void RecolectarCoco(int cantidad)
    {
        cocosRecolectados += cantidad;
        ActualizarContador();

        if (cocosRecolectados >= cocosNecesarios)
        {
            if (salidaNivel != null)
                salidaNivel.SetActive(true);

            Debug.Log("¡Nivel desbloqueado!");
        }
    }

    void ActualizarContador()
    {
        if (textoContador != null)
        {
            textoContador.text =
                "Cocos: " + cocosRecolectados +
                " / " + cocosNecesarios;
        }
    }
}
