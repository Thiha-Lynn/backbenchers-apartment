using UnityEditor;
using System.Linq;
public class ApartmentImporter : AssetPostprocessor {
 void OnPreprocessTexture(){
  if(!assetPath.StartsWith("Assets/HDRP Archviz Apartment/")) return;
  var t=(TextureImporter)assetImporter;bool large=new[]{"/Floor/","/Wall01/","/Wall02/","/Ceiling/","/Couch/","/Bed/","/Carpet/","/Painting","/Canvas/","/PosterFrame/","/KitchenWall/","/BathroomWall/","/BathroomFloor/"}.Any(part=>assetPath.Contains(part));bool data=assetPath.Contains("_Normal")||assetPath.Contains("_Mask");int cap=large?(data?512:1024):256;t.maxTextureSize=cap;t.isReadable=false;t.mipmapEnabled=true;t.textureCompression=TextureImporterCompression.Compressed;t.crunchedCompression=true;t.compressionQuality=55;
  var w=t.GetPlatformTextureSettings("WebGL");w.overridden=true;w.maxTextureSize=cap;w.format=TextureImporterFormat.DXT5Crunched;w.crunchedCompression=true;w.compressionQuality=55;t.SetPlatformTextureSettings(w);
 }
 void OnPreprocessModel(){
  if(!assetPath.StartsWith("Assets/HDRP Archviz Apartment/")) return;
  var m=(ModelImporter)assetImporter;m.meshCompression=ModelImporterMeshCompression.Medium;m.isReadable=true;m.generateSecondaryUV=true;m.importCameras=false;m.importLights=false;m.importAnimation=false;
 }
}
