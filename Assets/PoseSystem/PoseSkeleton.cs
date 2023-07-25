using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
/*
- lines array NUM -
0: nose_to_leftEye / 1: nose_to_rightEye / 2: leftEye_to_leftEar / 3: rightEye_to_rightEar / 4: leftShoulder_to_rightShoulder / 
5: leftShoulder_to_leftHip / 6: rightShoulder_to_rightHip / 7: leftShoulder_to_rightHip / 8: rightShoulder_to_leftHip / 
9: leftHip_to_rightHip / 10: leftShoulder_to_leftElbow / 11: leftElbow_to_leftWrist / 12: rightShoulder_to_rightElbow / 
13: rightElbow_to_rightWrist / 14: leftHip_to_leftKnee / 15: leftKnee_to_leftAnkle / 16: rightHip_to_rightKnee / 17: rightKnee_to_rightAnkle

- keypoints array NUM -
0: "nose" / 1: "leftEye" / 2: "rightEye" / 3: "leftEar" / 4: "rightEar" / 5: "leftShoulder" / 
6: "rightShoulder" / 7: "leftElbow" / 8: "rightElbow" / 9: "leftWrist" / 10: "rightWrist" / 
11: "leftHip" / 12: "rightHip" / 13: "leftKnee" / 14: "rightKnee" / 15: "leftAnkle" / 16: "rightAnkle"
*/

public class PoseSkeleton
{
    // Æ÷Áî °ü·Ã º¯¼ö
    private List<bool> poseInfo = new List<bool>(new bool[11]);
    private float biasElbow = 50;
    private float biasCross = 30;                   

    // The list of key point GameObjects that make up the pose skeleton
    public Transform[] keypoints;

    // The GameObjects that contain data for the lines between key points
    private GameObject[] lines;

    // The names of the body parts that will be detected by the PoseNet model
    private static string[] partNames = new string[]{
        "nose", "leftEye", "rightEye", "leftEar", "rightEar", "leftShoulder",
        "rightShoulder", "leftElbow", "rightElbow", "leftWrist", "rightWrist",
        "leftHip", "rightHip", "leftKnee", "rightKnee", "leftAnkle", "rightAnkle"
    };
    
    private static int NUM_KEYPOINTS = partNames.Length;

    // The pairs of key points that should be connected on a body
    private Tuple<int, int>[] jointPairs = new Tuple<int, int>[]{
        // Nose to Left Eye
        Tuple.Create(0, 1),
        // Nose to Right Eye
        Tuple.Create(0, 2),
        // Left Eye to Left Ear
        Tuple.Create(1, 3),
        // Right Eye to Right Ear
        Tuple.Create(2, 4),
        // Left Shoulder to Right Shoulder
        Tuple.Create(5, 6),
        // Left Shoulder to Left Hip
        Tuple.Create(5, 11),
        // Right Shoulder to Right Hip
        Tuple.Create(6, 12),
        // Left Shoulder to Right Hip
        Tuple.Create(5, 12),
        // Rigth Shoulder to Left Hip
        Tuple.Create(6, 11),
        // Left Hip to Right Hip
        Tuple.Create(11, 12),
        // Left Shoulder to Left Elbow
        Tuple.Create(5, 7),
        // Left Elbow to Left Wrist
        Tuple.Create(7, 9), 
        // Right Shoulder to Right Elbow
        Tuple.Create(6, 8),
        // Right Elbow to Right Wrist
        Tuple.Create(8, 10),
        // Left Hip to Left Knee
        Tuple.Create(11, 13), 
        // Left Knee to Left Ankle
        Tuple.Create(13, 15),
        // Right Hip to Right Knee
        Tuple.Create(12, 14), 
        // Right Knee to Right Ankle
        Tuple.Create(14, 16)
    };

    // Colors for the skeleton lines
    private Color[] colors = new Color[] {
        // Head
        Color.magenta, Color.magenta, Color.magenta, Color.magenta,
        // Torso
        Color.red, Color.red, Color.red, Color.red, Color.red, Color.red,
        // Arms
        Color.green, Color.green, Color.green, Color.green,
        // Legs
        Color.blue, Color.blue, Color.blue, Color.blue
    };

    // The width for the skeleton lines
    private float lineWidth;

    
    public PoseSkeleton(float pointScale = 10f, float lineWidth = 5f)
    {
        this.keypoints = new Transform[NUM_KEYPOINTS];

        Material keypointMat = new Material(Shader.Find("Unlit/Color"));
        keypointMat.color = Color.yellow;

        for (int i = 0; i < NUM_KEYPOINTS; i++)
        {
            this.keypoints[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            this.keypoints[i].position = new Vector3(0, 0, 0);
            this.keypoints[i].localScale = new Vector3(pointScale, pointScale, 0);
            this.keypoints[i].gameObject.GetComponent<MeshRenderer>().material = keypointMat;
            this.keypoints[i].gameObject.name = partNames[i];
        }

        this.lineWidth = lineWidth;

        // The number of joint pairs
        int numPairs = jointPairs.Length;
        // Initialize the lines array
        lines = new GameObject[numPairs];

        // Initialize the pose skeleton
        InitializeSkeleton();
    }

    /// <summary>
    /// Toggles visibility for the skeleton
    /// </summary>
    /// <param name="show"></param>
    public void ToggleSkeleton(bool show)
    {
        for (int i= 0; i < jointPairs.Length; i++)
        {
            lines[i].SetActive(show);
            keypoints[jointPairs[i].Item1].gameObject.SetActive(show);
            keypoints[jointPairs[i].Item2].gameObject.SetActive(show);
        }
    }

    /// <summary>
    /// Clean up skeleton GameObjects
    /// </summary>
    public void Cleanup()
    {

        for (int i = 0; i < jointPairs.Length; i++)
        {
            GameObject.Destroy(lines[i]);
            GameObject.Destroy(keypoints[jointPairs[i].Item1].gameObject);
            GameObject.Destroy(keypoints[jointPairs[i].Item2].gameObject);
        }
    }


    /// <summary>
    /// Create a line between the key point specified by the start and end point indices
    /// </summary>
    /// <param name="pairIndex"></param>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    /// <param name="width"></param>
    /// <param name="color"></param>
    private void InitializeLine(int pairIndex, float width, Color color)
    {
        int startIndex = jointPairs[pairIndex].Item1;
        int endIndex = jointPairs[pairIndex].Item2;

        // Create new line GameObject
        string name = $"{keypoints[startIndex].name}_to_{keypoints[endIndex].name}";
        lines[pairIndex] = new GameObject(name);

        // Add LineRenderer component
        LineRenderer lineRenderer = lines[pairIndex].AddComponent<LineRenderer>();
        // Make LineRenderer Shader Unlit
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        // Set the material color
        lineRenderer.material.color = color;

        // The line will consist of two points
        lineRenderer.positionCount = 2;

        // Set the width from the start point
        lineRenderer.startWidth = width;
        // Set the width from the end point
        lineRenderer.endWidth = width;
    }

    /// <summary>
    /// Initialize the pose skeleton
    /// </summary>
    private void InitializeSkeleton()
    {
        for (int i = 0; i < jointPairs.Length; i++)
        {
            InitializeLine(i, lineWidth, colors[i]);
        }
    }

    /// <summary>
    /// Update the positions for the key point GameObjects
    /// </summary>
    /// <param name="keypoints"></param>
    /// <param name="sourceScale"></param>
    /// <param name="sourceTexture"></param>
    /// <param name="mirrorImage"></param>
    /// <param name="minConfidence"></param>
    public void UpdateKeyPointPositions(Utils.Keypoint[] keypoints,
        float sourceScale, RenderTexture sourceTexture, bool mirrorImage, float minConfidence)
    {
        // Iterate through the key points
        for (int k = 0; k < keypoints.Length; k++)
        {
            // Check if the current confidence value meets the confidence threshold
            if (keypoints[k].score >= minConfidence / 100f)
            {
                // Activate the current key point GameObject
                this.keypoints[k].GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                // Deactivate the current key point GameObject
                this.keypoints[k].GetComponent<MeshRenderer>().enabled = false;
            }

            // Scale the keypoint position to the original resolution
            Vector2 coords = keypoints[k].position * sourceScale;

            // Flip the keypoint position vertically
            coords.y = sourceTexture.height - coords.y;

            // Mirror the x position if using a webcam
            if (mirrorImage) coords.x = sourceTexture.width - coords.x;

            // Update the current key point location
            // Set the z value to -1f to place it in front of the video screen
            this.keypoints[k].position = new Vector3(coords.x, coords.y, -1f);
        }
    }

    /// <summary>
    /// Draw the pose skeleton based on the latest location data
    /// </summary>
    public void UpdateLines()
    {
        // Iterate through the joint pairs
        for (int i = 0; i < jointPairs.Length; i++)
        {
            // Set the GameObject for the starting key point
            Transform startingKeyPoint = keypoints[jointPairs[i].Item1];
            // Set the GameObject for the ending key point
            Transform endingKeyPoint = keypoints[jointPairs[i].Item2];

            // Check if both the starting and ending key points are active
            if (startingKeyPoint.GetComponent<MeshRenderer>().enabled &&
                endingKeyPoint.GetComponent<MeshRenderer>().enabled)
            {
                // Activate the line
                lines[i].SetActive(true);

                LineRenderer lineRenderer = lines[i].GetComponent<LineRenderer>();
                // Update the starting position
                lineRenderer.SetPosition(0, startingKeyPoint.position);
                // Update the ending position
                lineRenderer.SetPosition(1, endingKeyPoint.position);
            }
            else
            {
                // Deactivate the line
                lines[i].SetActive(false);
            }
        }
        Motion_Detection();
    }

/*
- lines array NUM -
0: nose_to_leftEye / 1: nose_to_rightEye / 2: leftEye_to_leftEar / 3: rightEye_to_rightEar / 4: leftShoulder_0to_rightShoulder / 
5: leftShoulder_to_leftHip / 6: rightShoulder_to_rightHip / 7: leftShoulder_to_rightHip / 8: rightShoulder_to_leftHip / 
9: leftHip_to_rightHip / 10: leftShoulder_to_leftElbow / 11: leftElbow_to_leftWrist / 12: rightShoulder_to_rightElbow / 
13: rightElbow_to_rightWrist / 14: leftHip_to_leftKnee / 15: leftKnee_to_leftAnkle / 16: rightHip_to_rightKnee / 17: rightKnee_to_rightAnkle

- keypoints array NUM -
0: "nose" / 1: "leftEye" / 2: "rightEye" / 3: "leftEar" / 4: "rightEar" / 5: "leftShoulder" / 
6: "rightShoulder" / 7: "leftElbow" / 8: "rightElbow" / 9: "leftWrist" / 10: "rightWrist" / 
11: "leftHip" / 12: "rightHip" / 13: "leftKnee" / 14: "rightKnee" / 15: "leftAnkle" / 16: "rightAnkle"
*/
    private void Motion_Detection()
    {
        
        bool isNoseActivate = lines[0].activeSelf || lines[1].activeSelf;
        bool isLArmActivate = lines[10].activeSelf && lines[11].activeSelf;
        bool isRArmActivate = lines[12].activeSelf && lines[13].activeSelf;
        bool isLLegActivate = lines[14].activeSelf && lines[15].activeSelf;
        bool isRLegActivate = lines[16].activeSelf && lines[17].activeSelf;

        poseInfo[0] = false;

        // Raise Left Hand
        poseInfo[1] = isLArmActivate && !isRArmActivate &&
                    keypoints[7].transform.position.y < keypoints[9].transform.position.y
                    ? true : false;

        poseInfo[2] = !isLArmActivate && isRArmActivate &&
            keypoints[8].transform.position.y < keypoints[10].transform.position.y
            ? true : false;

        // Raise Left/Right Hand
        poseInfo[8] = isLArmActivate && isRArmActivate &&
            keypoints[7].transform.position.y < keypoints[9].transform.position.y &&
            keypoints[8].transform.position.y < keypoints[10].transform.position.y
            ? true : false;

        // elbow
        bool LElbowPow = lines[11].activeSelf && isNoseActivate &&                                      // ¿Þ ÆÈ²ÞÄ¡ Ä¡±â
            (keypoints[7].transform.position.x + biasElbow < keypoints[9].transform.position.x &&       // ¿ÞÆÈ²ÞÄ¡ÀÇ xÁÂÇ¥°¡ ¿Þ¼ÕÀÇ xÁÂÇ¥º¸´Ù ´õ ÀÛÀ» °æ¿ì
            keypoints[7].transform.position.y < keypoints[0].transform.position.y);                     // ¸Ó¸® À§·Î´Â °¡Áö ¾Ê°Ô
        bool RElbowPow = lines                  [13].activeSelf && isNoseActivate &&                                      // ¿À¸¥ ÆÈ²ÞÄ¡ Ä¡±â 
            (keypoints[8].transform.position.x - biasElbow > keypoints[10].transform.position.x &&      // ¿À¸¥ ÆÈ²ÞÄ¡ÀÇ xÁÂÇ¥°¡ ¿À¸¥¼ÕÀÇ xÁÂÇ¥º¸´Ù ´õ Å¬°æ¿ì
            keypoints[8].transform.position.y < keypoints[0].transform.position.y);                      // ¸Ó¸® À§·Î´Â °¡Áö ¾Ê°Ô
        poseInfo[3] = LElbowPow && !RElbowPow;
        poseInfo[4] = !LElbowPow && RElbowPow;
        poseInfo[5] = LElbowPow && RElbowPow;
                                                

        // cross
        poseInfo[6] = lines[11].activeSelf && isNoseActivate &&                         // ¿Þ¼Õ¸ñ-ÆÈ²ÞÄ¡ ÀÎ½Ä | ¿À¸¥¼Õ¸ñ-ÆÈ²ÞÄ¡ ¹ÌÀÎ½Ä
            keypoints[9].transform.position.x > keypoints[0].transform.position.x       // ¿Þ¼Õ¸ñÀÇ xÁÂÇ¥°¡ ÄÚÀÇ xÁÂÇ¥º¸´Ù Å¬ °æ¿ì
            ? true : false;
        // cross
        poseInfo[7] = lines[13].activeSelf && isNoseActivate &&                         // ¿Þ¼Õ¸ñ-ÆÈ²ÞÄ¡ ÀÎ½Ä
            keypoints[10].transform.position.x < keypoints[0].transform.position.x      // ¿À¸¥¼Õ¸ñÀÇ xÁÂÇ¥°¡ ÄÚÀÇ xÁÂÇ¥º¸´Ù ÀûÀ» °æ¿ì
            ? true : false;

        // Go Left 
        poseInfo[9] = lines[1].activeSelf && lines[4].activeSelf &&
            keypoints[1].transform.position.x < keypoints[5].transform.position.x + biasCross
            ? true : false;
        // Go Right
        poseInfo[10] = lines[2].activeSelf && lines[4].activeSelf &&
            keypoints[2].transform.position.x > keypoints[6].transform.position.x - biasCross
            ? true : false;



        // No Motion Check
        for (int i = 1; i < poseInfo.Count; i++)
        {
            poseInfo[0] = poseInfo[0] || poseInfo[i];
        }

        string debuglog = "";
        for (int i = 0; i < poseInfo.Count; i++)
            debuglog += (poseInfo[i] + " | ");
        

        // ¾î±ú ºÒ±ÕÇü Àâ´Â ÄÚµå ÀÓ°è°ª 20
        float diff1 = Mathf.Abs(keypoints[5].transform.position.y - keypoints[6].transform.position.y);
        bool testPose1 = lines[4].activeSelf && diff1 >= 20f;

        // ¿ÞÆÈÀÇ °¢µµ °è»ê
        if (isLArmActivate)
        {
            Vector2 shoulderPos = keypoints[5].transform.position;
            Vector2 elbowPos = keypoints[7].transform.position;
            Vector2 wristPos = keypoints[9].transform.position;

            Vector2 armDirection = wristPos - shoulderPos;
            Vector2 forearmDirection = wristPos - elbowPos;

            float angleRad = Mathf.Atan2(armDirection.y, armDirection.x) - Mathf.Atan2(forearmDirection.y, forearmDirection.x);
            float leftArmAngle = angleRad * Mathf.Rad2Deg;

            //Debug.Log("¿ÞÆÈ °¢µµ: " + leftArmAngle.ToString());
        }
        else
        {
            //Debug.Log("¿ÞÆÈ °¢µµ: Not Founded");
        }

        // ¿À¸¥ÆÈÀÇ °¢µµ °è»ê
        if (isRArmActivate)
        {
            Vector2 shoulderPos = keypoints[6].transform.position;
            Vector2 elbowPos = keypoints[8].transform.position;
            Vector2 wristPos = keypoints[10].transform.position;

            Vector2 armDirection = wristPos - shoulderPos;
            Vector2 forearmDirection = wristPos - elbowPos;

            float angleRad = Mathf.Atan2(armDirection.y, armDirection.x) - Mathf.Atan2(forearmDirection.y, forearmDirection.x);
            float rightArmAngle = angleRad * Mathf.Rad2Deg;
            //Debug.Log("¿À¸¥ÆÈ °¢µµ: " + rightArmAngle.ToString());
        }
        else
        {
            //Debug.Log("¿À¸¥ÆÈ °¢µµ: Not Founded");
        }

        // ´Ù¸® ÀÏÀÚ·Î °È°íÀÖ´ÂÁö È®ÀÎ 
        if (isLLegActivate)
        {
            Vector2 hipPos = keypoints[11].transform.position;
            Vector2 kneePos = keypoints[13].transform.position;
            Vector2 anklePos = keypoints[15].transform.position;

            Vector2 thighDirection = kneePos - hipPos;
            Vector2 legDirection = anklePos - kneePos;

            float angleRad = Mathf.Atan2(legDirection.y, legDirection.x) - Mathf.Atan2(thighDirection.y, thighDirection.x);
            float angleDeg = angleRad * Mathf.Rad2Deg;

            //Debug.Log("¿ÞÂÊ´Ù¸® °¢µµ: " + angleDeg);
        }
        else
        {

        }

        if (isRLegActivate)
        {
            Vector2 hipPos = keypoints[12].transform.position;
            Vector2 kneePos = keypoints[14].transform.position;
            Vector2 anklePos = keypoints[16].transform.position;

            Vector2 thighDirection = kneePos - hipPos;
            Vector2 legDirection = anklePos - kneePos;

            float angleRad = Mathf.Atan2(legDirection.y, legDirection.x) - Mathf.Atan2(thighDirection.y, thighDirection.x);
            float angleDeg = angleRad * Mathf.Rad2Deg;

            //Debug.Log("¿ìÃø´Ù¸® °¢µµ: " + angleDeg);
        }
        else
        {

        }

    }

    public List<bool> GetMotionType()
    {
        return poseInfo;
    }

}
