using UnityEngine;

public class Workstation : MonoBehaviour
{
    public Transform placePoint;

    // =========================
    // PLACE ITEM INTO STATION
    // =========================
    public virtual bool TryPlaceItem(GameObject item, GameObject player)
    {
        return false;
    }

    // =========================
    // INTERACT (PRESS F)
    // =========================
    public virtual void Use(GameObject player)
    {
    }

    // =========================
    // GET PLACE POINT
    // =========================
    public Transform GetPlacePoint()
    {
        return placePoint;
    }
}
