using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
    public struct SEVertex
    {
        public Vector3 position;
        public Vector2 uv;
        public Color32 color;

        public Vector2 uv1;
        public Vector3 normal;
        public Vector4 tangent;

        public byte mode;

        public const byte MODE_Main = 0;
        public const byte MODE_Glow = 2;

		public int characterID;


        // initializing arrays in a struct is a pain, so this is a workaround:
        public byte byte0, byte1, byte2, byte3, byte4, byte5;
        public float float0, float1, float2, float3, float4, float5;

        public byte surface { get{ return byte0;} set {byte0=value;} }
        public byte edge    { get{ return byte1;} set {byte1=value;} }

        //--[ Mode: Main ]------------------------------------------
		public Color32 outlineColor /*byte2345*/	{ get{ MathUtils.tempColor.r=byte2; MathUtils.tempColor.g=byte3; MathUtils.tempColor.b=byte4; MathUtils.tempColor.a=byte5; return MathUtils.tempColor; } set{ if (mode==MODE_Main) {byte2=value.r; byte3=value.g; byte4=value.b; byte5=value.a;} } }
		public Vector2 faceUV       /*float01*/		{ get{ MathUtils.tempV2.x = float0; MathUtils.tempV2.y=float1; return MathUtils.tempV2; }    set{ float0 = value.x; float1 = value.y; } }
		public Vector2 outlineUV    /*float23*/		{ get{ MathUtils.tempV2.x = float2; MathUtils.tempV2.y=float3; return MathUtils.tempV2;  }    set{ if (mode==MODE_Main) {float2 = value.x; float3 = value.y;} } }

        //--[ Mode: Glow ]------------------------------------------
        public byte glowInnerWidth  /*byte2*/		{ get{ return mode==MODE_Glow?byte2:(byte)0;} set { if (mode==MODE_Glow) byte2=value;} }
        public byte glowOuterWidth  /*byte3*/		{ get{ return mode==MODE_Glow?byte3:(byte)0;} set { if (mode==MODE_Glow) byte3=value;} }
        public byte glowIntensity   /*byte4*/		{ get{ return mode==MODE_Glow?byte4:(byte)0;} set { if (mode==MODE_Glow) byte4=value;} }
        public byte glowPower       /*byte5*/		{ get{ return mode==MODE_Glow?byte5:(byte)0;} set { if (mode==MODE_Glow) byte5=value;} }


		public static SEVertex Get( UIVertex vert, byte characterID )
        {
            seVertex.position 		= vert.position;
            seVertex.color    		= vert.color;
            seVertex.uv       		= vert.uv0;
			seVertex.characterID 	= characterID;
            return seVertex;
        }

        public static SEVertex seVertex = new SEVertex();

        public static SEVertex Split( SEVertex a, SEVertex b, float t )
        {
            seVertex.position  = Vector3.Lerp (a.position, b.position, t);
            seVertex.uv        = Vector2.Lerp (a.uv, b.uv, t);
            seVertex.color     = Color32.Lerp(a.color, b.color, t);
            seVertex.mode      = a.mode;

            seVertex.byte0     = MathUtils.LerpByte(a.byte0, b.byte0, t);
            seVertex.byte1     = MathUtils.LerpByte(a.byte1, b.byte1, t);
            seVertex.byte2     = MathUtils.LerpByte(a.byte2, b.byte2, t);
            seVertex.byte3     = MathUtils.LerpByte(a.byte3, b.byte3, t);
            seVertex.byte4     = MathUtils.LerpByte(a.byte4, b.byte4, t);
            seVertex.byte5     = MathUtils.LerpByte(a.byte5, b.byte5, t);

            seVertex.float0    = Mathf.Lerp(a.float0, b.float0, t);
            seVertex.float1    = Mathf.Lerp(a.float1, b.float1, t);
            seVertex.float2    = Mathf.Lerp(a.float2, b.float2, t);
            seVertex.float3    = Mathf.Lerp(a.float3, b.float3, t);
            seVertex.float4    = Mathf.Lerp(a.float4, b.float4, t);
            seVertex.float5    = Mathf.Lerp(a.float5, b.float5, t);

            return seVertex;
        }
    }
}