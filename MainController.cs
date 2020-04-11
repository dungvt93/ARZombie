using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    #region unity editor object
    public Camera camera;
    public GameObject RestartBtn;
    public GameObject BackToMenuBtn;
    public Text ScoreBoard;
    #endregion
    public static Vector3? positionInPlane;
    private int MAX_STAR_QUANTITY = 1;
    private bool m_IsQuitting = false;

    /// <summary>
    /// The rotation in degrees need to apply to prefab when it is placed.
    /// </summary>
    private const float k_PrefabRotation = 180.0f;
    private List<string> prefabEnemyList = new List<string>() { "Prefabs/Zombie" };
    private List<string> prefabPlayerList = new List<string>() { "Prefabs/Player" };
    private string starPrefab = "Prefabs/Star";
    private DetectedPlane gamePlane;
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

        Share.displayingEnemyList = new List<GameObject>();
        Share.displayingStarList = new List<GameObject>();
        Share.isStop = false;
        Share.score = 0;
        RestartBtn.SetActive(false);
        BackToMenuBtn.SetActive(false);
        GetComponent<ARCoreSessionConfig>().PlaneFindingMode = DetectedPlaneFindingMode.Horizontal;
    }
    public void Update()
    {
        try
        {
            _UpdateApplicationLifecycle();
            // Check that motion tracking is tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }

            if (Share.isStop)
            {
                RestartBtn.SetActive(true);
                BackToMenuBtn.SetActive(true);
                return;
            }

            if (Share.player != null)
            {
                //set auto move position
                positionInPlane = GetPositionOnPlane();
                //spawn zombie
                if (Share.displayingEnemyList.Count <= Share.score / Share.SCORE_TO_ZOMBIE_QUANTITY)
                {
                    foreach (var prefabName in prefabEnemyList)
                    {
                        var tempObject = (GameObject)Resources.Load(prefabName, typeof(GameObject));
                        var displayingObject = SpawnRandomOnPlane(tempObject);
                        if (displayingObject != null)
                            Share.displayingEnemyList.Add(displayingObject);
                    }
                }
                //spawn star
                if (Share.displayingStarList.Count < MAX_STAR_QUANTITY)
                {
                    var tempObject = (GameObject)Resources.Load(starPrefab, typeof(GameObject));
                    var displayingObject = SpawnRandomOnPlane(tempObject);
                    if (displayingObject != null)
                    {
                        displayingObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                        Share.displayingStarList.Add(displayingObject);
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
                //has player
                if (Share.player != null)
                {
                    //move player
                    TrackableHit trackableHit;
                    TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                        TrackableHitFlags.FeaturePointWithSurfaceNormal;

                    if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out trackableHit))
                    {
                        if (trackableHit.Trackable is DetectedPlane)
                        {
                            // Use hit pose and camera pose to check if hittest is from the
                            // back of the plane, if it is, no need to create the anchor.
                            if (Vector3.Dot(Camera.main.transform.position - trackableHit.Pose.position,
                                    trackableHit.Pose.rotation * Vector3.up) < 0)
                            {
                                Debug.Log("Hit at back of the current DetectedPlane");
                            }
                            else
                            {
                                if (CheckIsGamePlane(trackableHit))
                                {
                                    Share.player.GetComponent<PlayerController>().targetPosition = trackableHit.Pose.position;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //create player
                    TrackableHit trackableHit;
                    TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                        TrackableHitFlags.FeaturePointWithSurfaceNormal;

                    if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out trackableHit))
                    {
                        if (trackableHit.Trackable is DetectedPlane)
                        {
                            // Use hit pose and camera pose to check if hittest is from the
                            // back of the plane, if it is, no need to create the anchor.
                            if (Vector3.Dot(Camera.main.transform.position - trackableHit.Pose.position,
                                    trackableHit.Pose.rotation * Vector3.up) < 0)
                            {
                                Debug.Log("Hit at back of the current DetectedPlane");
                            }
                            else
                            {
                                gamePlane = trackableHit.Trackable as DetectedPlane;
                                //create player
                                foreach (var prefabName in prefabPlayerList)
                                {
                                    var prefab = (GameObject)Resources.Load(prefabName, typeof(GameObject));
                                    Share.player = SpawnOnHit(prefab, trackableHit);
                                }

                            }
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
    public void RestartGame()
    {
        Share.interstitial.Show();
        SceneManager.LoadScene("Main");
    }
    public void BackToMenu()
    {
        Share.interstitial.Show();
        SceneManager.LoadScene("Menu");
    }
    public GameObject SpawnOnHit(GameObject prefab, TrackableHit hit)
    {
        if (CheckIsGamePlane(hit))
        {
            // Instantiate prefab at the hit pose.
            var gameObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

            // Compensate for the hitPose rotation facing away from the raycast (i.e.
            // camera).
            gameObject.transform.Rotate(0, k_PrefabRotation, 0, Space.Self);

            // Create an anchor to allow ARCore to track the hitpoint as understanding of
            // the physical world evolves.
            var anchor = hit.Trackable.CreateAnchor(hit.Pose);
            gameObject.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
            // Make game object a child of the anchor.
            gameObject.transform.parent = anchor.transform;

            return gameObject;
        }
        return null;
    }

    public GameObject SpawnRandomOnPlane(GameObject gameObject)
    {
        //Detected Plane
        List<DetectedPlane> m_NewPlanes = new List<DetectedPlane>();
        Session.GetTrackables<DetectedPlane>(m_NewPlanes, TrackableQueryFilter.All);

        if (m_NewPlanes.Count > 0)
        {
            // TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;
            Vector3 screenPos = camera.WorldToScreenPoint(
                gamePlane.CenterPose.position);
            // m_NewPlanes[UnityEngine.Random.Range(0, m_NewPlanes.Count)].CenterPose.position);
            TrackableHit trackableHit;
            if (Frame.Raycast(UnityEngine.Random.Range(screenPos.x - 1000, screenPos.x + 1000),
                                UnityEngine.Random.Range(screenPos.y - 1000, screenPos.y + 1000), raycastFilter, out trackableHit))
            {
                //check same plane
                if (CheckIsGamePlane(trackableHit))
                {
                    var createdObject = Instantiate(gameObject, trackableHit.Pose.position, trackableHit.Pose.rotation);
                    // temp.transform.Rotate(0, k_PrefabRotation, 0, Space.Self);
                    var anchor = trackableHit.Trackable.CreateAnchor(trackableHit.Pose);
                    // Make game object a child of the anchor.
                    createdObject.transform.parent = anchor.transform;
                    //set size for object
                    createdObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                    return createdObject;
                }
            }
        }
        return null;
    }
    public Vector3? GetPositionOnPlane(){
        //Detected Plane
        List<DetectedPlane> m_NewPlanes = new List<DetectedPlane>();
        Session.GetTrackables<DetectedPlane>(m_NewPlanes, TrackableQueryFilter.All);

        if (m_NewPlanes.Count > 0)
        {
            // TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;
            Vector3 screenPos = camera.WorldToScreenPoint(
                gamePlane.CenterPose.position);
            // m_NewPlanes[UnityEngine.Random.Range(0, m_NewPlanes.Count)].CenterPose.position);
            TrackableHit trackableHit;
            if (Frame.Raycast(UnityEngine.Random.Range(screenPos.x - 1000, screenPos.x + 1000),
                                UnityEngine.Random.Range(screenPos.y - 1000, screenPos.y + 1000), raycastFilter, out trackableHit))
            {
                //check same plane
                if (CheckIsGamePlane(trackableHit))
                {
                    return trackableHit.Pose.position;
                }
            }
        }
        return null;
    }
    private bool CheckIsGamePlane(TrackableHit hit)
    {
        var tempPlane = hit.Trackable as DetectedPlane;
        return tempPlane == gamePlane && tempPlane.PlaneType != DetectedPlaneType.Vertical;
    }
    private GameObject SetObjectRotation(GameObject gameObject)
    {
        //set object face to camera
        gameObject.transform.LookAt(Share.player.transform);
        // Quaternion objectRotation = gameObject.transform.rotation;
        // objectRotation.y = player.transform.rotation.y;
        // gameObject.transform.rotation = objectRotation;
        return gameObject;
    }

    /// <summary>
    /// Check and update the application lifecycle.
    /// </summary>
    private void _UpdateApplicationLifecycle()
    {
        //update score
        ScoreBoard.text = Share.score.ToString();

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