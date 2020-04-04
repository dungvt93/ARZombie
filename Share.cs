using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;
public class Share
{
    public volatile static float k_PositionSpeed = 1.0f;
    public volatile static bool isStop = false;
    public volatile static GameObject player;
    public volatile static List<GameObject> displayingStarList;
    public volatile static List<GameObject> displayingEnemyList;
    public volatile static int score = 0;
    public const int SCORE_TO_ZOMBIE_QUANTITY = 5;
    public volatile static GameModes mode = GameModes.RunGame;
    public enum GameModes
    {
        RunGame,
        KillGame
    }

    //ads
    public static string appId = "ca-app-pub-2286036981511088~8371056564";
    public static void RequestBanner()
    {
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";
        //real
        // string adUnitId = "ca-app-pub-2286036981511088/4770916420";

        // Create a 320x50 banner at the top of the screen.
        var banner = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the banner with the request.
        banner.LoadAd(request);
    }

    public static InterstitialAd interstitial;
    public static void RequestInterstitial()
    {
        string adUnitId = "ca-app-pub-3940256099942544/1033173712";
        //real
        // string adUnitId = "ca-app-pub-2286036981511088/3653878510";

        // Initialize an InterstitialAd.
        interstitial = new InterstitialAd(adUnitId);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        interstitial.LoadAd(request);
    }
}