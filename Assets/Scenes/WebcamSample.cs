using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebcamSample : MonoBehaviour
{
    public RawImage rawImage;

    void Start()
    {
        // ��� ������ ī�޶� ��ġ ��� Ȯ��
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.Log("ī�޶� ã�� �� �����ϴ�.");
            return;
        }

        // ù ��° ī�޶� �����Ͽ� ķ ���� �ؽ�ó ����
        WebCamTexture webcamTexture = new WebCamTexture(devices[0].name);

        // RawImage�� ķ ���� �ؽ�ó �Ҵ�
        rawImage.texture = webcamTexture;

        // ķ ���� ��� ����
        webcamTexture.Play();
    }
}