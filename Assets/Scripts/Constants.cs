using UnityEngine;

public static class Constants
{
    public const float STAGE_WIDTH = 40.0f;
    public static readonly Vector2 STAGE_OFFSET = new Vector2(20.0f, 5.0f);
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

    public static readonly string[] TIMER_FORMATS = new string[3]
    {
        @"mm\:ss",
        @"m\:ss",
        @"%s"
    };
    
    public static float MapFloat(float originalMin, float originalMax, float targetMin, float targetMax, float originalValue)
    {
        return (originalValue - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
    }

}