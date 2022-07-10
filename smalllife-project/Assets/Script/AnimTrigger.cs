using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimTrigger : MonoBehaviour
{
    public GameObject mBeTriggerGameObject;
    public GameObject mBeTriggerGameObject2;

    public GameObject mPaintingStartPos;
    public GameObject mPaintingEndPos;
    public bool mTriggered = false;

    public Canvas mCanvas;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnAnimEndTrigger()
    {
        if (this.mBeTriggerGameObject != null) {
            Animator anim = this.mBeTriggerGameObject.GetComponent<Animator>();
            if (anim != null) {
                anim.SetTrigger("click");
            }
            this.mBeTriggerGameObject = null;
        }
        else if (this.mBeTriggerGameObject2 != null)
        {
            Animator anim = this.mBeTriggerGameObject2.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("click");
            }
        }

    }

    public void OnPaintingCollected() {
        if (!this.mTriggered &&this.mBeTriggerGameObject) {
            //先从小变大
            Debug.Log("OnPaintingCollected");
            this.mTriggered = true;
            this.mBeTriggerGameObject.transform.DOMove(mPaintingStartPos.transform.position, 0.5f);
            this.mBeTriggerGameObject.transform.DOScale(Vector3.one, 0.5f).onComplete= () => {
                SpriteAnim ator = this.mBeTriggerGameObject.GetComponent<SpriteAnim>();
                ator.enabled = true;
                ator.IsPlaying = true;
                ator.onFinish = () =>
                {
                    this.mBeTriggerGameObject.transform.DOMove(this.mPaintingEndPos.transform.position, 0.5f);
                };
                //ator.SetTrigger("click");
            };
            
        }
    }

    public void OnPaintingCollected2()
    {
        
        if (!this.mTriggered && this.mBeTriggerGameObject)
        {
            //先从小变大
            Debug.Log("OnPaintingCollected2");

            Vector3 screenPos = Camera.main.WorldToScreenPoint(mPaintingStartPos.transform.position);
            //screenPos.z = 0;
            Vector2 uiPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out uiPos);
            RectTransform rectPainting= mBeTriggerGameObject.GetComponent<RectTransform>();
            rectPainting.anchoredPosition = uiPos;

            screenPos = Camera.main.WorldToScreenPoint(mPaintingEndPos.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvas.transform as RectTransform, screenPos, null, out uiPos);


            rectPainting.transform.DOScale(Vector3.one, .5f);
            rectPainting.DOLocalMove(uiPos, .5f).onComplete = () => {
                mBeTriggerGameObject.GetComponent<Animator>().SetTrigger("click");
            };

        }
    }



}
