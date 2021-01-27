using Unity.Entities;
using Unity.Mathematics;

namespace ECSSpriteSheetAnimation
{
    [InternalBufferCapacity(16)]
    public struct MatrixBuffer : IBufferElementData
    {
        public float4 matrix;

        public static implicit operator float4(MatrixBuffer e) { return e.matrix; }
        public static implicit operator MatrixBuffer(float4 e) { return new MatrixBuffer { matrix = e }; }
    } 
}
