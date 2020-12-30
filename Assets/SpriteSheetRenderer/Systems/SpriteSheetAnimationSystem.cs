using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

namespace ECSSpriteSheetAnimation
{
    public class SpriteSheetAnimationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;
            Dependency = Entities
                .WithBurst()
                .ForEach((ref SpriteSheetAnimation animation, ref SpriteIndex spriteIndex) =>
                {
                    float timePerFrame = 1 / animation.framesPerSecond;

                    switch (animation.playMode)
                    {
                        case PlayMode.Once:
                        {
                            animation.elapsedTime += deltaTime;
                            float frameTime = animation.elapsedTime % timePerFrame;
                            int elapsedFrames = (int)(animation.elapsedTime / timePerFrame);

                            animation.elapsedTime = frameTime;
                            spriteIndex.Value += elapsedFrames;
                            if (spriteIndex.Value >= animation.frameCount)
                            {
                                animation.playMode = PlayMode.None;
                                spriteIndex.Value = animation.frameCount - 1;
                            }
                            break;
                        }
                        case PlayMode.Loop:
                        {
                            animation.elapsedTime += deltaTime;
                            float frameTime = animation.elapsedTime % timePerFrame;
                            int elapsedFrames = (int)(animation.elapsedTime / timePerFrame);

                            animation.elapsedTime = frameTime;
                            spriteIndex.Value = (spriteIndex.Value + elapsedFrames) % animation.frameCount;
                            break;
                        }
                        case PlayMode.PingPong:
                        {
                            animation.elapsedTime += deltaTime;
                            float t = animation.elapsedTime * animation.framesPerSecond;
                            int l = animation.frameCount * 2;
                            t = math.clamp(t - math.floor(t / l) * l, 0, l);
                            spriteIndex.Value = (int)(animation.frameCount - math.abs(t - animation.frameCount));
                            break;
                        }
                    }
                })
                .ScheduleParallel(Dependency);
        }
    }
}
