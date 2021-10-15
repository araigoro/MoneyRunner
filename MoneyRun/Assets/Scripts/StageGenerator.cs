using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageGenerator : MonoBehaviour
{
    //１ステージの全長
    const int StageSize = 30;

    //Unityちゃんの位置記録
    public Transform Character;

    //プレハブステージの配列
    public GameObject[] Stage;

    //税収のPlaneを生成できるようGameObjectに入れておく
    public GameObject TaxObject;

    //生成済みステージのリスト
    public List<GameObject> generatedStageList = new List<GameObject>();

    //原点の2ステージの番号を-1と0、その先を1,2,・・・としたとき、何番まで生成されているか
    public int generatedStageNo;

    //キャラが何番のステージに立っているか
    public int currentStageNo;

    //いくつ先までステージを生成するか
    public int numofgeneration;

    //税収のPlaneを生成した回数
    public int taxnum;
    
    // Start is called before the first frame update
    void Start()
    {
        //この時点ではデフォルトの2ステージしかないので2番まで存在している
        generatedStageNo = 1;

        //ここで最初に先のステージを更新
        UpdateStage(numofgeneration);

        //まだ一度も税収のPlaneを生成していないため0にする
        taxnum = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //以下ステージの更新タイミングの管理

        //位置をサイズで割ることで立っているステージの番号を算出
        currentStageNo = (int)(Character.position.z / StageSize);

        //進むにつれてステージが足りなくなったときの処理
        if(currentStageNo+numofgeneration>generatedStageNo)
        {
            //現在の位置から指定された数ぶん先にステージを生成
            UpdateStage(currentStageNo + numofgeneration);
        }

        //300mごとに税収のPlaneを生成するため、50m手前になった時の処理
        if(Character.position.z>(taxnum+1)*300-50)
        {
            //300mごとの地点にプレハブを生成
            Instantiate(TaxObject, new Vector3(0, 3, (taxnum + 1) * 300), Quaternion.identity);

            //生成数にプラス1する
            taxnum += 1;
        }
        //生成数を記録する
        PlayerPrefs.SetInt("tax", taxnum);
    }

    //ステージを更新するメソッド
    public void UpdateStage(int num)
    {
        //生成済みのステージ番号より後ろが指定されたら処理を飛ばす
        if (num <= generatedStageNo) return;

        //繰り返し処理でステージ生成
        for (int i = generatedStageNo + 1; i <= num;i++)
        {
            //生成したステージをstageに入れる
            GameObject stage = GenetateStage(i);

            //生成したステージをリストに追加
            generatedStageList.Add(stage);

            //古いステージを削除
            while (generatedStageList.Count > numofgeneration + 2) DestroyStage();

            //〇番まで生成完了を反映
            generatedStageNo = num;
        }
    }

    //指定の個数ステージをランダムに生成するメソッド
    public GameObject GenetateStage(int num)
    {
        //生成するステージを選ぶ
        int nextstageNo = Random.Range(0, Stage.Length);

        //生成処理をしてnextstageに入れる
        GameObject nextstage = (GameObject)Instantiate(
            Stage[nextstageNo],
            new Vector3(0, 0, num * StageSize),
            Quaternion.identity);

        //生成したオブジェクトを返す
        return nextstage;
    }

    //ステージの削除
    public void DestroyStage()
    {
        //リストの中で一番古い（一番後ろ）のステージを取得
        GameObject oldstage = generatedStageList[0];

        //リストからちゃんと消しておく
        generatedStageList.RemoveAt(0);
        Destroy(oldstage);
  
    }
}
