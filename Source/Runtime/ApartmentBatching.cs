using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
[DefaultExecutionOrder(100)]
public class ApartmentBatching:MonoBehaviour {
 #if UNITY_WEBGL && !UNITY_EDITOR
 [DllImport("__Internal")]static extern void ApartmentBatchReady(int count);
 #endif
 void Start(){
  var lighting=GetComponent<ApartmentLighting>();if(lighting)lighting.Theme(lighting.current);
  if(Application.absoluteURL.Contains("batch=0"))return;
  var rs=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>!r.name.StartsWith("LightBulb")&&r.GetComponent<MeshFilter>()&&r.GetComponent<MeshFilter>().sharedMesh&&r.GetComponent<MeshFilter>().sharedMesh.isReadable).ToArray();
  var sources=rs.Select(r=>r.GetComponent<MeshFilter>().sharedMesh).Distinct().ToArray();
  StaticBatchingUtility.Combine(rs.Select(r=>r.gameObject).ToArray(),null);
  #if UNITY_WEBGL && !UNITY_EDITOR
  ApartmentBatchReady(rs.Count(r=>r.isPartOfStaticBatch));
  #endif
  foreach(var mesh in sources)if(mesh&&mesh.isReadable)mesh.UploadMeshData(true);
 }
}
