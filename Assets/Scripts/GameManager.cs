using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<GameObject> BaseBuildings = new List<GameObject>();
    public List<EnemyBehaviour> Enemies = new List<EnemyBehaviour>();

    //coins
    public List<GameObject> Coins = new List<GameObject>();
    private  Vector3 offset = new Vector3(0, 3.5f, 0);

    public Transform SpawnPosition;

    private float spawnTimer = 0.25f;
    private float time = 0;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        time += Time.deltaTime;

        if(time>=spawnTimer)
        {
            GameObject enemyObj = GetEnemy(); // spawn enemy

            if(enemyObj != null)
            {
                enemyObj.transform.position = SpawnPosition.position;
                enemyObj.SetActive(true);
                time = 0;
            }  
        }
    }

    private GameObject GetEnemy()
    {
        foreach(EnemyBehaviour enemy in Enemies)
        {
            if(enemy != null && !enemy.gameObject.activeSelf)
            {
                return enemy.gameObject;   
            }
        }
        return null;
    }

    public void CoinBehavior(Transform enemy)
    {
        GameObject coin_one = null;
        GameObject coin_two = null;

        int inactiveCount = 0;

        foreach (GameObject c in Coins)
        {
            if (!c.activeInHierarchy)
            {
                if (coin_one == null)
                {
                    coin_one = c;
                }
                else if (coin_two == null)
                {
                    coin_two = c;
                    break; 
                }
            }
        }

        if (coin_one != null && coin_two != null)
        {
            

            coin_one.transform.position = enemy.position + offset;
            coin_two.transform.position = enemy.position + offset;

            coin_one.SetActive(true);
            coin_two.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Yeterli sayýda inaktif coin yok!");
        }
    }
}
