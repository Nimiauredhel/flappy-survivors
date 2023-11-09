using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class BuildUtils
{
    private const string BUILD_FOLDER = "F:/Builds/FlappySurvivors/";
    private const string BUILD_NAME_FORMAT = "Flappy Survivors {0} {1}/FlappySurvivors{2}";
    
    [MenuItem("Build/Android")]
    public static void BuildForAndroid()
    {
        // Get filename.
        string path = BUILD_FOLDER + string.Format(BUILD_NAME_FORMAT, Application.version, DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"), ".apk");
        string[] levels = new string[] {"Assets/Scenes/GameplayScene.unity"};

        // Build player.
        BuildPipeline.BuildPlayer(levels, path, BuildTarget.Android, BuildOptions.None);
    }

	[MenuItem("Build/WebGL")]
    public static void BuildForWebGL()
    {
        // Get filename.
        string path = BUILD_FOLDER + string.Format(BUILD_NAME_FORMAT, Application.version, DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"), "/");
        string[] levels = new string[] {"Assets/Scenes/GameplayScene.unity"};

        // Build player.
        BuildPipeline.BuildPlayer(levels, path, BuildTarget.WebGL, BuildOptions.None);
    }
}
