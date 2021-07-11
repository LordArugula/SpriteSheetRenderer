using Unity.Entities;

namespace ECSSpriteSheetAnimation
{
    public struct BufferHook : IComponentData
    {
        /// <summary>
        /// ID of the buffer that stores sprite data
        /// </summary>
        public int bufferID;
        
        /// <summary>
        /// ID of the entity inside the buffer
        /// </summary>
        public int entityID;
    }
}
