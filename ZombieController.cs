using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GoogleARCore;

public class ZombieController : MonoBehaviour
{

    public AudioSource audioSource;
    public AudioClip generalClip;

    public bool isDieFlg = false;
    private const float DIFF_TO_SEEK = 0.4f;
    private float speed;
    // Start is called before the first frame update
    void Start()
    {
        speed = UnityEngine.Random.Range(0.1f, 1.5f);
        audioSource.clip = generalClip;
        audioSource.loop = true;
        audioSource.Play(0);
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            if (isDieFlg)
            {
                GetComponent<BoxCollider>().enabled = false;
                return;
            }

            if (!Share.isStop)
            {
                //Kill mode
                if (Share.mode == Share.GameModes.KillGame)
                {
                    //get pose of player
                    transform.LookAt(Share.player.transform.position);
                    // Lerp position.
                    Vector3 oldLocalPosition = transform.position;
                    transform.position = Vector3.Lerp(
                        oldLocalPosition, Share.player.transform.position, Time.deltaTime * speed);
                    GetComponent<Animator>().SetBool("Walk", true);
                }
                else
                {
                    //seek when near player
                    if ((Share.player.transform.position - transform.position).magnitude < DIFF_TO_SEEK)
                    {
                        //get pose of player
                        transform.LookAt(Share.player.transform.position);
                        // Lerp position.
                        Vector3 oldLocalPosition = transform.position;
                        transform.position = Vector3.Lerp(
                            oldLocalPosition, Share.player.transform.position, Time.deltaTime * speed);
                        GetComponent<Animator>().SetBool("Walk", true);
                    }
                    else
                    {
                        GetComponent<Animator>().SetBool("Walk", false);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    IEnumerator ExampleCoroutine(float seconds)
    {
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);

        //yield on a new YieldInstruction that waits for  seconds.
        yield return new WaitForSeconds(seconds);

        //After we have waited 5 seconds print the time again.
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
    }
}
