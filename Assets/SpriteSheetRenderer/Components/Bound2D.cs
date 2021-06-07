using System.Diagnostics;
using Unity.Entities;
using Unity.Mathematics;

namespace ECSSpriteSheetAnimation
{
    [GenerateAuthoringComponent,
    DebuggerDisplay("Position: {position}, Scale: {scale}, (visible ? \"Visible\" : \"Invisible\")")]
    public struct Bound2D : IComponentData
    {
        public float2 position;
        public float2 scale;
        public bool visible;
    } 
}
