using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Reflection;
using System.IO.Compression;

public class RunAsAdminstrator : MonoBehaviour
{
    public Button button;
    public Text text;
    void Start()
    {
        button.onClick.AddListener(RunUpdate);

        UnityEngine.Debug.Log(System.IO.Path.GetFullPath("."));
    }

    private void RunUpdate()
    {
        ProcessStartInfo proc = new ProcessStartInfo();
        proc.UseShellExecute = true;
        string path = System.IO.Directory.GetParent(Application.dataPath) + "/Update/";

        proc.WorkingDirectory = path;
        proc.Verb = "runas";
        proc.FileName = "Update.exe";
        text.text += path + "\n";
        try
        {
            text.text += "Run" + "\n";
            Process.Start(proc);
            Application.Quit();
        }
        catch (Exception _ex)
        {
            text.text += _ex.ToString() + "\n";
            UnityEngine.Debug.Log("启动Update.exe失败：" + _ex.ToString());
        }
    }



}
