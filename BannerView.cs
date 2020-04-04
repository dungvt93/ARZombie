using System;
using UnityEngine;
using GoogleMobileAds.Api;
public class GoogleMobileAdsDemoScript : MonoBehaviour
{
    private BannerView bannerView;
    public void Start()
    {
#if UNITY_ANDROID
            string appId = "ca-app-pub-2286036981511088~8371056564";
#elif UNITY_IPHONE
            string appId = "";
#else
            string appId = "unexpected_platform";
#endif

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);

        this.RequestBanner();
    }

    public void RequestBanner()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-2286036981511088/4770916420";
#elif UNITY_IPHONE
            string adUnitId = "";
#else
            string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the top of the screen.
        this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
    }
}