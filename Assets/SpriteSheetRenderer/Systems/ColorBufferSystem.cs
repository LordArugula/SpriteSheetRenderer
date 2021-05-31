using Unity.Entities;
using UnityEngine;

namespace ECSSpriteSheetAnimation
{
    [UpdateInGroup(groupType: typeof(SpriteSheetPreperationGroup))]
    public class ColorBufferSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var buffers = DynamicBufferManager.GetColorBuffers();

            for (int bufferID = 0; bufferID < buffers.Length; bufferID++)
            {
                DynamicBuffer<SpriteColorBuffer> buffer = buffers[bufferID];
                Material material = DynamicBufferManager.GetMaterial(bufferID);

                Dependency = Entities
                    .WithBurst()
                    .WithSharedComponentFilter(new SpriteSheetMaterial(){material = material})
                    .WithNativeDisableContainerSafetyRestriction(buffer)
                    .ForEach((in SpriteSheetColor spriteSheetColor, in BufferHook bufferHook) =>
                    {
                        buffer[bufferHook.entityID] = spriteSheetColor.color;
                    })
                    .ScheduleParallel(Dependency);
            }
        }
    } 
}
