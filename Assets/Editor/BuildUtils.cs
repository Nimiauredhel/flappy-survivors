using UnityEditor;

public static class BuildUtils
{
    private const string BUILD_FOLDER = "Build/";
    private const string ANDROID_FOLDER = "Android/";
    private const string WEBGL_FOLDER = "WEBGL/";
    private const string BUILD_NAME_FORMAT = "TheFlappySurvivor{0}";
    
    [MenuItem("Build/Android")]
    public static void BuildForAndroid()
    {
        // Build APK
        // Get filename.
        string path = BUILD_FOLDER + ANDROID_FOLDER + string.Format(BUILD_NAME_FORMAT, ".apk");

        // Build player.
        PlayerSettings.Android.useCustomKeystore = false;
        EditorUserBuildSettings.buildAppBundle = false;
        BuildPipeline.BuildPlayer(GetScenes(), path, BuildTarget.Android, BuildOptions.None);
    }

	[MenuItem("Build/WebGL")]
    public static void BuildForWebGL()
    {
        // Get filename.
        string path = BUILD_FOLDER + WEBGL_FOLDER + string.Format(BUILD_NAME_FORMAT, "/");

        // Build player.
        BuildPipeline.BuildPlayer(GetScenes(), path, BuildTarget.WebGL, BuildOptions.None);
    }

    private static string[] GetScenes()
    {
        string[] scenes = {
            "Assets/Scenes/Intro.unity",
            "Assets/Scenes/Menu.unity",
            "Assets/Scenes/Gameplay.unity"
        };

        return scenes;
    }
}
