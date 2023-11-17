using UnityEngine;

public static class Constants
{
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

    public static float Map(float originalMin, float originalMax, float targetMin, float targetMax, float originalValue)
    {
        return (originalValue - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
    }

}