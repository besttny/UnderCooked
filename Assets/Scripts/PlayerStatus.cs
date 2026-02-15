using UnityEngine;
using System.Collections;

public class PlayerStatus : MonoBehaviour
{
    public bool IsStunned { get; private set; }

    public void Stun(float seconds)
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(StunRoutine(seconds));
    }

    IEnumerator StunRoutine(float s)
    {
        IsStunned = true;
        yield return new WaitForSeconds(s);
        IsStunned = false;
    }
}
