using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using TMPro;
using UnityEditor;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Obi;
using Unity.XR.CoreUtils;

public class DebugUI : MonoBehaviour
{
    public GameObject XROrigin;
    public GameObject Controller_L;
    public GameObject Controller_R;
    public GameObject Rope;

    public GameObject locomotion;


    public TextMeshProUGUI gravity_L;
    public TextMeshProUGUI gravity_R;
    public TextMeshProUGUI m_transform_L;
    public TextMeshProUGUI m_transform_R;

    public TextMeshProUGUI m_joystick_L;
    public TextMeshProUGUI m_joystick_R;
    public TextMeshProUGUI m_grip_L;
    public TextMeshProUGUI m_grip_R;

    public TextMeshProUGUI m_trigger_L;
    public TextMeshProUGUI m_trigger_R;

    public TextMeshProUGUI m_buttonX_L;
    public TextMeshProUGUI m_buttonY_L;

    public TextMeshProUGUI m_buttonA_R;
    public TextMeshProUGUI m_buttonB_R;

    public TextMeshProUGUI m_stiffness;

    public TextMeshProUGUI m_state;

    GravitationalMoveProvider gp;
    ObiRope rope;

    float start;
    bool temp;


    XRBaseController controller_L;
    XRBaseController controller_R;
    public InputHelpers.Button triggerButton = InputHelpers.Button.Trigger;
    public float pressThreshold = 0.1f;
    public float longPressDuration = 1.0f;

    private bool isPressing_L = false;
    private float pressTime_L = 0f;
    bool istrigger_L;
     private bool isPressing_R = false;
    private float pressTime_R = 0f;
    bool istrigger_R;

    public float amplitude;
    public float frequency;
    public float duration;

    public GameObject anchor;

    void Start()
    {
        gp = locomotion.GetComponent<GravitationalMoveProvider>();
        rope = Rope.GetComponent<ObiRope>();
        controller_L = Controller_L.GetComponent<ActionBasedController>();
        controller_R = Controller_R.GetComponent<ActionBasedController>();
    }

    private void OnLongPress_L()
    {
        // Handle long press action here
        Debug.Log("Left Long press detected");
        Debug.Log(controller_L);
        if((int)gp.currentState<2){
        gp.currentState+=1;
        gp.isLongPressed_L=true;
        if((int)gp.currentState==1){
            rope.stretchCompliance=1;
        }
        if((int)gp.currentState==2){
            rope.stretchCompliance=0;
        }
        }
        
    }

   
    private void OnLongPress_R()
    {
        // Handle long press action here
        Debug.Log("Right Long press detected");
        Debug.Log(controller_R);
        if((int)gp.currentState>0){
        gp.currentState-=1;
        gp.isLongPressed_R=true;
        }
        if((int)gp.currentState==1){
            rope.stretchCompliance=1;
        }
        if((int)gp.currentState==2){
            rope.stretchCompliance=0;
        }
    }

    void Update()
    {

         bool isPressed_L=istrigger_L;
        
            if (isPressed_L)
            {
                if (!isPressing_L)
                {
                    isPressing_L = true;
                    pressTime_L = Time.time;
                }
                else if (Time.time - pressTime_L >= longPressDuration)
                {
                    OnLongPress_L();
                    isPressing_L = false; // Reset to prevent continuous long-press detection
                }
            }
            else
            {
                isPressing_L = false;
            }
        bool isPressed_R=istrigger_R;
        
            if (isPressed_R)
            {
                if (!isPressing_R)
                {
                    isPressing_R = true;
                    pressTime_R = Time.time;
                }
                else if (Time.time - pressTime_R >= longPressDuration)
                {
                    OnLongPress_R();
                    isPressing_R = false; // Reset to prevent continuous long-press detection
                }
            }
            else
            {
                isPressing_R = false;
            }
        
        gravity_L.text = gp.gravitational_force_L.ToString();
        gravity_R.text = gp.gravitational_force_R.ToString();

        m_state.text = gp.currentState.ToString();

        m_stiffness.text = rope.stretchingScale.ToString();

        List<InputDevice> devices = new List<InputDevice>();

        InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristics, devices);
        foreach (var item in devices)
            {
                item.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion quaternion);
                m_transform_L.text = quaternion.eulerAngles.normalized.ToString();
                item.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystick_L);
                m_joystick_L.text = joystick_L.ToString();
                item.TryGetFeatureValue(CommonUsages.gripButton, out bool grip_L);
                m_grip_L.text = grip_L.ToString();
                item.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger_L);
                m_trigger_L.text = trigger_L.ToString();
                istrigger_L=trigger_L;
                item.TryGetFeatureValue(CommonUsages.primaryButton, out bool button_X);
                m_buttonX_L.text = button_X.ToString();
                item.TryGetFeatureValue(CommonUsages.secondaryButton, out bool button_Y);
                m_buttonY_L.text = button_Y.ToString();
            }

        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);
        foreach (var item in devices)
            {
                item.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion quaternion);
                m_transform_R.text = quaternion.eulerAngles.normalized.ToString();
                item.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystick_R);
                m_joystick_R.text = joystick_R.ToString();
                item.TryGetFeatureValue(CommonUsages.gripButton, out bool grip_R);
                m_grip_R.text = grip_R.ToString();
                item.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger_R);
                m_trigger_R.text = trigger_R.ToString();
                istrigger_R=trigger_R;
                item.TryGetFeatureValue(CommonUsages.primaryButton, out bool button_A);
                m_buttonA_R.text = button_A.ToString();
                item.TryGetFeatureValue(CommonUsages.secondaryButton, out bool button_B);
                m_buttonB_R.text = button_B.ToString();
            }

    }
}
