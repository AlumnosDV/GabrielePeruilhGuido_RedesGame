using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RedesGame.Player;

namespace RedesGame.Managers
{
    public class DynamicCameraController : MonoBehaviour
    {
        [Tooltip("Camera to control. Leave empty to use the Camera component on the same GameObject.")]
        [SerializeField] private Camera targetCamera;

        [Header("Position")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float smoothTime = 0.2f;

        [Header("Zoom")]
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 15f;
        [SerializeField] private float distanceForMaxZoom = 25f;
        [SerializeField] private float distanceForMinZoom = 5f;
        [SerializeField] private float zoomSmoothing = 5f;

        [Header("Players")]
        [SerializeField] private float targetsRefreshInterval = 0.5f;

        private readonly List<PlayerModel> _players = new();
        private Camera _camera;
        private Vector3 _currentVelocity;
        private float _nextRefreshTime;

        private void Awake()
        {
            _camera = targetCamera != null ? targetCamera : GetComponent<Camera>();

            if (_camera == null)
            {
                Debug.LogError("DynamicCameraController: No camera found. Assign a Camera reference or place the script on a Camera object.");
            }
        }

        private void OnEnable()
        {
            RefreshTargets();
            EventManager.StartListening("PlayerEliminated", OnPlayerStateChanged);
            EventManager.StartListening("MatchStarted", OnPlayerStateChanged);
        }

        private void OnDisable()
        {
            EventManager.StopListening("PlayerEliminated", OnPlayerStateChanged);
            EventManager.StopListening("MatchStarted", OnPlayerStateChanged);
        }

        private void LateUpdate()
        {
            if (Time.time >= _nextRefreshTime)
            {
                RefreshTargets();
            }

            if (_players.Count == 0)
                return;

            MoveCamera();
            AdjustZoom();
        }

        private void RefreshTargets()
        {
            _players.Clear();
            _players.AddRange(FindObjectsOfType<PlayerModel>()
                .Where(p => p != null && p.IsActive && !p.IsDead));

            _nextRefreshTime = Time.time + targetsRefreshInterval;
        }

        private void OnPlayerStateChanged(object[] obj)
        {
            RefreshTargets();
        }

        private void MoveCamera()
        {
            Vector3 centerPoint = GetBoundsCenter();
            Vector3 targetPosition = centerPoint + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, smoothTime);
        }

        private void AdjustZoom()
        {
            float distance = GetGreatestDistance();
            float lerpValue = Mathf.InverseLerp(distanceForMinZoom, distanceForMaxZoom, distance);
            lerpValue = Mathf.Clamp01(lerpValue);
            float targetZoom = Mathf.Lerp(minZoom, maxZoom, lerpValue);

            if (_camera.orthographic)
            {
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, targetZoom, zoomSmoothing * Time.deltaTime);
            }
            else
            {
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetZoom * 10f, zoomSmoothing * Time.deltaTime);
            }
        }

        private Vector3 GetBoundsCenter()
        {
            if (_players.Count == 1)
                return _players[0].transform.position;

            var bounds = new Bounds(_players[0].transform.position, Vector3.zero);
            foreach (var player in _players)
            {
                bounds.Encapsulate(player.transform.position);
            }

            return bounds.center;
        }

        private float GetGreatestDistance()
        {
            if (_players.Count <= 1)
                return 0f;

            var bounds = new Bounds(_players[0].transform.position, Vector3.zero);
            foreach (var player in _players)
            {
                bounds.Encapsulate(player.transform.position);
            }

            return Mathf.Max(bounds.size.x, bounds.size.y);
        }
    }
}
