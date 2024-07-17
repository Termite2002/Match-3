using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour
{
    private Vector3 from,
                    to;

    private float howfar;

    protected bool idle = true;

    public bool Idle
    {
        get
        {
            return idle;
        }
    }

    [SerializeField]
    private float speed = 1;

    public float Speed
    {
        get
        {
            return speed;
        }
    }

    // coroutine move from current position to new position
    public IEnumerator MoveToPostion(Vector3 targetPosition)
    {
        if (speed <= 0)
            Debug.LogWarning("Speed must be a positive number");

        from = transform.position;
        to = targetPosition;
        howfar = 0;
        idle = false;
        do
        {
            howfar += speed * Time.deltaTime;
            if (howfar > 1)
                howfar = 1;

            transform.position = Vector3.LerpUnclamped(from, to, Easing(howfar));

            yield return null;
        }
        while (howfar != 1);

        idle = true;
    }

    // coroutine move from current position to target, can chase target
    public IEnumerator MoveToTransform(Transform target)
    {
        if (speed <= 0)
            Debug.LogWarning("Speed must be a positive number");

        from = transform.position;
        to = target.position;
        howfar = 0;
        idle = false;
        do
        {
            howfar += speed * Time.deltaTime;
            if (howfar > 1)
                howfar = 1;

            to = target.position;

            transform.position = Vector3.LerpUnclamped(from, to, Easing(howfar));

            yield return null;
        }
        while (howfar != 1);

        idle = true;
    }

    private float Easing(float t)
    {
        float c1 = 1.70158f,
              c2 = c1 * 1.525f;
        return t < 0.5f
                ? (Mathf.Pow(t * 2, 2) * ((c2 + 1) * 2 * t - c2)) / 2
                : (Mathf.Pow(t * 2 - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;

        //return t * t;
    }
}
