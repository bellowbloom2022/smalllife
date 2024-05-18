using System.Collections;
using UnityEngine;

public class HintAnimator2 : MonoBehaviour
{
    public SelectController selectController;
    public StepByStepText stepByStepText;
    public GameObject hint1;
    public GameObject nextButton;

    private bool interactAnimationsClicked;
    private bool textRead;

    private IEnumerator Start()
    {
        if(hint1 != null)
        {
            hint1.SetActive(false);
        }
        if(nextButton != null)
        {
            nextButton.SetActive(false);
        }
        yield return new WaitForSeconds(2.0f);

        if (hint1 != null)
        {
            hint1.SetActive(true);
        }
    }
    private void Update()
    {
        if (!selectController.InteractAnimationsClicked)
        {
            return;
        }

        if (!stepByStepText.AllTextRead())
        {
            return;
        }
        hint1.SetActive(false);
        nextButton.SetActive(true);
    }
}
