using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JewelryGenerator : MonoBehaviour
{
    //プレハブの配列
    public GameObject[] Jewelry;

    // Start is called before the first frame update
    void Start()
    {
        //プレハブの中から一つ指定
        int num = Random.Range(0, Jewelry.Length);

        //それを生成
        Instantiate(Jewelry[num], transform.position, transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
