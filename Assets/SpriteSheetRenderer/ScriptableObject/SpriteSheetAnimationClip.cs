using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSpriteSheetAnimation
{
    /// <summary>
    /// Animation clips are ScriptableObjects that used to store 
    /// basic animation data for readonly use at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Sprite Sheet Renderer/Animation Clip")]
    public class SpriteSheetAnimationClip : ScriptableObject
    {
        [SerializeField]
        private string animationName;

        [SerializeField]
        private Sprite[] sprites;

        [SerializeField]
        private float framesPerSecond = 12;

        [SerializeField]
        private PlayMode playMode = PlayMode.Loop;

        /// <summary>
        /// The name of the animation.
        /// </summary>
        public string AnimationName => animationName;

        /// <summary>
        /// The frames used in the animation.
        /// </summary>
        public Sprite[] Sprites => sprites;

        /// <summary>
        /// The number of frames in the animation.
        /// </summary>
        public int FrameCount => sprites.Length;

        /// <summary>
        /// The number of frames played per second.
        /// </summary>
        public float FramesPerSecond => framesPerSecond;

        /// <summary>
        /// The play mode defines how the animation clip is played.
        /// </summary>
        public PlayMode PlayMode => playMode;
    }

    /// <summary>
    /// The play mode defines how an animation is played.
    /// </summary>
    public enum PlayMode
    {
        /// <summary>
        /// The animation is played once.
        /// </summary>
        Once,
        /// <summary>
        /// The animation plays continuously.
        /// </summary>
        Loop,
        /// <summary>
        /// The animation reverses after completing.
        /// </summary>
        PingPong,
        /// <summary>
        /// The animation does not play.
        /// </summary>
        None,
    }
}
