using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Vector3 diff;

    public GameObject character;
    public float followspeed;

    // Start is called before the first frame update
    void Start()
    {
        //キャラとカメラの位置の差
        diff = character.transform.position - transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        /*https://qiita.com/aimy-07/items/ad0d99191da21c0adbc3
         * Vector3.Lerp(a,b,t)
         * aとbの距離を１として割合tの位置を求める
         * 今回の場合は距離を一定の割合で縮める
         */
        transform.position = Vector3.Lerp(
            transform.position,
            character.transform.position - diff,
            Time.deltaTime * followspeed);
    }
}
