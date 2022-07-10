using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectController : MonoBehaviour
{
    Ray cameraRay;
    Vector3 mousePos = new Vector3();
    RaycastHit cameraHit;

    public GameObject mPrefabMouseClick;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mousePos.x = Input.mousePosition.x;
            mousePos.y = Input.mousePosition.y;
            mousePos.z = 0;
            cameraRay = Camera.main.ScreenPointToRay(mousePos);
            Debug.DrawRay(cameraRay.origin, cameraRay.direction, Color.red, 10);
            if (Physics.Raycast(cameraRay, out cameraHit, 1000))
            {
                GameObject go = cameraHit.transform.gameObject;
                Animator anim = go.GetComponent<Animator>();
                if (anim)
                {
                    if(anim.GetCurrentAnimatorStateInfo(0).IsName("A0_lunchbox_loop"))
                    {
                        Debug.Log(go.name + "click1");
                        anim.SetTrigger("click1");
                    }
                    else
                    {
                        Debug.Log(go.name + "click");
                        anim.SetTrigger("click");
                    }
                }
            }

            if (mPrefabMouseClick)
            {
                mPrefabMouseClick.GetComponent<Transform>().position = mousePos;
                mPrefabMouseClick.GetComponent<Animator>().SetTrigger("click");
            }
        }
    }
}
