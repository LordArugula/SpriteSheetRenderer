using Unity.Entities;

namespace ECSSpriteSheetAnimation
{
    [GenerateAuthoringComponent]
    public struct SpriteSheetAnimation : IComponentData
    {
        public PlayMode playMode;

        public float elapsedTime;
        public float framesPerSecond;
        public int frameCount;
    }
}
