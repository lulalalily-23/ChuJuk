using UnityEngine;

public class ItemTest : MonoBehaviour
{
    public ItemData testItem;


    private void Start()
    {
        Debug.Log("ItemTest 실행");

        Inventory.Instance.AddItem(testItem);
    }
}