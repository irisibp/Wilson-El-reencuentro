using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuView : MonoBehaviour
{
    public Button jugar,controles,salir,volver;
    public GameObject opciones,ayuda;
    private void Awake()
    {
        jugar.onClick.AddListener(Jugar);controles.onClick.AddListener(MostrarControles);salir.onClick.AddListener(GameMenus.SalirDelJuego);volver.onClick.AddListener(CerrarControles);
        ayuda.SetActive(false);opciones.SetActive(true);
    }
    private void Start(){if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(jugar.gameObject);}
    private void Update(){if(ayuda.activeSelf && Input.GetKeyDown(KeyCode.Escape))CerrarControles();}
    public void Jugar()=>GameMenus.Instance.EmpezarJuego();
    public void MostrarControles(){opciones.SetActive(false);ayuda.SetActive(true);if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(volver.gameObject);}
    public void CerrarControles(){ayuda.SetActive(false);opciones.SetActive(true);if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(jugar.gameObject);}
}
