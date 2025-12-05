using System.Collections;
using UnityEngine;
using TMPro;

public class ModifiableTypewriterEffect : MonoBehaviour
{
    [SerializeField] private TMP_Text textMesh;
    [SerializeField] private float typeSpeed = 0.03f;
    private string message;

    public void SetMessage(string newMessage)
    {
        message = newMessage;
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
