using UnityEngine;
using UnityEngine.Rendering;

namespace ITISKIRUHERE
{
    /// <summary>
    /// Persistent container for the materials used by <see cref="AdvancedOutline"/>.
    ///
    /// These are real project assets, so renderers can reference them safely from prefabs and
    /// scenes. One material exists per ZTest variant because ZTest is render state and cannot be
    /// overridden per renderer with a MaterialPropertyBlock. Colour, width and dash settings are
    /// still per-object and are written through property blocks, so these assets are never
    /// modified at runtime and can be shared by every outline in the project.
    /// </summary>
    [CreateAssetMenu( fileName = "AdvancedOutlineMaterialSet", menuName = "Advanced Outline/Material Set" )]
    public class AdvancedOutlineMaterialSet : ScriptableObject
    {
        public const string MaskShaderName = "Custom/Advanced Outline Mask";
        public const string FillShaderName = "Custom/Advanced Outline Fill";

        const string ZTestProperty = "_ZTest";
        const string ColorProperty = "_OutlineColor";
        const string WidthProperty = "_OutlineWidth";
        const string DashScaleProperty = "_DashScale";
        const string DashSpeedProperty = "_DashSpeed";

        [SerializeField] Material _maskAlways;
        [SerializeField] Material _maskLessEqual;
        [SerializeField] Material _fillAlways;
        [SerializeField] Material _fillLessEqual;
        [SerializeField] Material _fillGreater;

        // Shared in-memory set used only in play mode for objects that were never baked.
        static AdvancedOutlineMaterialSet s_runtimeFallback;
        static int s_runtimeFallbackUsers;

        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration )]
        static void ResetStatics()
        {
            s_runtimeFallback = null;
            s_runtimeFallbackUsers = 0;
        }

        public bool IsComplete => _maskAlways && _maskLessEqual && _fillAlways && _fillLessEqual && _fillGreater;

        public Material GetMask( AdvancedOutline.Mode mode )
        {
            return GetMaskZTest( mode ) == CompareFunction.LessEqual ? _maskLessEqual : _maskAlways;
        }

        public Material GetFill( AdvancedOutline.Mode mode )
        {
            switch ( GetFillZTest( mode ) )
            {
                case CompareFunction.LessEqual: return _fillLessEqual;
                case CompareFunction.Greater: return _fillGreater;
                default: return _fillAlways;
            }
        }

        public bool Contains( Material material )
        {
            if ( !material ) return false;
            return material == _maskAlways || material == _maskLessEqual
                || material == _fillAlways || material == _fillLessEqual || material == _fillGreater;
        }

        // ---------------------------------------------------------------------------------
        // Mode -> ZTest mapping (identical to the original component behaviour)
        // ---------------------------------------------------------------------------------

        public static CompareFunction GetMaskZTest( AdvancedOutline.Mode mode )
        {
            return mode == AdvancedOutline.Mode.OutlineAndSilhouette || mode == AdvancedOutline.Mode.SilhouetteOnly
                ? CompareFunction.LessEqual
                : CompareFunction.Always;
        }

        public static CompareFunction GetFillZTest( AdvancedOutline.Mode mode )
        {
            switch ( mode )
            {
                case AdvancedOutline.Mode.OutlineVisible:
                    return CompareFunction.LessEqual;
                case AdvancedOutline.Mode.OutlineHidden:
                case AdvancedOutline.Mode.SilhouetteOnly:
                    return CompareFunction.Greater;
                default: // OutlineAll, OutlineAndSilhouette
                    return CompareFunction.Always;
            }
        }

        /// <summary>True for any material that uses one of the outline shaders.</summary>
        public static bool IsOutlineMaterial( Material material )
        {
            if ( !material ) return false;
            Shader shader = material.shader;
            if ( !shader ) return false;
            string shaderName = shader.name;
            return shaderName == MaskShaderName || shaderName == FillShaderName;
        }

        // ---------------------------------------------------------------------------------
        // Material configuration helpers (used by the editor baker and the runtime fallback)
        // ---------------------------------------------------------------------------------

        /// <summary>Writes _ZTest if it differs. Returns true when the material was changed.</summary>
        public static bool ApplyZTest( Material material, CompareFunction zTest )
        {
            if ( !material || !material.HasProperty( ZTestProperty ) ) return false;

            float value = (float)zTest;
            if ( Mathf.Approximately( material.GetFloat( ZTestProperty ), value ) ) return false;

            material.SetFloat( ZTestProperty, value );
            return true;
        }

        /// <summary>
        /// Makes a fill material invisible by default (alpha 0, width 0). Components write the real
        /// values through property blocks, so a disabled or removed component leaves no outline behind.
        /// </summary>
        public static void ApplyHiddenDefaults( Material material )
        {
            if ( !material ) return;

            if ( material.HasProperty( ColorProperty ) ) material.SetColor( ColorProperty, new Color( 0f, 0f, 0f, 0f ) );
            if ( material.HasProperty( WidthProperty ) ) material.SetFloat( WidthProperty, 0f );
            if ( material.HasProperty( DashScaleProperty ) ) material.SetFloat( DashScaleProperty, 0f );
            if ( material.HasProperty( DashSpeedProperty ) ) material.SetFloat( DashSpeedProperty, 0f );
        }

        // ---------------------------------------------------------------------------------
        // Runtime fallback: only used in play mode when no complete persistent set is assigned
        // ---------------------------------------------------------------------------------

        public static AdvancedOutlineMaterialSet AcquireRuntimeFallback( Shader maskShader, Shader fillShader )
        {
            if ( !s_runtimeFallback )
            {
                if ( !maskShader ) maskShader = Shader.Find( MaskShaderName );
                if ( !fillShader ) fillShader = Shader.Find( FillShaderName );
                if ( !maskShader || !fillShader ) return null;

                AdvancedOutlineMaterialSet set = CreateInstance<AdvancedOutlineMaterialSet>();
                set.name = "AdvancedOutlineMaterialSet (Runtime Fallback)";
                set.hideFlags = HideFlags.HideAndDontSave;
                set._maskAlways = CreateRuntimeMaterial( maskShader, "Outline Mask (Always)", CompareFunction.Always, false );
                set._maskLessEqual = CreateRuntimeMaterial( maskShader, "Outline Mask (LessEqual)", CompareFunction.LessEqual, false );
                set._fillAlways = CreateRuntimeMaterial( fillShader, "Outline Fill (Always)", CompareFunction.Always, true );
                set._fillLessEqual = CreateRuntimeMaterial( fillShader, "Outline Fill (LessEqual)", CompareFunction.LessEqual, true );
                set._fillGreater = CreateRuntimeMaterial( fillShader, "Outline Fill (Greater)", CompareFunction.Greater, true );

                s_runtimeFallback = set;
                s_runtimeFallbackUsers = 0;
            }

            s_runtimeFallbackUsers++;
            return s_runtimeFallback;
        }

        public static void ReleaseRuntimeFallback()
        {
            if ( s_runtimeFallbackUsers > 0 )
            {
                s_runtimeFallbackUsers--;
            }
            if ( s_runtimeFallbackUsers > 0 || !s_runtimeFallback ) return;

            AdvancedOutlineMaterialSet set = s_runtimeFallback;
            s_runtimeFallback = null;

            DestroyUnityObject( set._maskAlways );
            DestroyUnityObject( set._maskLessEqual );
            DestroyUnityObject( set._fillAlways );
            DestroyUnityObject( set._fillLessEqual );
            DestroyUnityObject( set._fillGreater );
            DestroyUnityObject( set );
        }

        static Material CreateRuntimeMaterial( Shader shader, string materialName, CompareFunction zTest, bool isFill )
        {
            Material material = new Material( shader );
            material.name = materialName;
            material.hideFlags = HideFlags.HideAndDontSave;
            ApplyZTest( material, zTest );
            if ( isFill )
            {
                ApplyHiddenDefaults( material );
            }
            return material;
        }

        static void DestroyUnityObject( Object obj )
        {
            if ( !obj ) return;
            if ( Application.isPlaying )
            {
                Destroy( obj );
            }
            else
            {
                DestroyImmediate( obj );
            }
        }
    }
}
