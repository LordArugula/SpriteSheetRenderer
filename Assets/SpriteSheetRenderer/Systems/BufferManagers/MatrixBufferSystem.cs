using Unity.Entities;
using UnityEngine;

namespace ECSSpriteSheetAnimation
{
    [UpdateInGroup(groupType: typeof(SpriteSheetPreperationGroup))]
    public class MatrixBufferSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var buffers = DynamicBufferManager.GetMatrixBuffers();

            for (int bufferID = 0; bufferID < buffers.Length; bufferID++)
            {
                DynamicBuffer<MatrixBuffer> buffer = buffers[bufferID];
                Material material = DynamicBufferManager.GetMaterial(bufferID);
                
                Dependency = Entities
                    .WithBurst()
                    .WithSharedComponentFilter(new SpriteSheetMaterial(){material = material})
                    .WithNativeDisableContainerSafetyRestriction(buffer)
                    .ForEach((in SpriteMatrix spriteMatrix, in BufferHook bufferHook) =>
                    {
                        buffer[bufferHook.entityID] = spriteMatrix.matrix;
                    })
                    .ScheduleParallel(Dependency);
            }
        }
    } 
}
