using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class VirtualHand : MonoBehaviour
{
    #region Member Variables

    private enum VirtualHandsMethod 
    {
        Snap,
        Reparenting,
        Calculation
    }

    [Header("Input Actions")] 
    public InputActionProperty grabAction;
    public InputActionProperty toggleAction;

    [Header("Configuration")]
    [SerializeField] private VirtualHandsMethod grabMethod;
    public HandCollider handCollider;
    
    // calculation variables
    private GameObject grabbedObject;
    private Matrix4x4 offsetMatrix;

    private bool canGrab
    {
        get
        {
            if (handCollider.isColliding)
                return handCollider.collidingObject.GetComponent<ManipulationSelector>().RequestGrab();
            return false;
        }
    }

    #endregion

    #region MonoBehaviour Callbacks

    private void Start()
    {
        if(GetComponentInParent<NetworkObject>() != null)
            if (!GetComponentInParent<NetworkObject>().IsOwner)
            {
                Destroy(this);
                return;
            }
    }

    private void Update()
    {
        if (toggleAction.action.WasPressedThisFrame())
        {
            grabMethod = (VirtualHandsMethod)(((int)grabMethod + 1) % 3);
        }
        
        switch (grabMethod)
        {
            case VirtualHandsMethod.Snap:
                SnapGrab();
                break;
            case VirtualHandsMethod.Reparenting:
                ReparentingGrab();
                break;
            case VirtualHandsMethod.Calculation:
                CalculationGrab();
                break;
        }
    }

    #endregion

    #region Grab Methods

    private void SnapGrab()
    {
        if (grabAction.action.IsPressed())
        {
            if (grabbedObject == null && handCollider.isColliding && canGrab)
            {
                grabbedObject = handCollider.collidingObject;
            }

            if (grabbedObject != null)
            {
                grabbedObject.transform.position = transform.position;
                grabbedObject.transform.rotation = transform.rotation;
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if(grabbedObject != null)
                grabbedObject.GetComponent<ManipulationSelector>().Release();
            grabbedObject = null;
        }
    }

    private void ReparentingGrab()
    {
        // TODO: your solution for excercise 3.4
        // use this function to implement an object-grabbing that re-parents the object to the hand without snapping
        if (grabAction.action.IsPressed())
        {
            if (grabbedObject == null && handCollider.isColliding && canGrab)
            {
                grabbedObject = handCollider.collidingObject;
                grabbedObject.transform.SetParent(transform, true);
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if (grabbedObject != null)
            {
                grabbedObject.transform.SetParent(null, true);
                grabbedObject.GetComponent<ManipulationSelector>().Release();
                grabbedObject = null;
            }
        }
    }

    private void CalculationGrab()
    {
        // TODO: your solution for excercise 3.4
        // use this function to implement an object-grabbing that uses an offset calculation without snapping (and no re-parenting!)
        if (grabAction.action.IsPressed())
        {
            if (grabbedObject == null && handCollider.isColliding && canGrab)
            {
                grabbedObject = handCollider.collidingObject;
                offsetMatrix = GetTransformationMatrix(grabbedObject.transform).inverse * GetTransformationMatrix(transform);
            }

            if (grabbedObject != null)
            {
                Matrix4x4 currentMatrix = GetTransformationMatrix(transform) * offsetMatrix;
                grabbedObject.transform.SetPositionAndRotation(currentMatrix.GetColumn(3), currentMatrix.rotation);
            }
        }
        else if (grabAction.action.WasReleasedThisFrame())
        {
            if (grabbedObject != null)
            {
                grabbedObject.GetComponent<ManipulationSelector>().Release();
                grabbedObject = null;
            }
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
