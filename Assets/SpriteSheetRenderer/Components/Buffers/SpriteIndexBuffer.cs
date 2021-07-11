using Unity.Entities;

namespace ECSSpriteSheetAnimation
{
    [InternalBufferCapacity(sizeof(int))]
    public struct SpriteIndexBuffer : IBufferElementData
    {
        public int index;

        public static implicit operator int(SpriteIndexBuffer e) { return e.index; }
        public static implicit operator SpriteIndexBuffer(int e) { return new SpriteIndexBuffer { index = e }; }
    } 
}
