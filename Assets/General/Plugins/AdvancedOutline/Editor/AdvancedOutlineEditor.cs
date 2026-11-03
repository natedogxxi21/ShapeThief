using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ITISKIRUHERE
{
    [CustomEditor( typeof( AdvancedOutline ) )]
    [CanEditMultipleObjects]
    public class AdvancedOutlineEditor : Editor
    {
        static readonly GUIContent _outlineMaskShaderLabel = new GUIContent( "Mask Shader", "Shader used for masking the outline silhouette. Needed when creating the outline materials." );
        static readonly GUIContent _outlineFillShaderLabel = new GUIContent( "Fill Shader", "Shader used for rendering the outline stroke. Needed when creating the outline materials." );
        static readonly GUIContent _materialSetLabel = new GUIContent( "Material Set", "Persistent materials shared by all outlines. Created automatically the first time you bake." );
        static readonly GUIContent _outlineModeLabel = new GUIContent( "Outline Mode", "Select where and how the outline should be rendered relative to depth." );
        static readonly GUIContent _outlineColorLabel = new GUIContent( "Outline Color", "Base outline color." );
        static readonly GUIContent _outlineWidthLabel = new GUIContent( "Outline Width", "Base outline thickness." );

        static readonly GUIContent _pulseWidthLabel = new GUIContent( "Pulse Width", "Enable pulsing width effect." );
        static readonly GUIContent _pulseSpeedLabel = new GUIContent( "Pulse Speed", "Speed of the pulse oscillation." );
        static readonly GUIContent _pulseMinWidthLabel = new GUIContent( "Pulse Min Width", "Minimum width during pulse." );
        static readonly GUIContent _pulseMaxWidthLabel = new GUIContent( "Pulse Max Width", "Maximum width during pulse." );

        static readonly GUIContent _rainbowColorLabel = new GUIContent( "Rainbow Color", "Enable rainbow color cycle effect." );
        static readonly GUIContent _rainbowSpeedLabel = new GUIContent( "Rainbow Speed", "Speed of the rainbow color cycling." );

        static readonly GUIContent _dashedPatternLabel = new GUIContent( "Dashed Pattern", "Enable dashed crawling-ants line effect." );
        static readonly GUIContent _dashScaleLabel = new GUIContent( "Dash Scale", "Scale / density of the dashes." );
        static readonly GUIContent _dashSpeedLabel = new GUIContent( "Dash Speed", "Speed of the crawling dash movement." );

        SerializedProperty _outlineMaskShader;
        SerializedProperty _outlineFillShader;
        SerializedProperty _materialSet;
        SerializedProperty _outlineMode;
        SerializedProperty _outlineColor;
        SerializedProperty _outlineWidth;
        SerializedProperty _pulseWidth;
        SerializedProperty _pulseSpeed;
        SerializedProperty _pulseMinWidth;
        SerializedProperty _pulseMaxWidth;
        SerializedProperty _rainbowColor;
        SerializedProperty _rainbowSpeed;
        SerializedProperty _dashedPattern;
        SerializedProperty _dashScale;
        SerializedProperty _dashSpeed;

        bool _referencesFoldout = true;
        bool _generalFoldout = true;
        bool _effectsFoldout = true;
        bool _diagnosticsFoldout = false;

        void OnEnable()
        {
            _outlineMaskShader = serializedObject.FindProperty( "_outlineMaskShader" );
            _outlineFillShader = serializedObject.FindProperty( "_outlineFillShader" );
            _materialSet = serializedObject.FindProperty( "_materialSet" );
            _outlineMode = serializedObject.FindProperty( "_outlineMode" );
            _outlineColor = serializedObject.FindProperty( "_outlineColor" );
            _outlineWidth = serializedObject.FindProperty( "_outlineWidth" );

            _pulseWidth = serializedObject.FindProperty( "_pulseWidth" );
            _pulseSpeed = serializedObject.FindProperty( "_pulseSpeed" );
            _pulseMinWidth = serializedObject.FindProperty( "_pulseMinWidth" );
            _pulseMaxWidth = serializedObject.FindProperty( "_pulseMaxWidth" );

            _rainbowColor = serializedObject.FindProperty( "_rainbowColor" );
            _rainbowSpeed = serializedObject.FindProperty( "_rainbowSpeed" );

            _dashedPattern = serializedObject.FindProperty( "_dashedPattern" );
            _dashScale = serializedObject.FindProperty( "_dashScale" );
            _dashSpeed = serializedObject.FindProperty( "_dashSpeed" );
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            AdvancedOutline firstTarget = (AdvancedOutline)target;

            DrawPersistenceSection( firstTarget );
            EditorGUILayout.Space();

            DrawWarnings( firstTarget );

            // Changing these means the renderers must switch to a different set of baked materials.
            bool materialsNeedSwap = false;

            _referencesFoldout = EditorGUILayout.Foldout( _referencesFoldout, "References", true );
            if ( _referencesFoldout )
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField( _outlineMaskShader, _outlineMaskShaderLabel );
                EditorGUILayout.PropertyField( _outlineFillShader, _outlineFillShaderLabel );

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField( _materialSet, _materialSetLabel );
                if ( EditorGUI.EndChangeCheck() )
                {
                    materialsNeedSwap = true;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            _generalFoldout = EditorGUILayout.Foldout( _generalFoldout, "General Settings", true );
            if ( _generalFoldout )
            {
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField( _outlineMode, _outlineModeLabel );
                if ( EditorGUI.EndChangeCheck() )
                {
                    materialsNeedSwap = true;
                }

                if ( _outlineMode.enumValueIndex != (int)AdvancedOutline.Mode.SilhouetteOnly )
                {
                    EditorGUI.BeginDisabledGroup( _rainbowColor.boolValue );
                    EditorGUILayout.PropertyField( _outlineColor, _outlineColorLabel );
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup( _pulseWidth.boolValue );
                    EditorGUILayout.PropertyField( _outlineWidth, _outlineWidthLabel );
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            _effectsFoldout = EditorGUILayout.Foldout( _effectsFoldout, "Dynamic Effects (Blendable)", true );
            if ( _effectsFoldout )
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField( _pulseWidth, _pulseWidthLabel );
                EditorGUI.BeginDisabledGroup( !_pulseWidth.boolValue );
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField( _pulseSpeed, _pulseSpeedLabel );

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField( _pulseMinWidth, _pulseMinWidthLabel );
                    EditorGUILayout.PropertyField( _pulseMaxWidth, _pulseMaxWidthLabel );
                    if ( EditorGUI.EndChangeCheck() )
                    {
                        if ( _pulseMinWidth.floatValue > _pulseMaxWidth.floatValue )
                        {
                            _pulseMinWidth.floatValue = _pulseMaxWidth.floatValue;
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField( _rainbowColor, _rainbowColorLabel );
                EditorGUI.BeginDisabledGroup( !_rainbowColor.boolValue );
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField( _rainbowSpeed, _rainbowSpeedLabel );
                    EditorGUI.indentLevel--;
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField( _dashedPattern, _dashedPatternLabel );
                EditorGUI.BeginDisabledGroup( !_dashedPattern.boolValue );
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField( _dashScale, _dashScaleLabel );
                    EditorGUILayout.PropertyField( _dashSpeed, _dashSpeedLabel );
                    EditorGUI.indentLevel--;
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            _diagnosticsFoldout = EditorGUILayout.Foldout( _diagnosticsFoldout, "Diagnostics", true );
            if ( _diagnosticsFoldout )
            {
                EditorGUI.indentLevel++;
                DrawDiagnostics( firstTarget );
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            DrawControlButtons();

            // Apply, then (if needed) swap the baked materials in the same Undo step.
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();

            if ( serializedObject.ApplyModifiedProperties() && materialsNeedSwap )
            {
                foreach ( Object t in targets )
                {
                    AdvancedOutlineBaker.ApplyMode( (AdvancedOutline)t );
                }
                Undo.CollapseUndoOperations( undoGroup );
            }
        }

        // ---------------------------------------------------------------------------------
        // Persistence panel
        // ---------------------------------------------------------------------------------

        void DrawPersistenceSection( AdvancedOutline first )
        {
            EditorGUILayout.BeginVertical( EditorStyles.helpBox );
            EditorGUILayout.LabelField( "Persistent Outline Data", EditorStyles.boldLabel );

            AdvancedOutlineBaker.BakeStatus status = AdvancedOutlineBaker.GetStatus( first );

            switch ( status.state )
            {
                case AdvancedOutlineBaker.BakeState.NotBaked:
                    EditorGUILayout.HelpBox(
                        "Not baked. The outline is only generated in memory while the game is playing, so it is not visible in the editor " +
                        "and nothing is stored in this prefab/scene. Click \"Bake Outline Assets\" to make it persistent.",
                        MessageType.Info );
                    break;

                case AdvancedOutlineBaker.BakeState.UpToDate:
                    EditorGUILayout.HelpBox(
                        $"Baked: {status.bakedRenderers} renderer(s) using {status.bakedMeshAssets} outline mesh asset(s). " +
                        "Everything is stored as project assets; nothing is generated at runtime.",
                        MessageType.None );
                    break;

                case AdvancedOutlineBaker.BakeState.OutOfDate:
                    EditorGUILayout.HelpBox(
                        "The baked data is out of date:\n- " + string.Join( "\n- ", status.issues ) + "\n\nClick \"Re-Bake\" to rebuild it.",
                        MessageType.Warning );
                    break;
            }

            if ( PrefabUtility.IsPartOfPrefabInstance( first ) && PrefabStageUtility.GetCurrentPrefabStage() == null )
            {
                EditorGUILayout.HelpBox(
                    "This is a Prefab instance. Baking here stores the outline only as overrides on this instance. " +
                    "To make it part of the prefab itself, open the prefab (or select the prefab asset in the Project window) and bake there.",
                    MessageType.Info );
            }

            if ( targets.Length > 1 )
            {
                EditorGUILayout.HelpBox( "Several objects selected: the status above is for the first one. Buttons apply to all of them.", MessageType.None );
            }

            if ( Application.isPlaying )
            {
                EditorGUILayout.HelpBox( "Baking is only available in Edit Mode.", MessageType.None );
            }

            using ( new EditorGUI.DisabledScope( Application.isPlaying ) )
            {
                EditorGUILayout.BeginHorizontal();

                string bakeLabel = status.state == AdvancedOutlineBaker.BakeState.NotBaked ? "Bake Outline Assets" : "Re-Bake";
                if ( GUILayout.Button( bakeLabel, GUILayout.Height( 28 ) ) )
                {
                    AdvancedOutlineBaker.BakeAll( GetTargetOutlines() );
                    GUIUtility.ExitGUI();
                }

                using ( new EditorGUI.DisabledScope( status.state == AdvancedOutlineBaker.BakeState.NotBaked ) )
                {
                    if ( GUILayout.Button( "Remove Bake", GUILayout.Height( 28 ) ) )
                    {
                        AdvancedOutlineBaker.RemoveBakeAll( GetTargetOutlines() );
                        GUIUtility.ExitGUI();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if ( first.MaterialSet )
            {
                if ( GUILayout.Button( "Show Material Set" ) )
                {
                    EditorGUIUtility.PingObject( first.MaterialSet );
                }
            }

            EditorGUILayout.EndVertical();
        }

        AdvancedOutline[] GetTargetOutlines()
        {
            AdvancedOutline[] outlines = new AdvancedOutline[targets.Length];
            for ( int i = 0; i < targets.Length; i++ )
            {
                outlines[i] = (AdvancedOutline)targets[i];
            }
            return outlines;
        }

        // ---------------------------------------------------------------------------------
        // Warnings / diagnostics / buttons
        // ---------------------------------------------------------------------------------

        void DrawWarnings( AdvancedOutline targetOutline )
        {
            // The shader references are only needed to create materials, not once a complete set exists.
            bool hasSet = _materialSet.objectReferenceValue != null;
            if ( !hasSet && !_outlineMaskShader.objectReferenceValue )
            {
                EditorGUILayout.HelpBox( "Mask Shader reference is missing! Assign it under References so the outline materials can be created.", MessageType.Error );
            }
            if ( !hasSet && !_outlineFillShader.objectReferenceValue )
            {
                EditorGUILayout.HelpBox( "Fill Shader reference is missing! Assign it under References so the outline materials can be created.", MessageType.Error );
            }

            int rendererCount;
            int meshCount;
            AdvancedOutlineBaker.CountTargets( targetOutline, out rendererCount, out meshCount );

            if ( rendererCount > 30 )
            {
                EditorGUILayout.HelpBox( $"Warning: This GameObject contains {rendererCount} outlined renderers. Outlines on large hierarchies can significantly impact performance (two extra draw passes per renderer).", MessageType.Warning );
            }
        }

        void DrawDiagnostics( AdvancedOutline targetOutline )
        {
            int rendererCount;
            int meshCount;
            AdvancedOutlineBaker.CountTargets( targetOutline, out rendererCount, out meshCount );

            EditorGUILayout.BeginVertical( EditorStyles.helpBox );
            EditorGUILayout.LabelField( $"Outlined Renderers: {rendererCount}" );
            EditorGUILayout.LabelField( $"Unique Source Meshes: {meshCount}" );
            EditorGUILayout.LabelField( $"Storage: {( targetOutline.IsBaked ? "Baked (project assets)" : "Runtime only (generated in Play Mode)" )}" );
            EditorGUILayout.EndVertical();
        }

        void DrawControlButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if ( GUILayout.Button( "Refresh Renderers", GUILayout.Height( 24 ) ) )
            {
                foreach ( Object t in targets )
                {
                    AdvancedOutline outline = (AdvancedOutline)t;
                    if ( outline.isActiveAndEnabled )
                    {
                        outline.RefreshRenderers();
                    }
                }
            }

            if ( GUILayout.Button( "Reset Defaults", GUILayout.Height( 24 ) ) )
            {
                ResetToDefaults();
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        void ResetToDefaults()
        {
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();

            foreach ( Object t in targets )
            {
                AdvancedOutline outline = (AdvancedOutline)t;
                Undo.RecordObject( outline, "Reset Outline Defaults" );
                outline.OutlineMode = AdvancedOutline.Mode.OutlineVisible;
                outline.OutlineColor = Color.cyan;
                outline.OutlineWidth = 4f;
                outline.PulseWidth = false;
                outline.RainbowColor = false;
                outline.DashedPattern = false;
                EditorUtility.SetDirty( outline );

                // The mode may have changed, so baked renderers must use the matching materials.
                AdvancedOutlineBaker.ApplyMode( outline );
            }

            Undo.CollapseUndoOperations( undoGroup );
        }
    }
}
