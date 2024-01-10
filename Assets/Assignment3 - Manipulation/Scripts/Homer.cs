using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Homer : MonoBehaviour
{
    #region Member Variables

    [Header("H.O.M.E.R. Components")] 
    public Transform head;
    public float originHeadOffset = 0.2f;
    public Transform hand;

    [Header("H.O.M.E.R. Parameters")] 
    public LineRenderer ray;
    public float rayMaxLength = 100f;
    public LayerMask layerMask; // use this mask to raycast only for interactable objects
    
    [Header("Input Actions")] 
    public InputActionProperty grabAction;

    [Header("Grab Configuration")]
    public HandCollider handCollider;

    // grab calculation variables
    private GameObject grabbedObject;
    private Matrix4x4 offsetMatrix;
    
    // utility bool to check if you can grab an object
    private bool canGrab
    {
        get
        {
            if (handCollider.isColliding)
                return handCollider.collidingObject.GetComponent<ManipulationSelector>().RequestGrab();
            return false;
        }
    }
    
    // variables needed for hand offset calculation
    private RaycastHit hit;
    private float grabOffsetDistance;
    private float grabHandDistance;
    
    // convenience variables for hand offset calculations
    private Vector3 origin
    {
        get
        {
            Vector3 v = head.position;
            v.y -= originHeadOffset;
            return v;
        }
    }
    private Vector3 direction => hand.position - origin;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        ray.enabled = enabled;
    }

    private void Start()
    {
        if(GetComponentInParent<NetworkObject>() != null)
            if (!GetComponentInParent<NetworkObject>().IsOwner)
            {
                Destroy(this);
                return;
            }

        ray.positionCount = 2;
    }

    private void Update()
    {
        if (grabbedObject == null)
            UpdateRay();
        else
            ApplyHandOffset();

        GrabCalculation();
    }

    #endregion

    #region Custom Methods

    private void UpdateRay()
    {
        //TODO: your solution for excercise 3.5
        // use this function to calculate and adjust the ray of the h.o.m.e.r. technique
        ray.SetPosition(0, hand.position);
        Vector3 endPoint = hand.position + direction.normalized * rayMaxLength;
        ray.SetPosition(1, endPoint);

        if (Physics.Raycast(hand.position, direction, out hit, rayMaxLength, layerMask))
        {
            ray.SetPosition(1, hit.point);
        }
    }

    private void ApplyHandOffset()
    {
        //TODO: your solution for excercise 3.5
        // use this function to calculate and adjust the hand as described in the h.o.m.e.r. technique
        if (grabbedObject != null)
        {
            Vector3 newHandPosition = origin + direction.normalized * grabOffsetDistance;
            hand.position = newHandPosition;
        }
    }

    private void GrabCalculation()
    {
        // TODO: your solution for excercise 3.5
        // use this function to calculate the grabbing of an object
        if (grabAction.action.WasPressedThisFrame() && hit.collider != null && canGrab)
        {
            grabbedObject = hit.collider.gameObject;
            grabOffsetDistance = Vector3.Distance(origin, grabbedObject.transform.position);
            grabHandDistance = Vector3.Distance(hand.position, origin);
        }

        if (grabbedObject != null)
        {
            Vector3 newPosition = origin + direction.normalized * grabOffsetDistance;
            grabbedObject.transform.position = newPosition;
            // Additional code to handle object rotation
        }

        if (grabAction.action.WasReleasedThisFrame() && grabbedObject != null)
        {
            // Release the object
            grabbedObject = null;
        }
    }

    #endregion
    
    #region Utility Functions

    public Matrix4x4 GetTransformationMatrix(Transform t, bool inWorldSpace = true)
    {
        if (inWorldSpace)
        {
            return Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
        }
        else
        {
            return Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale);
        }
    }

    #endregion
}
