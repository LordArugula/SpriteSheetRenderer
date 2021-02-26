using Unity.Entities;

namespace ECSSpriteSheetAnimation
{
    [GenerateAuthoringComponent]
    public struct Scale2D : IComponentData
    {
        public float x;
        public float y;
    } 
}
