using UnityEngine;
using UnityEngine.XR.iOS;

namespace FaceTrackingVFX
{
    sealed class ARFaceMeshBaker : MonoBehaviour
    {
        [SerializeField] RenderTexture _positionMap = default;
        [SerializeField] ComputeShader _vertexBaker = default;

        UnityARSessionNativeInterface _session;

        ComputeBuffer _positionBuffer;
        RenderTexture _tmpPositionMap;
        int _vertexCountID, _transformID, _positionBufferID, _positionMapID;

        void Start()
        {
            _vertexCountID = Shader.PropertyToID("VertexCount");
            _transformID = Shader.PropertyToID("Transform");
            _positionBufferID = Shader.PropertyToID("PositionBuffer");
            _positionMapID = Shader.PropertyToID("PositionMap");

            _session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

            Application.targetFrameRate = 60;
            var config = new ARKitFaceTrackingConfiguration();
            config.alignment = UnityARAlignment.UnityARAlignmentGravity;
            config.enableLightEstimation = true;

            if (config.IsSupported)
            {
                _session.RunWithConfig(config);
                UnityARSessionNativeInterface.ARFaceAnchorAddedEvent += FaceAdded;
                UnityARSessionNativeInterface.ARFaceAnchorUpdatedEvent += FaceUpdated;
                UnityARSessionNativeInterface.ARFaceAnchorRemovedEvent += FaceRemoved;
            }
        }

        void FaceAdded(ARFaceAnchor anchorData)
        {
            var vertexCount = anchorData.faceGeometry.vertices.Length;
            _positionBuffer = new ComputeBuffer(vertexCount * 3, sizeof(float));
            _tmpPositionMap = _positionMap.Clone();
        }

        void FaceUpdated(ARFaceAnchor anchorData)
        {
            if (_positionBuffer == null) return;

            var mapWidth = _positionMap.width;
            var mapHeight = _positionMap.height;
            var vCount = anchorData.faceGeometry.vertices.Length;

            _positionBuffer.SetData(anchorData.faceGeometry.vertices);
            gameObject.transform.localPosition = UnityARMatrixOps.GetPosition(anchorData.transform);
            gameObject.transform.localRotation = UnityARMatrixOps.GetRotation(anchorData.transform);

            _vertexBaker.SetInt(_vertexCountID, vCount);
            _vertexBaker.SetMatrix(_transformID, gameObject.transform.localToWorldMatrix);
            _vertexBaker.SetBuffer(0, _positionBufferID, _positionBuffer);
            _vertexBaker.SetTexture(0, _positionMapID, _tmpPositionMap);

            _vertexBaker.Dispatch(0, mapWidth / 8, mapHeight / 8, 1);

            Graphics.CopyTexture(_tmpPositionMap, _positionMap);
        }

        void FaceRemoved(ARFaceAnchor anchorData)
        {
            _positionBuffer.TryDispose();
            _tmpPositionMap.TryDestroy();
        }
    }
}
