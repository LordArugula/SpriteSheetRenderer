using Unity.Entities;

namespace ECSSpriteSheetAnimation
{
    public class MatrixBufferSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var buffers = DynamicBufferManager.GetMatrixBuffers();

            for (int bufferID = 0; bufferID < buffers.Length; bufferID++)
            {
                DynamicBuffer<MatrixBuffer> buffer = buffers[bufferID];
                Dependency = Entities
                    .WithBurst()
                    .WithNativeDisableContainerSafetyRestriction(buffer)
                    .ForEach((in SpriteMatrix spriteMatrix, in BufferHook bufferHook) =>
                    {
                        if (bufferID == bufferHook.bufferEntityID)
                        {
                            buffer[bufferHook.bufferID] = spriteMatrix.matrix;
                        }
                    })
                    .ScheduleParallel(Dependency);
            }
        }
    } 
}
