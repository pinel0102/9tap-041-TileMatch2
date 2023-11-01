Shader "GUI/I2 SmartEdge/SDF FontData"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}

		_Spread ("Spread Size", Float) = 15
		_Color("Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"RenderType"="Transparent" 
		}
		
		Pass
		{
		}		
	}
}