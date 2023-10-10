using UnityEngine;

public static class AlphaCorrection
{
    ///<Summary>Returns color with expected alpha in Linear color space.</Summary>
    public static Color GetLinearAlpha(float alpha, Color colorToChangeAlpha)
    {
        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            colorToChangeAlpha.a = GetLinearAlpha(alpha);
        else
            colorToChangeAlpha.a = alpha;
        
        //Debug.Log(CodeManager.GetMethodName() + string.Format("{0} -> {1}", alpha, colorToChangeAlpha.a));

        return colorToChangeAlpha;
    }

    ///<Summary>Returns expected alpha in Linear color space.</Summary>
    public static float GetLinearAlpha(float alpha)
    {
        return 1f - Mathf.GammaToLinearSpace(1f - alpha);
    }
}
