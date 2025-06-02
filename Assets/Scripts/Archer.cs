using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : MonoBehaviour
{
    private Transform m_Target;
    public Transform m_Rotatable;
    public Animator m_Animator;
    public Transform m_FirePosition;

    public LayerMask EnemyLayerMask;
    public float detectionRadius = 10f;
    public float capsuleHeight = 2f;

    public float arrowFlightDuration = 0.2f;
    public float m_attackCooldown = 0.5f;
    float time = 0f;

    public ParticleSystem m_ArcherVFX;

    [SerializeField] private int currentArrowIndex = 0;
    private readonly Vector3 DefaultLook = new Vector3(0f, -5.87f, 0f);

    //Arrow Pool
    public List<GameObject> ArrowPool = new List<GameObject>();
    public List<TrailRenderer> ArrowTrails = new List<TrailRenderer>();

    [SerializeField] private bool m_IsLooking = false;

    [SerializeField] private bool m_targetLocked = false;

    //Sound
    public AudioSource m_BowSound;

    private void OnEnable()
    {
        m_ArcherVFX.Play();
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
                m_Rotatable.DOKill();
                m_Rotatable.DOLocalRotate(DefaultLook, 0.5f, RotateMode.Fast);
                m_IsLooking = false;
            }
            return;
        }


        if (!m_IsLooking)
            m_IsLooking = true;

        Vector3 direction = m_Target.position - m_Rotatable.position;

        float rawYAngle = Quaternion.LookRotation(direction).eulerAngles.y;

        float targetYAngle = Mathf.DeltaAngle(0f, rawYAngle);

        Vector3 newEuler = new Vector3(0f, targetYAngle, 0f);

        m_Rotatable.DOKill();
        m_Rotatable.DORotate(newEuler, 0.5f, RotateMode.Fast);

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
                Vector3.Lerp(midPoint, targetPosition + Vector3.up * 1.6f, t),
                t
            );

            arrowObj.transform.position = currentPos;
            arrowObj.transform.LookAt(targetPosition);

            yield return null;
        }

        arrowObj.SetActive(false);

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
}
