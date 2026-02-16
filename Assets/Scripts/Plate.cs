using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour
{
    public Transform stackPoint;   // where ingredients appear on plate
    private List<GameObject> items = new List<GameObject>();

    public bool CanAdd() => items.Count < 5; // max stack size 

    public bool TryAddIngredient(GameObject ingredient)
    {
        var ing = ingredient.GetComponent<Ingredient>();
        if (ing == null || !ing.canPlate || !CanAdd()) return false; // not an ingredient or can't be plated

        AddIngredient(ingredient);
        return true;
    }

    public void AddIngredient(GameObject ingredient)
    {
        ingredient.transform.SetParent(stackPoint != null ? stackPoint : transform);
        ingredient.transform.localPosition = Vector3.up * (0.1f * items.Count);
        ingredient.transform.localRotation = Quaternion.identity;

        items.Add(ingredient);
    }

    public List<GameObject> GetItems() => items;
}