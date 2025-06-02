using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class CoinCollection : MonoBehaviour
{
    private Vector3 DefPosition = new Vector3 (0, 3.5f, 0);
    private Vector3 playerOffset = new Vector3(0, 2.5f, 0);
    float duration = 0.4f;

    private void OnEnable()
    {
        Spread();
    }
    void Spread()
    {
        float range = Random.Range(-3f, 3f);

        transform.DOMoveX(transform.position.x + range, 0.3f).SetEase(Ease.InOutCirc).OnComplete(() =>
        {
            StartCoroutine(GoToPlayer());
        });
    }

    IEnumerator GoToPlayer()
    {
        yield return new WaitForSeconds(0.15f);

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentTarget = PlayerBehaviour.Instance.transform.position + playerOffset;
            transform.position = Vector3.Lerp(startPos, currentTarget, t);

            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        transform.position = DefPosition;
    }
}
