using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanZoom : MonoBehaviour
{
    Vector3 touchStart;
    public float zoomOutMin = 1;
    public float zoomOutMax = 8;

    public bool mCheckBoundary;
    public Vector3 mMaxPos;
    public Vector3 mMinPos;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            zoom(difference * 0.01f);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += direction;
            checkBoundary();
        }
        zoom(Input.GetAxis("Mouse ScrollWheel"));
    }

    void zoom(float increment)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, zoomOutMin, zoomOutMax);
    }

    void checkBoundary()
    {
        if (mCheckBoundary)
        {
            Vector3 pos = Camera.main.transform.position;
            if (pos.x < mMinPos.x) {
                pos.x = mMinPos.x;
            }
            if (pos.y < mMinPos.y)
            {
                pos.y = mMinPos.y;
            }
            if (pos.x > mMaxPos.x)
            {
                pos.x = mMaxPos.x;
            }
            if (pos.y > mMaxPos.y)
            {
                pos.y = mMaxPos.y;
            }
            Camera.main.transform.position = pos;
        }
    }

}
