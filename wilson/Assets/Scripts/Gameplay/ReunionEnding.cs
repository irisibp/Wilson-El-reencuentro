using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class ReunionEnding : MonoBehaviour
{
    public LevelExit salida;
    public WilsonController wilson;
    public Transform puntoEncuentro;
    public Canvas interfazNivel;
    public bool ReencuentroIniciado { get; private set; }
    public bool FinalVisible { get; private set; }
    private bool cargando;
    private GUIStyle titulo, texto, boton;

    public void MostrarFinal()
    {
        if (ReencuentroIniciado || salida == null || !salida.NivelCompletado || wilson == null || puntoEncuentro == null) return;
        ReencuentroIniciado = true;
        var vidas = wilson.GetComponent<PlayerRespawn>();
        if (vidas != null) vidas.enabled = false;
        if (interfazNivel != null) interfazNivel.gameObject.SetActive(false);
        StartCoroutine(Reencuentro());
    }

    private IEnumerator Reencuentro()
    {
        Vector3 inicio = wilson.transform.position;
        Vector3 destino = puntoEncuentro.position;
        destino.z = inicio.z;
        float elapsed = 0f;
        while (elapsed < 1.2f)
        {
            elapsed += Time.deltaTime;
            wilson.transform.position = Vector3.Lerp(inicio, destino, Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / 1.2f)));
            yield return null;
        }
        wilson.transform.position = destino;
        FinalVisible = true;
    }

    public void VolverAlInicio()
    {
        const string first = GameMenus.MenuPrincipal;
        if (!FinalVisible || cargando || !Application.CanStreamedLevelBeLoaded(first)) return;
        cargando = true;
        GameMenus.Instance.IrAlInicio();
    }

    private void OnGUI()
    {
        if (!FinalVisible) return;
        if (titulo == null)
        {
            titulo = new GUIStyle(GUI.skin.label) { fontSize = 28, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            titulo.normal.textColor = new Color(1f, .88f, .55f);
            texto = new GUIStyle(titulo) { fontSize = 20 };
            texto.normal.textColor = Color.white;
            boton = new GUIStyle(GUI.skin.button) { fontSize = 19 };
        }
        float width = Mathf.Min(660, Screen.width - 24);
        Rect panel = new Rect((Screen.width - width) / 2f, Mathf.Max(12, Screen.height * .06f), width, 264);
        int depth = GUI.depth;
        Color color = GUI.color;
        GUI.depth = -120;
        GUI.color = new Color(.025f, .10f, .16f, .96f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = color;
        GUI.Label(new Rect(panel.x + 14, panel.y + 12, width - 28, 70), "¡Wilson y Chuck, juntos otra vez!", titulo);
        GUI.Label(new Rect(panel.x + 14, panel.y + 82, width - 28, 90), "Fin de Wilson: El reencuentro\nGracias por jugar.", texto);
        bool enabled = GUI.enabled;
        GUI.enabled = !cargando;
        float anchoBoton = (width - 96) / 2;
        if (GUI.Button(new Rect(panel.x + 40, panel.y + 194, anchoBoton, 42), cargando ? "Cargando..." : "Volver al inicio", boton)) VolverAlInicio();
        if (GUI.Button(new Rect(panel.x + 56 + anchoBoton, panel.y + 194, anchoBoton, 42), "Salir del juego", boton)) GameMenus.SalirDelJuego();
        GUI.enabled = enabled;
        GUI.color = color;
        GUI.depth = depth;
    }
}
