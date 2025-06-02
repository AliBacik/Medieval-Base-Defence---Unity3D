using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class TowerBuild : MonoBehaviour
{
    [SerializeField] private int m_RequiredCoin;
    [SerializeField] private int m_CurrentCoin = 0;
    public TextMeshPro m_CoinCounter;
    public GameObject m_BuildedTower;
    public GameObject m_Archer;
    public GameObject m_PreBuild;
    public ParticleSystem m_BuildedVFX;
    public Collider m_Collider;
    private Coroutine coinTransferRoutine;

    private Vector3 towerScale;
    [SerializeField] bool m_Ready = false;
    private bool isTransferring = false;

    private void Start()
    {
        towerScale = m_BuildedTower.transform.localScale;
        m_CoinCounter.text = m_RequiredCoin.ToString();
    }
    void CheckStatus()
    {
        if(m_CurrentCoin >= m_RequiredCoin && m_Ready==false)
        {
            m_Ready = true;

            //build tower
            m_BuildedTower.transform.localScale = Vector3.zero;
            m_BuildedTower.SetActive(true);

            Sequence buildSequence = DOTween.Sequence();

            buildSequence.Append(m_BuildedTower.transform.DOScale(towerScale * 1.1f, 0.35f).SetEase(Ease.OutBack)).AppendCallback(() =>
            {
                //Build vfx
                m_BuildedVFX.Play();
            }); // ilk büyüme

            buildSequence.Append(m_BuildedTower.transform.DOScale(towerScale, 0.15f)).OnComplete(() =>
            {
                
                m_Archer.SetActive(true);
            }); 

            //hide counter
            m_PreBuild.SetActive(false);
            m_CoinCounter.gameObject.SetActive(false);

            //hide collider
            m_Collider.enabled = false;
        }

    }

    IEnumerator CollectCoinFromPlayer()
    {
        isTransferring = true;

        while (m_CurrentCoin < m_RequiredCoin)
        {
            int playerCoin = PlayerBehaviour.Instance.ReturnCoinAmount();
            if (playerCoin <= 0) break;

            PlayerBehaviour.Instance.SpendCoinToTower(transform);

            m_CurrentCoin++;

            yield return new WaitForSeconds(0.05f); // coinler arasý gecikme

            int coinLeft = m_RequiredCoin - m_CurrentCoin;

            m_CoinCounter.text = coinLeft.ToString();

            m_PreBuild.transform.DOKill();

            Sequence scaleSequence = DOTween.Sequence();

            scaleSequence.Append(m_PreBuild.transform.DOScale(Vector3.one * 1.2f, 0.15f).SetEase(Ease.OutQuad));
            scaleSequence.Append(m_PreBuild.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InQuad));

            CheckStatus();
        }

        isTransferring = false;

    }

    private void OnTriggerStay(Collider other)
    {
        if (m_Ready==true) return;

        if (other.CompareTag("Player") && !isTransferring)
        {
            coinTransferRoutine = StartCoroutine(CollectCoinFromPlayer());

            CheckStatus();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && coinTransferRoutine != null)
        {
            StopCoroutine(coinTransferRoutine);
            coinTransferRoutine = null;
            isTransferring = false;
        }
    }
}
