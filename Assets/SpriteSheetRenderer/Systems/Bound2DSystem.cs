using Unity.Entities;
using Unity.Transforms;

namespace ECSSpriteSheetAnimation
{
    [UpdateInGroup(groupType: typeof(SpriteSheetPreperationGroup))]
    public class Bound2DSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Dependency = Entities
                .WithBurst()
                .ForEach((ref Bound2D bound, in Position2D translation, in Scale scale) =>
                {
                    bound.scale = scale.Value;
                    bound.position = translation.Value;
                })
                .ScheduleParallel(Dependency);
        }
    } 
}
