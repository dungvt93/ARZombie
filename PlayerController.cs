using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public Vector3? targetPosition;
    public AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            if (!Share.isStop)
            {
                if (targetPosition != null)
                {
                    GetComponent<Animator>().SetBool("Run", true);
                    transform.LookAt(targetPosition.Value);
                    transform.position = Vector3.Lerp(transform.position, targetPosition.Value, Time.deltaTime * Share.k_PositionSpeed);
                    float diffLenght = (transform.position - targetPosition.Value).magnitude;

                    if (diffLenght < Share.stopAnimationRun)
                    {
                        transform.position = targetPosition.Value;
                        GetComponent<Animator>().SetBool("Run", false);
                        targetPosition = null;
                    }
                }
            } else{
                GetComponent<Animator>().SetBool("Run",false);
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
                audioSource.Play(0);
                Handheld.Vibrate();
                Share.isStop = true;
                Destroy(other.gameObject.GetComponent<Rigidbody>());
                other.gameObject.GetComponent<Animator>().SetBool("Kill",true);
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
