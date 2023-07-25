using System.Collections;
using UnityEngine;

public class DialogManager : MonoBehaviour
{

    public void ShowDialog(string[] dialog , float dialogInterval)
    {
        StartCoroutine(ShowDialogSequence(dialog, dialogInterval));
    }

    private IEnumerator ShowDialogSequence(string[] dialog, float dialogInterval)
    {
        for (int i = 0; i < dialog.Length; i++)
        {
            Debug.Log(dialog[i]);
            yield return new WaitForSeconds(dialogInterval);
        }
    }
}