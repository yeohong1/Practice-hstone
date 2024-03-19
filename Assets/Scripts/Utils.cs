using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]//객체를 데이터 스트림으로 변환하여 파일에 저장하거나 네트워크를 통해 전송하는 프로세스
public class PRS//positon, rotation, scale을 담는 클래스
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;

    //생성자를 해주면 알아서 들어감
    public PRS(Vector3 _pos, Quaternion _rot, Vector3 _scale)
    {
        this.pos = _pos;
        this.rot = _rot;
        this.scale = _scale;
    }
}
public class Utils : MonoBehaviour
{
    public static Quaternion QI => Quaternion.identity;
}
