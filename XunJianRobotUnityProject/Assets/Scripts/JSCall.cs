using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class JSCall : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ROS_Init();

    [DllImport("__Internal")]
    private static extern void ROS_Connect();

    [DllImport("__Internal")]
    private static extern void ROS_Connect2();


    [DllImport("__Internal")]
    private static extern void ROS1_Move(int isMove, float xSpeed, float ySpeed, float zSpeed);

    [DllImport("__Internal")]
    private static extern void ROS1_Camera(int isStop, int command, float speed);


    [DllImport("__Internal")]
    private static extern void ROS1_Zero();

    public Text text1;

    public Text text2;

    public class OnRobotPositionChange : UnityEngine.Events.UnityEvent<RobotPosition> { }
    public class OnRobotRotationChange : UnityEngine.Events.UnityEvent<RobotRotation> { }
    public class OnBackZero : UnityEngine.Events.UnityEvent<string> { }

    public OnRobotPositionChange onRobotPositionChange = new OnRobotPositionChange();
    public OnRobotRotationChange onRobotRotationChange = new OnRobotRotationChange();

    public OnBackZero onBackZero = new OnBackZero();

    public UnityEngine.Events.UnityEvent onRobot1BackZeroResult = new UnityEngine.Events.UnityEvent();

    public OnRobotPositionChange onRobot2PositionChange = new OnRobotPositionChange();
    public OnRobotRotationChange onRobot2RotationChange = new OnRobotRotationChange();

    public OnBackZero on2BackZero = new OnBackZero();

    public bool enableRobot2;

    private void Start()
    {
        ROS_Init();
    }

    public void OnLibraryInitialized(string _json)
    {
        // text1.text += "\n" + "OnLibraryInitialized";

        StartCoroutine(IE_ROS_Connect());
    }

    IEnumerator IE_ROS_Connect()
    {
        yield return new WaitForSeconds(2f);
        ROS_Connect();

        yield return null;

        if (enableRobot2)
            ROS_Connect2();
    }

    public void CallText2(string _text)
    {
        text1.text += "\n" + _text;
    }

    public void CallText22(string _text)
    {
        text2.text += "\n" + _text;
    }

    public void GetConnectState(int _state)
    {
        switch (_state)
        {
            case -1:
                CallText2("连接错误");
                break;
            case 0:
                CallText2("连接成功");
                break;
            case 1:
                CallText2("连接中。。。");
                break;
            case 2:
                CallText2("连接关闭");
                break;

        }
    }

    public void GetConnectState2(int _state)
    {
        switch (_state)
        {
            case -1:
                CallText22("连接错误");
                break;
            case 0:
                CallText22("连接成功");
                break;
            case 1:
                CallText22("连接中。。。");
                break;
            case 2:
                CallText22("连接关闭");
                break;

        }
    }

    public void GetRobotPosition(string _json)
    {
        CallText2("坐标 Json: " + _json);

        RobotPosition pos = null;
        try
        {
            pos = JsonUtility.FromJson<RobotPosition>(_json);
        }
        finally
        {
            if (pos != null)
            {
                onRobotPositionChange.Invoke(pos);
            }
        }
    }

    public void GetRobotPosition2(string _json)
    {
        CallText22("坐标 Json: " + _json);

        RobotPosition pos = null;
        try
        {
            pos = JsonUtility.FromJson<RobotPosition>(_json);
        }
        finally
        {
            if (pos != null)
            {
                onRobot2PositionChange.Invoke(pos);
            }
        }
    }

    public void GetCameraRotation(string _json)
    {
        CallText2("旋转 Json: " + _json);

        RobotRotation pos = null;
        try
        {
            pos = JsonUtility.FromJson<RobotRotation>(_json);
        }
        finally
        {
            if (pos != null)
            {
                onRobotRotationChange.Invoke(pos);
            }
        }
    }

    public void GetCameraRotation2(string _json)
    {
        CallText22("旋转 Json: " + _json);

        RobotRotation pos = null;
        try
        {
            pos = JsonUtility.FromJson<RobotRotation>(_json);
        }
        finally
        {
            if (pos != null)
            {
                onRobot2RotationChange.Invoke(pos);
            }
        }
    }

    public void BackZero(string _axis)
    {
        onBackZero.Invoke(_axis);
    }

    public void BackZero2(string _axis)
    {
        on2BackZero.Invoke(_axis);
    }

    public void ROS1_BackZeroResult()
    {
        onRobot1BackZeroResult.Invoke();
    }

    public void ROS1_MoveAction(bool isMove, float xSpeed, float ySpeed, float zSpeed)
    {
        ROS1_Move(isMove ? 1 : 0, xSpeed, ySpeed, zSpeed);
    }

    public void ROS1_CameraAction(bool active, CameraCommandType commandType, float speed)
    {
        ROS1_Camera(active ? 0 : 1, (int)commandType, speed);
    }

    public void ROS1_ZeroAction()
    {
        ROS1_Zero();
    }
}

public enum CameraCommandType
{
    LeftRotate = 23,
    RightRotate = 24,
    UpRotate = 21,
    DownRotate = 22,
    Max = 11,
    Min = 12,
}

[System.Serializable]
public class RobotPosition
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class RobotRotation
{
    public float p;
    public float t;
    public float z;
}