using UnityEngine;

public class ItemDropSpawner : MonoBehaviour
{
    [Header("아이템 프리팹")]
    [SerializeField]
    private ItemPickup itemPickupPrefab;

    [Header("아이템 생성 위치")]
    [SerializeField]
    private Transform dropPoint;

    public bool TrySpawn(ItemData itemData)
    {
        Debug.Log(
            $"TrySpawn 실행됨 / 아이템: " +
            $"{(itemData == null ? "null" : itemData.itemName)}",
            this
        );

        if (itemData == null)
        {
            Debug.LogError(
                "아이템 생성 실패: 전달된 ItemData가 null입니다.",
                this
            );
            return false;
        }

        if (itemPickupPrefab == null)
        {
            Debug.LogError(
                "아이템 생성 실패: Item Pickup Prefab이 비어 있습니다.",
                this
            );
            return false;
        }

        if (dropPoint == null)
        {
            Debug.LogError(
                "아이템 생성 실패: Drop Point가 비어 있습니다.",
                this
            );
            return false;
        }

        ItemPickup spawnedPickup = Instantiate(
            itemPickupPrefab,
            dropPoint.position,
            Quaternion.identity
        );

        if (spawnedPickup == null)
        {
            Debug.LogError(
                "아이템 생성 실패: Instantiate 결과가 null입니다.",
                this
            );
            return false;
        }

        spawnedPickup.SetItemData(itemData);

        Debug.Log(
            $"{itemData.itemName} 생성 성공",
            spawnedPickup
        );

        return true;
    }
}