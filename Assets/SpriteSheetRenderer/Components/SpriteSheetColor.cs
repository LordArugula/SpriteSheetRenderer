using Unity.Entities;
using Unity.Mathematics;

namespace ECSSpriteSheetAnimation
{
    [GenerateAuthoringComponent]
    public struct SpriteSheetColor : IComponentData
    {
        public float4 color;
    } 
}
