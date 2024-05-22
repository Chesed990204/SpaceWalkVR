using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using TMPro;
using UnityEditor;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DebugUI : MonoBehaviour
{
    public GameObject XROrigin;
    public GameObject Controller_L;
    public GameObject Controller_R;
    public GameObject Rope;

    public TextMeshProUGUI m_transform_L;
    public TextMeshProUGUI m_transform_R;

    public TextMeshProUGUI m_joystick_L;
    public TextMeshProUGUI m_joystick_R;
    public TextMeshProUGUI m_grip_L;
    public TextMeshProUGUI m_grip_R;

    public TextMeshProUGUI m_trigger_L;
    public TextMeshProUGUI m_trigger_R;


    void Start()
    {
      
    }

    void Update()
    {
        // m_transform_L.text=ValueMonitoring.transform_L.ToString();
        // m_transform_R.text=ValueMonitoring.transform_R.ToString();

        // m_joystick_L.text=ValueMonitoring.joystick_L.ToString();
        // m_joystick_R.text = ValueMonitoring.joystick_R.ToString();

        // m_grip_L.text=ValueMonitoring.grip_L.ToString();
        // m_grip_R.text=ValueMonitoring.grip_R.ToString();

        // m_trigger_L.text=ValueMonitoring.trigger_L.ToString();
        // m_trigger_R.text=ValueMonitoring.trigger_R.ToString();

    }
}
