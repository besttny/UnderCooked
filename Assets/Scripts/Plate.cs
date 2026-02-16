using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour
{
    [Header("Current ingredients on plate")]
    public List<string> ingredients = new List<string>();

    [Header("Ingredient visuals (stacking)")]
    public List<GameObject> ingredientVisualPrefabs; // assign in inspector
    private List<GameObject> spawnedVisuals = new List<GameObject>();

    [Header("Result Prefabs")]
    public GameObject saladPrefab;
    public GameObject steakPrefab;
    public GameObject steakwsaladPrefab;
    public GameObject wrapPrefab;

    public Transform contentPoint;

    float stackHeight = 0.02f; // height per layer

    void Awake()
    {
        if (contentPoint == null)
            contentPoint = transform;
    }

    // =====================================================
    // ADD INGREDIENT
    // =====================================================
    public bool TryAddIngredient(GameObject ingredientObj)
    {
        Ingredient ingredient = ingredientObj.GetComponent<Ingredient>();
        if (ingredient == null) return false;

        string name = ingredient.ingredientName;

        if (ingredients.Contains(name))
        {
            Debug.Log("Already on plate");
            return false;
        }

        ingredients.Add(name);
        Destroy(ingredientObj);

        Debug.Log("Added to plate: " + name);

        SpawnIngredientVisual(name); // ⭐ show stacking visual
        CheckRecipe();

        return true;
    }

    // =====================================================
    // SPAWN VISUAL STACK
    // =====================================================
    void SpawnIngredientVisual(string ingredientName)
    {
        foreach (GameObject prefab in ingredientVisualPrefabs)
        {
            if (prefab.name == ingredientName)
            {
                Vector3 pos = contentPoint.position + Vector3.up * stackHeight * spawnedVisuals.Count;

                GameObject v = Instantiate(prefab, pos, Quaternion.identity, contentPoint);
                spawnedVisuals.Add(v);
                return;
            }
        }

        Debug.LogWarning("No visual prefab for " + ingredientName);
    }

    // =====================================================
    // CHECK RECIPE
    // =====================================================
    void CheckRecipe()
    {
        if (Has("CookPork") && ingredients.Count == 1)
        {
            CreateDish(steakPrefab, "Steak");
            return;
        }

        if (Has("ChopTomato") && Has("ChopLettuce") && ingredients.Count == 2)
        {
            CreateDish(saladPrefab, "Salad");
            return;
        }

        if (Has("CookPork") && Has("ChopTomato") && Has("ChopLettuce") && ingredients.Count == 3)
        {
            CreateDish(steakwsaladPrefab, "SteakWithSalad");
            return;
        }

        if (Has("CookChopPork") && Has("ChopTomato") && Has("ChopLettuce") && Has("Tortilla") && ingredients.Count == 4)
        {
            CreateDish(wrapPrefab, "Wrap");
            return;
        }
    }

    bool Has(string name)
    {
        return ingredients.Contains(name);
    }


    void CreateDish(GameObject prefab, string dishName)
    {
        Debug.Log("Recipe complete → " + dishName);

        ingredients.Clear();

        // destroy stacked visuals
        foreach (GameObject v in spawnedVisuals)
            Destroy(v);

        spawnedVisuals.Clear();

        if (prefab != null)
            Instantiate(prefab, contentPoint.position, Quaternion.identity, contentPoint);
    }
}
