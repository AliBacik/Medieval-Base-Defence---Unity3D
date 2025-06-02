using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    private Transform m_Target;
    private NavMeshAgent m_Agent;
    private Animator m_Animator;

    private Vector3 scale;
    private readonly float m_stoppingDistance = 5f;
    private readonly float m_attackDistance = 26f;
    private readonly Vector3 m_DamageAnimVectors = new Vector3(0.2f, 0.2f, 0.2f);

    [SerializeField]private float m_BaseHp = 20f;
    [SerializeField]private float m_CurrentHp;

    private void Awake()
    {
        scale = transform.localScale;
        m_Animator = GetComponent<Animator>();
        m_Agent = GetComponent<NavMeshAgent>();
        m_Agent.stoppingDistance = m_stoppingDistance;
        m_CurrentHp = m_BaseHp; 
    }

    private void OnEnable()
    {
        transform.localScale = scale;

        if(m_CurrentHp< m_BaseHp)
        {
            m_CurrentHp = m_BaseHp;
        }
        
        FindTarget();
    }

    void Update()
    {
        if(m_Target != null)
        {
            Attack();
        }

        HandleAnimations();

    }

    private void OnDisable()
    {
        transform.DOKill();
        m_CurrentHp = m_BaseHp;
        transform.localScale = scale;
    }

    void FindTarget()
    {
        int random = Random.Range(0, GameManager.Instance.BaseBuildings.Count);

        GameObject obj = GameManager.Instance.BaseBuildings[random];

        m_Target=obj.transform;

        m_Agent.destination = m_Target.position;

    }

    void HandleAnimations()
    {
        if(m_Agent.velocity.magnitude > 0)
        {
            m_Animator.SetBool("Run", true);
        }
        else
        {
            m_Animator.SetBool("Run", false);
        }
    }
    void Attack()
    {
        float distance = (transform.position - m_Agent.destination).sqrMagnitude;

        if(distance < m_attackDistance)
        {
            m_Animator.SetBool("Attack", true);
        }
        else
        {
            m_Animator.SetBool("Attack", false);
        }
    }

    public void OnAttack()
    {
        PlayerBehaviour.Instance.RemoveWood();
    }

    public void GetDamage(float amount)
    {
        m_CurrentHp -= amount;
        DamageAnimation();
    

        if (m_CurrentHp <= 0)
        {
            GameManager.Instance.CoinBehavior(transform);

            gameObject.SetActive(false);

            PlayerBehaviour.Instance.AddCoin(2);
        }
    }

    void DamageAnimation()
    {
        gameObject.transform.DOKill();

        gameObject.transform.DOPunchScale(m_DamageAnimVectors, 0.3f, 1, 0.5f);
    }

    public float ReturnHP()
    {
        return m_CurrentHp;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow"))
        {
            GetDamage(20);
        }
    }
}
