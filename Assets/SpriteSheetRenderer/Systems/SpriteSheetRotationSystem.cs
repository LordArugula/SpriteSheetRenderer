using Unity.Entities;

namespace ECSSpriteSheetAnimation
{
    [UpdateInGroup(groupType: typeof(SpriteSheetPreperationGroup))]
    public class SpriteSheetRotationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Dependency = Entities
                .WithBurst()
                .ForEach((ref SpriteMatrix renderData, in Rotation2D rotation) =>
                {
                    renderData.matrix.z = rotation.angle;
                })
                .ScheduleParallel(Dependency);
        }
    } 
}
