using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Goal1 : MonoBehaviour
{

    public GameObject mGameObjectNovel;
    public GameObject mNovelPosStart;
    public GameObject mNovelPosMid;
    public GameObject mNovelPos;
    public Canvas mCanvas;
    private bool mIsTriggered;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnAnim1End()
    {
        this.GetComponent<Animator>().ResetTrigger("click");
        BoxCollider[] clis = this.GetComponents<BoxCollider>();
        for (int i = 0; i < clis.Length; ++i)
        {
            clis[i].enabled = !clis[i].enabled;
        }
    }

    void OnAnim2End()
    {
        if (!mIsTriggered)
        {
            Debug.Log("Get cat book");
            mIsTriggered = true;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(mNovelPosStart.transform.position);
            //screenPos.z = 0;
            Vector2 uiPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out uiPos);
            RectTransform rectPenZai = mGameObjectNovel.GetComponent<RectTransform>();
            rectPenZai.anchoredPosition = uiPos;

            screenPos = Camera.main.WorldToScreenPoint(mNovelPosMid.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out uiPos);

            mGameObjectNovel.transform.DOScale(Vector3.one * 2, 0.8f);
            rectPenZai.DOLocalMove(uiPos, 0.8f).onComplete = () =>
            {
                //screenPos = Camera.main.WorldToScreenPoint(mNovelPos.transform.position);
                //RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out uiPos);

                mGameObjectNovel.transform.DOScale(Vector3.one, .5f);
                RectTransform r = mNovelPos.transform as RectTransform;
                uiPos = r.anchoredPosition;
                rectPenZai.DOLocalMove(uiPos, .5f).onComplete = () =>
                {
                    //Sequence mSequence = DOTween.Sequence();
                    //mSequence.Append(mGameObjectNovel.transform.DOScale(Vector3.one * 1.3f, 0.2f));
                    //mSequence.Append(mGameObjectNovel.transform.DOScale(Vector3.one, 0.2f));
                    rectPenZai.GetComponent<Animator>().SetTrigger("click");
                    Level.ins.AddCount();
                };
            };

        }
    }

 }
