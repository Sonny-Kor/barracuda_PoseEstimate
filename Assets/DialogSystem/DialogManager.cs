using System.Collections;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public float dialogInterval = 1f; // ��ȭ ������ �����ϴ� ����

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