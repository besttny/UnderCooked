using UnityEngine;

public class PanStation : Workstation
{
    public Pan pan;

    public void Update()
    {
        // auto-detect pan on place point
        if (placePoint.childCount > 0)
            pan = placePoint.GetChild(0).GetComponent<Pan>();
        else
            pan = null;

        if (pan != null)
            pan.TickCook(Time.deltaTime);
    }
    public override void Use(GameObject player)
    {
        return; // no direct interaction
    }
}