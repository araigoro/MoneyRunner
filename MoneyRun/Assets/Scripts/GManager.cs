using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using NCMB;
using System;

public class GManager : MonoBehaviour
{
    /*
     * 取得される側のスクリプトのclassの上にnamespaceが定義されている場合そこから指定しなければいけない
     * 今回だと UnityChanControlScriptWithRgidBody　にnamespace UnityChanが定義されている
     */
    public UnityChan.UnityChanControlScriptWithRgidBody unitychan;

    public Text Mileage;
    public Text PlusMileage;
    public Text Money;
    public Text Rate;
    public Text MileScore;
    public Text MileScore2;

    private int milscore;
    private int moneyscore;

    private string name;

    public AudioClip[] SE;
    public  AudioSource[] audioSource;

    public Animator AnimatorRecord;
    public Animator PanelRankUpdate;

    public GameObject NEWtext;

    //データ取得
    private NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("HighScore");

    //データを保存するためのオブジェクト生成
    private NCMBObject highscore = new NCMBObject("HighScore");

    public InputField inputField;

    public Button[] button;
  
    // Start is called before the first frame update
    void Start()
    {
        //コンポーネント取得
        audioSource = gameObject.GetComponents<AudioSource>();
        inputField = inputField.GetComponent<InputField>();

        //Unityちゃんにつけているスクリプトの変数moneyを引用
        moneyscore = unitychan.money;

        //プレイ中は必要ないのでfalseにしておく
        NEWtext.SetActive(false);   
        inputField.interactable = false;
        button[2].interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Unityちゃんのスクリプトの変数mileを引用
        milscore = unitychan.mile;

        /*ゲーム画面にスコア/所持金/レート表示
         * ToString("エヌオー）ではなく（エヌゼロ）、数値をコンマ表示する
         */
        Mileage.text = "スコア " + milscore.ToString("N0") + "m";
        Money.text = unitychan.money.ToString("N0")+"円";
        Rate.text = "レート " + unitychan.rate + " ドル/円";

        /*soundnumの数値で出す音を変える
         * ずっと鳴るといけないので0以上の時だけにしておく
         */
        if (unitychan.soundnum >= 0)
        {
            audioSource[0].PlayOneShot(SE[unitychan.soundnum]);
        }

        //ゲームオーバーになった後の処理
        if (unitychan.gameover == true)
        {
            //BGM停止
            audioSource[1].Stop();

            //スコア結果をアニメーションを使って表示するためtrueにする
            AnimatorRecord.SetBool("record", true);
            MileScore.text = milscore.ToString("N0") + " m";
            
            //過去のベストスコアを超えた場合の処理
            if(milscore>unitychan.recordscore)
            {
                //ベストスコアを上書きする
                PlayerPrefs.SetInt("score", milscore);

                //NEW!の文字を表示する
                NEWtext.SetActive(true);
            }

            //以下ランキングに関する処理
            //取得したデータを降順に並び替え
            query.OrderByDescending("Score");
            
            //その中から上から5つのみ取得（1位～5位の記録を抽出）
            query.Limit = 5;

            //取得したデータに検索をかける
            query.FindAsync((List<NCMBObject> objList, NCMBException e) =>
            {
                //検索成功時の処理
                if(e==null)
                {
                    /*スコアが5位の記録を超えていた場合の処理
                     * (int)objList[4]["Score"]じゃダメだった
                     */
                    if (milscore > System.Convert.ToInt32(objList[4]["Score"]))
                    {
                        //ランキング登録画面を表示するため、その間はボタンを推せないようにする
                        button[0].interactable = false;
                        button[1].interactable = false;

                        //コルーチンの実行
                        StartCoroutine(Delay(4.7f, () =>
                         {
                             //action内の処理
                             //ランキング登録画面を表示するアニメーションをtrueにする
                             PanelRankUpdate.SetBool("rankupdatetrue", true);

                             //名前の入力を有効にする
                             inputField.interactable = true;
                         }));

                        //登録するスコアを表示
                        MileScore2.text = milscore.ToString("N0") + " m";
                    }
                }             
            });

            //これ以降の処理を停止する
            enabled = false;
        }

    }

    //リトライボタン処理
    public void Retry()
    {
        //ゲームシーンの再ロード
        SceneManager.LoadScene("Main");
    }

    //タイトルへ戻るボタンの処理
    public void Title()
    {
        //タイトルシーンのロード
        SceneManager.LoadScene("Title");
    }

    //IEnumeratorはコルーチンを使用するためのデータ型
    private IEnumerator Delay(float time, Action action)
    {
        //time秒だけ処理を止めてからaction内の処理を行なう
        yield return new WaitForSeconds(time);
        action();
    }

    //ランキング登録処理
    public void RankButton()
    {
        //ランキング登録画面をひっこめる
        PanelRankUpdate.SetBool("rankupdatefalse", true);

        //ボタンを押せるようにする
        button[0].interactable = true;
        button[1].interactable = true;

        //名前入力はできないようにしておく
        inputField.interactable = false;
                
        //NCMBのScoreに記録して送信（保存させる）
        highscore["Score"] = milscore;
        highscore["Name"] = name;
        highscore.SaveAsync();
                 
    }

    //名前入力の処理
    public void InputText()
    {
        //nameに入力された文字列を入れる
        name = inputField.text;

        //0文字以上の時だけ登録可にする
        if (name.Length > 0)
        {
            button[2].interactable = true;
        }
        else
        {
            button[2].interactable = false;
        }
    }
}
