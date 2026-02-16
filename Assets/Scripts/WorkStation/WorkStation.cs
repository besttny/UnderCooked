using UnityEngine;

public abstract class Workstation : MonoBehaviour
{
    public Transform placePoint;

    public virtual Transform GetPlacePoint()
    {
        return placePoint != null ? placePoint : transform;
    }
    public abstract void Use(GameObject player);
}