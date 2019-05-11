using UnityEngine;

namespace FlatKit {
    [ExecuteInEditMode, ImageEffectAllowedInSceneView, RequireComponent(typeof(Camera))]
    public class OutlineImageEffect : MonoBehaviour {
        public Color edgeColor = Color.white;

        [Range(0, 5)] public int thickness = 1;

        [Space] public bool useDepth = true;
        public bool useNormals = false;

        [Header("Advanced settings")] [Space] public float minDepthThreshold = 0f;
        public float maxDepthThreshold = 0.25f;
        [Space] public float minNormalsThreshold = 0f;
        public float maxNormalsThreshold = 0.25f;

        private Camera _camera;
        private Material _material;

        private static readonly int EdgeColorProperty = Shader.PropertyToID("_EdgeColor");
        private static readonly int ThicknessProperty = Shader.PropertyToID("_Thickness");
        private static readonly int DepthThresholdsProperty = Shader.PropertyToID("_DepthThresholds");
        private static readonly int NormalsThresholdsProperty = Shader.PropertyToID("_NormalsThresholds");

        void Awake() {
            _material = new Material(Shader.Find("Hidden/OutlinePlus"));
            _camera = GetComponent<Camera>();
            Debug.Assert(_camera.depthTextureMode != DepthTextureMode.None);
        }

        private void Start() {
            UpdateShader();
        }

        void OnValidate() {
            UpdateShader();
        }

        [ImageEffectOpaque]
        void OnRenderImage(RenderTexture source, RenderTexture destination) {
#if UNITY_EDITOR
            minDepthThreshold = Mathf.Clamp(minDepthThreshold, 0f, maxDepthThreshold);
            maxDepthThreshold = Mathf.Max(0f, maxDepthThreshold);
            minNormalsThreshold = Mathf.Clamp(minNormalsThreshold, 0f, maxNormalsThreshold);
            maxNormalsThreshold = Mathf.Max(0f, maxNormalsThreshold);
#endif // UNITY_EDITOR

            if (_material == null) {
                _material = new Material(Shader.Find("Hidden/OutlinePlus"));
            }

#if UNITY_EDITOR
            UpdateShader();
#endif

            Graphics.Blit(source, destination, _material);
        }

        private void UpdateShader() {
            if (_material == null) {
                return;
            }

            const string depthKeyword = "OUTLINE_USE_DEPTH";
            if (useDepth) {
                _material.EnableKeyword(depthKeyword);
                _camera.depthTextureMode = DepthTextureMode.Depth;
            }
            else {
                _material.DisableKeyword(depthKeyword);
            }

            const string normalsKeyword = "OUTLINE_USE_NORMALS";
            if (useNormals) {
                _material.EnableKeyword(normalsKeyword);
                _camera.depthTextureMode = DepthTextureMode.DepthNormals;
            }
            else {
                _material.DisableKeyword(normalsKeyword);
            }

            _material.SetColor(EdgeColorProperty, edgeColor);
            _material.SetFloat(ThicknessProperty, thickness);
            const float depthThresholdScale = 1e-3f;
            _material.SetVector(DepthThresholdsProperty,
                new Vector2(minDepthThreshold, maxDepthThreshold) * depthThresholdScale);
            _material.SetVector(NormalsThresholdsProperty, new Vector2(maxNormalsThreshold, maxNormalsThreshold));
        }
    }
}