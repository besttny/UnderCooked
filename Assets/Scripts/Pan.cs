using UnityEngine;

public class Pan : MonoBehaviour
{
    public Transform contentPoint;

    public GameObject currentItem;
    public float cookProgress;
    public float cookTarget;
    public GameObject cookedPrefab;

    public bool HasItem => currentItem != null;
    public bool IsDone => currentItem != null && cookProgress >= cookTarget;

    public bool TryInsert(GameObject item)
    {
        if (currentItem != null) return false;

        var cook = item.GetComponent<Cookable>();
        if (cook == null) return false;

        currentItem = item;
        cookedPrefab = cook.cookedResultPrefab;
        cookTarget = cook.cookTime;
        cookProgress = 0f;

        item.transform.SetParent(contentPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        return true;
    }

    public GameObject TakeItem()
    {
        if (currentItem == null) return null;
        if (cookProgress < cookTarget)
        {
            Debug.Log("Not cooked yet!");
            return null;
        }

        var obj = currentItem;
        obj.transform.SetParent(null);
        currentItem = null;
        cookProgress = 0f;
        cookTarget = 0f;
        cookedPrefab = null;
        return obj;
    }

    public void TickCook(float dt)
    {
        if (currentItem == null) return;
        if (cookProgress >= cookTarget) return;

        cookProgress += dt;

        if (cookProgress >= cookTarget)
        {
            Destroy(currentItem);
            currentItem = Instantiate(cookedPrefab, contentPoint.position, Quaternion.identity, contentPoint);
            foreach (var col in currentItem.GetComponentsInChildren<Collider>())
                col.enabled = false;
        }

    }
}
