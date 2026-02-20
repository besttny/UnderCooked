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

    void LateUpdate()
    {
        if (contentPoint == null) return;
        currentItem = null;
        foreach (Transform child in contentPoint)
        {
            // Ignore non-food objects
            if (child.GetComponent<Cookable>() != null)
            {
                currentItem = child.gameObject;
                break;
            }
        }
        if (currentItem == null)
        {
            cookProgress = 0f;
            cookTarget = 0f;
            cookedPrefab = null;
        }
    }

    public bool TryInsert(GameObject item)
    {
        if (currentItem != null)
        {
            Debug.Log("Rejected: pan already has item");
            return false;
        }

        var cook = item.GetComponent<Cookable>();
        if (cook == null)
        {
            Debug.Log("Rejected: item has no Cookable");
            return false;
        }

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

        GameObject obj = currentItem;

        // 🔥 DETACH
        obj.transform.SetParent(null);

        // 🔥 CLEAR ANY LEFTOVER VISUALS
        foreach (Transform child in contentPoint)
        {
            Destroy(child.gameObject);
        }

        currentItem = null;
        cookProgress = 0f;
        cookTarget = 0f;
        cookedPrefab = null;

        return obj;
    }

    public void TickCook(float dt)
    {
        if (currentItem == null || cookProgress >= cookTarget) return;

        cookProgress += dt;

        if (cookProgress >= cookTarget)
        {
            // Clear the raw meat
            Destroy(currentItem);
            
            // Using (prefab, parent) constructor is cleaner for local positioning
            currentItem = Instantiate(cookedPrefab, contentPoint); 
            
            // Reset local transform so it sits exactly where the contentPoint is
            currentItem.transform.localPosition = Vector3.zero;
            currentItem.transform.localRotation = Quaternion.identity;
            
            // Re-enable colliders so the player can grab the finished food
            foreach (var col in currentItem.GetComponentsInChildren<Collider>())
            {
                col.enabled = true; 
            }
            
            Debug.Log("Cooking Complete!");
        }
    }
}
