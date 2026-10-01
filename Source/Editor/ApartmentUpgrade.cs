using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class ApartmentUpgrade {
 static void Save(){EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();}
 public static void Prepare(){
  AssetDatabase.CopyAsset(ApartmentBuilder.Scene,"Assets/Apartment/BeforeUpgrade.unity");
  var removed=new System.Collections.Generic.List<string>();
  foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
   var b=r.bounds;if(b.min.x>.35f||b.max.x< -12.35f||b.min.z>10.7f||b.max.z< -8.2f||b.min.y>6.45f||b.max.y< -.25f){
    removed.Add(r.name+" "+b.center);var go=r.gameObject;foreach(var c in go.GetComponents<Collider>())UnityEngine.Object.DestroyImmediate(c);var mf=go.GetComponent<MeshFilter>();if(mf)UnityEngine.Object.DestroyImmediate(mf);UnityEngine.Object.DestroyImmediate(r);if(go.transform.childCount==0)UnityEngine.Object.DestroyImmediate(go);continue;
   }
   // Shared meshes avoid duplicated baked vertex buffers and support independent lightmap layouts.
   var filter=r.GetComponent<MeshFilter>();var flags=GameObjectUtility.GetStaticEditorFlags(r.gameObject);flags&=~StaticEditorFlags.BatchingStatic;GameObjectUtility.SetStaticEditorFlags(r.gameObject,flags);
  }
  foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){var p=l.transform.position;if(l.type!=LightType.Directional&&(p.x>.35f||p.z>10.7f||p.z< -8.2f||p.y<0))UnityEngine.Object.DestroyImmediate(l);}
  File.WriteAllLines("Inspection/removed-exterior.txt",removed);
  var exp=UnityEngine.Object.FindFirstObjectByType<ApartmentExperience>();var theme=exp.GetComponent<ApartmentLighting>()??exp.gameObject.AddComponent<ApartmentLighting>();theme.viewCamera=exp.viewCamera;
  theme.surfaces=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.InstanceID);
  Lightmapping.lightingSettings.lightmapResolution=14;Lightmapping.lightingSettings.indirectSampleCount=192;Lightmapping.lightingSettings.maxBounces=5;
  Save();Debug.Log("APARTMENT trimmed renderers="+removed.Count);
 }
 public static void SetBake(string value){
  bool night=value=="night";Lightmapping.lightingSettings.directionalityMode=LightmapsMode.NonDirectional;
  foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){
   l.enabled=true;l.lightmapBakeType=LightmapBakeType.Baked;
   if(l.name.StartsWith("Web daylight")){l.intensity=night?.1f:2.7f;l.color=night?new Color(.3f,.43f,.8f):new Color(1,.965f,.89f);}
   else if(l.name.StartsWith("Web bounce")){l.intensity=night?.35f:.8f;l.color=night?new Color(1,.78f,.6f):new Color(1,.97f,.91f);}
   else if(l.type==LightType.Directional){l.intensity=night?.035f:.8f;l.color=night?new Color(.34f,.46f,.8f):new Color(1,.97f,.92f);}
   else{l.enabled=false;l.lightmapBakeType=LightmapBakeType.Realtime;l.intensity=night?2.7f:.8f;l.color=night?new Color(1,.78f,.56f):new Color(1,.95f,.84f);l.range=night?8:9;}
  }
  RenderSettings.ambientSkyColor=night?new Color(.07f,.09f,.15f):new Color(.62f,.67f,.73f);RenderSettings.ambientEquatorColor=night?new Color(.1f,.065f,.04f):new Color(.49f,.47f,.43f);RenderSettings.ambientGroundColor=night?new Color(.03f,.025f,.02f):new Color(.27f,.28f,.3f);Save();
 }
 public static void CaptureBake(string value){
  var theme=UnityEngine.Object.FindFirstObjectByType<ApartmentLighting>();var folder="Assets/Apartment/Ambient-"+value;if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Apartment","Ambient-"+value);
  var indices=theme.surfaces.Select(r=>r?r.lightmapIndex:-1).ToArray();var offsets=theme.surfaces.Select(r=>r?r.lightmapScaleOffset:Vector4.zero).ToArray();
  var maps=LightmapSettings.lightmaps.Select((m,i)=>{string dest=folder+"/Lightmap-"+i+".exr";if(AssetDatabase.LoadAssetAtPath<Texture2D>(dest))AssetDatabase.DeleteAsset(dest);AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(m.lightmapColor),dest);return AssetDatabase.LoadAssetAtPath<Texture2D>(dest);}).ToArray();
  theme.probes=UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.InstanceID);
  var cubes=theme.probes.Select((p,i)=>{var src=AssetDatabase.GetAssetPath(p.bakedTexture);if(string.IsNullOrEmpty(src)||!src.StartsWith("Assets/"))return p.bakedTexture as Cubemap;string dest=folder+"/Reflection-"+i+Path.GetExtension(src);if(AssetDatabase.LoadAssetAtPath<Cubemap>(dest))AssetDatabase.DeleteAsset(dest);AssetDatabase.CopyAsset(src,dest);return AssetDatabase.LoadAssetAtPath<Cubemap>(dest);}).ToArray();
  if(value=="night")theme.nightProbes=cubes;else theme.dayProbes=cubes;

  if(value=="night"){theme.nightMaps=maps;theme.nightIndices=indices;theme.nightOffsets=offsets;}else{theme.dayMaps=maps;theme.dayIndices=indices;theme.dayOffsets=offsets;}
  Save();Debug.Log("APARTMENT captured "+value+" lightmaps="+maps.Length);
 }
 public static void CaptureViews(){
  var theme=UnityEngine.Object.FindFirstObjectByType<ApartmentLighting>();var exp=UnityEngine.Object.FindFirstObjectByType<ApartmentExperience>();var cam=exp.viewCamera;var oldPos=cam.transform.position;var oldRot=cam.transform.rotation;
  var rt=new RenderTexture(1600,900,24);var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);var active=RenderTexture.active;var old=cam.targetTexture;
  try{cam.targetTexture=rt;foreach(var mood in new[]{"day","night"}){theme.Theme(mood);var fixtures=UnityEngine.Object.FindFirstObjectByType<ApartmentFixtures>();if(fixtures){fixtures.DefaultMood(mood);foreach(var f in fixtures.fixtures)f.level=mood=="night"?.5f:.25f;fixtures.ApplyImmediate();}for(int i=0;i<exp.viewpoints.Length;i++){cam.transform.position=exp.viewpoints[i];cam.transform.eulerAngles=exp.angles[i];cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();var bytes=tex.EncodeToJPG(84);File.WriteAllBytes("../apartment-web/assets/"+(mood=="day"?"room-":"night-")+i+".jpg",bytes);if(i==0)File.WriteAllBytes("../apartment-web/assets/apartment"+(mood=="night"?"-night":"")+".jpg",bytes);}}}
  finally{cam.targetTexture=old;RenderTexture.active=active;cam.transform.position=oldPos;cam.transform.rotation=oldRot;UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);theme.Theme("day");Save();}
 }
 public static void Finish(){Lightmapping.lightingDataAsset=null;var theme=UnityEngine.Object.FindFirstObjectByType<ApartmentLighting>();theme.Theme("day");SetBake("day");var fixtures=UnityEngine.Object.FindFirstObjectByType<ApartmentFixtures>();if(fixtures){fixtures.DefaultMood("day");fixtures.ApplyImmediate();}QualitySettings.pixelLightCount=2;Save();}
}
