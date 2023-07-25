using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebcamSample : MonoBehaviour
{
    public RawImage rawImage;

    void Start()
    {
        // 사용 가능한 카메라 장치 목록 확인
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.Log("카메라를 찾을 수 없습니다.");
            return;
        }

        // 첫 번째 카메라를 선택하여 캠 영상 텍스처 생성
        WebCamTexture webcamTexture = new WebCamTexture(devices[0].name);

        // RawImage에 캠 영상 텍스처 할당
        rawImage.texture = webcamTexture;

        // 캠 영상 재생 시작
        webcamTexture.Play();
    }
}