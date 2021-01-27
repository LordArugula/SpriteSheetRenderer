using Unity.Entities;

namespace ECSSpriteSheetAnimation
{
    public struct BufferHook : IComponentData
    {
        public int bufferID;
        public int bufferEntityID;
    } 
}
