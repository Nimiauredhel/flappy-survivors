using UnityEngine;

public static class Constants
{
    public const float STAGE_WIDTH = 40.0f;
    public const float FLOAT_TOLERANCE = 0.01f;
    public static readonly Vector2 STAGE_OFFSET = new Vector2(20.0f, 5.0f);
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

    // "Tropical Sunset Paradise" color palette (just add black)
    public static readonly Color AMBER = new Color32(241, 180, 22, 255);
    public static readonly Color BURNT_SIENNA = new Color32(229, 106, 25, 255);
    public static readonly Color CRIMSON = new Color32(144, 0, 0, 255);

    public static readonly string[] TIMER_FORMATS = new string[3]
    {
        @"mm\:ss",
        @"m\:ss",
        "%s"
    };
    
    public static float MapFloat(float originalMin, float originalMax, float targetMin, float targetMax, float originalValue)
    {
        return (originalValue - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
    }
}