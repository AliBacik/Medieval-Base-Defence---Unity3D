using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public static PlayerBehaviour Instance;

    private Transform m_Target;


    public Animator m_Animator;
    public Transform m_Bow;
    public Transform m_FirePosition;
    public Transform m_RotatablePlayer;
    public LayerMask EnemyLayerMask;

    public float detectionRadius = 10f;
    public float capsuleHeight = 2f;
    public float arrowFlightDuration = 0.2f;
    public float m_attackCooldown = 0.5f;
    float time = 0f;

    [SerializeField] private int m_CollectedCoin = 0;
    [SerializeField] private int m_CollectedWoods = 0;
    [SerializeField] private int currentArrowIndex = 0;
    private Quaternion m_DefaultRotation;

    private readonly Vector3 DefaultLook = new Vector3(0f, 0f, 0f);
    private readonly Vector3 m_BowAnimVectors = new Vector3(0.2f, 0.2f, 0.2f);

    //Arrow Pool
    public List<GameObject> ArrowPool = new List<GameObject>();
    public List<TrailRenderer> ArrowTrails = new List<TrailRenderer>();

    //Golds
    public List<GameObject> Golds = new List<GameObject>();
    public Transform m_GoldDefaultPosition;

    //Woods
    public List<GameObject> Woods = new List<GameObject>();
    public Transform m_WoodDefaultPosition;

    [SerializeField] private bool m_IsLooking = false;

    [SerializeField] private bool m_targetLocked = false;

    //Sound
    public AudioSource m_BowSound;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        m_DefaultRotation = m_RotatablePlayer.localRotation;
    }
    void Update()
    {

        if (m_targetLocked == false)
        {
            FindTarget();
            LookAtEnemy();
        }
        else
        {
            
            if (m_Target == null)
            {
                m_targetLocked = false;
                return;
            }

            EnemyBehaviour component = GameManager.Instance.Enemies.Find(x => x.transform == m_Target);
            if (component == null || component.ReturnHP() <= 0)
            {
                m_Target = null;
                m_targetLocked = false;
                return;
            }

            
            float sqrDist = (m_Target.position - transform.position).sqrMagnitude;
            if (sqrDist > detectionRadius * detectionRadius)
            {
                m_Target = null;
                m_targetLocked = false;
                return;
            }

            AttackToTarget();
            LookAtEnemy();
        }
    }


    void AttackToTarget()
    {
        time += Time.deltaTime;

        if (time >= m_attackCooldown)
        {
            FireArrow();
            m_BowSound.Play();
            time = 0f;
        }
    }

    void FindTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, EnemyLayerMask);

        float closestSqrDistance = Mathf.Infinity;
        Transform closestTarget = null;

        Vector3 playerPos = transform.position;

        foreach (Collider col in colliders)
        {
            float sqrDist = (col.transform.position - playerPos).sqrMagnitude;

            if (sqrDist < closestSqrDistance)
            {
                closestSqrDistance = sqrDist;
                closestTarget = col.transform;
            }
        }

        m_Target = closestTarget;
        
        // Debug
        if (m_Target != null)
        {
            Debug.DrawLine(transform.position, m_Target.position, Color.red);
            m_targetLocked = true;
        }

    }

    void LookAtEnemy()
    {
        if (m_Target == null)
        {
            if (m_IsLooking)
            {
                m_RotatablePlayer.DOKill();
                m_RotatablePlayer.DOLocalRotate(DefaultLook, 0.5f, RotateMode.Fast);
                m_IsLooking = false;
            }
            return;
        }

        
        if (!m_IsLooking)
            m_IsLooking = true;

        Vector3 direction = m_Target.position - m_RotatablePlayer.position;

        float rawYAngle = Quaternion.LookRotation(direction).eulerAngles.y;

        float targetYAngle = Mathf.DeltaAngle(0f, rawYAngle);

        //targetYAngle = Mathf.Clamp(targetYAngle, -80f, 80f);

        Vector3 newEuler = new Vector3(0f, targetYAngle, 0f);

        m_RotatablePlayer.DOKill();
        m_RotatablePlayer.DORotate(newEuler, 0.5f, RotateMode.Fast);

    }

    void FireArrow()
    {
        if (m_Target == null) return;

        EnemyBehaviour component = GameManager.Instance.Enemies.Find(t => t.transform == m_Target);
        if (component == null || component.ReturnHP() <= 0 || !component.gameObject.activeInHierarchy)
        {
            m_targetLocked = false;
            m_Target = null;
            return;
        }

        GameObject Arrow = GetArrowFromPool();
        if (Arrow == null) return;

        int index = ArrowPool.IndexOf(Arrow);
        if (index != -1)
        {
            ArrowTrails[index].Clear();
        }

        Arrow.transform.position = m_FirePosition.position;
        Arrow.transform.rotation = Quaternion.identity;


        Arrow.SetActive(true);
        m_Animator.SetTrigger("Attack");
        BowAnimation();

        StartCoroutine(FollowArrowToPosition(Arrow, m_Target.position, arrowFlightDuration));
    }

    IEnumerator FollowArrowToPosition(GameObject arrowObj, Vector3 targetPosition, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = arrowObj.transform.position;
        Vector3 midPoint = (startPos + targetPosition) / 2f + Vector3.up * 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentPos = Vector3.Lerp(
                Vector3.Lerp(startPos, midPoint, t),
                Vector3.Lerp(midPoint, targetPosition+Vector3.up * 1.6f, t),
                t
            );

            arrowObj.transform.position = currentPos;
            arrowObj.transform.LookAt(targetPosition);

            yield return null;
        }

        arrowObj.SetActive(false);

    }

    void BowAnimation()
    {
        m_Bow.DOKill(); 

        m_Bow.DOPunchScale(m_BowAnimVectors, 0.3f, 1, 0.5f);
    }
    GameObject GetArrowFromPool()
    {
        int poolSize = ArrowPool.Count;

        for (int i = 0; i < poolSize; i++)
        {
            int index = (currentArrowIndex + i) % poolSize;

            if (ArrowPool[index] != null && !ArrowPool[index].activeInHierarchy)
            {
                currentArrowIndex = (index + 1) % poolSize;
                return ArrowPool[index];
            }

        }

        return null;
    }

    public int ReturnCoinAmount()
    {
        return m_CollectedCoin;
    }

    public int ReturnWoodAmount()
    {
        return m_CollectedWoods;
    }
    public void AddCoin(int amount)
    {
        m_CollectedCoin += amount;
        InGameUIManagement.Instance.UpdateCoinText(m_CollectedCoin); //UI
    }

    public void AddWood(int amount)
    {
        m_CollectedWoods += amount;
        InGameUIManagement.Instance.UpdateWoodText(m_CollectedWoods); //UI
    }

    public void RemoveWood()
    {
        m_CollectedWoods--;
        InGameUIManagement.Instance.UpdateWoodText(m_CollectedWoods); //UI
    }

    public void SpendCoinToTower(Transform towerTransform)
    {
        if (m_CollectedCoin <= 0) return;

        m_CollectedCoin--;
        InGameUIManagement.Instance.UpdateCoinText(m_CollectedCoin); //UI

        GameObject goldObj = null;

        foreach (var c in Golds)
        {
            if (!c.gameObject.activeInHierarchy)
            {
                goldObj = c.gameObject;
                break;
            }
        }

        if (goldObj == null) return;

        goldObj.SetActive(true);

        goldObj.transform.position = m_GoldDefaultPosition.position;

        
        Vector3 start = goldObj.transform.position;
        Vector3 end = towerTransform.position;

        
        Vector3 top = (start + end) / 2f + Vector3.up * 4.5f;

       
        Vector3[] path = new Vector3[] { top, end };

        goldObj.transform.DOPath(path, 0.4f, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                goldObj.SetActive(false); 
            });

    }

    public void SpendWoodToCastle(Transform castleTransform)
    {
        if (m_CollectedWoods <= 0) return;

        m_CollectedWoods--;
        InGameUIManagement.Instance.UpdateWoodText(m_CollectedWoods); //UI

        GameObject woodObj = null;

        foreach (var c in Woods)
        {
            if (!c.gameObject.activeInHierarchy)
            {
                woodObj = c.gameObject;
                break;
            }
        }

        if (woodObj == null) return;

        woodObj.SetActive(true);

        woodObj.transform.position = m_WoodDefaultPosition.position;

        woodObj.transform.LookAt(Camera.main.transform);

        Vector3 start = woodObj.transform.position;
        Vector3 end = castleTransform.position;


        Vector3 top = (start + end) / 2f + Vector3.up * 4.5f;


        Vector3[] path = new Vector3[] { top, end };

        woodObj.transform.DOPath(path, 0.4f, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                woodObj.SetActive(false);
            });

    }
}
