using UnityEngine;

public enum GameSound { Coco, Checkpoint, Fallo, Victoria, Continuar, Desbloqueo }

[CreateAssetMenu(menuName = "Wilson/Audio del juego")]
public class GameAudioSettings : ScriptableObject
{
    [Header("Volumen")]
    [Range(0f, 1f)] public float volumenMusica = .18f;
    [Range(0f, 1f)] public float volumenEfectos = .7f;
    [Header("Musica por nivel")]
    public AudioClip musica;
    public AudioClip musicaNivel01;
    public AudioClip musicaNivel02;
    public AudioClip musicaNivel03;
    public AudioClip musicaNivel04;
    public AudioClip musicaNivel05;
    public AudioClip musicaGameOver;
    [Header("Efectos")]
    public AudioClip coco;
    public AudioClip checkpoint;
    public AudioClip fallo;
    public AudioClip victoria;
    public AudioClip continuar;
    public AudioClip desbloqueo;

    public AudioClip MusicaParaEscena(string scene)
    {
        if (scene == "Nivel01_PerdidoEnElMar" && musicaNivel01 != null) return musicaNivel01;
        if (scene == "Nivel02_CavernaDeCoral" && musicaNivel02 != null) return musicaNivel02;
        if (scene == "Nivel03_RuinasSumergidas" && musicaNivel03 != null) return musicaNivel03;
        if (scene == "Nivel04_LaPlaya" && musicaNivel04 != null) return musicaNivel04;
        if (scene == "Nivel05_CiudadCostera" && musicaNivel05 != null) return musicaNivel05;
        return musica;
    }

    public AudioClip Clip(GameSound sound)
    {
        switch (sound)
        {
            case GameSound.Coco: return coco;
            case GameSound.Checkpoint: return checkpoint;
            case GameSound.Fallo: return fallo;
            case GameSound.Victoria: return victoria;
            case GameSound.Continuar: return continuar;
            case GameSound.Desbloqueo: return desbloqueo;
            default: return null;
        }
    }
}
