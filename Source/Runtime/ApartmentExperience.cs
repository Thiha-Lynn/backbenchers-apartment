using UnityEngine;
using System.Runtime.InteropServices;
public class ApartmentExperience:MonoBehaviour {
 public Camera viewCamera;public Vector3[] viewpoints,angles;public Vector2 xBounds=new Vector2(-50,50),zBounds=new Vector2(-50,50);public float eyeHeight=1.65f;
 Vector2 move,look;float yaw,pitch,reportTime,sampleTime;int frames;bool reduced,paused;CharacterController body;float speed=1.5f;float stepDistance;Vector3 previousPosition;
 #if UNITY_WEBGL && !UNITY_EDITOR
 [DllImport("__Internal")] static extern void ApartmentStep();
 [DllImport("__Internal")] static extern void ApartmentReady(string renderer);
 [DllImport("__Internal")] static extern void ApartmentReport(float fps,float x,float z,float yaw);
 #endif
 public void SetViewpoints(){viewpoints=new[]{new Vector3(-4.8f,1.7f,1.2f),new Vector3(-3.2f,1.7f,-.6f),new Vector3(-4.8f,1.7f,-4.3f),new Vector3(-5.1f,1.7f,6.3f),new Vector3(-6.3f,1.7f,8.8f),new Vector3(-10.4f,1.7f,6.6f),new Vector3(-4.5f,1.7f,9.6f)};angles=new[]{new Vector3(3,305,0),new Vector3(4,90,0),new Vector3(5,135,0),new Vector3(4,60,0),new Vector3(4,250,0),new Vector3(5,180,0),new Vector3(2,180,0)};xBounds=new Vector2(-11.85f,-.25f);zBounds=new Vector2(-7.7f,10.35f);}
 void Start(){body=viewCamera.GetComponent<CharacterController>();yaw=viewCamera.transform.eulerAngles.y;pitch=viewCamera.transform.eulerAngles.x;previousPosition=viewCamera.transform.position;Application.targetFrameRate=-1;QualitySettings.vSyncCount=0;
 #if UNITY_WEBGL && !UNITY_EDITOR
 ApartmentReady(SystemInfo.graphicsDeviceType.ToString());
 #endif
 }
 public void Move(string s){var a=s.Split(',');if(a.Length==2&&float.TryParse(a[0],out var x)&&float.TryParse(a[1],out var y))move=Vector2.ClampMagnitude(new Vector2(x,y),1);}
 public void Look(string s){var a=s.Split(',');if(a.Length==2&&float.TryParse(a[0],out var x)&&float.TryParse(a[1],out var y)){yaw+=x*.14f;pitch=Mathf.Clamp(pitch+y*.14f,-70,70);}}
 public void Visit(string s){if(!int.TryParse(s,out var i)||i<0||i>=viewpoints.Length)return;body.enabled=false;viewCamera.transform.position=viewpoints[i];eyeHeight=viewpoints[i].y;yaw=angles[i].y;pitch=angles[i].x;viewCamera.transform.rotation=Quaternion.Euler(pitch,yaw,0);body.enabled=true;move=Vector2.zero;previousPosition=viewCamera.transform.position;stepDistance=0;}
 public void Pause(string v){paused=v=="1";move=Vector2.zero;}
 public void Comfort(string v){reduced=v=="1";}
 public void Speed(string v){if(float.TryParse(v,out var n))speed=Mathf.Clamp(n,.6f,2.6f);}
 public void Quality(string v){QualitySettings.antiAliasing=v=="light"?0:2;QualitySettings.globalTextureMipmapLimit=v=="light"?1:0;}
 void Update(){
  if(paused)return;var delta=Mathf.Min(Time.unscaledDeltaTime,.05f);var keyboard=new Vector2((Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0),(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0));
  var v=Vector2.ClampMagnitude(move+keyboard,1);var rotation=Quaternion.Euler(0,yaw,0);body.Move(rotation*new Vector3(v.x,0,v.y)*speed*delta);
  var p=viewCamera.transform.position;p.x=Mathf.Clamp(p.x,xBounds.x,xBounds.y);p.z=Mathf.Clamp(p.z,zBounds.x,zBounds.y);p.y=eyeHeight;viewCamera.transform.position=p;
  float travelled=Vector3.Distance(p,previousPosition);previousPosition=p;if(travelled<.3f)stepDistance+=travelled;if(stepDistance>.85f){stepDistance=0;
  #if UNITY_WEBGL && !UNITY_EDITOR
  ApartmentStep();
  #endif
  }
  viewCamera.transform.rotation=reduced?Quaternion.Euler(pitch,yaw,0):Quaternion.Slerp(viewCamera.transform.rotation,Quaternion.Euler(pitch,yaw,0),1-Mathf.Exp(-22*delta));
  frames++;sampleTime+=Time.unscaledDeltaTime;reportTime+=Time.unscaledDeltaTime;if(reportTime>2){
   #if UNITY_WEBGL && !UNITY_EDITOR
   ApartmentReport(frames/Mathf.Max(sampleTime,.001f),p.x,p.z,yaw);
   #endif
   frames=0;sampleTime=0;reportTime=0;
  }
 }
}
