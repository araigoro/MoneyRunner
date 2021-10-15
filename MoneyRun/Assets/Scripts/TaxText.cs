using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaxText : MonoBehaviour
{
    

    public Text taxtext;

    // Start is called before the first frame update
    void Start()
    {
        //距離が進むつれ徴収額を増額して表示する
        taxtext.text = "- "+(100000 + (PlayerPrefs.GetInt("tax",0)-1) * 50000).ToString("N0");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
