using Unity.Entities;
using UnityEngine;

namespace ECSSpriteSheetAnimation
{
    [UpdateInGroup(groupType: typeof(SpriteSheetPreperationGroup))]
    public class LayerBufferSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var buffers = DynamicBufferManager.GetLayerBuffers();

            for (int bufferID = 0; bufferID < buffers.Length; bufferID++)
            {
                DynamicBuffer<SpriteLayerBuffer> buffer = buffers[bufferID];
                Material material = DynamicBufferManager.GetMaterial(bufferID);

                Dependency = Entities
                    .WithBurst()
                    .WithSharedComponentFilter(new SpriteSheetMaterial(){material = material})
                    .WithNativeDisableContainerSafetyRestriction(buffer)
                    .ForEach((in SpriteSheetSortingLayer spriteSheetLayer, in BufferHook bufferHook) =>
                    {
                        buffer[bufferHook.entityID] = (float)spriteSheetLayer.value;
                    })
                    .ScheduleParallel(Dependency);
            }
        }
    } 
}
