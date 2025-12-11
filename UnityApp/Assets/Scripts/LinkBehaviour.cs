using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LinkBehaviour : MonoBehaviour
{
    private NodeBehaviour _a;
    private NodeBehaviour _b;
    private LineRenderer _line;

    public void Initialize(NodeBehaviour a, NodeBehaviour b)
    {
        _a = a;
        _b = b;
        _line = GetComponent<LineRenderer>();
        UpdatePositions();
    }

    private void LateUpdate()
    {
        if (_a == null || _b == null)
        {
            Destroy(gameObject);
            return;
        }
        UpdatePositions();
    }

    private void UpdatePositions()
    {
        _line.SetPosition(0, _a.transform.position);
        _line.SetPosition(1, _b.transform.position);
    }
}
