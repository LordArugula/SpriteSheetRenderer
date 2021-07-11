using Unity.Entities;
using Unity.Mathematics;

namespace ECSSpriteSheetAnimation
{
    /// <summary>
    /// This describes the number of buffer elements that should be reserved
    /// in chunk data for each instance of a buffer. In this case, 32 bytes
    /// along with the size of the buffer header (16 bytes on 64-bit targets).
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct UvBuffer : IBufferElementData
    {
        public static implicit operator float4(UvBuffer e) { return e.uv; }
        public static implicit operator UvBuffer(float4 e) { return new UvBuffer { uv = e }; }
        public float4 uv;
    }
}
