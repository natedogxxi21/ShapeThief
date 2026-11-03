#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace ITISKIRUHERE
{
    /// <summary>
    /// Creates, applies, validates and removes the persistent data behind <see cref="AdvancedOutline"/>.
    ///
    /// Bake writes:
    ///   * one outline mesh asset per source mesh (smooth normals in UV4, optional combined submesh)
    ///   * one <see cref="AdvancedOutlineMaterialSet"/> with the five mask/fill materials
    /// and then points the renderers at them, so prefabs and scenes are fully self-contained.
    /// A record of the original meshes is stored on the component so "Remove Bake" restores everything.
    ///
    /// Components that live inside prefab ASSETS (selected in the Project window) are handled by
    /// loading the prefab contents, baking there, and saving the prefab back.
    /// </summary>
    public static class AdvancedOutlineBaker
    {
        public const string DefaultFolder = "Assets/AdvancedOutline/Generated";
        const string UndoName = "Advanced Outline";
        const string OutlineMeshSuffix = " (Outline)";

        public enum BakeState
        {
            NotBaked,
            UpToDate,
            OutOfDate
        }

        public class BakeReport
        {
            public int renderers;
            public int meshAssets;
            public string error;
            public readonly List<string> warnings = new List<string>();

            public bool Succeeded => string.IsNullOrEmpty( error );
        }

        public class BakeStatus
        {
            public BakeState state;
            public int bakedRenderers;
            public int bakedMeshAssets;
            public readonly List<string> issues = new List<string>();
        }

        // One session per user action. Lets several components share the same generated mesh asset.
        class Session
        {
            public readonly BakeReport report = new BakeReport();
            public readonly Dictionary<(Mesh mesh, int flags), Mesh> meshes = new Dictionary<(Mesh mesh, int flags), Mesh>();
            public readonly HashSet<AdvancedOutlineMaterialSet> verifiedSets = new HashSet<AdvancedOutlineMaterialSet>();
        }

        // ---------------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------------

        public static BakeReport Bake( AdvancedOutline outline )
        {
            return BakeAll( new[] { outline } );
        }

        public static BakeReport BakeAll( IEnumerable<AdvancedOutline> outlines )
        {
            Session session = new Session();
            Process( outlines, session, BakeComponent );
            FinishSession( session, "Baked" );
            return session.report;
        }

        public static void RemoveBake( AdvancedOutline outline )
        {
            RemoveBakeAll( new[] { outline } );
        }

        public static void RemoveBakeAll( IEnumerable<AdvancedOutline> outlines )
        {
            Session session = new Session();
            Process( outlines, session, RemoveComponentBake );
            FinishSession( session, null );
        }

        /// <summary>
        /// Swaps the baked renderers over to the mask/fill materials for the component's current Mode
        /// (and Material Set). Records Undo and saves prefab assets.
        /// </summary>
        public static void ApplyMode( AdvancedOutline outline )
        {
            if ( !outline || !outline.IsBaked ) return;

            List<Renderer> targets = new List<Renderer>();
            outline.GatherTargets( targets );
            foreach ( Renderer renderer in targets )
            {
                Record( renderer );
            }

            outline.ApplyMaterialsForCurrentMode();

            foreach ( Renderer renderer in targets )
            {
                MarkDirty( renderer );
            }
            MarkDirty( outline );

            if ( EditorUtility.IsPersistent( outline ) )
            {
                PrefabUtility.SavePrefabAsset( outline.transform.root.gameObject );
            }
        }

        public static BakeStatus GetStatus( AdvancedOutline outline )
        {
            BakeStatus status = new BakeStatus();
            IReadOnlyList<AdvancedOutline.BakedRenderer> entries = outline.BakedRenderers;
            status.bakedRenderers = entries.Count;

            if ( entries.Count == 0 )
            {
                status.state = BakeState.NotBaked;
                return status;
            }

            AdvancedOutlineMaterialSet set = outline.MaterialSet;
            bool setOk = set && set.IsComplete;
            Material mask = setOk ? set.GetMask( outline.OutlineMode ) : null;
            Material fill = setOk ? set.GetFill( outline.OutlineMode ) : null;

            HashSet<Mesh> bakedMeshes = new HashSet<Mesh>();
            HashSet<Renderer> known = new HashSet<Renderer>();
            int missing = 0, wrongMesh = 0, outdated = 0, wrongMaterials = 0;

            foreach ( AdvancedOutline.BakedRenderer entry in entries )
            {
                if ( !entry.renderer || !entry.bakedMesh )
                {
                    missing++;
                    continue;
                }

                known.Add( entry.renderer );
                bakedMeshes.Add( entry.bakedMesh );

                if ( AdvancedOutlineMeshUtility.GetSharedMesh( entry.renderer ) != entry.bakedMesh )
                {
                    wrongMesh++;
                }
                else if ( entry.originalMesh && entry.originalMesh.vertexCount != entry.bakedMesh.vertexCount )
                {
                    outdated++;
                }

                Material[] materials = entry.renderer.sharedMaterials;
                int n = materials.Length;
                bool pairOk = setOk && n >= 2 && materials[n - 2] == mask && materials[n - 1] == fill;
                if ( !pairOk )
                {
                    wrongMaterials++;
                }
            }

            List<Renderer> targets = new List<Renderer>();
            outline.GatherTargets( targets );
            int added = 0;
            foreach ( Renderer renderer in targets )
            {
                if ( !known.Contains( renderer ) && AdvancedOutlineMeshUtility.GetSharedMesh( renderer ) )
                {
                    added++;
                }
            }

            if ( !setOk ) status.issues.Add( "The Material Set is missing or incomplete." );
            if ( missing > 0 ) status.issues.Add( $"{missing} baked renderer(s) or mesh asset(s) no longer exist." );
            if ( wrongMesh > 0 ) status.issues.Add( $"{wrongMesh} renderer(s) no longer use their baked mesh." );
            if ( outdated > 0 ) status.issues.Add( $"{outdated} source mesh(es) changed since baking." );
            if ( setOk && wrongMaterials > 0 ) status.issues.Add( $"{wrongMaterials} renderer(s) lack the outline materials for the current mode." );
            if ( added > 0 ) status.issues.Add( $"{added} renderer(s) were added after baking." );

            status.bakedMeshAssets = bakedMeshes.Count;
            status.state = status.issues.Count == 0 ? BakeState.UpToDate : BakeState.OutOfDate;
            return status;
        }

        public static void CountTargets( AdvancedOutline outline, out int rendererCount, out int uniqueMeshCount )
        {
            List<Renderer> targets = new List<Renderer>();
            outline.GatherTargets( targets );

            HashSet<Mesh> meshes = new HashSet<Mesh>();
            foreach ( Renderer renderer in targets )
            {
                Mesh mesh = AdvancedOutlineMeshUtility.GetSharedMesh( renderer );
                if ( mesh )
                {
                    meshes.Add( mesh );
                }
            }

            rendererCount = targets.Count;
            uniqueMeshCount = meshes.Count;
        }

        /// <summary>Removes any outline mask/fill materials from every renderer under root. Returns renderers changed.</summary>
        public static int StripOutlineMaterials( GameObject root )
        {
            int changed = 0;
            foreach ( Renderer renderer in root.GetComponentsInChildren<Renderer>( true ) )
            {
                if ( StripOutlineMaterials( renderer ) )
                {
                    changed++;
                }
            }
            return changed;
        }

        // ---------------------------------------------------------------------------------
        // Dispatch: scene / prefab-stage objects directly, prefab assets via LoadPrefabContents
        // ---------------------------------------------------------------------------------

        static void Process( IEnumerable<AdvancedOutline> outlines, Session session, System.Action<AdvancedOutline, Session> action )
        {
            HashSet<string> handledAssets = new HashSet<string>();
            List<AdvancedOutline> direct = new List<AdvancedOutline>();

            foreach ( AdvancedOutline outline in outlines )
            {
                if ( !outline ) continue;

                if ( EditorUtility.IsPersistent( outline ) )
                {
                    string path = AssetDatabase.GetAssetPath( outline );
                    if ( handledAssets.Add( path ) )
                    {
                        ProcessPrefabAsset( path, session, action );
                    }
                }
                else if ( !direct.Contains( outline ) )
                {
                    direct.Add( outline );
                }
            }

            if ( direct.Count == 0 ) return;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName( UndoName );
            int group = Undo.GetCurrentGroup();

            foreach ( AdvancedOutline outline in direct )
            {
                action( outline, session );
            }

            Undo.CollapseUndoOperations( group );
        }

        static void ProcessPrefabAsset( string path, Session session, System.Action<AdvancedOutline, Session> action )
        {
            if ( string.IsNullOrEmpty( path ) || !path.EndsWith( ".prefab" ) )
            {
                session.report.warnings.Add( $"'{path}' is not a prefab asset. Bake it from a scene instance instead." );
                return;
            }

            // Make sure edits made through the inspector are on disk before we load the contents.
            AssetDatabase.SaveAssets();

            GameObject root = PrefabUtility.LoadPrefabContents( path );
            try
            {
                AdvancedOutline[] components = root.GetComponentsInChildren<AdvancedOutline>( true );
                foreach ( AdvancedOutline component in components )
                {
                    action( component, session );
                }

                if ( components.Length > 0 )
                {
                    PrefabUtility.SaveAsPrefabAsset( root, path );
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents( root );
            }
        }

        static void FinishSession( Session session, string verb )
        {
            AssetDatabase.SaveAssets();

            BakeReport report = session.report;
            foreach ( string warning in report.warnings )
            {
                Debug.LogWarning( "[Advanced Outline] " + warning );
            }

            if ( !report.Succeeded )
            {
                Debug.LogError( "[Advanced Outline] " + report.error );
            }
            else if ( verb != null )
            {
                Debug.Log( $"[Advanced Outline] {verb} {report.renderers} renderer(s) using {report.meshAssets} outline mesh asset(s)." );
            }
        }

        // ---------------------------------------------------------------------------------
        // Bake / remove one component
        // ---------------------------------------------------------------------------------

        static void BakeComponent( AdvancedOutline outline, Session session )
        {
            BakeReport report = session.report;

            AdvancedOutlineMaterialSet set = EnsureMaterialSet( outline, session );
            if ( !set ) return;

            // Start from the original state so baking is repeatable.
            RestoreOriginals( outline );

            Material mask = set.GetMask( outline.OutlineMode );
            Material fill = set.GetFill( outline.OutlineMode );

            List<Renderer> targets = new List<Renderer>();
            outline.GatherTargets( targets );

            List<AdvancedOutline.BakedRenderer> entries = new List<AdvancedOutline.BakedRenderer>();
            foreach ( Renderer renderer in targets )
            {
                // Also cleans up leftovers written by the old runtime-only version.
                StripOutlineMaterials( renderer );

                Mesh source = AdvancedOutlineMeshUtility.GetSharedMesh( renderer );
                if ( !source )
                {
                    report.warnings.Add( $"'{renderer.name}' has no mesh and was skipped." );
                    continue;
                }
                if ( source.name.EndsWith( OutlineMeshSuffix ) )
                {
                    report.warnings.Add( $"'{renderer.name}' already uses an outline mesh ('{source.name}') that this component did not bake. " +
                        "Assign the original mesh again and re-bake." );
                    continue;
                }

                Material[] materials = renderer.sharedMaterials;
                bool skinned = renderer is SkinnedMeshRenderer;
                bool combine = AdvancedOutlineMeshUtility.NeedsCombinedSubmesh( source, materials.Length );

                Mesh baked = GetOrCreateBakedMesh( source, skinned, combine, set, session, renderer.name );
                if ( !baked ) continue;

                Object holder = MeshHolder( renderer );
                Record( holder );
                AdvancedOutlineMeshUtility.SetSharedMesh( renderer, baked );
                MarkDirty( holder );

                Record( renderer );
                renderer.sharedMaterials = Append( materials, mask, fill );
                MarkDirty( renderer );

                entries.Add( new AdvancedOutline.BakedRenderer
                {
                    renderer = renderer,
                    originalMesh = source,
                    bakedMesh = baked
                } );
                report.renderers++;
            }

            WriteRecord( outline, set, entries );

            if ( CanUndo( outline ) )
            {
                outline.RefreshRenderers();
            }
        }

        static void RemoveComponentBake( AdvancedOutline outline, Session session )
        {
            RestoreOriginals( outline );

            List<Renderer> targets = new List<Renderer>();
            outline.GatherTargets( targets );
            foreach ( Renderer renderer in targets )
            {
                StripOutlineMaterials( renderer );
            }

            WriteRecord( outline, null, new List<AdvancedOutline.BakedRenderer>() );

            if ( CanUndo( outline ) )
            {
                outline.RefreshRenderers();
            }
        }

        static void RestoreOriginals( AdvancedOutline outline )
        {
            foreach ( AdvancedOutline.BakedRenderer entry in outline.BakedRenderers )
            {
                Renderer renderer = entry.renderer;
                if ( !renderer ) continue;

                // Only restore if the baked mesh is still in use; don't clobber a mesh the user swapped in.
                if ( entry.originalMesh && AdvancedOutlineMeshUtility.GetSharedMesh( renderer ) == entry.bakedMesh )
                {
                    Object holder = MeshHolder( renderer );
                    Record( holder );
                    AdvancedOutlineMeshUtility.SetSharedMesh( renderer, entry.originalMesh );
                    MarkDirty( holder );
                }

                StripOutlineMaterials( renderer );
            }
        }

        // Stores the bake record (and material set) on the component through SerializedObject,
        // which handles Undo and prefab overrides correctly.
        static void WriteRecord( AdvancedOutline outline, AdvancedOutlineMaterialSet set, List<AdvancedOutline.BakedRenderer> entries )
        {
            SerializedObject so = new SerializedObject( outline );

            if ( set )
            {
                so.FindProperty( "_materialSet" ).objectReferenceValue = set;
            }

            SerializedProperty list = so.FindProperty( "_bakedRenderers" );
            list.arraySize = 0;
            list.arraySize = entries.Count;
            for ( int i = 0; i < entries.Count; i++ )
            {
                SerializedProperty element = list.GetArrayElementAtIndex( i );
                element.FindPropertyRelative( "renderer" ).objectReferenceValue = entries[i].renderer;
                element.FindPropertyRelative( "originalMesh" ).objectReferenceValue = entries[i].originalMesh;
                element.FindPropertyRelative( "bakedMesh" ).objectReferenceValue = entries[i].bakedMesh;
            }

            if ( CanUndo( outline ) )
            {
                so.ApplyModifiedProperties();
            }
            else
            {
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            MarkDirty( outline );
        }

        // ---------------------------------------------------------------------------------
        // Mesh assets
        // ---------------------------------------------------------------------------------

        static Mesh GetOrCreateBakedMesh( Mesh source, bool skinned, bool combine, AdvancedOutlineMaterialSet set, Session session, string ownerName )
        {
            int flags = ( skinned ? 1 : 0 ) | ( combine ? 2 : 0 );
            var key = ( source, flags );

            Mesh cached;
            if ( session.meshes.TryGetValue( key, out cached ) && cached )
            {
                return cached;
            }

            Mesh fresh = AdvancedOutlineMeshUtility.CreateOutlineMesh( source, skinned, combine );
            if ( !fresh )
            {
                session.report.warnings.Add( $"Mesh '{source.name}' (used by '{ownerName}') could not be read. " +
                    "Enable Read/Write in the model's import settings, then bake again." );
                return null;
            }

            string folder = MeshFolder( set );
            EnsureFolder( folder );
            string path = folder + "/" + BuildMeshFileName( source, skinned, combine ) + ".asset";

            // Rebuild in place when it already exists so existing references stay valid.
            Mesh asset = AssetDatabase.LoadAssetAtPath<Mesh>( path );
            if ( asset )
            {
                EditorUtility.CopySerialized( fresh, asset );
                Object.DestroyImmediate( fresh );
                EditorUtility.SetDirty( asset );
            }
            else
            {
                AssetDatabase.CreateAsset( fresh, path );
                asset = fresh;
            }

            session.meshes[key] = asset;
            session.report.meshAssets++;
            return asset;
        }

        static string BuildMeshFileName( Mesh source, bool skinned, bool combine )
        {
            string id;
            string guid;
            long localId;
            if ( AssetDatabase.TryGetGUIDAndLocalFileIdentifier( source, out guid, out localId ) )
            {
                id = Hash128.Compute( guid + ":" + localId ).ToString().Substring( 0, 8 );
            }
            else
            {
                id = Hash128.Compute( source.name + ":" + source.vertexCount + ":" + source.bounds ).ToString().Substring( 0, 8 );
            }

            string variant = ( skinned ? "Skinned" : "" ) + ( combine ? "Combined" : "" );
            return SanitizeFileName( source.name ) + "_" + id + "_Outline" + variant;
        }

        static string MeshFolder( AdvancedOutlineMaterialSet set )
        {
            string setFolder = Path.GetDirectoryName( AssetDatabase.GetAssetPath( set ) ).Replace( '\\', '/' );
            return setFolder + "/Meshes";
        }

        // ---------------------------------------------------------------------------------
        // Material set assets
        // ---------------------------------------------------------------------------------

        static AdvancedOutlineMaterialSet EnsureMaterialSet( AdvancedOutline outline, Session session )
        {
            AdvancedOutlineMaterialSet set = outline.MaterialSet;
            if ( !set ) set = FindProjectMaterialSet();
            if ( !set ) set = CreateMaterialSet();

            if ( !set )
            {
                session.report.error = "Could not create an Advanced Outline Material Set.";
                return null;
            }

            if ( !session.verifiedSets.Contains( set ) )
            {
                if ( !FillMaterialSet( set, outline, session.report ) ) return null;
                session.verifiedSets.Add( set );
            }

            return set;
        }

        static AdvancedOutlineMaterialSet FindProjectMaterialSet()
        {
            string[] guids = AssetDatabase.FindAssets( "t:AdvancedOutlineMaterialSet" );
            if ( guids.Length == 0 ) return null;

            string chosen = AssetDatabase.GUIDToAssetPath( guids[0] );
            foreach ( string guid in guids )
            {
                string path = AssetDatabase.GUIDToAssetPath( guid );
                if ( path.StartsWith( DefaultFolder ) )
                {
                    chosen = path;
                    break;
                }
            }
            return AssetDatabase.LoadAssetAtPath<AdvancedOutlineMaterialSet>( chosen );
        }

        static AdvancedOutlineMaterialSet CreateMaterialSet()
        {
            EnsureFolder( DefaultFolder );
            AdvancedOutlineMaterialSet set = ScriptableObject.CreateInstance<AdvancedOutlineMaterialSet>();
            string path = AssetDatabase.GenerateUniqueAssetPath( DefaultFolder + "/AdvancedOutlineMaterialSet.asset" );
            AssetDatabase.CreateAsset( set, path );
            return set;
        }

        // Makes sure all five materials exist, use the right shader and have the right ZTest.
        static bool FillMaterialSet( AdvancedOutlineMaterialSet set, AdvancedOutline outline, BakeReport report )
        {
            Shader maskShader = outline.OutlineMaskShader ? outline.OutlineMaskShader : Shader.Find( AdvancedOutlineMaterialSet.MaskShaderName );
            Shader fillShader = outline.OutlineFillShader ? outline.OutlineFillShader : Shader.Find( AdvancedOutlineMaterialSet.FillShaderName );
            if ( !maskShader || !fillShader )
            {
                report.error = "The 'Custom/Advanced Outline Mask' / 'Custom/Advanced Outline Fill' shaders could not be found. " +
                    "Assign them on the component's References section.";
                return false;
            }

            string setFolder = Path.GetDirectoryName( AssetDatabase.GetAssetPath( set ) ).Replace( '\\', '/' );
            string materialFolder = setFolder + "/Materials";
            EnsureFolder( materialFolder );

            SerializedObject so = new SerializedObject( set );
            EnsureMaterial( so, "_maskAlways", materialFolder, "AdvancedOutline_Mask_Always", maskShader, CompareFunction.Always, false );
            EnsureMaterial( so, "_maskLessEqual", materialFolder, "AdvancedOutline_Mask_LessEqual", maskShader, CompareFunction.LessEqual, false );
            EnsureMaterial( so, "_fillAlways", materialFolder, "AdvancedOutline_Fill_Always", fillShader, CompareFunction.Always, true );
            EnsureMaterial( so, "_fillLessEqual", materialFolder, "AdvancedOutline_Fill_LessEqual", fillShader, CompareFunction.LessEqual, true );
            EnsureMaterial( so, "_fillGreater", materialFolder, "AdvancedOutline_Fill_Greater", fillShader, CompareFunction.Greater, true );

            if ( so.ApplyModifiedPropertiesWithoutUndo() )
            {
                EditorUtility.SetDirty( set );
            }
            return true;
        }

        static void EnsureMaterial( SerializedObject so, string propertyName, string folder, string fileName,
            Shader shader, CompareFunction zTest, bool isFill )
        {
            SerializedProperty property = so.FindProperty( propertyName );
            Material material = property.objectReferenceValue as Material;

            if ( !material )
            {
                material = AssetDatabase.LoadAssetAtPath<Material>( folder + "/" + fileName + ".mat" );
            }

            if ( !material )
            {
                material = new Material( shader );
                material.name = fileName;
                if ( isFill )
                {
                    AdvancedOutlineMaterialSet.ApplyHiddenDefaults( material );
                }
                AdvancedOutlineMaterialSet.ApplyZTest( material, zTest );
                AssetDatabase.CreateAsset( material, folder + "/" + fileName + ".mat" );
            }
            else
            {
                bool changed = false;
                if ( material.shader != shader )
                {
                    material.shader = shader;
                    changed = true;
                }
                changed |= AdvancedOutlineMaterialSet.ApplyZTest( material, zTest );
                if ( changed )
                {
                    EditorUtility.SetDirty( material );
                }
            }

            property.objectReferenceValue = material;
        }

        // ---------------------------------------------------------------------------------
        // Renderer helpers
        // ---------------------------------------------------------------------------------

        static bool StripOutlineMaterials( Renderer renderer )
        {
            Material[] materials = renderer.sharedMaterials;
            List<Material> kept = new List<Material>( materials.Length );
            foreach ( Material material in materials )
            {
                if ( !AdvancedOutlineMaterialSet.IsOutlineMaterial( material ) )
                {
                    kept.Add( material );
                }
            }

            if ( kept.Count == materials.Length ) return false;

            Record( renderer );
            renderer.sharedMaterials = kept.ToArray();
            MarkDirty( renderer );
            return true;
        }

        static Material[] Append( Material[] materials, Material mask, Material fill )
        {
            Material[] result = new Material[materials.Length + 2];
            materials.CopyTo( result, 0 );
            result[materials.Length] = mask;
            result[materials.Length + 1] = fill;
            return result;
        }

        // The object whose serialized data holds the mesh reference (MeshFilter or SkinnedMeshRenderer).
        static Object MeshHolder( Renderer renderer )
        {
            if ( renderer is SkinnedMeshRenderer ) return renderer;
            return renderer.GetComponent<MeshFilter>();
        }

        // ---------------------------------------------------------------------------------
        // Undo / dirty / file helpers
        // ---------------------------------------------------------------------------------

        static GameObject GameObjectOf( Object obj )
        {
            Component component = obj as Component;
            if ( component ) return component.gameObject;
            return obj as GameObject;
        }

        // Objects opened with LoadPrefabContents live in a hidden preview scene that isn't part of
        // the Undo history; objects in a real Prefab Mode stage are.
        static bool IsTemporaryPreview( GameObject go )
        {
            return go && EditorSceneManager.IsPreviewSceneObject( go ) && PrefabStageUtility.GetPrefabStage( go ) == null;
        }

        static bool CanUndo( Object obj )
        {
            if ( !obj || EditorUtility.IsPersistent( obj ) ) return false;
            return !IsTemporaryPreview( GameObjectOf( obj ) );
        }

        static void Record( Object obj )
        {
            if ( CanUndo( obj ) )
            {
                Undo.RecordObject( obj, UndoName );
            }
        }

        static void MarkDirty( Object obj )
        {
            if ( !obj ) return;

            EditorUtility.SetDirty( obj );

            if ( !CanUndo( obj ) ) return;

            GameObject go = GameObjectOf( obj );
            if ( go && PrefabUtility.IsPartOfPrefabInstance( go ) )
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications( obj );
            }
            if ( go && go.scene.IsValid() )
            {
                EditorSceneManager.MarkSceneDirty( go.scene );
            }
        }

        static void EnsureFolder( string folder )
        {
            if ( AssetDatabase.IsValidFolder( folder ) ) return;

            string parent = Path.GetDirectoryName( folder ).Replace( '\\', '/' );
            EnsureFolder( parent );
            AssetDatabase.CreateFolder( parent, Path.GetFileName( folder ) );
        }

        static string SanitizeFileName( string name )
        {
            foreach ( char c in Path.GetInvalidFileNameChars() )
            {
                name = name.Replace( c, '_' );
            }
            return string.IsNullOrEmpty( name ) ? "Mesh" : name;
        }

        // ---------------------------------------------------------------------------------
        // Menu items (work on scene objects and prefab assets in the current selection)
        // ---------------------------------------------------------------------------------

        static List<AdvancedOutline> CollectSelectedOutlines()
        {
            List<AdvancedOutline> result = new List<AdvancedOutline>();
            foreach ( GameObject go in Selection.gameObjects )
            {
                foreach ( AdvancedOutline outline in go.GetComponentsInChildren<AdvancedOutline>( true ) )
                {
                    if ( !result.Contains( outline ) )
                    {
                        result.Add( outline );
                    }
                }
            }
            return result;
        }

        [MenuItem( "Tools/Advanced Outline/Bake Selected" )]
        static void MenuBake()
        {
            List<AdvancedOutline> outlines = CollectSelectedOutlines();
            if ( outlines.Count == 0 )
            {
                EditorUtility.DisplayDialog( "Advanced Outline", "Select scene objects or prefab assets that contain an Advanced Outline component.", "OK" );
                return;
            }
            BakeAll( outlines );
        }

        [MenuItem( "Tools/Advanced Outline/Remove Bake From Selected" )]
        static void MenuRemoveBake()
        {
            List<AdvancedOutline> outlines = CollectSelectedOutlines();
            if ( outlines.Count == 0 )
            {
                EditorUtility.DisplayDialog( "Advanced Outline", "Select scene objects or prefab assets that contain an Advanced Outline component.", "OK" );
                return;
            }
            RemoveBakeAll( outlines );
        }

        [MenuItem( "Tools/Advanced Outline/Strip Outline Materials From Selected" )]
        static void MenuStrip()
        {
            int changed = 0;
            foreach ( GameObject go in Selection.gameObjects )
            {
                if ( EditorUtility.IsPersistent( go ) )
                {
                    string path = AssetDatabase.GetAssetPath( go );
                    if ( !path.EndsWith( ".prefab" ) ) continue;

                    GameObject root = PrefabUtility.LoadPrefabContents( path );
                    try
                    {
                        int count = StripOutlineMaterials( root );
                        if ( count > 0 )
                        {
                            PrefabUtility.SaveAsPrefabAsset( root, path );
                            changed += count;
                        }
                    }
                    finally
                    {
                        PrefabUtility.UnloadPrefabContents( root );
                    }
                }
                else
                {
                    changed += StripOutlineMaterials( go );
                }
            }

            Debug.Log( $"[Advanced Outline] Removed outline materials from {changed} renderer(s). " +
                "Meshes are not restored by this command; use 'Remove Bake' on objects that still have the component." );
        }
    }
}
#endif
