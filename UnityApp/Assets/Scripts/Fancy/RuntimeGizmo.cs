using UnityEngine;
using UnityEngine.InputSystem;

public sealed class RuntimeGizmo : MonoBehaviour
{
    public static RuntimeGizmo Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject gizmoRoot;
    [SerializeField] private Collider xHandle;
    [SerializeField] private Collider yHandle;
    [SerializeField] private Collider zHandle;

    [Header("Raycast")]
    [SerializeField] private LayerMask selectableMask = ~0;
    [SerializeField] private LayerMask gizmoMask = ~0;
    [SerializeField] private float rayDistance = 5000f;

    [Header("Gizmo")]
    [SerializeField] private float gizmoScaleFactor = 0.1f;
    [SerializeField] private float minGizmoScale = 0.25f;
    [SerializeField] private float maxGizmoScale = 10f;

    private Transform _selected;
    private Camera _cam;

    private enum Axis { None, X, Y, Z }
    private Axis _activeAxis = Axis.None;

    private Plane _dragPlane;
    private Vector3 _startTargetPos;
    private Vector3 _startHitPointOnPlane;

    #region Unity lifecycle

    private void Awake()
    {
        // --- Singleton enforcement ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _cam = Camera.main;

        if (gizmoRoot != null)
            gizmoRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (_cam == null)
            return;

        UpdateGizmoTransform();

        // Dragging in progress
        if (_activeAxis != Axis.None)
        {
            if (Mouse.current.leftButton.isPressed)
                DragSelectedAlongAxis();
            else
                _activeAxis = Axis.None;

            return;
        }

        // Ignore selection while rotating camera
        if (Mouse.current.rightButton.isPressed)
            return;

        // Try grabbing gizmo handle first
        if (Mouse.current.leftButton.wasPressedThisFrame && TryBeginDragOnHandle())
            return;

        // Otherwise select object
        if (Mouse.current.leftButton.wasPressedThisFrame)
            TrySelectObjectUnderMouse();
    }

    #endregion

    #region Selection

    private void TrySelectObjectUnderMouse()
    {
        var ray = ScreenRay();

        if (Physics.Raycast(ray, out var hit, rayDistance, selectableMask, QueryTriggerInteraction.Ignore))
            _selected = hit.transform;
        else
            _selected = null;
    }

    #endregion

    #region Gizmo

    private void UpdateGizmoTransform()
    {
        if (gizmoRoot == null)
            return;

        if (_selected == null)
        {
            if (gizmoRoot.activeSelf)
                gizmoRoot.SetActive(false);
            return;
        }

        if (!gizmoRoot.activeSelf)
            gizmoRoot.SetActive(true);

        gizmoRoot.transform.position = _selected.position;
        gizmoRoot.transform.rotation = Quaternion.identity;

        float distance = Vector3.Distance(_cam.transform.position, gizmoRoot.transform.position);
        float scale = Mathf.Clamp(distance * gizmoScaleFactor, minGizmoScale, maxGizmoScale);
        gizmoRoot.transform.localScale = Vector3.one * scale;
    }

    private bool TryBeginDragOnHandle()
    {
        if (_selected == null)
            return false;

        var ray = ScreenRay();

        if (!Physics.Raycast(ray, out var hit, rayDistance, gizmoMask, QueryTriggerInteraction.Ignore))
            return false;

        if (hit.collider == xHandle) _activeAxis = Axis.X;
        else if (hit.collider == yHandle) _activeAxis = Axis.Y;
        else if (hit.collider == zHandle) _activeAxis = Axis.Z;
        else return false;

        _startTargetPos = _selected.position;

        Vector3 axisDir = AxisDirectionWorld(_activeAxis);
        Vector3 planeNormal = Vector3.Cross(axisDir, _cam.transform.forward);

        if (planeNormal.sqrMagnitude < 1e-6f)
            planeNormal = Vector3.Cross(axisDir, _cam.transform.up);

        planeNormal.Normalize();
        _dragPlane = new Plane(planeNormal, _startTargetPos);

        if (!_dragPlane.Raycast(ray, out float enter))
        {
            _activeAxis = Axis.None;
            return false;
        }

        _startHitPointOnPlane = ray.GetPoint(enter);
        return true;
    }

    private void DragSelectedAlongAxis()
    {
        if (_selected == null)
        {
            _activeAxis = Axis.None;
            return;
        }

        var ray = ScreenRay();

        if (!_dragPlane.Raycast(ray, out float enter))
            return;

        Vector3 hitPoint = ray.GetPoint(enter);
        Vector3 deltaOnPlane = hitPoint - _startHitPointOnPlane;

        Vector3 axisDir = AxisDirectionWorld(_activeAxis);
        float amount = Vector3.Dot(deltaOnPlane, axisDir);

        _selected.position = _startTargetPos + axisDir * amount;
    }

    #endregion

    #region Utilities

    private Ray ScreenRay()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        return _cam.ScreenPointToRay(mousePos);
    }

    private static Vector3 AxisDirectionWorld(Axis axis)
    {
        return axis switch
        {
            Axis.X => Vector3.right,
            Axis.Y => Vector3.up,
            Axis.Z => Vector3.forward,
            _ => Vector3.zero
        };
    }

    #endregion
}
