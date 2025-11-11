using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private TMP_Text textMesh;
    [SerializeField] private float typeSpeed = 0.03f;
    [TextArea][SerializeField] private string message;

    private void OnEnable()
    {
        StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        textMesh.text = "";
        foreach (char c in message)
        {
            textMesh.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }
}
