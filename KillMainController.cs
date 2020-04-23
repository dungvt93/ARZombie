using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GoogleMobileAds.Api;
public class KillMainController : MonoBehaviour
{
    #region unity editor object
    public Camera camera;
    public GameObject RestartBtn;
    public GameObject BackToMenuBtn;
    public GameObject HelpBtn;
    public Text ScoreBoard;
    #endregion


    private bool m_IsQuitting = false;
    private const float k_PrefabRotation = 180.0f;
    private List<string> prefabs = new List<string>() { "Prefabs/Zombie" };
    private Dictionary<GameObject, int> displayingObject;
    // Start is called before the first frame update
    void Start()
    {
        //ads 
        try
        {
            Share.RequestInterstitial();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        Share.isStop = false;
        RestartBtn.SetActive(false);
        BackToMenuBtn.SetActive(false);
        Share.mode = Share.GameModes.KillGame;
        Share.score = 0;
        displayingObject = new Dictionary<GameObject, int>();
        Share.player = new GameObject("virtual player");
        Share.player.transform.position = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            _UpdateApplicationLifecycle();

            if (Share.isStop)
            {
                RestartBtn.SetActive(true);
                BackToMenuBtn.SetActive(true);
                HelpBtn.SetActive(false);
                return;
            }

            if (displayingObject.Count <= Share.score / Share.SCORE_TO_ZOMBIE_QUANTITY)
            {
                //spawn zombie
                foreach (var prefab in prefabs)
                {
                    var tempObject = (GameObject)Resources.Load(prefab, typeof(GameObject));
                    var instance = SpawnRandomEveryWhere(tempObject);
                    if (instance != null)
                    {
                        instance.transform.Rotate(0, k_PrefabRotation, 0, Space.Self);
                        displayingObject.Add(instance, 0);
                    }
                }
            }
            //Touch process
            Touch touch;
            if (Input.touchCount >= 1 && (touch = Input.GetTouch(0)).phase == TouchPhase.Began)
            {
                // Should not handle input if the player is pointing on UI.
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return;
                }

                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        //time of touch
                        displayingObject[hit.collider.gameObject] += 1;
                        if (displayingObject[hit.collider.gameObject] >= 0)
                        {
                            ZombieDieAction(hit.collider.gameObject);
                        }
                        else
                        {
                            hit.collider.gameObject.GetComponent<Animator>().SetTrigger("Reaction");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void ZombieDieAction(GameObject zombieObject)
    {
        zombieObject.GetComponent<Animator>().SetTrigger("Die");
        displayingObject.Remove(zombieObject);
        Destroy(zombieObject, 0.5f);
        Share.score += 1;
    }
    public GameObject SpawnRandomEveryWhere(GameObject gameObject)
    {
        Vector3 randomPoint = Vector3.zero;
        randomPoint = camera.ScreenToWorldPoint(
                    new Vector3(UnityEngine.Random.Range(10, Screen.width - 10),
                     UnityEngine.Random.Range(0, Screen.height - 50),
                     UnityEngine.Random.Range(2, 7))
                    );
        var createdObject = Instantiate(gameObject, randomPoint, camera.transform.rotation);
        createdObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        return createdObject;
    }
    public void RestartGame()
    {
        SceneManager.LoadScene("KillMain");
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void HelpButton()
    {
        List<GameObject> zombieList = new List<GameObject>(displayingObject.Keys);
        for (var i = 0; i < zombieList.Count; i++)
        {
            ZombieDieAction(zombieList[i]);
        }
        HelpBtn.SetActive(false);
    }
    /// <summary>
    /// Check and update the application lifecycle.
    /// </summary>
    private void _UpdateApplicationLifecycle()
    {
        //update score
        ScoreBoard.text = Share.score.ToString();

        //end game
        foreach (var temp in displayingObject)
        {
            if ((Share.player.transform.position - temp.Key.transform.position).magnitude < 0.3f)
            {
                Share.isStop = true;
                Share.interstitial.Show();
            }
        }

        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to
        // appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage(
                "ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit()
    {
        Application.Quit();
    }
    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity =
            unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject =
                    toastClass.CallStatic<AndroidJavaObject>(
                        "makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
