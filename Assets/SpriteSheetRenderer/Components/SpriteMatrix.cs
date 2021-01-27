using Unity.Entities;
using Unity.Mathematics;

namespace ECSSpriteSheetAnimation
{
    public struct SpriteMatrix : IComponentData
    {
        public float4 matrix;
    } 
}
