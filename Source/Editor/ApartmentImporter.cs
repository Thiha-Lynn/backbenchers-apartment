using UnityEditor;
using System.Linq;
public class ApartmentImporter : AssetPostprocessor {
 void OnPreprocessTexture(){
  if(!assetPath.StartsWith("Assets/HDRP Archviz Apartment/")) return;
  var t=(TextureImporter)assetImporter;bool large=new[]{"/Floor/","/Wall01/","/Wall02/","/Ceiling/","/Couch/","/Bed/","/Carpet/","/Painting","/Canvas/","/PosterFrame/","/KitchenWall/","/BathroomWall/","/BathroomFloor/"}.Any(part=>assetPath.Contains(part));int cap=(large&&!assetPath.Contains("_Normal")&&!assetPath.Contains("_Mask"))?1024:512;t.maxTextureSize=cap;t.isReadable=false;t.mipmapEnabled=true;t.textureCompression=TextureImporterCompression.Compressed;
  var w=t.GetPlatformTextureSettings("WebGL");w.overridden=true;w.maxTextureSize=cap;w.format=TextureImporterFormat.DXT5;w.compressionQuality=60;t.SetPlatformTextureSettings(w);
 }
 void OnPreprocessModel(){
  if(!assetPath.StartsWith("Assets/HDRP Archviz Apartment/")) return;
  var m=(ModelImporter)assetImporter;m.isReadable=false;m.generateSecondaryUV=true;m.importCameras=false;m.importLights=false;m.importAnimation=false;
 }
}
