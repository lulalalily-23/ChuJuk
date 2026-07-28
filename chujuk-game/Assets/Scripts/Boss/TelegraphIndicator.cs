using UnityEngine;

// 공격 예고(텔레그래프) 표시를 담당하는 싱글톤.
// 씬에 빈 오브젝트를 만들어 이 스크립트를 붙이고, 사각형/원형 프리팹을 인스펙터에서 연결해서 사용.
public class TelegraphIndicator : MonoBehaviour
{
    public static TelegraphIndicator Instance { get; private set; }
    public GameObject squarePrefab;
    public GameObject circlePrefab;

    void Awake()
    {
        if (Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowCircle(Vector2 position, float radius, float duration)
    {
        GameObject indicator = Instantiate(circlePrefab, position, Quaternion.identity);
        indicator.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
        Destroy(indicator, duration);
    }

    public void ShowSquare(Vector2 position, Vector2 size, float duration, float rotationDegrees = 0f)
    {
        GameObject indicator = Instantiate(squarePrefab, position, Quaternion.Euler(0f, 0f, rotationDegrees));
        indicator.transform.localScale = new Vector3(size.x, size.y, 1f);
        Destroy(indicator, duration);
    }
    
}
