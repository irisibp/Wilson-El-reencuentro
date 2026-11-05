using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class GameAudioSetup
{
    public const string Root="Assets/Audio";
    public const string SettingsPath="Assets/Resources/WilsonAudioSettings.asset";
    public static readonly string[] Files={
        "Local/Music/POL-star-way-short.wav", "Local/Music/POL-test-your-skills-short.wav", "ThirdParty/OMEN/OMEN_chip.WAV",
        "Local/SFX/Coin2.ogg", "Local/SFX/confirmation_003.ogg",
        "Local/SFX/error_007.ogg", "Local/SFX/confirmation_004.ogg", "Local/SFX/toggle_004.ogg", "Local/SFX/confirmation_002.ogg"
    };

    [MenuItem("Tools/Wilson/Audio/Preparar musica y efectos")]
    public static void Configure()
    {
        foreach(string name in Files)if(!File.Exists(Root+"/"+name))throw new FileNotFoundException("Falta el audio local: "+Root+"/"+name);
        AssetDatabase.Refresh();
        foreach(string name in Files)
        {
            string path=Root+"/"+name;bool music=!name.Contains("/SFX/");
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);
            importer.forceToMono=false;importer.loadInBackground=music;
            var settings=importer.defaultSampleSettings;
            settings.loadType=music?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat=music?AudioCompressionFormat.Vorbis:AudioCompressionFormat.PCM;
            settings.quality=.8f;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData=!music;
            importer.defaultSampleSettings=settings;importer.SaveAndReimport();
        }
        Directory.CreateDirectory("Assets/Resources");AssetDatabase.Refresh();
        var config=AssetDatabase.LoadAssetAtPath<GameAudioSettings>(SettingsPath);
        if(config==null){config=ScriptableObject.CreateInstance<GameAudioSettings>();AssetDatabase.CreateAsset(config,SettingsPath);}
        config.musicaNivel01=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/"+Files[0]);
        config.musicaNivel02=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/"+Files[1]);
        if(config.musicaNivel03==null)config.musicaNivel03=config.musicaNivel02;
        config.musicaGameOver=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/"+Files[2]);
        config.musica=config.musicaNivel01;
        config.coco=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/"+Files[3]);
        config.checkpoint=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/"+Files[4]);
        config.fallo=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/"+Files[5]);
        config.victoria=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/"+Files[6]);
        config.continuar=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/"+Files[7]);
        config.desbloqueo=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/"+Files[8]);
        EditorUtility.SetDirty(config);AssetDatabase.SaveAssets();
        Debug.Log("Audio de Wilson preparado: musica en bucle y seis efectos.");
    }
    public static void RunBatch()
    {
        try
        {
            Configure();var config=AssetDatabase.LoadAssetAtPath<GameAudioSettings>(SettingsPath);
            foreach(var clip in new[]{config.musicaNivel01,config.musicaNivel02,config.musicaGameOver})
                if(clip==null || clip.length<10)throw new Exception("Pista de musica invalida");
            foreach(GameSound sound in Enum.GetValues(typeof(GameSound)))
            {
                var clip=config.Clip(sound);if(clip==null || clip.length<=0 || clip.length>1)throw new Exception("Efecto invalido: "+sound);
                clip.LoadAudioData();var samples=new float[clip.samples*clip.channels];
                if(!clip.GetData(samples,0) || !Array.Exists(samples,s=>Mathf.Abs(s)>.001f))throw new Exception("No se decodifico el sonido "+sound);
            }
            Directory.CreateDirectory(Path.GetFullPath("../cambios_gameover"));
            File.WriteAllText(Path.GetFullPath("../cambios_gameover/VERIFICACION_IMPORTACION.txt"),"Unity "+Application.unityVersion+"\nNivel 1: "+config.musicaNivel01.name+", "+config.musicaNivel01.length+" s.\nNivel 2: "+config.musicaNivel02.name+", "+config.musicaNivel02.length+" s.\nGame Over: "+config.musicaGameOver.name+", "+config.musicaGameOver.length+" s.\nTres pistas Streaming/Vorbis y seis efectos importados/decodificados.\n");
            Debug.Log("AUDIO_IMPORT_OK");EditorApplication.Exit(0);
        }
        catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
