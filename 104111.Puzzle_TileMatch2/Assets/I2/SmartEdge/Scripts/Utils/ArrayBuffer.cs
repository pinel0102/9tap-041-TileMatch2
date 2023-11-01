using UnityEngine;
using System.Collections;
using System;

namespace I2.SmartEdge
{
	public class ArrayBuffer<T>
	{
		public int Size;
		public T[] Buffer = new T[0];


		public void ReserveExtra(int newElements)
		{
            int newSize = Size + newElements;
            if (Buffer.Length < newSize)
			    Array.Resize (ref Buffer, newSize);
		}

		public void ReserveTotal(int Capacity)
		{
			if (Buffer.Length< Capacity)
				Array.Resize (ref Buffer, Capacity);
		}

        public void Clear(int Capacity=-1)
        {
            if (Capacity > 0 && Buffer.Length < Capacity)
                Buffer = new T[Capacity];
            Size = 0;
        }

        public void Reset(int NewSize=-1)
		{
            if (NewSize > 0 && Buffer.Length < NewSize)
                Buffer = new T[NewSize];
            Size = NewSize>0 ? NewSize : 0;
        }

        public void CopyFrom( ArrayBuffer<T> from, int fromIndex=-1, int toIndex=-1, int count=-1, bool trim=true)
        {
            if (fromIndex < 0) fromIndex = 0;
            if (count < 0) count = from.Size - fromIndex;
            if (toIndex < 0) toIndex = Size;
            var newSize = toIndex + count;

            if (Buffer.Length < newSize)
            {
                if (Size > 0)
                    Array.Resize(ref Buffer, newSize);
                else
                    Buffer = new T[newSize];
                Size = newSize;
            }
            else
            if (trim)
                Size = newSize;

            Array.Copy(from.Buffer, fromIndex, Buffer, toIndex, count);
        }
    }

    public class ArrayBufferSEVertex : ArrayBuffer<SEVertex>
    {
        public void SplitSegment_Inlined(int newV, SEVertex[] source, int a, int b, float t)
        {
            Vector3 v;
            v.x = source[a].position.x + (source[b].position.x - source[a].position.x) * t;
            v.y = source[a].position.y + (source[b].position.y - source[a].position.y) * t;
            v.z = source[a].position.z + (source[b].position.z - source[a].position.z) * t;

            Buffer[newV].position.x = v.x;
            Buffer[newV].position.y = v.y;
            Buffer[newV].position.z = v.z;

            Buffer[newV].color.r = (byte)(source[a].color.r + (source[b].color.r - (int)source[a].color.r) * t);
            Buffer[newV].color.g = (byte)(source[a].color.g + (source[b].color.g - (int)source[a].color.g) * t);
            Buffer[newV].color.b = (byte)(source[a].color.b + (source[b].color.b - (int)source[a].color.b) * t);
            Buffer[newV].color.a = (byte)(source[a].color.a + (source[b].color.a - (int)source[a].color.a) * t);

            Buffer[newV].uv.x = source[a].uv.x + (source[b].uv.x - source[a].uv.x) * t;
            Buffer[newV].uv.y = source[a].uv.y + (source[b].uv.y - source[a].uv.y) * t;

            Buffer[newV].uv1.x = source[a].uv1.x + (source[b].uv1.x - source[a].uv1.x) * t;
            Buffer[newV].uv1.y = source[a].uv1.y + (source[b].uv1.y - source[a].uv1.y) * t;

            //Buffer[newV].tangent = source[a].tangent + (source[b].tangent - source[a].tangent) * t;
            //Normals[newV] = Normals[a] + (Normals[b] - Normals[a]) * t;
            Buffer[newV].characterID = source[a].characterID;

            Buffer[newV].mode = source[a].mode;

            Buffer[newV].byte0 = (byte)(source[a].byte0 + (source[b].byte0 - source[a].byte0) * t);
            Buffer[newV].byte1 = (byte)(source[a].byte1 + (source[b].byte1 - source[a].byte1) * t);
            Buffer[newV].byte2 = (byte)(source[a].byte2 + (source[b].byte2 - source[a].byte2) * t);
            Buffer[newV].byte3 = (byte)(source[a].byte3 + (source[b].byte3 - source[a].byte3) * t);
            Buffer[newV].byte4 = (byte)(source[a].byte4 + (source[b].byte4 - source[a].byte4) * t);
            Buffer[newV].byte5 = (byte)(source[a].byte5 + (source[b].byte5 - source[a].byte5) * t);

            Buffer[newV].float0 = source[a].float0 + (source[b].float0 - source[a].float0) * t;
            Buffer[newV].float1 = source[a].float1 + (source[b].float1 - source[a].float1) * t;
            Buffer[newV].float2 = source[a].float2 + (source[b].float2 - source[a].float2) * t;
            Buffer[newV].float3 = source[a].float3 + (source[b].float3 - source[a].float3) * t;
        }
    }
}