using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class ExceptionTestTool : EditorWindow
{
    [MenuItem("Tools/异常操作测试工具")]
    public static void ShowWindow()
    {
        GetWindow<ExceptionTestTool>("异常操作测试");
    }

    void OnGUI()
    {
        GUILayout.Label("异常测试场景", EditorStyles.boldLabel);

        if (GUILayout.Button("A1 - 模拟强退后重启"))
        {
            SimulateForceQuitAndRestart();
        }

        if (GUILayout.Button("A2 - Step切换→存档→读档"))
        {
            SimulateStepSwitchSaveLoad();
        }

        if (GUILayout.Button("A3 - 快速双击目标交互"))
        {
            SimulateDoubleClickTarget();
        }
    }

    // A1：模拟强制关闭再重启（可用于手动验证）
    void SimulateForceQuitAndRestart()
    {
        SaveSystem.SaveGame();
        Debug.LogWarning("模拟强退完成（保存已执行），请手动重启游戏后观察是否能正确加载。");
    }

    // A2：目标切换、存档、读档（验证目标状态保存情况）
    void SimulateStepSwitchSaveLoad()
    {
        if (SaveSystem.GameData == null)
        {
            Debug.LogError("SaveSystem.GameData 为 null，无法模拟操作！");
            return;
        }

        if (SaveSystem.GameData.goalProgressMap == null)
        {
            Debug.LogWarning("goalProgressMap 为 null，尝试新建...");
            SaveSystem.GameData.goalProgressMap = new Dictionary<string, GoalProgress>();
        }

        var goalKey = "0_101"; // 示例key
        if (!SaveSystem.GameData.goalProgressMap.ContainsKey(goalKey))
            SaveSystem.GameData.goalProgressMap[goalKey] = new GoalProgress();

        var progress = SaveSystem.GameData.goalProgressMap[goalKey];
        progress.step1Completed = !progress.step1Completed; // 切换状态
        progress.step2Completed = !progress.step2Completed; // 切换状态

        SaveSystem.SaveGame();
        SaveSystem.LoadGame();

        Debug.Log($"目标 [{goalKey}] 状态切换并保存/读取完成：Step1={progress.step1Completed}, Step2={progress.step2Completed}");
    }

    // A3：模拟快速双击（可适配你的目标交互逻辑）
    void SimulateDoubleClickTarget()
    {
        var targetID = "101"; // 示例目标 ID
        Debug.Log($"模拟双击目标：{targetID}");
        // 你可以在这里调用目标点击逻辑两次
        SimulateClickTarget(101);
        SimulateClickTarget(101);
    }

    void SimulateClickTarget(int goalID)
    {
        var goals = GameObject.FindObjectsOfType<Goal>();
        Debug.Log($"找到 {goals.Length} 个 Goal");
        foreach (var goal in goals)
        {
            Debug.Log($"目标名称：{goal.name}，GoalID：{goal.GoalID}");
            if (goal.GoalID == goalID)
            {
                Animator anim = goal.GetComponent<Animator>();
                if (anim != null)
                {
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("A0_lunchbox_loop"))
                    {
                        anim.SetTrigger("click1");
                        Debug.Log($"模拟点击 {goal.name}（step1）");
                    }
                    else
                    {
                        anim.SetTrigger("click");
                        Debug.Log($"模拟点击 {goal.name}（step2）");
                    }
                }
                else
                {
                    Debug.LogWarning($"目标 {goal.name} 没有 Animator 组件");
                }
                return;
            }
        }
        Debug.LogWarning($"未找到 Goal：GoalID={goalID}");
    }
}
