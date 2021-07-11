using Unity.Entities;
using Unity.Mathematics;

namespace ECSSpriteSheetAnimation
{
    [InternalBufferCapacity(sizeof(float))]
    public struct SpriteLayerBuffer : IBufferElementData
    {
        public float layer;

        public static implicit operator float(SpriteLayerBuffer e) { return e.layer; }
        public static implicit operator SpriteLayerBuffer(float e) { return new SpriteLayerBuffer { layer = e }; }
    } 
}