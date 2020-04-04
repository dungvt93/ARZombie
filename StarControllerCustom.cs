using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StarControllerCustom : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter(Collider other)
    {
        try
        {
            if (other.tag == "Player")
            {
                // collisionInfo.gameObject.GetComponent<Animator>().SetTrigger("Die");
                // collisionInfo.gameObject.GetComponent<ZombieController>().isDieFlg = true;
                // Share.displayingEnemyList.Remove(collisionInfo.gameObject);
                // Destroy(collisionInfo.gameObject,2f);

                Share.displayingStarList.Remove(gameObject);
                Destroy(gameObject);

                Share.score += 1;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
