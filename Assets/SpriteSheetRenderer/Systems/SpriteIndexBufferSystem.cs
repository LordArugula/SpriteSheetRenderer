using Unity.Entities;

namespace ECSSpriteSheetAnimation
{
    public class SpriteIndexBufferSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var buffers = DynamicBufferManager.GetIndexBuffers();

            for (int bufferID = 0; bufferID < buffers.Length; bufferID++)
            {
                DynamicBuffer<SpriteIndexBuffer> buffer = buffers[bufferID];
                Dependency = Entities
                    .WithBurst()
                    .WithNativeDisableContainerSafetyRestriction(buffer)
                    .ForEach((in SpriteIndex spriteIndex, in BufferHook bufferHook) =>
                    {
                        buffer[bufferHook.bufferID] = spriteIndex.Value;
                    })
                    .ScheduleParallel(Dependency);
            }
        }
    } 
}
