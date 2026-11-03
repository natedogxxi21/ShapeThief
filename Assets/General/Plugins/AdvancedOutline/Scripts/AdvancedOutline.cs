using System;
using System.Collections.Generic;
using UnityEngine;

namespace ITISKIRUHERE
{
    /// <summary>
    /// Screen-space outline for meshes and skinned meshes.
    ///
    /// Persistence model
    /// -----------------
    /// * BAKED (recommended): use "Bake Outline Assets" in the inspector. This writes real assets
    ///   (outline meshes + a shared material set) and stores the references on the renderers
    ///   themselves, so prefabs and scenes contain everything needed. At runtime the component only
    ///   writes colour/width/dash values through MaterialPropertyBlocks.
    /// * LIVE (fallback): objects that were never baked still work in Play Mode / builds. The needed
    ///   meshes and materials are generated in memory, shared between components, and fully reverted
    ///   when the component is disabled.
    ///
    /// In edit mode (including Prefab Mode) this component NEVER modifies meshes, materials or any
    /// other serialized data. Only the explicit Bake / Remove Bake actions do.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class AdvancedOutline : MonoBehaviour
    {
        public enum Mode
        {
            OutlineAll,
            OutlineVisible,
            OutlineHidden,
            OutlineAndSilhouette,
            SilhouetteOnly
        }

        /// <summary>Record of one baked renderer, kept so the bake can be reverted cleanly.</summary>
        [Serializable]
        public class BakedRenderer
        {
            public Renderer renderer;
            public Mesh originalMesh;
            public Mesh bakedMesh;
        }

        struct LiveRecord
        {
            public Renderer renderer;
            public Mesh original;
            public Mesh clone;
            public bool skinned;
            public bool combined;
        }

        [Header( "References" )]
        [Tooltip( "Used when creating the outline materials. Auto-assigned when the component is added." )]
        [SerializeField] Shader _outlineMaskShader;
        [SerializeField] Shader _outlineFillShader;
        [Tooltip( "Persistent materials shared by all outlines. Created automatically the first time you bake." )]
        [SerializeField] AdvancedOutlineMaterialSet _materialSet;

        [Header( "General Settings" )]
        [SerializeField] Mode _outlineMode = Mode.OutlineVisible;
        [SerializeField] Color _outlineColor = Color.cyan;
        [SerializeField] [Range( 0f, 50f )] float _outlineWidth = 4f;

        [Header( "Aesthetic Effects" )]
        [SerializeField] bool _pulseWidth;
        [SerializeField] [Range( 0.1f, 10f )] float _pulseSpeed = 2f;
        [SerializeField] [Range( 0f, 50f )] float _pulseMinWidth = 1.5f;
        [SerializeField] [Range( 0f, 50f )] float _pulseMaxWidth = 5f;

        [Space]
        [SerializeField] bool _rainbowColor;
        [SerializeField] [Range( 0.1f, 5f )] float _rainbowSpeed = 0.5f;

        [Space]
        [SerializeField] bool _dashedPattern;
        [SerializeField] [Range( 0.001f, 0.1f )] float _dashScale = 0.03f;
        [SerializeField] [Range( -50f, 50f )] float _dashSpeed = 15f;

        // What was baked, so it can be undone. Written only by the editor baker.
        [SerializeField] [HideInInspector] List<BakedRenderer> _bakedRenderers = new List<BakedRenderer>();

        // ---- Non-serialized runtime state ----
        readonly List<Renderer> _targets = new List<Renderer>();
        readonly List<Renderer> _outlinedRenderers = new List<Renderer>();
        readonly List<LiveRecord> _liveRecords = new List<LiveRecord>();
        readonly List<Material> _tempMaterials = new List<Material>();

        MaterialPropertyBlock _block;
        AdvancedOutlineMaterialSet _activeSet;
        bool _ownsFallbackSet;
        bool _needsUpdate;
        bool _warnedMissingMaterials;
        int _uniqueMeshCount;

        bool _idsCached;
        int _outlineColorId;
        int _outlineWidthId;
        int _dashScaleId;
        int _dashSpeedId;

        // ---------------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------------

        public Mode OutlineMode
        {
            get => _outlineMode;
            set
            {
                if ( _outlineMode == value ) return;

                _outlineMode = value;
                _needsUpdate = true;

                // ZTest lives on the (shared, persistent) materials, so changing mode swaps which
                // materials the renderers use instead of editing the assets. In edit mode the
                // inspector does this through the baker so it is recorded for Undo and saved.
                if ( CanGenerateRuntimeData && isActiveAndEnabled )
                {
                    ApplyMaterialsForCurrentMode();
                }
            }
        }

        public Color OutlineColor { get => _outlineColor; set { _outlineColor = value; _needsUpdate = true; } }
        public float OutlineWidth { get => _outlineWidth; set { _outlineWidth = value; _needsUpdate = true; } }

        public bool PulseWidth { get => _pulseWidth; set { _pulseWidth = value; _needsUpdate = true; } }
        public float PulseSpeed { get => _pulseSpeed; set { _pulseSpeed = value; _needsUpdate = true; } }
        public float PulseMinWidth { get => _pulseMinWidth; set { _pulseMinWidth = value; _needsUpdate = true; } }
        public float PulseMaxWidth { get => _pulseMaxWidth; set { _pulseMaxWidth = value; _needsUpdate = true; } }

        public bool RainbowColor { get => _rainbowColor; set { _rainbowColor = value; _needsUpdate = true; } }
        public float RainbowSpeed { get => _rainbowSpeed; set { _rainbowSpeed = value; _needsUpdate = true; } }

        public bool DashedPattern { get => _dashedPattern; set { _dashedPattern = value; _needsUpdate = true; } }
        public float DashScale { get => _dashScale; set { _dashScale = value; _needsUpdate = true; } }
        public float DashSpeed { get => _dashSpeed; set { _dashSpeed = value; _needsUpdate = true; } }

        public Shader OutlineMaskShader { get => _outlineMaskShader; set => _outlineMaskShader = value; }
        public Shader OutlineFillShader { get => _outlineFillShader; set => _outlineFillShader = value; }
        public AdvancedOutlineMaterialSet MaterialSet { get => _materialSet; set { _materialSet = value; _needsUpdate = true; } }

        /// <summary>True when outline meshes/materials have been baked into this object's renderers.</summary>
        public bool IsBaked => _bakedRenderers != null && _bakedRenderers.Count > 0;
        public IReadOnlyList<BakedRenderer> BakedRenderers => _bakedRenderers;

        public int RendererCount => _targets.Count;
        public int MeshCount => _uniqueMeshCount;

        /// <summary>
        /// Finds the renderers this component outlines: mesh and skinned mesh renderers under this
        /// object (inactive ones included) that are not owned by a nearer AdvancedOutline.
        /// </summary>
        public void GatherTargets( List<Renderer> results )
        {
            results.Clear();

            Renderer[] all = GetComponentsInChildren<Renderer>( true );
            for ( int i = 0; i < all.Length; i++ )
            {
                Renderer renderer = all[i];
                if ( !renderer ) continue;

                bool skinned = renderer is SkinnedMeshRenderer;
                bool staticMesh = renderer is MeshRenderer && renderer.GetComponent<MeshFilter>() != null;
                if ( !skinned && !staticMesh ) continue;

                if ( FindOwner( renderer.transform ) != this ) continue;

                results.Add( renderer );
            }
        }

        /// <summary>
        /// Rebuilds the renderer cache. In Play Mode this also generates live outline data for any
        /// renderer that was not baked. In edit mode it never changes any asset or serialized data.
        /// </summary>
        public void RefreshRenderers()
        {
            EnsureRuntimeState();

            ReleaseLiveData();
            GatherTargets( _targets );
            CountUniqueMeshes();

            ResolveMaterialSet();
            if ( CanGenerateRuntimeData && isActiveAndEnabled )
            {
                ApplyRuntimeOutline();
            }

            CollectOutlinedRenderers();
            _needsUpdate = true;

            if ( isActiveAndEnabled )
            {
                ApplyProperties();
            }
            else
            {
                HideOutline();
            }
        }

        /// <summary>
        /// Makes every outlined renderer use the mask/fill materials that match the current Mode.
        /// Safe at runtime. In edit mode the caller is responsible for Undo and for saving
        /// (the inspector uses AdvancedOutlineBaker.ApplyMode, which does both).
        /// </summary>
        public void ApplyMaterialsForCurrentMode()
        {
            AdvancedOutlineMaterialSet set = ResolveMaterialSet();
            if ( !set ) return;

            Material mask = set.GetMask( _outlineMode );
            Material fill = set.GetFill( _outlineMode );

            GatherTargets( _targets );
            for ( int i = 0; i < _targets.Count; i++ )
            {
                Renderer renderer = _targets[i];
                if ( renderer && HasOutlineMaterial( renderer ) )
                {
                    SetOutlineMaterials( renderer, mask, fill );
                }
            }

            CollectOutlinedRenderers();
            _needsUpdate = true;
        }

        // ---------------------------------------------------------------------------------
        // Unity messages
        // ---------------------------------------------------------------------------------

        void OnEnable()
        {
            RefreshRenderers();
        }

        void OnDisable()
        {
            // Baked outlines live on the renderers (they are part of the prefab/scene), so a disabled
            // component simply hides them. Only data generated at runtime is actually removed.
            HideOutline();
            ReleaseLiveData();
            ReleaseFallbackSet();

            _outlinedRenderers.Clear();
            _targets.Clear();
        }

        void OnTransformChildrenChanged()
        {
            if ( isActiveAndEnabled )
            {
                RefreshRenderers();
            }
        }

        void Update()
        {
            if ( !_needsUpdate && !_pulseWidth && !_rainbowColor ) return;

            _needsUpdate = false;
            ApplyProperties();
        }

        void OnValidate()
        {
            // Intentionally touches nothing but the component's own values. Opening a prefab or
            // scene must never dirty it.
            if ( _pulseMinWidth > _pulseMaxWidth )
            {
                _pulseMinWidth = _pulseMaxWidth;
            }
            if ( _dashScale < 0f )
            {
                _dashScale = 0f;
            }

            _needsUpdate = true;
        }

        #if UNITY_EDITOR
        void Reset()
        {
            // Runs only when the component is added or reset in the editor.
            if ( !_outlineMaskShader ) _outlineMaskShader = Shader.Find( AdvancedOutlineMaterialSet.MaskShaderName );
            if ( !_outlineFillShader ) _outlineFillShader = Shader.Find( AdvancedOutlineMaterialSet.FillShaderName );

            if ( !_materialSet )
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets( "t:AdvancedOutlineMaterialSet" );
                if ( guids.Length > 0 )
                {
                    _materialSet = UnityEditor.AssetDatabase.LoadAssetAtPath<AdvancedOutlineMaterialSet>(
                        UnityEditor.AssetDatabase.GUIDToAssetPath( guids[0] ) );
                }
            }
        }
        #endif

        // ---------------------------------------------------------------------------------
        // Runtime-only (play mode / build) generation for objects that were never baked
        // ---------------------------------------------------------------------------------

        /// <summary>
        /// Live generation is only allowed in play mode, on scene objects. This is what guarantees that
        /// edit mode, Prefab Mode and prefab assets are never touched.
        /// </summary>
        bool CanGenerateRuntimeData
        {
            get
            {
                if ( !Application.isPlaying ) return false;

                #if UNITY_EDITOR
                if ( UnityEditor.EditorUtility.IsPersistent( this ) ) return false;
                if ( UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage( gameObject ) != null ) return false;
                #endif

                return true;
            }
        }

        void ApplyRuntimeOutline()
        {
            AdvancedOutlineMaterialSet set = _activeSet;
            if ( !set )
            {
                if ( !_warnedMissingMaterials )
                {
                    _warnedMissingMaterials = true;
                    Debug.LogWarning( "[AdvancedOutline] No complete Material Set is assigned and the outline shaders could not be found. " +
                        "Bake the outline in the editor or assign the shaders.", this );
                }
                return;
            }

            Material mask = set.GetMask( _outlineMode );
            Material fill = set.GetFill( _outlineMode );

            for ( int i = 0; i < _targets.Count; i++ )
            {
                Renderer renderer = _targets[i];
                if ( !renderer ) continue;

                // Baked renderers already carry the materials and the outline mesh; only unbaked ones
                // need generated data.
                if ( !HasOutlineMaterial( renderer ) )
                {
                    PrepareLiveRenderer( renderer );
                }

                SetOutlineMaterials( renderer, mask, fill );
            }
        }

        void PrepareLiveRenderer( Renderer renderer )
        {
            LiveRecord record = new LiveRecord { renderer = renderer };

            Mesh source = AdvancedOutlineMeshUtility.GetSharedMesh( renderer );
            if ( source )
            {
                bool skinned = renderer is SkinnedMeshRenderer;
                bool combine = AdvancedOutlineMeshUtility.NeedsCombinedSubmesh( source, renderer.sharedMaterials.Length );

                Mesh clone = AdvancedOutlineMeshUtility.AcquireLiveMesh( source, skinned, combine );
                if ( clone )
                {
                    AdvancedOutlineMeshUtility.SetSharedMesh( renderer, clone );
                    record.original = source;
                    record.clone = clone;
                    record.skinned = skinned;
                    record.combined = combine;
                }
                else
                {
                    Debug.LogWarning( $"[AdvancedOutline] Mesh '{source.name}' is not readable, so smooth normals could not be generated. " +
                        "Enable Read/Write on its import settings, or bake the outline in the editor.", renderer );
                }
            }

            _liveRecords.Add( record );
        }

        void ReleaseLiveData()
        {
            for ( int i = 0; i < _liveRecords.Count; i++ )
            {
                LiveRecord record = _liveRecords[i];

                if ( record.renderer )
                {
                    RemoveOutlineMaterials( record.renderer );

                    if ( record.clone && AdvancedOutlineMeshUtility.GetSharedMesh( record.renderer ) == record.clone )
                    {
                        AdvancedOutlineMeshUtility.SetSharedMesh( record.renderer, record.original );
                    }
                }

                if ( record.clone )
                {
                    AdvancedOutlineMeshUtility.ReleaseLiveMesh( record.original, record.skinned, record.combined );
                }
            }
            _liveRecords.Clear();
        }

        // ---------------------------------------------------------------------------------
        // Materials
        // ---------------------------------------------------------------------------------

        AdvancedOutlineMaterialSet ResolveMaterialSet()
        {
            if ( _materialSet && _materialSet.IsComplete )
            {
                ReleaseFallbackSet();
                _activeSet = _materialSet;
                return _activeSet;
            }

            // Never invent materials outside of play mode.
            if ( Application.isPlaying )
            {
                if ( !_ownsFallbackSet )
                {
                    _activeSet = AdvancedOutlineMaterialSet.AcquireRuntimeFallback( _outlineMaskShader, _outlineFillShader );
                    _ownsFallbackSet = _activeSet != null;
                }
                return _activeSet;
            }

            _activeSet = null;
            return null;
        }

        void ReleaseFallbackSet()
        {
            if ( _ownsFallbackSet )
            {
                AdvancedOutlineMaterialSet.ReleaseRuntimeFallback();
                _ownsFallbackSet = false;
            }
            _activeSet = null;
        }

        bool IsOutlineMaterial( Material material )
        {
            if ( !material ) return false;
            if ( _activeSet && _activeSet.Contains( material ) ) return true;
            if ( _materialSet && _materialSet.Contains( material ) ) return true;
            return AdvancedOutlineMaterialSet.IsOutlineMaterial( material );
        }

        bool HasOutlineMaterial( Renderer renderer )
        {
            renderer.GetSharedMaterials( _tempMaterials );

            bool found = false;
            for ( int i = 0; i < _tempMaterials.Count; i++ )
            {
                if ( IsOutlineMaterial( _tempMaterials[i] ) )
                {
                    found = true;
                    break;
                }
            }

            _tempMaterials.Clear();
            return found;
        }

        /// <summary>Puts the mask+fill pair at the end of the renderer's materials. Returns true if anything changed.</summary>
        bool SetOutlineMaterials( Renderer renderer, Material mask, Material fill )
        {
            renderer.GetSharedMaterials( _tempMaterials );

            int count = _tempMaterials.Count;
            if ( count >= 2 && _tempMaterials[count - 2] == mask && _tempMaterials[count - 1] == fill )
            {
                _tempMaterials.Clear();
                return false;
            }

            _tempMaterials.RemoveAll( IsOutlineMaterial );
            _tempMaterials.Add( mask );
            _tempMaterials.Add( fill );
            renderer.sharedMaterials = _tempMaterials.ToArray();

            _tempMaterials.Clear();
            return true;
        }

        void RemoveOutlineMaterials( Renderer renderer )
        {
            renderer.GetSharedMaterials( _tempMaterials );

            int removed = _tempMaterials.RemoveAll( IsOutlineMaterial );
            if ( removed > 0 )
            {
                renderer.sharedMaterials = _tempMaterials.ToArray();
            }

            _tempMaterials.Clear();
        }

        void CollectOutlinedRenderers()
        {
            _outlinedRenderers.Clear();
            for ( int i = 0; i < _targets.Count; i++ )
            {
                Renderer renderer = _targets[i];
                if ( renderer && HasOutlineMaterial( renderer ) )
                {
                    _outlinedRenderers.Add( renderer );
                }
            }
        }

        void CountUniqueMeshes()
        {
            HashSet<Mesh> meshes = new HashSet<Mesh>();
            for ( int i = 0; i < _targets.Count; i++ )
            {
                Mesh mesh = AdvancedOutlineMeshUtility.GetSharedMesh( _targets[i] );
                if ( mesh )
                {
                    meshes.Add( mesh );
                }
            }
            _uniqueMeshCount = meshes.Count;
        }

        // ---------------------------------------------------------------------------------
        // Per-object visual properties (MaterialPropertyBlock only: never serialized, never dirties anything)
        // ---------------------------------------------------------------------------------

        static float CurrentTime => Application.isPlaying ? Time.time : Time.realtimeSinceStartup;

        void ApplyProperties()
        {
            if ( _outlinedRenderers.Count == 0 ) return;

            float time = CurrentTime;

            Color color = _outlineColor;
            if ( _rainbowColor )
            {
                float hue = ( time * _rainbowSpeed ) % 1.0f;
                color = Color.HSVToRGB( hue, 1f, 1f );
                color.a = _outlineColor.a;
            }

            float width = _outlineWidth;
            if ( _outlineMode == Mode.SilhouetteOnly )
            {
                width = 0f;
            }
            else if ( _pulseWidth )
            {
                float lerpVal = ( Mathf.Sin( time * _pulseSpeed ) + 1f ) * 0.5f;
                width = Mathf.Lerp( _pulseMinWidth, _pulseMaxWidth, lerpVal );
            }

            WriteProperties( color, width, _dashedPattern ? _dashScale : 0f, _dashedPattern ? _dashSpeed : 0f );
        }

        void HideOutline()
        {
            if ( _outlinedRenderers.Count == 0 ) return;
            WriteProperties( Color.clear, 0f, 0f, 0f );
        }

        void WriteProperties( Color color, float width, float dashScale, float dashSpeed )
        {
            EnsureRuntimeState();

            for ( int i = 0; i < _outlinedRenderers.Count; i++ )
            {
                Renderer renderer = _outlinedRenderers[i];
                if ( !renderer ) continue;

                // Merge into any existing block so other scripts' per-renderer overrides survive.
                renderer.GetPropertyBlock( _block );
                _block.SetColor( _outlineColorId, color );
                _block.SetFloat( _outlineWidthId, width );
                _block.SetFloat( _dashScaleId, dashScale );
                _block.SetFloat( _dashSpeedId, dashSpeed );
                renderer.SetPropertyBlock( _block );
            }
        }

        void EnsureRuntimeState()
        {
            if ( _block == null )
            {
                _block = new MaterialPropertyBlock();
            }

            if ( !_idsCached )
            {
                _outlineColorId = Shader.PropertyToID( "_OutlineColor" );
                _outlineWidthId = Shader.PropertyToID( "_OutlineWidth" );
                _dashScaleId = Shader.PropertyToID( "_DashScale" );
                _dashSpeedId = Shader.PropertyToID( "_DashSpeed" );
                _idsCached = true;
            }
        }

        static AdvancedOutline FindOwner( Transform transform )
        {
            while ( transform )
            {
                AdvancedOutline outline = transform.GetComponent<AdvancedOutline>();
                if ( outline ) return outline;
                transform = transform.parent;
            }
            return null;
        }
    }
}
