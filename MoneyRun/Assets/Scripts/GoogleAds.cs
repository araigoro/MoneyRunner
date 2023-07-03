using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class GoogleAds : MonoBehaviour
{
    //using GoogleMobileAds.Api;

    //広告を表示するためのBannerViewクラスのインスタンス
    private BannerView bannerView;

    // Start is called before the first frame update
    void Start()
    {
        //Initialize the Google Mobile Ads SDK(初期化）
        MobileAds.Initialize(initStatus => { });
        //MobieAds.Initialize(appId)はエラー（仕様が変わった？）

        RequestBanner();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RequestBanner()
    {
        //広告ユニットID取得(テスト用）
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";

        //Create a 320×50 banner at the top of the screen
        //BannewView(広告ID,バナー（広告）の種類,位置指定）、今回は底
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

        //Create an empty ad request
        AdRequest request = new AdRequest.Builder().Build();

        //Load the banner with the requset（読み込み）
        bannerView.LoadAd(request);
    }

    private void OnDestroy()
    {
        bannerView.Destroy();
    }
}
