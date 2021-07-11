using Unity.Entities;
using UnityEngine;

namespace ECSSpriteSheetAnimation
{
    [UpdateInGroup(groupType: typeof(SpriteSheetPreperationGroup))]
    public class SpriteIndexBufferSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var buffers = DynamicBufferManager.GetIndexBuffers();

            for (int bufferID = 0; bufferID < buffers.Length; bufferID++)
            {
                DynamicBuffer<SpriteIndexBuffer> buffer = buffers[bufferID];
                Material material = DynamicBufferManager.GetMaterial(bufferID);
                
                Dependency = Entities
                    .WithBurst()
                    .WithSharedComponentFilter(new SpriteSheetMaterial(){material = material})
                    .WithNativeDisableContainerSafetyRestriction(buffer)
                    .ForEach((in SpriteIndex spriteIndex, in BufferHook bufferHook) =>
                    {
                        buffer[bufferHook.entityID] = spriteIndex.Value;
                    })
                    .ScheduleParallel(Dependency);
            }
        }
    } 
}
