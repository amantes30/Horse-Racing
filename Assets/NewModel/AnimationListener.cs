using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimationListener : MonoBehaviour
{


    //
    //[SerializeField] private mStaticThings player;

    public Animator am;

    // Start is called before the first frame update
    public void Start()
    {
        am = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void Update()
    {

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Debug.Log(x + ", " + y);

        if (y != 0)
        {

            am.SetBool("iswalking", true);
            if (Input.GetKey(KeyCode.LeftShift))
            {
                am.SetBool("Jump", false);
                am.SetBool("isrunning", true);

            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                am.SetBool("Jump", false);
                am.SetBool("isrunning", false);

            }

        }

        else
        {
            am.SetBool("Jump", false);
            am.SetBool("isrunning", false);
            am.SetBool("iswalking", false);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            am.SetBool("Jump", true);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            am.SetBool("Jump", false);
        }

    }

    void LoadAvatar(mStaticThings p)
    {

    }
}
