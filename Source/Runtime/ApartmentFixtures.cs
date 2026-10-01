using System;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;
public class ApartmentFixtures:MonoBehaviour {
 [Serializable] public class Fixture {public string label;public Light source;public Renderer[] bulbs;public float level=1;public bool on=true;public Color color=new Color(1,.78f,.56f);[NonSerialized]public float actual;[NonSerialized]public bool dirty=true;}
 public Fixture[] fixtures;public Camera viewCamera;public float master=1;public bool active=true;MaterialPropertyBlock block;
 [Serializable] public class Edit {public int index;public float level=1;public bool on=true;public string color="#ffcc91";}
 #if UNITY_WEBGL && !UNITY_EDITOR
 [DllImport("__Internal")]static extern void ApartmentLightAudit(string json);
 [DllImport("__Internal")]static extern void ApartmentFixtureList(string json);
 [DllImport("__Internal")]static extern void ApartmentFixtureSelected(int index);
 #endif
 [Serializable]class Labels {public string[] labels;}
 void Start(){block=new MaterialPropertyBlock();Publish();ApplyImmediate();}
 public void Publish(){
  #if UNITY_WEBGL && !UNITY_EDITOR
  ApartmentFixtureList(JsonUtility.ToJson(new Labels{labels=Array.ConvertAll(fixtures,f=>f.label)}));
  #endif
 }
 public void Lamp(string json){var edit=JsonUtility.FromJson<Edit>(json);if(edit.index<0||edit.index>=fixtures.Length)return;var f=fixtures[edit.index];f.dirty=true;f.on=edit.on;f.level=Mathf.Clamp(edit.level,0,1.5f);if(ColorUtility.TryParseHtmlString(edit.color,out var color))f.color=color;}
 public void Master(string value){if(float.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out var v))master=Mathf.Clamp(v,0,1.5f);}
 public void Power(string value){active=value=="1";}
 public void DefaultMood(string value){foreach(var f in fixtures){f.dirty=true;f.on=true;f.level=value=="night"?1:.35f;f.color=value=="night"?new Color(1,.78f,.56f):new Color(1,.95f,.84f);}master=1;active=true;}
 public void ApplyImmediate(){block??=new MaterialPropertyBlock();foreach(var f in fixtures){f.actual=active&&f.on?f.level*master:0;Render(f);}}
 void Update(){if(fixtures==null)return;foreach(var f in fixtures){float target=active&&f.on?f.level*master:0;if(!f.dirty&&Mathf.Approximately(f.actual,target))continue;f.actual=Mathf.MoveTowards(f.actual,target,Time.unscaledDeltaTime*3);Render(f);}}
 void Render(Fixture f){f.dirty=false;if(!f.source)return;f.source.enabled=f.actual>.002f;f.source.intensity=f.actual*2.8f;f.source.color=f.color;foreach(var r in f.bulbs)if(r){r.GetPropertyBlock(block);block.SetColor("_EmissionColor",f.color*(f.actual*2.2f));r.SetPropertyBlock(block);}}
 [Serializable]class AuditState {public float[] intensity;public bool[] enabled;public string[] colors;public Vector3[] viewport;}
 public void LightAudit(string unused){
  #if UNITY_WEBGL && !UNITY_EDITOR
  ApartmentLightAudit(JsonUtility.ToJson(new AuditState{intensity=Array.ConvertAll(fixtures,f=>f.source.intensity),enabled=Array.ConvertAll(fixtures,f=>f.source.enabled),colors=Array.ConvertAll(fixtures,f=>ColorUtility.ToHtmlStringRGB(f.source.color)),viewport=Array.ConvertAll(fixtures,f=>viewCamera.WorldToViewportPoint(f.source.transform.position))}));
  #endif
 }
 public void Pick(string value){var s=value.Split(',');if(s.Length!=2||!float.TryParse(s[0],NumberStyles.Float,CultureInfo.InvariantCulture,out var x)||!float.TryParse(s[1],NumberStyles.Float,CultureInfo.InvariantCulture,out var y))return;var ray=viewCamera.ViewportPointToRay(new Vector3(x,1-y,0));if(!Physics.Raycast(ray,out var hit,15,~0,QueryTriggerInteraction.Collide))return;int index=-1;float d=.9f;for(int i=0;i<fixtures.Length;i++){var dist=Vector3.Distance(hit.point,fixtures[i].source.transform.position);if(dist<d){d=dist;index=i;}}if(index<0)return;
  #if UNITY_WEBGL && !UNITY_EDITOR
  ApartmentFixtureSelected(index);
  #endif
 }
}
