﻿using Unity.Entities;
using UnityEngine;

namespace ECSSpriteSheetAnimation
{
    public class RenderInformation
    {
        public ComputeBuffer matrixBuffer;
        public ComputeBuffer argsBuffer;
        public ComputeBuffer colorsBuffer;
        public ComputeBuffer uvBuffer;
        public ComputeBuffer indexBuffer;
        public Entity bufferEntity;
        public int spriteCount;
        public Material material;
        public uint[] args;
        public bool updateUvs;
        
        /// <summary>
        /// Area in world space that sprites using this material are covering
        /// </summary>
        public Bounds bounds;

        public RenderInformation(Material material, Entity bufferEntity)
        {
            this.material = material;
            spriteCount = SpriteSheetCache.GetLength(material);
            this.bufferEntity = bufferEntity;
            args = new uint[5] { 6, 0, 0, 0, 0 };
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            updateUvs = true;
        }

        public void DestroyBuffers()
        {
            if (matrixBuffer != null)
            {
                matrixBuffer.Release();
                matrixBuffer = null;
            }

            if (argsBuffer != null)
            {
                argsBuffer.Release();
                argsBuffer = null;
            }

            if (colorsBuffer != null)
            {
                colorsBuffer.Release();
                colorsBuffer = null;
            }

            if (uvBuffer != null)
            {
                uvBuffer.Release();
                uvBuffer = null;
            }

            if (indexBuffer != null)
            {
                indexBuffer.Release();
                indexBuffer = null;
            }
        }
    } 
}
