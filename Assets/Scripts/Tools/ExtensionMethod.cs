using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethod
{
    private const float dotThreshold = 0.5f;
    public static bool IsFacingTarget(this Transform transform, Transform target)
    {
        var vetcorToTarget = target.position - transform.position;
        vetcorToTarget.Normalize();

        float dot = Vector3.Dot(transform.forward, vetcorToTarget);

        return dot >= dotThreshold;
    }
}
