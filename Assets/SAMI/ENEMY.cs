using UnityEngine;

public class ENEMY : MonoBehaviour
{
        public GameObject enemyPrefab;

void oncollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
        enemyPrefab.SetActive(true);
        }
    }

}
