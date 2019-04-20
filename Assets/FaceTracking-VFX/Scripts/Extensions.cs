using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FaceTrackingVFX
{
    internal static class Extensions
    {
        internal static void TryDispose(this IDisposable disposable)
        {
            disposable?.Dispose();
        }

        internal static void TryDestroy(this UnityObject obj)
        {
            if (obj == null) return;
            UnityObject.Destroy(obj);
            obj = null;
        }

        internal static RenderTexture Clone(this RenderTexture source)
        {
            var rt = new RenderTexture(source.width, source.height, 0, source.format) {enableRandomWrite = true};
            rt.Create();
            return rt;
        }
    }
}
