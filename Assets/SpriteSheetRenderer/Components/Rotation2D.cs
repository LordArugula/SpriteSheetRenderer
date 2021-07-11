using Unity.Entities;

namespace ECSSpriteSheetAnimation
{
    [GenerateAuthoringComponent]
    public struct Rotation2D : IComponentData
    {
        public float angle;
    }

}