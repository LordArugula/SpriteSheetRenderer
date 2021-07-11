using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace ECSSpriteSheetAnimation
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class SpriteSheetRendererSystem : SystemBase
    {
        private Mesh mesh;

        protected override void OnCreate()
        {
            mesh = MeshUtility.CreateQuad();
        }

        protected override void OnDestroy()
        {
            SpriteSheetManager.CleanBuffers();
        }

        protected override void OnUpdate()
        {
            for (int i = 0; i < SpriteSheetManager.renderInformation.Count; i++)
            {
                if (UpdateBuffers(i) > 0)
                {
                    Graphics.DrawMeshInstancedIndirect(
                        mesh,
                        0, 
                        SpriteSheetManager.renderInformation[i].material,
                        SpriteSheetManager.renderInformation[i].bounds,
                        SpriteSheetManager.renderInformation[i].argsBuffer);
                }

                Entity bufferEntity = SpriteSheetManager.renderInformation[i].bufferEntity;

                //this is w.i.p to clean the old buffers
                DynamicBuffer<SpriteIndexBuffer> indexBuffer = EntityManager.GetBuffer<SpriteIndexBuffer>(bufferEntity);
                int start = indexBuffer.Length - 1;
                while (start >= 0 && indexBuffer[start].index == -1)
                {
                    start--;
                }

                start++;
                int toRemove = indexBuffer.Length - start;
                if (toRemove > 0)
                {
                    EntityManager.GetBuffer<SpriteIndexBuffer>(bufferEntity).RemoveRangeSwapBack(start, toRemove);
                    EntityManager.GetBuffer<MatrixBuffer>(bufferEntity).RemoveRangeSwapBack(start, toRemove);
                    EntityManager.GetBuffer<SpriteColorBuffer>(bufferEntity).RemoveRangeSwapBack(start, toRemove);
                    EntityManager.GetBuffer<SpriteLayerBuffer>(bufferEntity).RemoveRangeSwapBack(start, toRemove);
                }
            }
        }

        // we should only update the index of the changed datas for 
        // indexBuffer, matrixBuffer, and colorBuffer inside a bursted
        // job to avoid overhead
        private int UpdateBuffers(int renderIndex)
        {
            SpriteSheetManager.ReleaseBuffer(renderIndex);

            RenderInformation renderInformation = SpriteSheetManager.renderInformation[renderIndex];
            int instanceCount = EntityManager.GetBuffer<SpriteIndexBuffer>(renderInformation.bufferEntity).Length;
            if (instanceCount > 0)
            {
                if (renderInformation.updateUvs)
                {
                    int stride = instanceCount >= 16 ? 16 : 16 * SpriteSheetCache.GetLength(renderInformation.material);
                    SpriteSheetManager.ReleaseUvBuffer(renderIndex);
                    renderInformation.uvBuffer = new ComputeBuffer(instanceCount, stride);
                    renderInformation.uvBuffer.SetData(EntityManager.GetBuffer<UvBuffer>(renderInformation.bufferEntity).Reinterpret<float4>().AsNativeArray());
                    renderInformation.material.SetBuffer("uvBuffer", renderInformation.uvBuffer);
                    renderInformation.updateUvs = false;
                }

                renderInformation.indexBuffer = new ComputeBuffer(instanceCount, sizeof(int));
                renderInformation.indexBuffer.SetData(EntityManager.GetBuffer<SpriteIndexBuffer>(renderInformation.bufferEntity).Reinterpret<int>().AsNativeArray());
                renderInformation.material.SetBuffer("indexBuffer", renderInformation.indexBuffer);

                renderInformation.matrixBuffer = new ComputeBuffer(instanceCount, 16);
                renderInformation.matrixBuffer.SetData(EntityManager.GetBuffer<MatrixBuffer>(renderInformation.bufferEntity).Reinterpret<float4>().AsNativeArray());
                renderInformation.material.SetBuffer("matrixBuffer", renderInformation.matrixBuffer);

                renderInformation.args[1] = (uint)instanceCount;
                renderInformation.argsBuffer.SetData(renderInformation.args);

                renderInformation.colorsBuffer = new ComputeBuffer(instanceCount, 16);
                renderInformation.colorsBuffer.SetData(EntityManager.GetBuffer<SpriteColorBuffer>(renderInformation.bufferEntity).Reinterpret<float4>().AsNativeArray());
                renderInformation.material.SetBuffer("colorsBuffer", renderInformation.colorsBuffer);

                renderInformation.layerBuffer = new ComputeBuffer(instanceCount, sizeof(float));
                renderInformation.layerBuffer.SetData(EntityManager.GetBuffer<SpriteLayerBuffer>(renderInformation.bufferEntity).Reinterpret<float>().AsNativeArray());
                renderInformation.material.SetBuffer("layerBuffer", renderInformation.layerBuffer);
            }
            return instanceCount;
        }
    }
}
