using UnityEditor;

public static class BuildUtils
{
    private const string BUILD_FOLDER = "Build/";
    private const string ANDROID_FOLDER = "Android/";
    private const string WEBGL_FOLDER = "WEBGL/";
    private const string BUILD_NAME_FORMAT = "FlappySurvivors{0}";
    
    [MenuItem("Build/Android")]
    public static void BuildForAndroid()
    {
        // Get filename.
        string path = BUILD_FOLDER + ANDROID_FOLDER + string.Format(BUILD_NAME_FORMAT, ".apk");
        string[] levels = new string[] {"Assets/Scenes/GameplayScene.unity"};

        // Build player.
        BuildPipeline.BuildPlayer(levels, path, BuildTarget.Android, BuildOptions.None);
    }

	[MenuItem("Build/WebGL")]
    public static void BuildForWebGL()
    {
        // Get filename.
        string path = BUILD_FOLDER + WEBGL_FOLDER + string.Format(BUILD_NAME_FORMAT, "/");
        string[] levels = new string[] {"Assets/Scenes/GameplayScene.unity"};

        // Build player.
        BuildPipeline.BuildPlayer(levels, path, BuildTarget.WebGL, BuildOptions.None);
    }
}
