using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
public static class ApartmentBuilder {
 public const string Source="Assets/HDRP Archviz Apartment/Scenes/DemoScene.unity", Scene="Assets/Apartment/Apartment.unity";
 [Serializable] public class MaterialRows {public MaterialRow[] materials;}
 [Serializable] public class MaterialRow {public string path,name,baseMap,normalMap,maskMap; public Color color; public float metallic,smoothness,normalScale,transparent,cutout,doubleSided;}
 public static void Convert(){
  foreach(var row in JsonUtility.FromJson<MaterialRows>(File.ReadAllText("Inspection/materials.json")).materials){
   var m=AssetDatabase.LoadAssetAtPath<Material>(row.path);if(!m)continue;
   m.shader=Shader.Find("Standard");m.shaderKeywords=Array.Empty<string>();m.color=row.color;
   m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(row.baseMap);
   m.SetFloat("_Metallic",row.metallic);m.SetFloat("_Glossiness",Mathf.Min(row.smoothness,.7f));
   var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(row.normalMap);if(normal){m.SetTexture("_BumpMap",normal);m.SetFloat("_BumpScale",Mathf.Min(row.normalScale,.8f));m.EnableKeyword("_NORMALMAP");}
   var mask=AssetDatabase.LoadAssetAtPath<Texture2D>(row.maskMap);if(mask){m.SetTexture("_MetallicGlossMap",mask);m.EnableKeyword("_METALLICGLOSSMAP");m.SetFloat("_GlossMapScale",.65f);m.SetTexture("_OcclusionMap",mask);m.SetFloat("_OcclusionStrength",.65f);}
   m.SetFloat("_Mode",0);m.SetInt("_SrcBlend",1);m.SetInt("_DstBlend",0);m.SetInt("_ZWrite",1);m.renderQueue=-1;m.SetOverrideTag("RenderType","");
   if(row.transparent>0){m.SetFloat("_Mode",3);m.SetInt("_SrcBlend",1);m.SetInt("_DstBlend",10);m.SetInt("_ZWrite",0);m.EnableKeyword("_ALPHAPREMULTIPLY_ON");m.renderQueue=3000;m.SetOverrideTag("RenderType","Transparent");var c=m.color;c.a=Mathf.Min(c.a,.25f);m.color=c;}
   else if(row.cutout>0){m.SetFloat("_Mode",1);m.EnableKeyword("_ALPHATEST_ON");m.SetFloat("_Cutoff",.4f);m.renderQueue=2450;m.SetOverrideTag("RenderType","TransparentCutout");}
   m.doubleSidedGI=true;m.enableInstancing=true;EditorUtility.SetDirty(m);
  }
  AssetDatabase.SaveAssets();Debug.Log("APARTMENT materials converted");
 }
 public static void Inspect(){
  var scene=EditorSceneManager.OpenScene(Source);
  var rs=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
  var lines=rs.Select(r=>new {name=r.name,position=r.transform.position,size=r.bounds.size,center=r.bounds.center,mesh=r.GetComponent<MeshFilter>()?.sharedMesh?.name,vertices=r.GetComponent<MeshFilter>()?.sharedMesh?.vertexCount??0});
  File.WriteAllText("Inspection/scene.json",Newtonsoft.Json.JsonConvert.SerializeObject(lines,Newtonsoft.Json.Formatting.Indented,new Newtonsoft.Json.JsonSerializerSettings{ReferenceLoopHandling=Newtonsoft.Json.ReferenceLoopHandling.Ignore}));
  Debug.Log("APARTMENT inspected "+rs.Length+" renderers");
 }
 public static void Prepare(){
  var scene=EditorSceneManager.OpenScene(Source);EditorSceneManager.SaveScene(scene,Scene);
  foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None))GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
  foreach(var c in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(c.gameObject);
  foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){l.lightmapBakeType=LightmapBakeType.Baked;l.intensity=l.type==LightType.Directional?1.1f:1.8f;l.range=9;l.color=new Color(1,.89f,.74f);l.shadows=LightShadows.Soft;}
  foreach(var p in UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)){p.resolution=128;p.mode=ReflectionProbeMode.Baked;p.refreshMode=ReflectionProbeRefreshMode.OnAwake;p.intensity=.65f;p.boxProjection=true;}
  foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){
   r.gameObject.isStatic=true;r.receiveGI=ReceiveGI.Lightmaps;r.scaleInLightmap=r.bounds.size.magnitude<.5f?.3f:1;
   var mf=r.GetComponent<MeshFilter>();if(mf&&mf.sharedMesh&&!r.GetComponent<Collider>()&&r.bounds.size.magnitude>.5f){var c=r.gameObject.AddComponent<MeshCollider>();c.sharedMesh=mf.sharedMesh;}
  }
  var cam=new GameObject("Apartment camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();cam.tag="MainCamera";cam.fieldOfView=65;cam.nearClipPlane=.06f;cam.farClipPlane=100;cam.allowHDR=false;cam.backgroundColor=new Color(.67f,.75f,.8f);cam.clearFlags=CameraClearFlags.SolidColor;
  var exp=new GameObject("ApartmentExperience").AddComponent<ApartmentExperience>();exp.viewCamera=cam;
  exp.SetViewpoints();cam.transform.position=exp.viewpoints[0];cam.transform.eulerAngles=exp.angles[0];
  var body=cam.gameObject.AddComponent<CharacterController>();body.height=1.55f;body.center=new Vector3(0,-.68f,0);body.radius=.18f;body.stepOffset=.15f;body.skinWidth=.015f;
  RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.62f,.69f,.78f);RenderSettings.ambientEquatorColor=new Color(.49f,.46f,.41f);RenderSettings.ambientGroundColor=new Color(.27f,.28f,.3f);RenderSettings.ambientIntensity=1;RenderSettings.reflectionIntensity=.65f;RenderSettings.fog=false;
  QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=2;QualitySettings.pixelLightCount=0;QualitySettings.shadows=ShadowQuality.Disable;
  var settings=new LightingSettings();settings.name="Apartment daylight";settings.autoGenerate=false;settings.bakedGI=true;settings.realtimeGI=false;settings.lightmapper=LightingSettings.Lightmapper.ProgressiveGPU;settings.lightmapResolution=16;settings.lightmapMaxSize=2048;settings.lightmapPadding=4;settings.directionalityMode=LightmapsMode.NonDirectional;settings.compressLightmaps=true;settings.directSampleCount=32;settings.indirectSampleCount=128;settings.environmentSampleCount=64;settings.maxBounces=4;settings.ao=true;settings.aoMaxDistance=.5f;settings.aoExponentIndirect=.7f;
  var path="Assets/Apartment/Daylight.lighting";if(AssetDatabase.LoadAssetAtPath<LightingSettings>(path))AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(settings,path);Lightmapping.lightingSettings=settings;
  Lightmapping.lightingDataAsset=null;LightmapSettings.lightmaps=Array.Empty<LightmapData>();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Scene,true)};
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("APARTMENT prepared");
 }
 public static void Optimize(){
  // Both baked moods are installed at runtime; automatic stripping cannot discover them.
  var graphics=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
  graphics.FindProperty("m_LightmapStripping").intValue=1;
  graphics.FindProperty("m_LightmapKeepPlain").boolValue=true;
  foreach(var key in new[]{"m_LightmapKeepDirCombined","m_LightmapKeepDynamicPlain","m_LightmapKeepDynamicDirCombined","m_LightmapKeepShadowMask","m_LightmapKeepSubtractive"})graphics.FindProperty(key).boolValue=false;
  graphics.ApplyModifiedPropertiesWithoutUndo();

  var t=NamedBuildTarget.WebGL;PlayerSettings.companyName="Backbenchers";PlayerSettings.productName="The Apartment";PlayerSettings.bundleVersion="1.0.0";PlayerSettings.colorSpace=ColorSpace.Linear;
  PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL,new[]{GraphicsDeviceType.WebGPU,GraphicsDeviceType.OpenGLES3});
  PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Brotli;PlayerSettings.WebGL.decompressionFallback=true;PlayerSettings.WebGL.dataCaching=true;PlayerSettings.WebGL.initialMemorySize=128;PlayerSettings.WebGL.maximumMemorySize=1024;PlayerSettings.WebGL.memoryGrowthMode=WebGLMemoryGrowthMode.Geometric;PlayerSettings.WebGL.exceptionSupport=WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;PlayerSettings.WebGL.debugSymbolMode=WebGLDebugSymbolMode.Off;PlayerSettings.WebGL.wasm2023=false;PlayerSettings.WebGL.threadsSupport=false;
  PlayerSettings.SetStaticBatchingForPlatform(BuildTarget.WebGL,true);PlayerSettings.stripEngineCode=true;PlayerSettings.stripUnusedMeshComponents=false;PlayerSettings.SetManagedStrippingLevel(t,ManagedStrippingLevel.High);PlayerSettings.SetIl2CppCodeGeneration(t,Il2CppCodeGeneration.OptimizeSize);UnityEditor.WebGL.UserBuildSettings.codeOptimization=UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;PlayerSettings.runInBackground=true;AssetDatabase.SaveAssets();
 }
 public static void Capture(){
  var c=Camera.main;var rt=new RenderTexture(1600,900,24);var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);var old=c.targetTexture;var active=RenderTexture.active;
  try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes("../apartment-web/assets/apartment.jpg",tex.EncodeToJPG(88));}finally{c.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
 }
 public static void Build(){
  foreach(var renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var go=renderer.gameObject;GameObjectUtility.SetStaticEditorFlags(go,GameObjectUtility.GetStaticEditorFlags(go)&~StaticEditorFlags.BatchingStatic);PrefabUtility.RecordPrefabInstancePropertyModifications(go);}
  EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();Optimize();var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene},locationPathName="../apartment-web/unity",target=BuildTarget.WebGL,options=BuildOptions.None});File.WriteAllText("Inspection/build-result.txt",r.summary.result+"\n"+r.summary.totalSize+"\n"+r.summary.totalErrors);Debug.Log("APARTMENT BUILD "+r.summary.result);if(r.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Apartment build did not succeed: "+r.summary.result);
 }
}
