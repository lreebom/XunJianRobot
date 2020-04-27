using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class Robot2 : MonoBehaviour
{
    public JSCall jSCall;

    public Transform posTarget;

    public Transform rotPTarget;

    public Transform rotZTarget;

    Vector3 targetPos;

    Vector3 rotPEuler;
    Vector3 rotZEuler;

    public float posSmoothSpeed = 10f;
    public float rotSmoothSpeed = 10f;

    bool isBackZeroing = false;

    public float m_xBackZeroSpeed = 1f;
    public float m_zBackZeroSpeed = 0.5f;

    public float xBackZeroSpeed
    {
        get
        {
            return m_xBackZeroSpeed;
        }
        set
        {
            m_xBackZeroSpeed = value;
            xText.text = m_xBackZeroSpeed.ToString();
        }
    }
    public float zBackZeroSpeed
    {
        get
        {
            return m_zBackZeroSpeed;
        }
        set
        {
            m_zBackZeroSpeed = value;
            zText.text = m_zBackZeroSpeed.ToString();
        }
    }

    //public Slider xSlider;
    public InputField xInput;

    //public Slider zSlider;
    public InputField zInput;

    public Text xText;
    public Text zText;

    public Toggle reverseXToggle;

    private void Start()
    {
        targetPos = posTarget.localPosition;

        rotPEuler = rotPTarget.localEulerAngles;
        rotZEuler = rotZTarget.localEulerAngles;

        jSCall.onRobot2PositionChange.AddListener((RobotPosition pos) =>
        {
            targetPos = new Vector3(pos.x, posTarget.localPosition.y, pos.y);
        });

        jSCall.onRobot2RotationChange.AddListener((RobotRotation pos) =>
        {
            rotPEuler = new Vector3(0f, -pos.p, 0f);

            rotZEuler = new Vector3(-pos.t, 0f, 0f);
        });

        jSCall.on2BackZero.AddListener((string _axis) =>
        {
            isBackZeroing = true;
            StopAllCoroutines();
            StartCoroutine(IE_StartBackZero(_axis));
        });

        xInput.onValueChanged.AddListener((string _value) =>
        {
            xBackZeroSpeed = float.Parse(_value);
        });

        zInput.onValueChanged.AddListener((string _value) =>
        {
            zBackZeroSpeed = float.Parse(_value);
        });
    }

    private void Update()
    {
        if (isBackZeroing) return;

        posTarget.localPosition = Vector3.Lerp(posTarget.localPosition, targetPos, Time.deltaTime * posSmoothSpeed);

        rotPTarget.localEulerAngles = Vector3.Lerp(rotPTarget.localEulerAngles, rotPEuler, Time.deltaTime * posSmoothSpeed);
        rotZTarget.localEulerAngles = Vector3.Lerp(rotZTarget.localEulerAngles, rotZEuler, Time.deltaTime * posSmoothSpeed);
    }

    IEnumerator IE_StartBackZero(string _axis)
    {
        yield return null;
        // switch (_axis)
        // {
        //     case "x":
        //         yield return StartCoroutine(IE_StartBackXZero());
        //         break;
        //     case "z":
        //         yield return StartCoroutine(IE_StartBackZZero());
        //         break;
        //     case "xz":
        //         yield return StartCoroutine(IE_StartBackXZero());
        //         yield return null;
        //         yield return StartCoroutine(IE_StartBackZZero());
        //         break;
        // }
        yield return StartCoroutine(IE_StartBackZero());
        yield return new WaitForSeconds(2f);
        isBackZeroing = false;
    }

    IEnumerator IE_StartBackZero()
    {

        bool xIsZheng = posTarget.localPosition.x >= 0f;
        bool zIsZheng = posTarget.localPosition.z >= 0f;

        bool xIsZero = false;
        bool zIsZero = false;

        while (!xIsZero || !zIsZero)
        {
            float curX = posTarget.localPosition.x + xBackZeroSpeed * Time.deltaTime * (xIsZheng ? -1f : 1f);

            if (xIsZheng)
            {
                if (curX < 0)
                {
                    curX = 0f;
                    xIsZero = true;
                }
            }
            else
            {
                if (curX > 0)
                {
                    curX = 0f;
                    xIsZero = true;
                }
            }

            float curZ = posTarget.localPosition.z + zBackZeroSpeed * Time.deltaTime * (zIsZheng ? -1f : 1f);

            if (zIsZheng)
            {
                if (curZ < 0)
                {
                    curZ = 0f;
                    zIsZero = true;
                }
            }
            else
            {
                if (curZ > 0)
                {
                    curZ = 0f;
                    zIsZero = true;
                }
            }

            posTarget.localPosition = new Vector3(curX, posTarget.localPosition.y, curZ);

            yield return null;
        }
        yield return null;
    }

}
