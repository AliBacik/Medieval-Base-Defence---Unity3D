using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class CastleArea : MonoBehaviour
{
    [SerializeField] private int m_RequiredWood;
    [SerializeField] private int m_CurrentWood = 0;

    public TextMeshPro m_WoodCounter;
    public GameObject m_BuildedBuilding;

    public GameObject m_PreBuild;
    public ParticleSystem m_BuildedVFX;
    public Collider m_Collider;
    private Coroutine coinTransferRoutine;

    private Vector3 towerScale;
    [SerializeField] bool m_Ready = false;
    private bool isTransferring = false;

    private void Start()
    {
        towerScale = m_BuildedBuilding.transform.localScale;
        m_WoodCounter.text =m_RequiredWood.ToString();
    }
    void CheckStatus()
    {
        if (m_CurrentWood >=m_RequiredWood && m_Ready == false)
        {
            m_Ready = true;

            //build tower
            m_BuildedBuilding.transform.localScale = Vector3.zero;
            m_BuildedBuilding.SetActive(true);

            Sequence buildSequence = DOTween.Sequence();

            buildSequence.Append(m_BuildedBuilding.transform.DOScale(towerScale * 1.1f, 0.35f).SetEase(Ease.OutBack)).AppendCallback(() =>
            {
                //Build vfx
                m_BuildedVFX.Play();
            }); // ilk büyüme

            buildSequence.Append(m_BuildedBuilding.transform.DOScale(towerScale, 0.15f));

            //hide counter
            m_PreBuild.SetActive(false);
            m_WoodCounter.gameObject.SetActive(false);

            //hide collider
            m_Collider.enabled = false;
        }
    }
    IEnumerator CollectWoodFromPlayer()
    {
        isTransferring = true;

        while (m_CurrentWood <m_RequiredWood)
        {
            int playerWood = PlayerBehaviour.Instance.ReturnWoodAmount();
            if (playerWood <= 0) break;

            PlayerBehaviour.Instance.SpendWoodToCastle(transform);

            m_CurrentWood++;

            yield return new WaitForSeconds(0.05f); // coinler arasý gecikme

            int woodLeft =m_RequiredWood - m_CurrentWood;

            m_WoodCounter.text = woodLeft.ToString();

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
        if (m_Ready == true) return;

        if (other.CompareTag("Player") && !isTransferring)
        {
            coinTransferRoutine = StartCoroutine(CollectWoodFromPlayer());

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
