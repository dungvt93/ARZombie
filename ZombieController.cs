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
    private const float DIFF_TO_SEEK = 0.2f;
    private float speed;
    private Vector3? autoRunLocation;
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
                    GetComponent<Animator>().SetBool("Walk", true);
                    //get pose of player
                    transform.LookAt(Share.player.transform.position);
                    // Lerp position.
                    Vector3 oldLocalPosition = transform.position;
                    transform.position = Vector3.Lerp(
                        oldLocalPosition, Share.player.transform.position, Time.deltaTime * speed);
                }
                else
                {
                    //seek when near player
                    if ((Share.player.transform.position - transform.position).magnitude < DIFF_TO_SEEK)
                    {
                        // reset automove
                        autoRunLocation = null;
                        GetComponent<Animator>().SetBool("Walk", true);
                        //get pose of player
                        transform.LookAt(Share.player.transform.position);
                        // Lerp position.
                        Vector3 oldLocalPosition = transform.position;
                        transform.position = Vector3.Lerp(
                            oldLocalPosition, Share.player.transform.position, Time.deltaTime * speed);
                    }
                    else
                    {
                        //Auto move
                        if (autoRunLocation != null)
                        {
                            transform.LookAt(autoRunLocation.Value);
                            GetComponent<Animator>().SetBool("Walk", true);
                            Vector3 oldLocalPosition = transform.position;
                            transform.position = Vector3.Lerp(
                                oldLocalPosition, autoRunLocation.Value, Time.deltaTime * speed);
                            //reset when arrived
                            if ((transform.position - autoRunLocation.Value).magnitude < Share.stopAnimationRun)
                                autoRunLocation = null;
                        }
                        else
                        {
                            GetComponent<Animator>().SetBool("Walk", false);
                            //random for start automove
                            if (UnityEngine.Random.Range(0, 10) == 0)
                            {
                                Debug.Log(UnityEngine.Random.Range(0, 10));
                                autoRunLocation = MainController.positionInPlane;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }
    void OnCollisionEnter(Collision collisionInfo)
    {
        try
        {
            if (collisionInfo.gameObject.tag == "Zombie")
            {
                autoRunLocation = null;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        try
        {
            if (other.tag == "Zombie")
            {
                autoRunLocation = null;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
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
