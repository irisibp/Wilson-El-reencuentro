using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }
    private GameAudioSettings settings;
    private AudioSource musica;
    private AudioSource efectos;
    private bool enVictoria;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() { Instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded -= EnsureAudio;
        SceneManager.sceneLoaded += EnsureAudio;
    }

    private static void EnsureAudio(Scene scene, LoadSceneMode mode)
    {
        if ((scene.name.StartsWith("Nivel") || scene.name == GameMenus.MenuPrincipal) && Instance == null)
            new GameObject("AudioDelJuego").AddComponent<GameAudio>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        settings = Resources.Load<GameAudioSettings>("WilsonAudioSettings");
        if (settings == null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        musica = gameObject.AddComponent<AudioSource>();
        musica.playOnAwake = false;
        musica.spatialBlend = 0f;
        musica.loop = true;
        musica.volume = settings.volumenMusica;
        musica.priority = 128;
        efectos = gameObject.AddComponent<AudioSource>();
        efectos.playOnAwake = false;
        efectos.spatialBlend = 0f;
        efectos.loop = false;
        efectos.volume = settings.volumenEfectos;
        efectos.priority = 64;
        PrepararListener();
        ReproducirNivel(SceneManager.GetActiveScene());
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        enVictoria = false;
        PrepararListener();
        ReproducirNivel(scene);
    }

    private void ReproducirNivel(Scene scene)
    {
        var clip = settings.MusicaParaEscena(scene.name);
        bool cambio = musica.clip != clip || !musica.loop;
        musica.loop = true;
        if (!cambio && musica.isPlaying) return;
        musica.Stop();
        musica.clip = clip;
        if (clip != null) musica.Play();
    }

    public static void GameOver()
    {
        if (Instance == null) return;
        Instance.enVictoria = false;
        Instance.musica.Stop();
        Instance.musica.loop = false;
        Instance.musica.clip = Instance.settings.musicaGameOver;
        if (Instance.musica.clip != null) Instance.musica.Play();
    }

    private void PrepararListener()
    {
        Camera cam = Camera.main;
        var target = cam != null ? cam.gameObject : gameObject;
        var listener = target.GetComponent<AudioListener>();
        if (listener == null) listener = target.AddComponent<AudioListener>();
        listener.enabled = true;
        foreach (var other in FindObjectsByType<AudioListener>())
            if (other != listener) other.enabled = false;
    }

    private void Update()
    {
        float volumenObjetivo = settings.volumenMusica * (enVictoria ? .35f : 1f);
        musica.volume = Mathf.MoveTowards(musica.volume, volumenObjetivo, Time.unscaledDeltaTime * .8f);
        efectos.volume = settings.volumenEfectos;
    }

    public static void Sonar(GameSound sound)
    {
        if (Instance == null) return;
        var clip = Instance.settings.Clip(sound);
        if (clip == null) return;
        float gain;
        switch (sound)
        {
            case GameSound.Coco: gain = .4f; break;
            case GameSound.Continuar: gain = .4f; break;
            case GameSound.Desbloqueo: gain = .6f; break;
            case GameSound.Fallo: gain = .65f; break;
            case GameSound.Checkpoint: gain = .7f; break;
            default: gain = .85f; break;
        }
        if (sound == GameSound.Victoria) Instance.enVictoria = true;
        Instance.efectos.PlayOneShot(clip, gain);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }
}
