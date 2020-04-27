using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class Robot : MonoBehaviour
{
    public JSCall jSCall;

    public Transform posXTarget;

    public Transform posZTarget;

    public Transform rotPTarget;

    public Transform rotZTarget;

    Vector3 posXPos;
    Vector3 posZPos;

    Vector3 rotPEuler;
    Vector3 rotZEuler;

    public float posSmoothSpeed = 10f;
    public float rotSmoothSpeed = 10f;

    bool isBackZeroing = false;

    public float m_xBackZeroSpeed = 1f;
    public float m_zBackZeroSpeed = 0.5f;

    public InputField pOffsetInput;

    public PressButton upButton;
    public PressButton downButton;
    public PressButton leftButton;
    public PressButton rightButton;

    public InputField zuoyouMoveSpeedInput;
    public InputField shangXiaMoveSpeedInput;

    public PressButton cameraUpButton;
    public PressButton cameraDownButton;
    public PressButton cameraLeftButton;
    public PressButton cameraRightButton;

    public PressButton cameraMaxButton;
    public PressButton cameraMinButton;

    public InputField rotateSpeedInput;
    public InputField zoomSpeedInput;

    public Button backZeroBtn;

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

    float hMmoveSpeed
    {
        get
        {
            return float.Parse(zuoyouMoveSpeedInput.text);
        }
    }

    float vMmoveSpeed
    {
        get
        {
            return float.Parse(shangXiaMoveSpeedInput.text);
        }
    }

    float rotateSpeed
    {
        get
        {
            return float.Parse(rotateSpeedInput.text);
        }
    }

    float zoomSPeed
    {
        get
        {
            return float.Parse(zoomSpeedInput.text);
        }
    }

    private void Start()
    {
        posXPos = posXTarget.localPosition;
        posZPos = posZTarget.localPosition;

        rotPEuler = rotPTarget.localEulerAngles;
        rotZEuler = rotZTarget.localEulerAngles;

        jSCall.onRobotPositionChange.AddListener((RobotPosition pos) =>
        {
            posXPos = new Vector3(-pos.x * (reverseXToggle.isOn ? -1f : 1f), posXTarget.localPosition.y, posXTarget.localPosition.z);

            posZPos = new Vector3(posZTarget.localPosition.x, -pos.z, posZTarget.localPosition.z);
        });

        jSCall.onRobotRotationChange.AddListener((RobotRotation pos) =>
        {
            float pOffset = 0f;

            if (pOffsetInput)
            {
                if (!float.TryParse(pOffsetInput.text, out pOffset))
                {
                    pOffset = 0f;
                }
            }

            rotPEuler = new Vector3(0f, -pos.p + pOffset, 0f);

            rotZEuler = new Vector3(-pos.t, 0f, 0f);
        });

        jSCall.onBackZero.AddListener((string _axis) =>
        {
            isBackZeroing = true;
            StopAllCoroutines();
            StartCoroutine(IE_StartBackZero(_axis));
        });

        jSCall.onRobot1BackZeroResult.AddListener(() =>
        {
            Debug.Log("Unity收到 ROS1的回零result");
            backZeroBtn.interactable = true;
        });

        xInput.onValueChanged.AddListener((string _value) =>
        {
            xBackZeroSpeed = float.Parse(_value);
        });

        zInput.onValueChanged.AddListener((string _value) =>
        {
            zBackZeroSpeed = float.Parse(_value);
        });

        upButton.onPress.AddListener((bool _press) =>
        {
            jSCall.ROS1_MoveAction(_press, 0f, 0f, -vMmoveSpeed);
        });
        downButton.onPress.AddListener((bool _press) =>
        {
            jSCall.ROS1_MoveAction(_press, 0f, 0f, vMmoveSpeed);
        });
        leftButton.onPress.AddListener((bool _press) =>
        {
            jSCall.ROS1_MoveAction(_press, -hMmoveSpeed, 0f, 0f);
        });
        rightButton.onPress.AddListener((bool _press) =>
        {
            jSCall.ROS1_MoveAction(_press, hMmoveSpeed, 0f, 0f);
        });

        cameraUpButton.onPress.AddListener((bool _press) =>
        {
            jSCall.ROS1_CameraAction(_press, CameraCommandType.UpRotate, rotateSpeed);
        });
        cameraDownButton.onPress.AddListener((bool _press) =>
        {
            jSCall.ROS1_CameraAction(_press, CameraCommandType.DownRotate, rotateSpeed);
        });
        cameraLeftButton.onPress.AddListener((bool _press) =>
        {
            jSCall.ROS1_CameraAction(_press, CameraCommandType.LeftRotate, rotateSpeed);
        });
        cameraRightButton.onPress.AddListener((bool _press) =>
        {
            jSCall.ROS1_CameraAction(_press, CameraCommandType.RightRotate, rotateSpeed);
        });

        cameraMaxButton.onPress.AddListener((bool _press) =>
        {
            jSCall.ROS1_CameraAction(_press, CameraCommandType.Max, zoomSPeed);
        });
        cameraMinButton.onPress.AddListener((bool _press) =>
        {
            jSCall.ROS1_CameraAction(_press, CameraCommandType.Min, zoomSPeed);
        });

        backZeroBtn.onClick.AddListener(() =>
        {
            backZeroBtn.interactable = false;
            jSCall.ROS1_ZeroAction();
        });
    }

    private void Update()
    {
        if (isBackZeroing) return;

        posXTarget.localPosition = Vector3.Lerp(posXTarget.localPosition, posXPos, Time.deltaTime * posSmoothSpeed);
        posZTarget.localPosition = Vector3.Lerp(posZTarget.localPosition, posZPos, Time.deltaTime * posSmoothSpeed);

        rotPTarget.localEulerAngles = Vector3.Lerp(rotPTarget.localEulerAngles, rotPEuler, Time.deltaTime * posSmoothSpeed);
        rotZTarget.localEulerAngles = Vector3.Lerp(rotZTarget.localEulerAngles, rotZEuler, Time.deltaTime * posSmoothSpeed);
    }

    IEnumerator IE_StartBackZero(string _axis)
    {
        yield return null;
        switch (_axis)
        {
            case "x":
                yield return StartCoroutine(IE_StartBackXZero());
                break;
            case "z":
                yield return StartCoroutine(IE_StartBackZZero());
                break;
            case "xz":
                yield return StartCoroutine(IE_StartBackXZero());
                yield return null;
                yield return StartCoroutine(IE_StartBackZZero());
                break;
        }
        yield return new WaitForSeconds(2f);
        isBackZeroing = false;
    }

    IEnumerator IE_StartBackXZero()
    {
        bool finished = false;

        while (!finished)
        {
            float curX = posXTarget.localPosition.x + xBackZeroSpeed * Time.deltaTime * (reverseXToggle.isOn ? -1f : 1f);
            if (reverseXToggle.isOn)
            {
                if (curX < 0)
                {
                    curX = 0f;
                    finished = true;
                }
            }
            else
            {
                if (curX > 0)
                {
                    curX = 0f;
                    finished = true;
                }
            }

            posXTarget.localPosition = new Vector3(curX, posXTarget.localPosition.y, posXTarget.localPosition.z);

            yield return null;
        }
        yield return null;
    }

    IEnumerator IE_StartBackZZero()
    {
        bool finished = false;

        while (!finished)
        {
            float curY = posZTarget.localPosition.y + xBackZeroSpeed * Time.deltaTime;
            if (curY > 0)
            {
                curY = 0f;
                finished = true;
            }
            posZTarget.localPosition = new Vector3(posZTarget.localPosition.x, curY, posZTarget.localPosition.z);

            yield return null;
        }
        yield return null;
    }

}
