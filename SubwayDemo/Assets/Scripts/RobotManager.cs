using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Toggle toggleMul;
    public Toggle toggleAuto;

    public RobotController robotController;

    public Animation animation;

    public Transform pointStart;
    public Transform[] points;

    public Button startBtn;

    public Transform camPan;

    void Start()
    {
        toggleMul.onValueChanged.AddListener((bool _isOn) =>
        {
            if (_isOn)
            {
                SwitchMul();
            }
            else
            {
                SwitchAuto();
            }
        });

        startBtn.onClick.AddListener(() =>
        {
            StopAllCoroutines();
            StartCoroutine(IE_Auto());
        });
    }

    void SwitchMul()
    {
        StopAllCoroutines();
        robotController.enableCtrl = true;

        animation.Stop();
        camPan.localRotation = Quaternion.identity;
    }

    void SwitchAuto()
    {
        robotController.enableCtrl = false;
        animation.Play("Auto");
        AnimationState animationState = animation["Auto"];
        animationState.normalizedSpeed = 0f;
        animationState.normalizedTime = 0f;
    }

    IEnumerator IE_Auto()
    {
        float curTime = 0f;
        float totalTime = 36f;

        animation.Play("Auto");
        AnimationState animationState = animation["Auto"];
        animationState.normalizedSpeed = 0f;
        animationState.normalizedTime = 0f;

        while (true)
        {
            curTime += Time.deltaTime;
            animationState.normalizedTime = Mathf.Clamp01(curTime / totalTime);

            if (curTime >= totalTime)
            {
                break;
            }
            yield return null;
        }

        //LeanTween.move(robotController.gameObject, points[0], )

    }

}
