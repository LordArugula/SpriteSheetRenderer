using Unity.Entities;

namespace ECSSpriteSheetAnimation
{
    [GenerateAuthoringComponent]
    public struct BufferHook : IComponentData
    {
        public int bufferID;
        public int bufferEntityID;
    } 
}
