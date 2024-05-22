using UnityEngine;

public class HintAnimator3 : MonoBehaviour
{
    public GameObject coverUI; // узуж UI т╙кь

    public void GoalAchieved()
    {
        if (coverUI != null)
        {
            coverUI.SetActive(false); // ╫Шсцузуж
            Debug.Log("узужрярфЁЩё╛©ирт╣Ц╩В nextButton");
        }
        else
        {
            Debug.LogError("Cover UI reference is not set in the HintAnimator3 script");
        }
    }
}
