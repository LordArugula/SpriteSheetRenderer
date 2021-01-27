﻿using Unity.Entities;

namespace ECSSpriteSheetAnimation
{
    public class SpriteSheetPositionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Dependency = Entities
                .WithBurst()
                .WithChangeFilter<Position2D>()
                .ForEach((ref SpriteMatrix renderData, in Position2D translation) =>
                {
                    renderData.matrix.x = translation.Value.x;
                    renderData.matrix.y = translation.Value.y;
                })
                .ScheduleParallel(Dependency);
        }
    } 
}
