using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour
{
    public static Level ins;

    public GameObject mBtnNext;

    public int TotalCount;
    public string NextLevelName;

    public int mCount = 0;

    private void Awake()
    {
        ins = this;
    }
    // Start is called before the first frame update
    void Start()
    {
 
    }

    public void AddCount() {
        ++this.mCount;
        if (this.mCount > (this.TotalCount -1) ){
            mBtnNext.SetActive(true);
            //设置是否完成关卡
            if (NextLevelName == "Level2") {
                PlayerPrefs.SetInt("Level1", 1);
            } else if (NextLevelName == "Level2") {
                PlayerPrefs.SetInt("Level2", 1);
            }
        }
    }

    public void onBtnNextClicked() {
        SceneManager.LoadScene(NextLevelName);
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
