using UnityEngine;

public class BoundingBox : MonoBehaviour
{
    #region Serialized fields

    #endregion

    #region Unity events

    private void OnDrawGizmosSelected()
    {
        var mat = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = mat;
    }

    private void Update()
    {
        Shader.SetGlobalMatrix("_BoundingBoxMat", transform.worldToLocalMatrix);
    }
        
    #endregion
}
