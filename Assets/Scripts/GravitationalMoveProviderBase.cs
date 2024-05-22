using System.Collections.Generic;
using System.Numerics;
using UnityEngine.Assertions;
using UnityEngine.XR;
namespace UnityEngine.XR.Interaction.Toolkit
{

    public abstract class GravitationalMoveProviderBase : LocomotionProvider
    {
        public enum GravityApplicationMode
        {
            AttemptingMove,
            Immediately,
        }

        [SerializeField]
        [Tooltip("The speed, in units per second, to move forward.")]
        protected float m_MoveSpeed = 1f;
        public float moveSpeed
        {
            get => m_MoveSpeed;
            set => m_MoveSpeed = value;
        }



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

        List<float> gravitaional_force_queue;



        CharacterController m_CharacterController;

        bool m_AttemptedGetCharacterController;

        bool m_IsMovingXROrigin;

        Vector3 m_VerticalVelocity;

        public GameObject XROrigin;
        private UnityEngine.XR.Interaction.Toolkit.ValueMonitoring m_VM;

        // void Start()
        // {
        //     m_VM = XROrigin.GetComponent<ValueMonitoring>();
        // }


        protected void Update()
        {
            m_IsMovingXROrigin = false;
            AccumulateRotation(m_ForwardSource_R);
            AccumulateRotation(m_ForwardSource_L);

            var xrOrigin = system.xrOrigin?.Origin;
            if (xrOrigin == null)
                return;

            var input_L = ReadInput_L();
            var input_R = ReadInput_R();
            var translationInWorldSpace = ComputeDesiredMove(input_L, input_R);

            // Debug.Log("Button X:"+m_VM.button_X_L);
            // Debug.Log("Button Y:"+m_VM.button_Y_L);
            // Debug.Log("Button A:"+m_VM.button_A_R);
            // Debug.Log("Button B:"+m_VM.button_B_R);

            MoveRig(translationInWorldSpace);

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

        Quaternion accumulatedRotation = Quaternion.identity;

        void AccumulateRotation(Transform forwardSource)
        {
            if (forwardSource == null)
                return;

            List<InputDevice> devices = new List<InputDevice>();
            InputDeviceCharacteristics controllerCharacteristics = forwardSource == m_ForwardSource_R ?
                InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller :
                InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;

            InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

            foreach (var device in devices)
            {
                // Read input values and accumulate rotations
                Quaternion deviceRotation=Quaternion.identity;
                
                if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 input))
                {
                    float turnedAmount=input.magnitude * (Mathf.Sign(input.x) * 100f * Time.deltaTime);
                    deviceRotation *= Quaternion.AngleAxis(turnedAmount,XROrigin.transform.up);
                    accumulatedRotation *= deviceRotation;
                }

                // Other input reading and rotation accumulation code...
            }
        }
        protected virtual Vector3 ComputeDesiredMove(Vector2 input_L, Vector2 input_R)
        {

            // if (input_L == Vector2.zero)
            //     return Vector3.zero;

            var xrOrigin = system.xrOrigin;
            if (xrOrigin == null)
                return Vector3.zero;

            // Implementation: Get Right Controller Orientation 
            List<InputDevice> devices = new List<InputDevice>();
            InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, devices);
            foreach (var item in devices)
            {
                item.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion quaternion);
                Debug.Log("right rotation: " + quaternion);
                //m_ForwardSource_R.rotation = quaternion * turned;
                item.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 m_joystick_R);
                //turnedAmount += m_joystick_R.magnitude * (Mathf.Sign(m_joystick_R.x) * 60f * Time.deltaTime);
                //turned *= Quaternion.AngleAxis(turnedAmount, xrOrigin.transform.up);

                gravitational_force_R += m_joystick_R.y;
                Debug.Log("gravity_R:" + gravitational_force_R);
                
            }


            // Implementation: Get Left Controller Orientation 
            InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristics, devices);

            foreach (var item in devices)
            {
                item.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion quaternion);
                Debug.Log("left rotation: " + quaternion);
               // m_ForwardSource_L.rotation = quaternion * turned;
                item.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 m_joystick_L);
                //turnedAmount += m_joystick_L.magnitude * (Mathf.Sign(m_joystick_L.x) * 60f * Time.deltaTime);
                //turned *= Quaternion.AngleAxis(turnedAmount, xrOrigin.transform.up);
                    gravitational_force_L += m_joystick_L.y;
                    Debug.Log("gravity_L:" + gravitational_force_L);
            }

            var inputMove_R = Vector3.ClampMagnitude(new Vector3(0f, 0f, 1), 1f);
            var inputMove_L = Vector3.ClampMagnitude(new Vector3(0f, 0f, 1), 1f);

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

            var translationInRigSpace_R = forwardRotation_R * inputMove_R * speedFactor * gravitational_force_R;
            var translationInRigSpace_L = forwardRotation_L * inputMove_L * speedFactor * gravitational_force_L;
            Debug.Log("Left Movement:" + translationInRigSpace_L+"mag:"+translationInRigSpace_L.magnitude);
            Debug.Log("Right Movement:" + translationInRigSpace_R+"mag:"+translationInRigSpace_R.magnitude);
            var translationInWorldSpace = originTransform.TransformDirection(translationInRigSpace_R + translationInRigSpace_L);

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
