using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class ApartmentPolish {
 static Material Material(string name,Color color,float smooth=.25f){string path="Assets/Apartment/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.SetFloat("_Glossiness",smooth);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
 static void Area(string name,Vector3 p,Vector3 direction,Vector2 size,float intensity,Color color){var go=new GameObject(name);go.transform.position=p;go.transform.rotation=Quaternion.LookRotation(direction);var l=go.AddComponent<Light>();l.type=LightType.Rectangle;l.areaSize=size;l.intensity=intensity;l.color=color;l.lightmapBakeType=LightmapBakeType.Baked;l.range=12;l.shadows=LightShadows.Soft;}
 public static void Apply(){
  var clay=Material("Warm ceramic",new Color(.57f,.3f,.18f));var trim=AssetDatabase.LoadAssetAtPath<Material>("Assets/HDRP Archviz Apartment/Models/Materials/Baseboards.mat")??Material("Walnut trim",new Color(.26f,.16f,.1f));var graphite=Material("Projector graphite",new Color(.17f,.19f,.2f));int repaired=0;
  foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)if(!mats[i]||!mats[i].shader||mats[i].shader.name.Contains("Error")){mats[i]=r.name.Contains("Cylinder")?clay:r.name.Contains("Base")?trim:graphite;repaired++;}r.sharedMaterials=mats;}
  foreach(var g in AssetDatabase.FindAssets("t:Material",new[]{"Assets/HDRP Archviz Apartment"})){var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));if(m.name.Contains("HeadPillow")||m.name.Contains("Blanket")||m.name.Contains("Mattress")||m.name.Contains("Duvet")){m.color=new Color(.82f,.79f,.71f);m.SetFloat("_Metallic",0);m.SetFloat("_GlossMapScale",.13f);EditorUtility.SetDirty(m);}}
  foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){if(l.name.StartsWith("Web daylight")||l.name.StartsWith("Web bounce"))Object.DestroyImmediate(l.gameObject);else if(l.type!=LightType.Directional)l.color=new Color(1,.95f,.85f);}
  foreach(float z in new[]{-5.6f,-.2f,3.3f,7.7f})Area("Web daylight "+z,new Vector3(-.65f,3,z),Vector3.left,new Vector2(3.3f,3.8f),3.2f,new Color(.88f,.94f,1));
  Area("Web bounce lounge",new Vector3(-6,3.8f,2.3f),Vector3.down,new Vector2(4,4),.9f,new Color(1,.97f,.9f));
  Area("Web bounce bedroom",new Vector3(-3.7f,3.5f,-6),Vector3.down,new Vector2(3,3),1.1f,new Color(1,.96f,.89f));
  Lightmapping.lightingSettings.indirectSampleCount=256;Lightmapping.lightingSettings.maxBounces=5;EditorUtility.SetDirty(Lightmapping.lightingSettings);
  var exp=Object.FindFirstObjectByType<ApartmentExperience>();exp.eyeHeight=1.7f;
  EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("APARTMENT repaired missing slots="+repaired);
 }
}
