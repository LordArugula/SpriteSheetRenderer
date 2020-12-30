﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECSSpriteSheetAnimation.Examples
{
    public class SingleSpriteSheetSpawner : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        private string spriteSheetName = "emoji";

        public Sprite[] sprites;

        public void Convert(Entity entity, EntityManager eManager, GameObjectConversionSystem conversionSystem)
        {
            // 1) Create Archetype
            EntityArchetype archetype = eManager.CreateArchetype(
                typeof(Position2D),
                typeof(Rotation2D),
                typeof(Scale),
                //required params
                typeof(SpriteIndex),
                typeof(SpriteSheetAnimation),
                typeof(SpriteSheetMaterial),
                typeof(SpriteSheetColor),
                typeof(SpriteMatrix),
                typeof(BufferHook)
            );

            // 2) Record and bake this spritesheet(only once)
            SpriteSheetManager.RecordSpriteSheet(sprites, spriteSheetName);

            int maxSprites = SpriteSheetCache.GetLength(spriteSheetName);
            var color = UnityEngine.Random.ColorHSV(.35f, .85f);

            // 3) Populate components
            List<IComponentData> components = new List<IComponentData> {
                new Position2D { Value = float2.zero },
                new Scale { Value = 15 },
                new SpriteIndex { Value = UnityEngine.Random.Range(0, maxSprites) },
                new SpriteSheetAnimation { frameCount = maxSprites, playMode = PlayMode.Loop, framesPerSecond = 10 },
                new SpriteSheetColor { color = new float4(color.r, color.g, color.b, color.a) }
            };
            // 4) Instantiate the entity
            _ = SpriteSheetManager.Instantiate(archetype, components, spriteSheetName);
        }
    }
}
