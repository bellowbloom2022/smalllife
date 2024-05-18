using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Lean.Localization;

public class TextDisplay : MonoBehaviour
{
    public Text text;
    public string content;
    public float letterDelay = 0.1f;

    void Start()
    {
        StartCoroutine(AnimateText());
    }

    IEnumerator AnimateText()
    {
        for (int i = 0; i <= content.Length; i++)
        {
            text.text = content.Substring(0, i);
            yield return new WaitForSeconds(letterDelay);
        }
    }
}
