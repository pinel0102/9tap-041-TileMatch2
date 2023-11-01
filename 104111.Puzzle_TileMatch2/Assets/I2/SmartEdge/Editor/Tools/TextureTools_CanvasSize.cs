using UnityEngine;
using UnityEditor;
using System.Collections;

namespace I2.SmartEdge
{
	public static partial class TextureTools 
	{
		#region Expand
		
		static public void AddTextureBorders( Texture2D texture, int LeftBorder, int RightBorder, int TopBorder, int BottomBorder)
		{
			// Add extra space on the borders
			Color32[] colors = texture.GetPixels32 ();
			
			int Width = texture.width;
			int Height = texture.height;
			
			AddTextureBorders(colors, out colors, ref Width, ref Height, LeftBorder, RightBorder, TopBorder, BottomBorder);
			
			texture.Resize (Width, Height);
			texture.SetPixels32 (colors);
			texture.Apply ();
		}
		
		static public void AddTextureBorders (Color32[] source, out Color32[] dest, ref int Width, ref int Height, int LeftBorder, int RightBorder, int TopBorder, int BottomBorder )
		{
			int NewWidth = Width + LeftBorder + RightBorder;
			int NewHeight= Height + TopBorder + BottomBorder;
			dest = new Color32[NewWidth * NewHeight];
			
			for (int y=0; y<NewHeight; ++y)
			for (int x=0; x<NewWidth; ++x)
				if (x < LeftBorder || y < TopBorder || (NewWidth - x) <= RightBorder || (NewHeight - y) <= BottomBorder)
				{
					// find closest pixel on border
					int BorderX = Mathf.Clamp(x-LeftBorder, 0, Width-1);
					int BorderY = Mathf.Clamp(y-TopBorder, 0, Height-1);
					dest [y * NewWidth + x] = source [BorderY * Width + BorderX];
					dest [y * NewWidth + x].a = 0;
				}
				else
				{
					dest [y * NewWidth + x] = source [(y - TopBorder) * Width + x - LeftBorder];
				}
			
			Width = NewWidth;
			Height = NewHeight;
		}
		
		#endregion
		
		#region Crop
		
		public static void CropEmptySpace (Color32[] inColors, int inWidth, int inHeight, out Color32[] outColors, out int outWidth, out int outHeight, Color32 empty = default(Color32), bool CheckR=true, bool CheckG=true, bool CheckB=true, bool CheckA=true, bool KeepAspectRatio=false)
		{
			//System.Diagnostics.Stopwatch mPerfWatch = new System.Diagnostics.Stopwatch();
			//mPerfWatch.Start();

			//-- Find Min Max borders -----------------
			int xMin=0, xMax=inWidth, 
			yMin=0, yMax=inHeight;
			for (int x=0; x<inWidth && xMin==0; ++x)
			for (int y=0; y<inHeight; ++y)
				if (!IsEmpty(inColors[y*inWidth+x], empty, CheckR, CheckG, CheckB, CheckA))
				{
					xMin = x;
					break;
				}
			
			for (int x=inWidth-1; x>=0 && xMax==inWidth; --x)
			for (int y=0; y<inHeight; ++y)
				if (!IsEmpty(inColors[y*inWidth+x], empty, CheckR, CheckG, CheckB, CheckA))
				{
					xMax = x+1;
					break;
				}
			
			for (int y=0; y<inHeight && yMin==0; ++y)
			for (int x=0; x<inWidth; ++x)
				if (!IsEmpty(inColors[y*inWidth+x], empty, CheckR, CheckG, CheckB, CheckA))
				{
					yMin = y;
					break;
				}
			
			for (int y=inHeight-1; y>=0 && yMax==inHeight; --y)
			for (int x=0; x<inWidth; ++x)
				if (!IsEmpty(inColors[y*inWidth+x], empty, CheckR, CheckG, CheckB, CheckA))
				{
					yMax = y+1;
					break;
				}
			
			outWidth  = xMax-xMin;
			outHeight = yMax-yMin;

			if (KeepAspectRatio)
			{
				float orgAspect = inWidth/(float)inHeight;
				float newAspect = outWidth/(float)outHeight;

				if (newAspect<orgAspect)
				{
					var w = Mathf.FloorToInt( outWidth/orgAspect );
					xMin = Mathf.Max (0, xMin - (w-outWidth));
					outWidth = w;
				}
				else
				{
					var h = Mathf.FloorToInt( outWidth*orgAspect );
					yMin = Mathf.Max (0, yMin - (h-outHeight));
					outHeight = h;
				}
			}
			
			outColors = new Color32[outWidth*outHeight];
			for (int y=0; y<outHeight; ++y)
				for (int x=0; x<outWidth; ++x)
					outColors[y*outWidth+x] = inColors[(y+yMin)*inWidth + x+xMin];
			
			//mPerfWatch.Stop();
			//Debug.Log("Crop Time: "+ (mPerfWatch.ElapsedTicks / (double)System.TimeSpan.TicksPerMillisecond));
		}

		static bool IsEmpty(Color32 c, Color32 Empty = default(Color32), bool CheckR=true, bool CheckG=true, bool CheckB=true, bool CheckA=true)
		{
			if ((CheckR && c.r!=Empty.r) || (CheckG && c.g!=Empty.g) || (CheckB && c.b!=Empty.b) || (CheckA && c.a!=Empty.a))
			    return false;
		    return true;
		}
		
		public static void CropEmptySpace (Texture2D texture, Color32 empty = default(Color32), bool CheckR=true, bool CheckG=true, bool CheckB=true, bool CheckA=true, bool KeepAspectRatio=false )
		{
			Color32[] colors = TextureTools.GetPixels32(texture);
			if (colors==null) return;
			
			Color32[] newColors;
			int newWidth, newHeight;
			
			CropEmptySpace( colors, texture.width, texture.height, out newColors, out newWidth, out newHeight, empty, CheckR, CheckG, CheckB, CheckA, KeepAspectRatio );
			
			texture.Resize(newWidth, newHeight);
			texture.SetPixels32(newColors);
			texture.Apply();
		}
		#endregion
	}
}