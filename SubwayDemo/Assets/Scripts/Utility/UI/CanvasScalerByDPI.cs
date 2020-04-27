using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace DSI.LB
{
    public class CanvasScalerByDPI : CanvasScaler
    {
        protected override void HandleConstantPhysicalSize()
        {
            float dpi = (Screen.dpi == 0 ? m_FallbackScreenDPI : Screen.dpi);
            float targetDPI = 1f;
            switch (m_PhysicalUnit)
            {
                case Unit.Centimeters: targetDPI = 2.54f; break;
                case Unit.Millimeters: targetDPI = 25.4f; break;
                case Unit.Inches: targetDPI = 1f; break;
                case Unit.Points: targetDPI = 72f; break;
                case Unit.Picas: targetDPI = 6f; break;
            }

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            targetDPI *= (70f / 72f);
            SetScaleFactor(dpi / targetDPI);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit * targetDPI / m_DefaultSpriteDPI * (96f / 70f));
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WEBGL
            targetDPI *= (96f / 72f);
            SetScaleFactor(dpi / targetDPI);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit * targetDPI / m_DefaultSpriteDPI);
#elif UNITY_ANDROID || UNITY_IOS
            float realySizeRatio = 6f / ((float)(Screen.width + Screen.height) / dpi);
            float l_dpiRatio = Mathf.Sqrt(realySizeRatio);
            targetDPI *= (3f * l_dpiRatio);
            SetScaleFactor(dpi / targetDPI);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit * targetDPI / m_DefaultSpriteDPI * 0.4f / l_dpiRatio);
#else
            targetDPI *= (96f / 72f);
            SetScaleFactor(dpi / targetDPI);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit * targetDPI / m_DefaultSpriteDPI);
#endif

        }
    }
}