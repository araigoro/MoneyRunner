using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToonChicken : MonoBehaviour
{
    private Animator anim;

    public float movetime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        movetime += Time.deltaTime;

       if(movetime>=3&&6>movetime)
        {
            anim.SetBool("Eat", true);
        }
       else if(movetime>=6&&9>movetime)
        {
            anim.SetBool("Turn Head", true);
            anim.SetBool("Eat", false);
        }
       else if(movetime>=9)
        {
            movetime = 0.0f;
            
            anim.SetBool("Turn Head", false);
        }
                
               
    }
}
