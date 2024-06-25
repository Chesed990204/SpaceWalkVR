using System;
using System.Collections.Generic;
using System.Numerics;
using IdlessChaye.IdleToolkit.AVGEngine;
using UnityEngine.Assertions;
using UnityEngine.XR;
using Obi;
using UnityEngine.UIElements;
using UnityEngine.TextCore.LowLevel;
using UnityEditor.MemoryProfiler;
using Unity.XR.CoreUtils;
namespace UnityEngine.XR.Interaction.Toolkit

{

    public abstract class GravitationalMoveProviderBase : LocomotionProvider
    {
        public enum GravityApplicationMode
        {
            AttemptingMove,
            Immediately,
        }

        public enum State{
            Free,
            Rope,
            Stop
        }

        [SerializeField]
        [Tooltip("The speed, in units per second, to move forward.")]
        protected float m_MoveSpeed = 1f;
        public float moveSpeed
        {
            get => m_MoveSpeed;
            set => m_MoveSpeed = value;
        }

        public GameObject Rope;

        public State currentState=State.Free;

        public bool isLongPressed_L=false;
        public bool isLongPressed_R=false;


        public bool isLongPressed_Grip=false;

        [SerializeField]
        [Tooltip("The source Transform to define the forward direction.")]
        Transform m_ForwardSource_R;
        public Transform ForwardSource_R
        {
            get => m_ForwardSource_R;
            set => m_ForwardSource_R = value;
        }

        [SerializeField]
        [Tooltip("The source Transform to define the forward direction.")]
        Transform m_ForwardSource_L;
        public Transform ForwardSource_L
        {
            get => m_ForwardSource_L;
            set => m_ForwardSource_L = value;
        }


        public float gravitational_force_L = 0.0f;
        public float gravitational_force_R = 0.0f;

        List<Vector3> gravitaional_force_queue ;



        CharacterController m_CharacterController;

        bool m_AttemptedGetCharacterController;

        bool m_IsMovingXROrigin;

        Vector3 m_VerticalVelocity;

        ObiRope rope;

        public GameObject solver;

        public GameObject thruster_L;
        public GameObject thruster_R;
        public GameObject thruster_result;

        public GameObject left_controller;
        public GameObject right_controller;
        XRRayInteractor left_ray;
        XRRayInteractor right_ray;

        ObiParticleAttachment ropeAttach;

        ObiRopeExtrudedRenderer rope_renderer;

        public GameObject anchor;
       
        
        void Start(){
            rope = Rope.GetComponent<ObiRope>();
            ropeAttach = Rope.GetComponent<ObiParticleAttachment>();
            left_ray=left_controller.GetComponent<XRRayInteractor>();
            right_ray=right_controller.GetComponent<XRRayInteractor>();
            rope_renderer = Rope.GetComponent<ObiRopeExtrudedRenderer>();
            gravitaional_force_queue=new List<Vector3>();
            
        }
        protected void Update()
        {
            //Debug.Log("Left Ray: "+left_ray.rayOriginTransform.rotation);
            //Debug.Log("Right Ray: "+right_ray.rayOriginTransform.rotation);

        
            
            m_IsMovingXROrigin = false;
            var xrOrigin = system.xrOrigin?.Origin;
            
            if (xrOrigin == null)
                return;

            var input_L = ReadInput_L();
            var input_R = ReadInput_R();
            var translationInWorldSpace = ComputeDesiredMove(input_L, input_R);
            
            if(gravitaional_force_queue.Count>10000){gravitaional_force_queue.RemoveAt(0);}
            gravitaional_force_queue.Add(xrOrigin.transform.position);

            Vector3 result=Vector3.one;
            result.x= translationInWorldSpace.magnitude*100;
            thruster_result.transform.localScale = result;
            thruster_result.transform.forward = Quaternion.AngleAxis(90,Vector3.up)*translationInWorldSpace.normalized;

            
            if(currentState!=State.Free){
                ropeAttach.compliance=0;
                rope.stretchCompliance=0;
                rope_renderer.thicknessScale = 0.5f;
                }
            else{
                 ropeAttach.compliance=1;
                 rope.stretchCompliance=1;
                 rope_renderer.thicknessScale = 0.0f;
            }
            
           
           if(currentState!=State.Stop){
                rope.bendCompliance=1;
                MoveRig(translationInWorldSpace);
                
           }

           else{
            Rigidbody rb = xrOrigin.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;
            ropeAttach.compliance=1;
            rope.bendCompliance=0;
            rope.DeactivateParticle(0);
            translationInWorldSpace=Vector3.zero;
             m_IsMovingXROrigin = false;
           }
            

            switch (locomotionPhase)
            {
                case LocomotionPhase.Idle:
                case LocomotionPhase.Started:
                    if (m_IsMovingXROrigin)
                        locomotionPhase = LocomotionPhase.Moving;
                    break;
                case LocomotionPhase.Moving:
                    if (!m_IsMovingXROrigin)
                        locomotionPhase = LocomotionPhase.Done;
                    break;
                case LocomotionPhase.Done:
                    locomotionPhase = m_IsMovingXROrigin ? LocomotionPhase.Moving : LocomotionPhase.Idle;
                    break;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(LocomotionPhase)}={locomotionPhase}");
                    break;
            }
        }

        protected abstract Vector2 ReadInput_L();
        protected abstract Vector2 ReadInput_R();


        void reverseTrajectory(){
            var xrOrigin = system.xrOrigin;
            
            List<InputDevice> devices = new List<InputDevice>();
            InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);
            foreach (var item in devices)
            {
                item.TryGetFeatureValue(CommonUsages.gripButton, out bool grip_R);
                bool isReversing=grip_R;
                while(isReversing && gravitaional_force_queue.Count>0){ 
                    item.TryGetFeatureValue(CommonUsages.gripButton, out bool b);
                    isReversing=b;
                    xrOrigin.transform.position =gravitaional_force_queue[gravitaional_force_queue.Count-1];
                    item.SendHapticImpulse(0u, 0.7f,0.05f);
                    gravitaional_force_queue.RemoveAt(gravitaional_force_queue.Count-1);
            }
            }
        }

        void Delay(){Debug.Log("Delay");}

       
        public virtual Vector3 ComputeDesiredMove(Vector2 input_L, Vector2 input_R)
        {

            // if (input_L == Vector2.zero)
            //     return Vector3.zero;

            var xrOrigin = system.xrOrigin;
            if (xrOrigin == null)
                return Vector3.zero;

            
            List<InputDevice> devices = new List<InputDevice>();
            InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);
            foreach (var item in devices)
            {
                if(isLongPressed_R){
                item.SendHapticImpulse(0u, 0.7f,1f);
                isLongPressed_R=false;
                }
                item.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion quaternion);
                ForwardSource_R.forward = quaternion*ForwardSource_R.forward;
                thruster_R.transform.forward = Quaternion.AngleAxis(270,Vector3.up)*right_ray.rayOriginTransform.forward;
                
                item.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 m_joystick_R);                
                if(Mathf.Abs(m_joystick_R.y)>Mathf.Abs(m_joystick_R.x)){gravitational_force_R += m_joystick_R.y;}
                item.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger_R);
               
                Vector3 thrust_scale = Vector3.one;
                thrust_scale.x = gravitational_force_R*10;
                thruster_R.transform.localScale = -thrust_scale;
                item.TryGetFeatureValue(CommonUsages.primaryButton, out bool button_X);
                if(button_X&& rope.stretchCompliance<10){rope.stretchingScale+=0.1f;}
                item.TryGetFeatureValue(CommonUsages.secondaryButton, out bool button_Y);
                if(button_Y&& rope.stretchCompliance>0){rope.stretchingScale-=0.1f;}   
                item.TryGetFeatureValue(CommonUsages.gripButton, out bool grip_R);
                if(grip_R&&currentState==State.Rope){
                    Invoke("reverseTrajectory",2.0f);
                }
                
           
            }

            // Implementation: Get Left Controller Orientation 
            InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristics, devices);

            foreach (var item in devices)
            {
                if(isLongPressed_L){
                item.SendHapticImpulse(0u, 0.7f,1f);
                isLongPressed_L=false;
                ///// 여기에 field 크기 바꾸는 코드
                
                }
                item.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion quaternion);
                ForwardSource_L.forward = quaternion*ForwardSource_L.forward;
                thruster_L.transform.forward = Quaternion.AngleAxis(-90,Vector3.up)*left_ray.rayOriginTransform.forward;

                item.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 m_joystick_L);
                if(Mathf.Abs(m_joystick_L.y)>Mathf.Abs(m_joystick_L.x)){gravitational_force_L += m_joystick_L.y;}
                Vector3 thrust_scale = Vector3.one;
                thrust_scale.x = gravitational_force_R*10;
                thruster_L.transform.localScale = -thrust_scale;
                item.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger_L);
                // if(trigger_L){field_renderer.enabled=true;}
                // else{field_renderer.enabled=false;}
                item.TryGetFeatureValue(CommonUsages.primaryButton, out bool button_A);
                if(button_A &&rope.bendCompliance<10){rope.stretchingScale+=0.1f;}
                item.TryGetFeatureValue(CommonUsages.secondaryButton, out bool button_B);
                if(button_B&& rope.bendCompliance>0){rope.stretchingScale-=0.1f;}
                item.TryGetFeatureValue(CommonUsages.gripButton, out bool grip_L);
                // if(grip_L){ 
                //     field_renderer.enabled=true;
                //     field.transform.localScale=Vector3.one*rope.CalculateLength()*rope.stretchingScale*180;
                // }
                // else{
                //     field_renderer.enabled=false;
                // }   
            }



            var inputMove_R = Vector3.ClampMagnitude(new Vector3(0f, 0f, 1), 1f);
            var inputMove_L = Vector3.ClampMagnitude(new Vector3(0f, 0f, 1), 1f);

            //Debug.Log("Left Forward Source:" + m_ForwardSource_L.forward);
            //Debug.Log("Right Forward Source:" + m_ForwardSource_R.forward);

            // Determine frame of reference for what the input direction is relative to    
            var forwardSourceTransform_R = m_ForwardSource_R == null ? xrOrigin.Camera.transform : m_ForwardSource_R;
            var inputForwardInWorldSpace_R = forwardSourceTransform_R.forward;

            var forwardSourceTransform_L = m_ForwardSource_L == null ? xrOrigin.Camera.transform : m_ForwardSource_L;
            var inputForwardInWorldSpace_L = forwardSourceTransform_L.forward;

            var originTransform = xrOrigin.Origin.transform;
            var speedFactor = m_MoveSpeed * Time.deltaTime * originTransform.localScale.x; // Adjust speed with user scale

            var originUp = originTransform.up;

            if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(inputForwardInWorldSpace_R, originUp)), 1f))
            {
                inputForwardInWorldSpace_R = -forwardSourceTransform_R.up;
            }
            if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(inputForwardInWorldSpace_L, originUp)), 1f))
            {
                inputForwardInWorldSpace_L = -forwardSourceTransform_L.up;
            }

            var inputForwardProjectedInWorldSpace_R = Vector3.ProjectOnPlane(inputForwardInWorldSpace_R, originUp);
            var inputForwardProjectedInWorldSpace_L = Vector3.ProjectOnPlane(inputForwardInWorldSpace_L, originUp);
            var forwardRotation_R = Quaternion.FromToRotation(originTransform.forward, inputForwardProjectedInWorldSpace_R);
            var forwardRotation_L = Quaternion.FromToRotation(originTransform.forward, inputForwardProjectedInWorldSpace_L);
            

            var translationInRigSpace = forwardRotation_R * inputMove_R * speedFactor * gravitational_force_R+forwardRotation_L * inputMove_L * speedFactor * gravitational_force_L;
        
            var translationInWorldSpace = originTransform.TransformDirection(translationInRigSpace);

            
            
            

            return translationInWorldSpace;
        }

        /// <summary>
        /// Creates a locomotion event to move the rig by <paramref name="translationInWorldSpace"/>,
        /// and optionally applies gravity.
        /// </summary>
        /// <param name="translationInWorldSpace">The translation amount in world space to move the rig (pre-gravity).</param>
        protected virtual void MoveRig(Vector3 translationInWorldSpace)
        {
            var xrOrigin = system.xrOrigin?.Origin;
            if (xrOrigin == null)
                return;

            FindCharacterController();

            var motion = translationInWorldSpace;

            if (m_CharacterController != null && m_CharacterController.enabled)
            {

                m_VerticalVelocity = Vector3.zero;


                motion += m_VerticalVelocity * Time.deltaTime;

                if (CanBeginLocomotion() && BeginLocomotion())
                {
                    // Note that calling Move even with Vector3.zero will have an effect by causing isGrounded to update
                    m_IsMovingXROrigin = true;
                    m_CharacterController.Move(motion);
                    EndLocomotion();
                }
            }
            else
            {
                if (CanBeginLocomotion() && BeginLocomotion())
                {
                    m_IsMovingXROrigin = true;
                    xrOrigin.transform.position += motion;
                    EndLocomotion();
                }
            }
        }

        void FindCharacterController()
        {
            var xrOrigin = system.xrOrigin?.Origin;
            if (xrOrigin == null)
                return;

            // Save a reference to the optional CharacterController on the rig GameObject
            // that will be used to move instead of modifying the Transform directly.
            if (m_CharacterController == null && !m_AttemptedGetCharacterController)
            {
                // Try on the Origin GameObject first, and then fallback to the XR Origin GameObject (if different)
                if (!xrOrigin.TryGetComponent(out m_CharacterController) && xrOrigin != system.xrOrigin.gameObject)
                    system.xrOrigin.TryGetComponent(out m_CharacterController);

                m_AttemptedGetCharacterController = true;
            }
        }
    }
}
