using System.Collections;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public float dialogInterval = 1f; // 대화 간격을 조절하는 변수

    public void ShowDialog(string[] dialog)
    {
        StartCoroutine(ShowDialogSequence(dialog));
    }

    private IEnumerator ShowDialogSequence(string[] dialog)
    {
        for (int i = 0; i < dialog.Length; i++)
        {
            Debug.Log(dialog[i]);
            yield return new WaitForSeconds(dialogInterval);
        }
    }
}