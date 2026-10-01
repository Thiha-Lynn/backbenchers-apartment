using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
public static class ApartmentMaterialCleanup {
 public static void Apply(){
  int count=0;
  foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{"Assets/HDRP Archviz Apartment","Assets/Apartment"})){
   var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));if(!m||m.shader.name!="Standard")continue;
   var clean=new Material(m.shader);clean.name=m.name;
   for(int i=0;i<m.shader.GetPropertyCount();i++){
    string p=m.shader.GetPropertyName(i);switch(m.shader.GetPropertyType(i)){
     case ShaderPropertyType.Color:clean.SetColor(p,m.GetColor(p));break;
     case ShaderPropertyType.Vector:clean.SetVector(p,m.GetVector(p));break;
     case ShaderPropertyType.Float:case ShaderPropertyType.Range:clean.SetFloat(p,m.GetFloat(p));break;
     case ShaderPropertyType.Int:clean.SetInteger(p,m.GetInteger(p));break;
     case ShaderPropertyType.Texture:clean.SetTexture(p,m.GetTexture(p));clean.SetTextureScale(p,m.GetTextureScale(p));clean.SetTextureOffset(p,m.GetTextureOffset(p));break;
    }
   }
   clean.shaderKeywords=m.shaderKeywords;clean.renderQueue=m.renderQueue;clean.enableInstancing=true;clean.doubleSidedGI=m.doubleSidedGI;clean.globalIlluminationFlags=m.globalIlluminationFlags;
   if(m.name=="Covers"||m.name=="HeadPillow"){clean.color=new Color(.82f,.79f,.71f);clean.SetFloat("_GlossMapScale",.13f);}
   if(clean.GetFloat("_Mode")==3)clean.SetOverrideTag("RenderType","Transparent");else if(clean.GetFloat("_Mode")==1)clean.SetOverrideTag("RenderType","TransparentCutout");
   EditorUtility.CopySerialized(clean,m);Object.DestroyImmediate(clean);EditorUtility.SetDirty(m);count++;
  }
  AssetDatabase.SaveAssets();Debug.Log("APARTMENT stripped obsolete HDRP properties from "+count+" materials");
 }
}
