using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <author>Pinelia Luna</author>
/// <Summary>BackgroundFill v1.0
/// <para>백그라운드 이미지 사이즈 관리용 클래스.</para>
/// <para>UGUI Image로 된 백그라운드 텍스쳐의 사이즈를 디바이스 비율에 맞게 조정합니다.</para>
/// <para>디바이스 비율이 respectedRatio 이상일 경우 원본 사이즈를 유지합니다. (스마트폰)</para>
/// <para>디바이스 비율이 respectedRatio 미만일 경우 이미지 사이즈를 늘립니다. (타블렛)</para>
/// <para>원본 리소스의 사이즈는 20:9 권장. (1600x720 / 2400x1080)</para>
/// </Summary>
public class BackgroundFill : MonoBehaviour
{    
    public int resourceWidth = 720;
    public int resourceHeight = 1600;
    public float respectedWidth = 720f;
    public float respectedHeight = 1280f;
    private float respectedRatio;
    private float deviceRatio;
    private float resolutionScale;
    private RectTransform backgroundImage;

    private void Awake()
    {
        backgroundImage = GetComponent<RectTransform>();
        if (backgroundImage != null)
            SetResolution();
    }

    private void SetResolution()
    {
        respectedRatio = respectedWidth / respectedHeight;
        deviceRatio = (float)Screen.width / (float)Screen.height;

        if (deviceRatio > respectedRatio) // Tablet
        {
            resolutionScale  = (float)Screen.height / respectedHeight;
            backgroundImage.sizeDelta = new Vector2((float)Screen.width/resolutionScale, (float)Screen.height/resolutionScale);
        }
        else // Phone
        {
            resolutionScale = (float)Screen.width / respectedWidth;
            backgroundImage.sizeDelta = new Vector2(resourceWidth, resourceHeight);
        }
    }
}
