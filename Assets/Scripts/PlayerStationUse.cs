using UnityEngine;

public class PlayerStationUse : MonoBehaviour
{
    public float useRadius = 2f;
    public LayerMask stationLayer;

    public void OnUseStation()
    {
        Collider[] hits =
            Physics.OverlapSphere(transform.position, useRadius, stationLayer);

        Workstation best = null;
        float bestD = float.MaxValue;

        foreach (var h in hits)
        {
            var ws = h.GetComponentInParent<Workstation>();
            if (ws == null) continue;

            float d = Vector3.Distance(transform.position, ws.transform.position);
            if (d < bestD)
            {
                bestD = d;
                best = ws;
            }
        }

        if (best != null)
            best.Use(gameObject);
    }
}