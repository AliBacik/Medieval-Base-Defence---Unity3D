using UnityEngine;

public class Timber : MonoBehaviour
{
    private Animator m_Animator;
    private int m_HitToGetWoods = 2;
    private int m_currentHitAmount =0;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }
    void Start()
    {
        m_Animator.SetTrigger("Timber");
    }

    public void GetWoods()
    {
        m_currentHitAmount++;

        if(m_currentHitAmount == m_HitToGetWoods)
        {
            PlayerBehaviour.Instance.AddWood(1);
            m_currentHitAmount = 0;

        }
    }
   
}
