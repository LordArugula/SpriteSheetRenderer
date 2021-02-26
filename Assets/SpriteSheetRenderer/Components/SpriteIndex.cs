using Unity.Entities;
using Unity.Mathematics;

namespace ECSSpriteSheetAnimation
{
    [GenerateAuthoringComponent]
    public struct SpriteIndex : IComponentData
    {
        public int Value;
    } 
}
