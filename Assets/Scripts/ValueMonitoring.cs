using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
namespace UnityEngine.XR.Interaction.Toolkit
{
    public class ValueMonitoring : MonoBehaviour
    {
        List<InputDevice> devices_L = new List<InputDevice>();
        List<InputDevice> devices_R = new List<InputDevice>();

        public InputDevice left_controller;
        public InputDevice right_controller;
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;

        public Transform transform_L;
        public Transform transform_R;
        public Vector2 joystick_L;
        public Vector2 joystick_R;
        public bool grip_L;
        public bool grip_R;
        public bool trigger_L;
        public bool trigger_R;

        public bool button_X_L;
        public bool button_Y_L;
        public bool button_A_R;
        public bool button_B_R;

        // Start is called before the first frame update
        void Start()
        {
            // UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, devices_L);
            // UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, devices_R);
            // left_controller = devices_L[0];
            // right_controller = devices_R[0];
        }

        // Update is called once per frame
        void Update()
        {
            
            InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, devices_L);
            InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, devices_R);
           
            left_controller = devices_L[0];
            right_controller = devices_R[0];
           
            if(left_controller!=null){
            left_controller.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion quaternion_L);
            transform_L.rotation=quaternion_L;
            left_controller.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 m_joystick_L);
            joystick_L =m_joystick_L;
            left_controller.TryGetFeatureValue(CommonUsages.gripButton, out bool m_grip_L);
            grip_L=m_grip_L;
            left_controller.TryGetFeatureValue(CommonUsages.gripButton, out bool m_trigger_L);
            trigger_L=m_trigger_L;
            left_controller.TryGetFeatureValue(CommonUsages.primaryButton, out bool m_button_X_L);
            button_X_L=m_button_X_L;
            left_controller.TryGetFeatureValue(CommonUsages.secondaryButton, out bool m_button_Y_L);
            button_Y_L=m_button_Y_L;
           
            Debug.Log("left controller rotation: " + transform_L.rotation);
            Debug.Log("left button x: " + button_X_L);
            Debug.Log("left button y: " + button_Y_L);
            }

            if(right_controller!=null){
            right_controller.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion quaternion_R);
            transform_R.rotation=quaternion_R;
            right_controller.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 input_R);
            joystick_R =input_R;
            right_controller.TryGetFeatureValue(CommonUsages.gripButton, out bool m_grip_R);
            grip_R=m_grip_R;
            right_controller.TryGetFeatureValue(CommonUsages.gripButton, out bool m_trigger_R);
            trigger_R=m_trigger_R;
            right_controller.TryGetFeatureValue(CommonUsages.primaryButton, out bool m_button_A_R);
            button_A_R=m_button_A_R;
            right_controller.TryGetFeatureValue(CommonUsages.secondaryButton, out bool m_button_B_R);
            button_B_R=m_button_B_R;
            Debug.Log("right controller rotation: " + transform_R.rotation);}
        }
        }
}