using UnityEngine;
public class ApartmentLighting:MonoBehaviour {
 public Texture2D[] dayMaps,nightMaps;
 public ReflectionProbe[] probes;public Cubemap[] dayProbes,nightProbes;
 public Renderer[] surfaces;
 public int[] dayIndices,nightIndices;
 public Vector4[] dayOffsets,nightOffsets;
 public Camera viewCamera;
 public string current="day";
 public void Theme(string value){
  bool night=value=="night";var maps=night?nightMaps:dayMaps;if(maps==null||maps.Length==0)return;
  LightmapSettings.lightmapsMode=LightmapsMode.NonDirectional;
  var data=new LightmapData[maps.Length];for(int i=0;i<maps.Length;i++)data[i]=new LightmapData{lightmapColor=maps[i]};LightmapSettings.lightmaps=data;
  var indices=night?nightIndices:dayIndices;var offsets=night?nightOffsets:dayOffsets;
  for(int i=0;i<surfaces.Length;i++)if(surfaces[i]){surfaces[i].lightmapIndex=indices[i];if(!surfaces[i].isPartOfStaticBatch)surfaces[i].lightmapScaleOffset=offsets[i];}
  RenderSettings.ambientSkyColor=night?new Color(.1f,.12f,.2f):new Color(.62f,.67f,.73f);
  RenderSettings.ambientEquatorColor=night?new Color(.16f,.11f,.065f):new Color(.49f,.47f,.43f);
  RenderSettings.ambientGroundColor=night?new Color(.055f,.04f,.03f):new Color(.27f,.28f,.3f);
  RenderSettings.reflectionIntensity=night?.18f:.6f;
  var cubes=night?nightProbes:dayProbes;if(probes!=null&&cubes!=null)for(int i=0;i<probes.Length;i++)if(probes[i]){if(i<cubes.Length&&cubes[i])probes[i].bakedTexture=cubes[i];probes[i].intensity=night?.4f:.6f;}
  if(viewCamera)viewCamera.backgroundColor=night?new Color(.018f,.025f,.05f):new Color(.67f,.74f,.8f);
  var fixtures=GetComponent<ApartmentFixtures>();if(fixtures)fixtures.DefaultMood(value);
  current=night?"night":"day";
 }
 void Start(){Theme(current);}
}
