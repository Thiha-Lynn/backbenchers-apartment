using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class ApartmentFixtureSetup {
 public static void Prepare(){
  var exp=Object.FindFirstObjectByType<ApartmentExperience>();var control=exp.GetComponent<ApartmentFixtures>();if(!control)control=exp.gameObject.AddComponent<ApartmentFixtures>();if(!exp.GetComponent<ApartmentBatching>())exp.gameObject.AddComponent<ApartmentBatching>();control.viewCamera=exp.viewCamera;
  var previous=control.fixtures==null?new System.Collections.Generic.Dictionary<Light,string>():control.fixtures.Where(f=>f.source).ToDictionary(f=>f.source,f=>f.label);
  var lights=Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.type==LightType.Point||l.type==LightType.Spot).OrderBy(l=>l.transform.position.z).ThenBy(l=>l.transform.position.x).ToArray();
  var bulbs=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("LightBulb")&&!r.name.Contains("Wire")).ToArray();
  control.fixtures=lights.Select((light,i)=>{var p=light.transform.position;string room=p.z< -2?"Bedroom pendant":p.x< -9?"Bathroom pendant":p.y<2?(p.z>8?"Entrance floor lamp":"Reading floor lamp"):light.type==LightType.Spot?"Living room spotlight":p.z<2?(p.x> -4?"Kitchen pendant":"Living room pendant"):p.z<7?"Artist’s corner pendant":p.x> -4?"Games room pendant":"Entrance pendant";
   if(previous.TryGetValue(light,out var label))room=label;
   light.lightmapBakeType=LightmapBakeType.Realtime;light.shadows=LightShadows.None;light.renderMode=LightRenderMode.Auto;light.range=7;light.enabled=false;light.bounceIntensity=0;light.cullingMask=~0;
   var globe=light.gameObject.GetComponent<SphereCollider>();if(!globe)globe=light.gameObject.AddComponent<SphereCollider>();globe.isTrigger=true;globe.radius=.28f;
   var rs=bulbs.Where(r=>Vector3.Distance(r.bounds.center,p)<.6f).Cast<Renderer>().ToArray();
   if(rs.Length>0){
    // Emit at the visible bulb; the shade directs the beam away from the ceiling.
    var center=rs[0].bounds.center;var direction=p.y<2?(p-center).normalized:Vector3.down;
    if(direction.sqrMagnitude<.01f)direction=light.transform.forward;
    light.transform.position=center;light.transform.rotation=Quaternion.LookRotation(direction);
    light.type=LightType.Spot;light.spotAngle=p.y<2?110:125;light.innerSpotAngle=p.y<2?65:85;light.range=p.y<2?5:7;
    string path="Assets/Apartment/Fixture-glow-"+i+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(mat,path);}mat.color=new Color(.65f,.65f,.6f);mat.SetFloat("_Glossiness",.25f);mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",Color.black);mat.globalIlluminationFlags=MaterialGlobalIlluminationFlags.EmissiveIsBlack;foreach(var r in rs)r.sharedMaterial=mat;EditorUtility.SetDirty(mat);}
   return new ApartmentFixtures.Fixture{label=room,source=light,bulbs=rs};}).ToArray();
  System.IO.File.WriteAllText("Inspection/fixture-labels.json",Newtonsoft.Json.JsonConvert.SerializeObject(control.fixtures.Select(f=>f.label)));
  QualitySettings.pixelLightCount=2;EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("Adjustable fixtures="+lights.Length+" bulb renderers="+bulbs.Length);
 }
}
