using Unity.Entities;
using Unity.Mathematics;

namespace ECSSpriteSheetAnimation
{
    [GenerateAuthoringComponent]
    public struct SpriteMatrix : IComponentData
    {
        public float4 matrix;
    } 
}
