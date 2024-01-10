using Unity.Netcode;

public class ManipulationSelector : NetworkBehaviour
{
    #region Member Variables

    private NetworkVariable<bool> isGrabbed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    #endregion

    #region Selector Methods

    public bool RequestGrab()
    {
        // TODO: your solution for excercise 3.8
        // check if object can be grabbed by a user
        // trigger ownership handling
        // trigger grabbed state update
        if (isGrabbed.Value)
        {
            // Object is already grabbed by someone else
            return false;
        }
        else
        {
            ChangeOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
            isGrabbed.Value = true;
            return true;
        }
        //return true; // <-- this is just a placeholder, determine the actual return value by your implemented policy
    }

    public void Release()
    {
        // TODO: your solution for excercise 3.8
        // use this function trigger a grabbed state update on object release
        isGrabbed.Value = false;
        // Additional logic may be required here to handle network updates
    }

    #endregion

    #region RPCs

    // TODO: your solution for excercise 3.8
    // implement a rpc to transfer the ownership of an object 
    // implement a rpc to update the isGrabbed value
    [ServerRpc]
    void ChangeOwnershipServerRpc(ulong newOwnerId)
    {
        if (IsServer)
        {
            var networkObject = GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.ChangeOwnership(newOwnerId);
            }
        }
    }
    [ClientRpc]
    void UpdateGrabbedStateClientRpc(bool grabbed)
    {
        isGrabbed.Value = grabbed;
    }


    #endregion
}
