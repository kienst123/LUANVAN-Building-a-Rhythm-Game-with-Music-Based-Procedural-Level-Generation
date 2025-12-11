using UnityEngine;

namespace RhythmGame3D.Core
{
    /// <summary>
    /// Camera controller for 3D rhythm game
    /// Sets up perspective view looking down the lanes
    /// </summary>
    public class CameraController3D : MonoBehaviour
    {
        [Header("Camera Settings")]
        [Tooltip("Distance behind the hit position")]
        public float cameraDistance = 5f;
        
        [Tooltip("Height above the lanes")]
        public float cameraHeight = 3f;
        
        [Tooltip("Angle looking down (degrees)")]
        public float lookDownAngle = 15f;
        
        [Tooltip("Field of view")]
        public float fieldOfView = 60f;
        
        [Header("Target")]
        public Transform targetLookAt;  // Usually the hit position
        
        private Camera cam;
        
        void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = gameObject.AddComponent<Camera>();
            }
            
            SetupCamera();
        }
        
        void SetupCamera()
        {
            // Set camera properties
            cam.fieldOfView = fieldOfView;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);  // Dark blue
            
            // Position camera
            Vector3 position = new Vector3(0, cameraHeight, -cameraDistance);
            transform.position = position;
            
            // Rotate camera to look down
            if (targetLookAt != null)
            {
                transform.LookAt(targetLookAt);
            }
            else
            {
                transform.rotation = Quaternion.Euler(lookDownAngle, 0, 0);
            }
            
            Debug.Log($"[CameraController3D] Camera setup at {position}, FOV: {fieldOfView}");
        }
        
        /// <summary>
        /// Adjust camera for different screen aspects
        /// </summary>
        void Update()
        {
            // Auto-adjust FOV for ultrawide screens
            float aspect = (float)Screen.width / Screen.height;
            if (aspect > 2f)  // Ultrawide
            {
                cam.fieldOfView = fieldOfView * 1.2f;
            }
        }
        
        /// <summary>
        /// Reconfigure camera settings at runtime
        /// </summary>
        public void ReconfigureCamera(float distance, float height, float angle)
        {
            cameraDistance = distance;
            cameraHeight = height;
            lookDownAngle = angle;
            SetupCamera();
        }
    }
}
