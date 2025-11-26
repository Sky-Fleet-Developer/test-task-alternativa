using System.Collections.Generic;
using UnityEngine;

namespace MyTestTask.Misc.View
{
    public static class TweenUtility
    {
        public static IEnumerable<float> Tween(float duration)
        {
            float startT = Time.time;

            while (startT + duration > Time.time)
            {
                float t = (Time.time - startT) / duration;
                float sinus = Mathf.Sin(-1.57f + t * 3.14f) * 0.5f + 0.5f;
                float pow = Mathf.Pow(sinus, 3);
                float pow2 = 1 - Mathf.Pow(1 - pow, 3);
                yield return pow2;
            }
        }
    }
}