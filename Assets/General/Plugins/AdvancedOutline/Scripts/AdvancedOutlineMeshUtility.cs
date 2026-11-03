using System.Collections.Generic;
using UnityEngine;

namespace ITISKIRUHERE
{
    /// <summary>
    /// Mesh helpers shared by the runtime component and the editor baker, so both produce
    /// exactly the same outline mesh data.
    ///
    /// An "outline mesh" is a copy of a source mesh where:
    ///  - UV4 (TEXCOORD3) holds the smoothed normals used for extrusion (zeros for skinned meshes,
    ///    because custom UVs are not skinned; the shader then falls back to the vertex normal);
    ///  - optionally an extra submesh containing every triangle is appended, so the two outline
    ///    materials (added after the original ones) draw the whole mesh in a single pass each.
    /// </summary>
    public static class AdvancedOutlineMeshUtility
    {
        const int SkinnedFlag = 1;
        const int CombinedFlag = 2;

        class LiveEntry
        {
            public Mesh mesh;
            public int users;
        }

        // Runtime-only cache: one generated mesh per (source mesh, variant), shared by all components.
        static readonly Dictionary<(Mesh mesh, int flags), LiveEntry> s_liveMeshes =
            new Dictionary<(Mesh mesh, int flags), LiveEntry>();

        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration )]
        static void ResetStatics()
        {
            s_liveMeshes.Clear();
        }

        // ---------------------------------------------------------------------------------
        // Renderer <-> mesh access
        // ---------------------------------------------------------------------------------

        public static Mesh GetSharedMesh( Renderer renderer )
        {
            SkinnedMeshRenderer skinned = renderer as SkinnedMeshRenderer;
            if ( skinned ) return skinned.sharedMesh;

            MeshFilter filter = renderer ? renderer.GetComponent<MeshFilter>() : null;
            return filter ? filter.sharedMesh : null;
        }

        public static void SetSharedMesh( Renderer renderer, Mesh mesh )
        {
            SkinnedMeshRenderer skinned = renderer as SkinnedMeshRenderer;
            if ( skinned )
            {
                // Assigning a mesh can reset the bounds; keep whatever the author configured.
                Bounds bounds = skinned.localBounds;
                skinned.sharedMesh = mesh;
                skinned.localBounds = bounds;
                return;
            }

            MeshFilter filter = renderer ? renderer.GetComponent<MeshFilter>() : null;
            if ( filter )
            {
                filter.sharedMesh = mesh;
            }
        }

        // ---------------------------------------------------------------------------------
        // Geometry
        // ---------------------------------------------------------------------------------

        /// <summary>
        /// A combined submesh is needed when the mesh has several triangle submeshes that are all
        /// covered by the renderer's own materials. Same rule as the original component.
        /// </summary>
        public static bool NeedsCombinedSubmesh( Mesh mesh, int materialCount )
        {
            if ( !mesh || mesh.subMeshCount <= 1 || mesh.subMeshCount > materialCount ) return false;

            for ( int i = 0; i < mesh.subMeshCount; i++ )
            {
                if ( mesh.GetTopology( i ) != MeshTopology.Triangles ) return false;
            }
            return true;
        }

        public static bool CanReadVertexData( Mesh mesh )
        {
            if ( !mesh ) return false;

            // Player builds (and play mode) cannot read non-readable meshes. In edit mode we try anyway
            // and verify that data actually came back, so unreadable models don't always need re-importing.
            if ( Application.isPlaying && !mesh.isReadable ) return false;

            return mesh.vertices.Length == mesh.vertexCount;
        }

        public static List<Vector3> BuildSmoothNormals( Mesh mesh )
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            if ( normals.Length != vertices.Length )
            {
                normals = new Vector3[vertices.Length];
            }

            // Sum the normals of all vertices that share a position, then normalise.
            Dictionary<Vector3, Vector3> sums = new Dictionary<Vector3, Vector3>( vertices.Length );
            for ( int i = 0; i < vertices.Length; i++ )
            {
                Vector3 sum;
                sums.TryGetValue( vertices[i], out sum );
                sums[vertices[i]] = sum + normals[i];
            }

            List<Vector3> smoothNormals = new List<Vector3>( vertices.Length );
            for ( int i = 0; i < vertices.Length; i++ )
            {
                Vector3 sum = sums[vertices[i]];
                smoothNormals.Add( sum.sqrMagnitude > 0f ? sum.normalized : normals[i] );
            }
            return smoothNormals;
        }

        /// <summary>
        /// Creates a new readable outline mesh from <paramref name="source"/>.
        /// Returns null if the source's vertex data cannot be read.
        /// </summary>
        public static Mesh CreateOutlineMesh( Mesh source, bool skinned, bool combineSubmeshes )
        {
            if ( !CanReadVertexData( source ) ) return null;

            Mesh mesh = Object.Instantiate( source );
            mesh.name = source.name + " (Outline)";

            if ( skinned )
            {
                mesh.SetUVs( 3, new Vector3[mesh.vertexCount] );
            }
            else
            {
                mesh.SetUVs( 3, BuildSmoothNormals( source ) );
            }

            if ( combineSubmeshes )
            {
                int[] triangles = mesh.triangles;
                mesh.subMeshCount++;
                mesh.SetTriangles( triangles, mesh.subMeshCount - 1, false );
            }

            return mesh;
        }

        // ---------------------------------------------------------------------------------
        // Runtime cache (play mode / builds only, for objects that were never baked)
        // ---------------------------------------------------------------------------------

        public static Mesh AcquireLiveMesh( Mesh source, bool skinned, bool combine )
        {
            var key = ( source, MakeFlags( skinned, combine ) );

            LiveEntry entry;
            if ( s_liveMeshes.TryGetValue( key, out entry ) && entry.mesh )
            {
                entry.users++;
                return entry.mesh;
            }

            Mesh mesh = CreateOutlineMesh( source, skinned, combine );
            if ( !mesh ) return null;

            mesh.hideFlags = HideFlags.HideAndDontSave;
            s_liveMeshes[key] = new LiveEntry { mesh = mesh, users = 1 };
            return mesh;
        }

        public static void ReleaseLiveMesh( Mesh source, bool skinned, bool combine )
        {
            var key = ( source, MakeFlags( skinned, combine ) );

            LiveEntry entry;
            if ( !s_liveMeshes.TryGetValue( key, out entry ) ) return;
            if ( --entry.users > 0 ) return;

            s_liveMeshes.Remove( key );
            if ( entry.mesh )
            {
                if ( Application.isPlaying )
                {
                    Object.Destroy( entry.mesh );
                }
                else
                {
                    Object.DestroyImmediate( entry.mesh );
                }
            }
        }

        static int MakeFlags( bool skinned, bool combine )
        {
            return ( skinned ? SkinnedFlag : 0 ) | ( combine ? CombinedFlag : 0 );
        }
    }
}
