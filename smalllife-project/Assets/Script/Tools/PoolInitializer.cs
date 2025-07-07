using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolInitializer : MonoBehaviour
{
    public GameObject pausePanelPrefab;
    public GameObject goalDialogPrefab;
    public GameObject ripplePrefab;
    public GameObject birdAniPrefab;
    public GameObject grassAniPrefab;
    public GameObject dustAniPrefab;
    public GameObject IronPillarAniPrefab;
    public GameObject PeopleTalkAniPrefab;
    public GameObject RoundTreeAniPrefab;
    public GameObject TreeClickAniPrefab;
    public GameObject WaitanWaterAniPrefab;


    void Start()
    {
        ObjectPoolManager.Instance.RegisterPrefab("PausePanel", pausePanelPrefab);
        ObjectPoolManager.Instance.RegisterPrefab("GoalDialog", goalDialogPrefab);
        ObjectPoolManager.Instance.RegisterPrefab("Ripple", ripplePrefab);
        ObjectPoolManager.Instance.RegisterPrefab("BirdClickAni", birdAniPrefab);
        ObjectPoolManager.Instance.RegisterPrefab("GrassClickAni", grassAniPrefab);
        ObjectPoolManager.Instance.RegisterPrefab("DustClickAni", dustAniPrefab);
        ObjectPoolManager.Instance.RegisterPrefab("IronPillarAni", IronPillarAniPrefab);
        ObjectPoolManager.Instance.RegisterPrefab("PeopleTalkAni", PeopleTalkAniPrefab);
        ObjectPoolManager.Instance.RegisterPrefab("RoundTreeAni", RoundTreeAniPrefab);
        ObjectPoolManager.Instance.RegisterPrefab("TreeClickAni", TreeClickAniPrefab);
        ObjectPoolManager.Instance.RegisterPrefab("WaitanWaterAni", WaitanWaterAniPrefab);
    }
}

