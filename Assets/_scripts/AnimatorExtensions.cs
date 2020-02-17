using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimatorExtensions {
    public static bool onAnimationComplete(this Animator animator, string animName, float duration) {
        int animHash = Animator.StringToHash(animName);

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0); 
        return info.shortNameHash == animHash && info.normalizedTime >= duration;
    }
}
