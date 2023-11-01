using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
    public enum SEVerticesLayers {
        Original,
        FloorReflection_Back,
        Shadow,
        Glow_Back,
        Outline,
        Face,
        Glow_Front,
        FloorReflection_Front,
        length
    }

    public class SEMesh
    {
        public ArrayBufferSEVertex[] mLayers = null;
        public int numLayers;

        public SEMesh()
        {
            numLayers = (int)SEVerticesLayers.length;
            mLayers = new ArrayBufferSEVertex[numLayers];
            for (int i = 0; i < numLayers; ++i)
                mLayers[i] = new ArrayBufferSEVertex();
        }
    }
}