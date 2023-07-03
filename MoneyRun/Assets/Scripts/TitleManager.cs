using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NCMB;

public class TitleManager : MonoBehaviour
{
    // Start is called before the first frame update

    public Animator anim;

    public AudioSource[] audiosource;
    public AudioClip SE;

    public Text BestScore;

    public int score;

    public Text[] highscoretext;

    /*取得したデータを格納
     * ""の中にはクラス名を入れる
     * 変数を入れている記録してるやつの名前と混同しない
     * 今回だとNameとScore
     */
    private NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("HighScore");

    void Start()
    {
        //コンポーネントの取得
        audiosource = gameObject.GetComponents<AudioSource>();

        //記録してあるスコアを変数に入れて、ベストスコアとして画面に表示
        score = PlayerPrefs.GetInt("score", 0);
        BestScore.text = score + " m";

        //取得したデータを降順に並び替え
        query.OrderByDescending("Score");
        //上から5つのみ取得
        query.Limit = 5;

        //関数に代入
        query.FindAsync((List<NCMBObject> objList, NCMBException e) =>
        {
            if (e == null)
            {
                //繰り返し処理を使って1位～5位のスコアをTOP5として表示する
                for(int i=0;i<=4;i++)
                {
                    highscoretext[i].text = objList[i]["Name"] + " : " + objList[i]["Score"].ToString() + " m" ;
                }
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //ゲームスタートボタンの処理
    public void ButtonStart()
    {
        //Unityちゃんのアニメーションを動かす
        anim.SetBool("win", true);

        //スタート音を鳴らす
        audiosource[0].PlayOneShot(SE);

        //BGMを止める
        audiosource[1].Stop();

        //シーンが急に変わらないように、FadeManagerでゆっくりシーンが変わっているように見せる
        FadeManager.Instance.LoadScene("Main", 4.0f);
    }
}
