using UnityEngine;
public enum IngredientState
{
    Raw,
    Chopped,
    Cooked,
    Burned
}

public class Ingredient : MonoBehaviour
{
    public IngredientState[] state;
    public bool canPlate = true; // can be put on plate
}
