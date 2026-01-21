using System.Collections;
using TMPro;
using UnityEngine;

public class Twinkle : MonoBehaviour
{
    private string text;
    public TMP_Text targetText;
    private float delay = 0.1f;
    void Start()
    {
        text = targetText.text.ToString();
        targetText.text = " ";

        StartCoroutine(textPrint(delay));
    }

    IEnumerator textPrint(float delay)
    {
        int count = 0;
        while (count != text.Length)
        {
            if(count < text.Length)
            {
                targetText.text += text[count];
                count++;
            }
            yield return new WaitForSeconds(delay);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
