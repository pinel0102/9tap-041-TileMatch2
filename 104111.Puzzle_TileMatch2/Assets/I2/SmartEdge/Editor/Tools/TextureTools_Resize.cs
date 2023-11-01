// Only works on ARGB32, RGB24 and Alpha8 textures that are marked readable

using System.Threading;
using UnityEngine;

namespace I2.SmartEdge
{
	public static partial class TextureTools
	{
		public class ThreadData
		{
			public int start;
			public int end;
			public ThreadData (int s, int e) {
				start = s;
				end = e;
			}
		}
		
		private static Color[] texColors;
		private static Color[] newColors;
		private static int w;
		private static float ratioX;
		private static float ratioY;
		private static int w2;
		private static int finishCount;
		private static Mutex mutex;

		#region Color

		public static void Point (Texture2D tex, int newWidth, int newHeight)
		{
			ThreadedScale (tex, newWidth, newHeight, false);
		}
		
		public static void Bilinear (Texture2D tex, int newWidth, int newHeight)
		{
			ThreadedScale (tex, newWidth, newHeight, true);
		}

		public static void Bilinear (ref Color[] sourceColors, int curWidth, int curHeight, int newWidth, int newHeight)
		{
			ThreadedScale (ref sourceColors, curWidth, curHeight, newWidth, newHeight, true);
		}

		/*public static void Bilinear (ref Color32[] sourceColors, int curWidth, int curHeight, int newWidth, int newHeight)
		{
			int nColors = sourceColors.Length;
			Color[] colors = new Color[nColors];
			for (int i=0; i<nColors; ++i)
				colors[i] = sourceColors[i];

			ThreadedScale (ref colors, curWidth, curHeight, newWidth, newHeight, true);

			nColors = newWidth * newHeight;
			sourceColors = new Color32[nColors];
			for (int i=0; i<nColors; ++i)
				sourceColors[i] = colors[i];
		}*/


		
		private static void ThreadedScale (Texture2D tex, int newWidth, int newHeight, bool useBilinear)
		{
			Color[] sourceColors = tex.GetPixels();

			ThreadedScale (ref sourceColors, tex.width, tex.height, newWidth, newHeight, useBilinear);

			tex.Resize(newWidth, newHeight);
			tex.SetPixels(newColors);
			tex.Apply();
		}

		private static void ThreadedScale (ref Color[] sourceColors, int curWidth, int curHeight, int newWidth, int newHeight, bool useBilinear)
		{
			texColors = sourceColors;
			newColors = new Color[newWidth * newHeight];
			if (useBilinear)
			{
				ratioX = 1.0f / ((float)newWidth / (curWidth-1));
				ratioY = 1.0f / ((float)newHeight / (curHeight-1));
			}
			else {
				ratioX = ((float)curWidth) / newWidth;
				ratioY = ((float)curHeight) / newHeight;
			}
			w = curWidth;
			w2 = newWidth;
			var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
			var slice = newHeight/cores;
			
			finishCount = 0;
			if (mutex == null) {
				mutex = new Mutex(false);
			}
			if (cores > 1)
			{
				int i = 0;
				ThreadData threadData;
				for (i = 0; i < cores-1; i++) {
					threadData = new ThreadData(slice * i, slice * (i + 1));
					ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
					Thread thread = new Thread(ts);
					thread.Start(threadData);
				}
				threadData = new ThreadData(slice*i, newHeight);
				if (useBilinear)
				{
					BilinearScale(threadData);
				}
				else
				{
					PointScale(threadData);
				}
				while (finishCount < cores)
				{
					Thread.Sleep(1);
				}
			}
			else
			{
				ThreadData threadData = new ThreadData(0, newHeight);
				if (useBilinear)
				{
					BilinearScale(threadData);
				}
				else
				{
					PointScale(threadData);
				}
			}
			sourceColors = newColors;
		}
		
		public static void BilinearScale (System.Object obj)
		{
			ThreadData threadData = (ThreadData) obj;
			for (var y = threadData.start; y < threadData.end; y++)
			{
				int yFloor = (int)Mathf.Floor(y * ratioY);
				var y1 = yFloor * w;
				var y2 = (yFloor+1) * w;
				var yw = y * w2;
				
				for (var x = 0; x < w2; x++) {
					int xFloor = (int)Mathf.Floor(x * ratioX);
					var xLerp = x * ratioX-xFloor;
					newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor+1], xLerp),
					                                       ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor+1], xLerp),
					                                       y*ratioY-yFloor);
				}
			}
			
			mutex.WaitOne();
			finishCount++;
			mutex.ReleaseMutex();
		}
		
		public static void PointScale (System.Object obj)
		{
			ThreadData threadData = (ThreadData) obj;
			for (var y = threadData.start; y < threadData.end; y++)
			{
				var thisY = (int)(ratioY * y) * w;
				var yw = y * w2;
				for (var x = 0; x < w2; x++) {
					newColors[yw + x] = texColors[(int)(thisY + ratioX*x)];
				}
			}
			
			mutex.WaitOne();
			finishCount++;
			mutex.ReleaseMutex();
		}
		
		private static Color ColorLerpUnclamped (Color c1, Color c2, float value)
		{
			return new Color (c1.r + (c2.r - c1.r)*value, 
			                  c1.g + (c2.g - c1.g)*value, 
			                  c1.b + (c2.b - c1.b)*value, 
			                  c1.a + (c2.a - c1.a)*value);
		}

		#endregion

		#region Color32

		public static void Bilinear (ref Color32[] aSourceColor, int curWidth, int curHeight, int newWidth, int newHeight)
		{
			Vector2 vSourceSize = new Vector2(curWidth, curHeight);

			int xLength = newWidth * newHeight;
			Color32[] newColors = new Color32[xLength];

			Vector2 vPixelSize = new Vector2(vSourceSize.x / newWidth, vSourceSize.y / newHeight);
			
			Vector2 vCenter = new Vector2();
			for(var i=0; i<xLength; i++)
			{
				
				//*** Figure out x&y
				float xX = (float)i % newWidth;
				float xY = Mathf.Floor((float)i / newWidth);
				
				//*** Calculate Center
				vCenter.x = (xX / newWidth) * vSourceSize.x;
				vCenter.y = (xY / newHeight) * vSourceSize.y;
				
				//*** Calculate grid around point
				int xXFrom = (int)Mathf.Max(Mathf.Floor(vCenter.x - (vPixelSize.x * 0.5f)), 0);
				int xXTo = (int)Mathf.Min(Mathf.Ceil(vCenter.x + (vPixelSize.x * 0.5f)), vSourceSize.x);
				int xYFrom = (int)Mathf.Max(Mathf.Floor(vCenter.y - (vPixelSize.y * 0.5f)), 0);
				int xYTo = (int)Mathf.Min(Mathf.Ceil(vCenter.y + (vPixelSize.y * 0.5f)), vSourceSize.y);
				
				//*** Loop and accumulate
				Vector4 oColorTemp = new Vector4();
				int xGridCount = 0;
				for(int iy = xYFrom; iy < xYTo; iy++)
				for(int ix = xXFrom; ix < xXTo; ix++)
				{
					Color32 c = aSourceColor[(int)(((float)iy * vSourceSize.x) + ix)];
					oColorTemp.x += c.r;
					oColorTemp.y += c.g;
					oColorTemp.z += c.b;
					oColorTemp.w += c.a;
					xGridCount++;
				}
				//*** Average Color
				newColors[i].r = (byte)(oColorTemp.x / xGridCount);
				newColors[i].g = (byte)(oColorTemp.y / xGridCount);
				newColors[i].b = (byte)(oColorTemp.z / xGridCount);
				newColors[i].a = (byte)(oColorTemp.w / xGridCount);
			}
			aSourceColor = newColors;
		}

		public static void BilinearCenter (Texture2D texture, int Width, int Height, int ScaleDownFactor)
		{
			Color32[] colors = texture.GetPixels32();
			BilinearCenter( ref colors, Width, Height, Width/ScaleDownFactor, Height/ScaleDownFactor );
			texture.Resize(Width/ScaleDownFactor, Height/ScaleDownFactor);
			texture.SetPixels32(colors);
		}

		public static void BilinearCenter (ref Color32[] aSourceColor, int curWidth, int curHeight, int ScaleDownFactor)
		{
			int newWidth = Mathf.CeilToInt (curWidth / (float)ScaleDownFactor);
			int newHeight = Mathf.CeilToInt (curHeight / (float)ScaleDownFactor);

			Color32[] newColors = new Color32[newWidth * newHeight];

			int offset = Mathf.FloorToInt(ScaleDownFactor / 2.0f);

			for (int y = offset, yy=0; y < curHeight; y += ScaleDownFactor, yy++)
			for (int x = offset, xx=0; x < curWidth; x += ScaleDownFactor, xx++)
			{
				newColors[xx + yy * newWidth] = aSourceColor[x + y * curWidth];
			}
			aSourceColor = newColors;
		}

		public static void BilinearCenter (ref Color32[] aSourceColor, int curWidth, int curHeight, int newWidth, int newHeight)
		{
			Color32[] newColors = new Color32[newWidth * newHeight];

			float factorX = curWidth / (float)newWidth;
			float factorY = curHeight/ (float)newHeight;

			for (int y=0; y<newHeight; y++)
			for (int x=0; x<newWidth; x++)
			{
				int xx = Mathf.FloorToInt(factorX/2.0f + x * factorX);
				int yy = Mathf.FloorToInt(factorY/2.0f + y * factorY);

				newColors[x + y * newWidth] = aSourceColor[xx + yy * curWidth];
			}
			aSourceColor = newColors;
		}
		#endregion
	}
}