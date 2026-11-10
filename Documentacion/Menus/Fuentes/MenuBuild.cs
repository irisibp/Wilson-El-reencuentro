using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class MenuBuild
{
    public static string Output=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"ChatGPT/wilson/Entregas/WilsonDemo_InicioYPausa");
    [MenuItem("Tools/Wilson/Menus/Exportar Windows con inicio y pausa")]
    public static void Export()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Sali de Play antes de exportar.");
        MenuSetup.Prepare();
        string[] scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
        if(scenes.Length!=6 || scenes[0]!=MenuSetup.MenuPath)throw new Exception("La exportacion requiere inicio y los cinco niveles.");
        Directory.CreateDirectory(Output);
        var options=new BuildPlayerOptions{scenes=scenes,locationPathName=Path.Combine(Output,"Wilson-El-Reencuentro.exe"),target=BuildTarget.StandaloneWindows64,subtarget=(int)StandaloneBuildSubtarget.Player,options=BuildOptions.CompressWithLz4HC};
        BuildReport report=BuildPipeline.BuildPlayer(options);
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("No se pudo exportar: "+report.summary.result+". Revisar Console.");
        File.WriteAllText(Path.Combine(Output,"COMO_JUGAR.txt"),"WILSON: EL REENCUENTRO\n\nAbrir Wilson-El-Reencuentro.exe. No requiere Unity.\n\nJugar: comienza una nueva partida en el nivel 1.\nA / D o flechas: moverse.\nEspacio: saltar.\nEsc: pausa / continuar.\n\nDesde la pausa podes continuar, reiniciar el nivel, volver al inicio o salir del juego. Reiniciar o volver al inicio comienza de nuevo; no hay guardado de progreso entre sesiones.\nSalir del juego cierra completamente el ejecutable.\n\nMantener juntos todos los archivos de esta carpeta. Extraer el ZIP completo antes de jugar.\n");
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(Output),"VERIFICACION_BUILD_MENUS.txt"),"PASS: Windows Intel 64-bit, player, sin Development Build; inicio y cinco niveles; "+report.summary.totalSize+" bytes; "+report.summary.totalErrors+" errores.\nSalida: "+Output+"\n");
        Debug.Log("MENUS_BUILD_OK "+Output);
    }
    public static void RunBatch(){try{Export();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}
