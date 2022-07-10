using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Image))]
public class ImageAnim : MonoBehaviour
{
    private Image ImageSource;
    private int mCurFrame = 0;
    private float mDelta = 0;

    public float FPS = 10;
    public List<Sprite> SpriteFrames;
    public bool IsPlaying = false;
    public bool Foward = true;
    public bool AutoPlay = false;
    public bool Loop = false;
    public bool StopOnFirstFrame = false;
    public bool IsSetNativeSize = false;
    public Action onFinish;

    private float TimeGab = 0.1f;

    public int FrameCount
    {
        get
        {
            return SpriteFrames.Count;
        }
    }

    void Awake()
    {
        ImageSource = GetComponent<Image>();
    }

    void Start()
    {
        TimeGab = 1 / FPS;
        if (AutoPlay)
        {
            Play();
        }
        else
        {
            IsPlaying = false;
        }
    }

    public void SetSprite(int idx)
    {
        ImageSource.sprite = SpriteFrames[idx];
        //该部分为设置成原始图片大小，如果只需要显示Image设定好的图片大小，注释掉该行即可。
        if (IsSetNativeSize)
        {
            ImageSource.SetNativeSize();
        }

    }

    public void Play()
    {
        IsPlaying = true;
        Foward = true;
    }

    public void PlayReverse()
    {
        IsPlaying = true;
        Foward = false;
    }

    void Update()
    {
        if (!IsPlaying || 0 == FrameCount)
        {
            return;
        }

        mDelta += Time.deltaTime;
        if (mDelta > TimeGab)
        {
            mDelta -= TimeGab;
            if (Foward)
            {
                mCurFrame++;
            }
            else
            {
                mCurFrame--;
            }

            if (mCurFrame >= FrameCount)
            {
                if (Loop)
                {
                    mCurFrame = 0;
                }
                else
                {
                    IsPlaying = false;
                    OnAnimEnd();
                    return;
                }
            }
            else if (mCurFrame < 0)
            {
                if (Loop)
                {
                    mCurFrame = FrameCount - 1;
                }
                else
                {
                    IsPlaying = false;
                    OnAnimEnd();
                    return;
                }
            }

            SetSprite(mCurFrame);
        }
    }

    public void Pause()
    {
        IsPlaying = false;
    }

    private void OnAnimEnd()
    {
        if (StopOnFirstFrame)
        {
            SetSprite(0);
        }
        if (this.onFinish != null)
        {
            this.onFinish();
        }
        //this.gameObject.SetActive(false);
    }

    public void SetFinishCallback(Action ac)
    {
        this.onFinish = ac;
    }

    public void Resume()
    {
        if (!IsPlaying)
        {
            IsPlaying = true;
        }
    }

    public void Stop()
    {
        mCurFrame = 0;
        SetSprite(mCurFrame);
        IsPlaying = false;
    }

    public void Rewind()
    {
        mCurFrame = 0;
        SetSprite(mCurFrame);
        Play();
    }
}
