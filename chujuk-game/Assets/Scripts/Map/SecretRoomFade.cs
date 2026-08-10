using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapCollider2D))]
public class SecretRoomFade : MonoBehaviour
{
    [Header("페이드 설정")]
    [SerializeField]
    private float fadeDuration = 0.5f;

    [SerializeField]
    [Range(0f, 1f)]
    private float hiddenAlpha = 0f;

    private Tilemap tilemap;

    private Coroutine fadeCoroutine;

    private readonly HashSet<Collider2D> playerColliders =
        new HashSet<Collider2D>();

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();

        TilemapCollider2D tilemapCollider =
            GetComponent<TilemapCollider2D>();

        tilemapCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerColliders.Add(other);

        if (playerColliders.Count == 1)
        {
            StartFade(hiddenAlpha);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        playerColliders.Remove(other);

        if (playerColliders.Count == 0)
        {
            StartFade(1f);
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") ||
               other.transform.root.CompareTag("Player");
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine =
            StartCoroutine(FadeTilemap(targetAlpha));
    }

    private IEnumerator FadeTilemap(float targetAlpha)
    {
        Color startColor = tilemap.color;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsedTime / fadeDuration
                );

            Color color = tilemap.color;

            color.a =
                Mathf.Lerp(
                    startColor.a,
                    targetAlpha,
                    t
                );

            tilemap.color = color;

            yield return null;
        }

        Color finalColor = tilemap.color;
        finalColor.a = targetAlpha;
        tilemap.color = finalColor;

        fadeCoroutine = null;
    }
}