using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffect : MonoBehaviour
{
  [SerializeField] private Material effectMat;

  private void OnRenderImage(RenderTexture _src, RenderTexture _dest)//모든 렌더링에서 이미지 렌더링을 완료한 후 호출
  {
   if(effectMat ==null)
       return;
   
   Graphics.Blit(_src,_dest,effectMat);
  }

  private void OnDestroy()
  {
      SetGrayScale(false);
  }

  public void SetGrayScale(bool isGrayscale)
  {
      effectMat.SetFloat("_GrayscaleAmount", isGrayscale ? 1 : 0);
      effectMat.SetFloat("_DarkAmount",isGrayscale?0.12f:0);
  }
}
