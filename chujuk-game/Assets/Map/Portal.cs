using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public string nextSceneName;

    private bool playerInPortal;

    private void Update()
    {
        if (playerInPortal && Input.GetKeyDown(KeyCode.F))
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        // 이벤트 중복 방지
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 새 Scene의 Player 찾기
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError(
                "[Portal] 새 Scene에서 Player를 찾을 수 없습니다."
            );

            return;
        }

        // 새 Scene의 SpawnPoint 찾기
        GameObject spawnPoint =
            GameObject.Find("SpawnPoint");

        if (spawnPoint == null)
        {
            Debug.LogError(
                "[Portal] 새 Scene에 SpawnPoint가 없습니다."
            );

            return;
        }

        // Player를 SpawnPoint 위치로 이동
        player.transform.position =
            spawnPoint.transform.position;

        Debug.Log(
            "[Portal] Player를 SpawnPoint 위치로 이동했습니다."
        );
    }

    private void OnTriggerEnter2D(
        Collider2D collision
    )
    {
        if (collision.CompareTag("Player"))
        {
            playerInPortal = true;

            Debug.Log("포탈 진입");
        }
    }

    private void OnTriggerExit2D(
        Collider2D collision
    )
    {
        if (collision.CompareTag("Player"))
        {
            playerInPortal = false;

            Debug.Log("포탈 나감");
        }
    }
}