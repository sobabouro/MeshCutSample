using System.Collections.Generic;
using UnityEngine;

namespace Feat1.MeshCut.MeshCutModule {

    /// <summary>
    /// メッシュ情報を管理するためのクラス
    /// </summary>
    public class MeshContainer {

        /// <summary>
        /// 頂点の座標情報
        /// </summary>
        public List<Vector3> Vertices {
            get; private set;
        }

        /// <summary>
        /// 頂点の法線情報
        /// </summary>
        public List<Vector3> Normals {
            get; private set;
        }

        /// <summary>
        /// 頂点の UV 座標情報
        /// </summary>
        public List<Vector2> UVs {
            get; private set;
        }

        /// <summary>
        /// 頂点が所属するサブメッシュのインデックス番号情報
        /// </summary>
        public List<List<int>> Submesh {
            get; private set;
        }

        /// <summary>
        /// サブメッシュの数
        /// </summary>
        public int SubmeshCount {
            get {
                return Submesh.Count;
            }
        }

        /// <summary>
        /// 空のメッシュ情報を生成するコンストラクタ
        /// </summary>
        public MeshContainer() {
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            UVs = new List<Vector2>();
            Submesh = new List<List<int>>();
        }

        /// <summary>
        /// 指定されたメッシュ情報をもとに、container を生成するコンストラクタ
        /// </summary>
        public MeshContainer(Mesh originMesh) {
            Vertices = new List<Vector3>(originMesh.vertices);

			if (originMesh.normals != null && originMesh.normals.Length > 0) {
				Normals = new List<Vector3>(originMesh.normals);
			}
			else {
				Normals = new List<Vector3>();
			}

			if (originMesh.uv != null && originMesh.uv.Length > 0) {
				UVs = new List<Vector2>(originMesh.uv);
			}
			else {
				UVs = new List<Vector2>();
			}

			Submesh = new List<List<int>>();
			for (int i = 0; i < originMesh.subMeshCount; i++) {
				Submesh.Add(new List<int>(originMesh.GetIndices(i)));
			}
		}

        /// <summary>
        /// 切断対象のメッシュ情報のうち、任意の情報を新規メッシュ情報として追加するメソッド
        /// </summary>
        /// <param name="originVertices"> 切断対象の頂点の座標情報 </param>
        /// <param name="originNormals"> 切断対象の頂点の法線情報 </param>
        /// <param name="originUVs"> 切断対象の頂点の UV 座標情報 </param>
        /// <param name="submeshDepartment"> 新規メッシュのどのサブメッシュに所属させるか </param>
        /// <param name="originIndex1"> 切断対象の頂点のインデックス </param>
        /// <param name="originIndex2"> 切断対象の頂点のインデックス </param>
        /// <param name="originIndex3"> 切断対象の頂点のインデックス </param>
        public void AddMesh(
            Vector3[] originVertices,
            Vector3[] originNormals,
            Vector2[] originUVs,
            int submeshDepartment,
            int originIndex1,
            int originIndex2,
            int originIndex3
        ) {
            int indexCount = Vertices.Count;

            for (int i = 0; i < 3; i++) {
                Submesh[submeshDepartment].Add(indexCount + i);
            }
            Vertices.AddRange(new Vector3[] {
                    originVertices[originIndex1],
                    originVertices[originIndex2],
                    originVertices[originIndex3]
                });
            Normals.AddRange(new Vector3[] {
                    originNormals[originIndex1],
                    originNormals[originIndex2],
                    originNormals[originIndex3]
                });
            UVs.AddRange(new Vector2[] {
                    originUVs[originIndex1],
                    originUVs[originIndex2],
                    originUVs[originIndex3]
                });
        }

        /// <summary>
        /// 新しく生成された各情報を新規メッシュ情報として追加するメソッド
        /// </summary>
        /// <param name="submeshDepartment"> 新規メッシュのどのサブメッシュに所属させるか </param>
        /// <param name="face"> 構成されるポリゴンの法線ベクトル。これをもとに頂点の格納する順番を決定する。</param>
        /// <param name="newVertices"> 新規メッシュの頂点の座標情報 </param>
        /// <param name="newNormals"> 新規メッシュの頂点の法線情報 </param>
        /// <param name="newUVs"> 新規メッシュの頂点の UV 座標情報 </param>
        public void AddMesh(
            int submeshDepartment,
            Vector3 face,
            Vector3[] newVertices,
            Vector3[] newNormals,
            Vector2[] newUVs
        ) {
            int indexCount = Vertices.Count;
            int[] sequence = new int[] { 0, 1, 2 };

            Vector3 calNormal = Vector3.Cross(
                newVertices[1] - newVertices[0],
                newVertices[2] - newVertices[0]
            ).normalized;

            if (Vector3.Dot(calNormal, face) < 0) {
                sequence[0] = 2;
                sequence[1] = 1;
                sequence[2] = 0;
            }

            for (int i = 0; i < 3; i++) {
                Submesh[submeshDepartment].Add(indexCount + i);
            }
            Vertices.AddRange(new Vector3[] {
                    newVertices[sequence[0]],
                    newVertices[sequence[1]],
                    newVertices[sequence[2]]
                });
            Normals.AddRange(new Vector3[] {
                    newNormals[sequence[0]],
                    newNormals[sequence[1]],
                    newNormals[sequence[2]]
                });
            UVs.AddRange(new Vector2[] {
                    newUVs[sequence[0]],
                    newUVs[sequence[1]],
                    newUVs[sequence[2]]
                });
        }

        /// <summary>
        /// 新規メッシュの頂点の各情報を追加するメソッド
        /// <param name="vertex"> 新規メッシュの頂点の座標情報 </param>
        /// <param name="normal"> 新規メッシュの頂点の法線情報 </param>
        /// <param name="uv"> 新規メッシュの頂点の UV 座標情報 </param>
        public void AddVertex(
            Vector3 vertex,
            Vector3 normal,
            Vector2 uv,
            out int index
        ) {
            Vertices.Add(vertex);
            Normals.Add(normal);
            UVs.Add(uv);
            index = Vertices.Count - 1;
        }

        /// <summary>
        /// 新規メッシュの頂点番号をもとに、法線ベクトルを計算するメソッド
        /// </summary>
        /// <param name="triangle"> ポリゴンのトライアングル情報 </param>
        /// <returns> ポリゴンの法線 (正規化) </returns>
        public Vector3 GetNormal(int[] triangle) {
            return Vector3.Cross(
                Vertices[triangle[1]] - Vertices[triangle[0]],
                Vertices[triangle[2]] - Vertices[triangle[0]]
            ).normalized;
        }

        /// <summary>
        /// 新規メッシュの頂点番号をもとに、トライアングルとサブメッシュ情報を追加するメソッド
        /// <param name="submeshDepartment"> 新規メッシュのどのサブメッシュに所属させるか </param>
        /// <param name="thisIndex1"> 新規メッシュの頂点のインデックス </param>
        /// <param name="thisIndex2"> 新規メッシュの頂点のインデックス </param>
        /// <param name="thisIndex3"> 新規メッシュの頂点のインデックス </param>
        public void MakeTriangle(
            int submeshDepartment,
            int thisIndex1,
            int thisIndex2,
            int thisIndex3
        ) {
            Submesh[submeshDepartment].AddRange(new int[] {
                    thisIndex1,
                    thisIndex2,
                    thisIndex3
                });
        }

        /// <summary>
        /// Mesh オブジェクトを生成するメソッド
        /// </summary>
        public Mesh ToMesh(string name) {
            Mesh mesh = new Mesh {
                name = name
            };
            mesh.SetVertices(Vertices.ToArray());
            if (Normals.Count == Vertices.Count)
                mesh.SetNormals(Normals.ToArray());
            if (UVs.Count == Vertices.Count)
                mesh.SetUVs(0, UVs.ToArray());

            mesh.subMeshCount = Submesh.Count;
            for (int i = 0; i < Submesh.Count; i++) {
                mesh.SetIndices(Submesh[i].ToArray(), MeshTopology.Triangles, i, false);
            }
            mesh.RecalculateBounds();
            //DisplayMeshInfo();
            return mesh;
        }

        private void DisplayMeshInfo() {
            Debug.Log($"MeshContainer: ^^^^^^^^ DisplayMeshInfo() ^^^^^^^^");
            Debug.Log($"MeshContainer: Vertices count: {Vertices.Count}");
            Debug.Log($"----------------");
            for (int i = 0; i < Vertices.Count; i++) {
                Debug.Log($"MeshContainer: Vertex {i}: {Vertices[i]}");
            }
            Debug.Log($"----------------");
            //Debug.Log($"MeshContainer: Normals count: {Normals.Count}");
            //Debug.Log($"----------------");
            //for (int i = 0; i < Normals.Count; i++) {
            //    Debug.Log($"MeshContainer: Normal {i}: {Normals[i]}");
            //}
            //Debug.Log($"----------------");
            //Debug.Log($"MeshContainer: UVs count: {UVs.Count}");
            //Debug.Log($"----------------");
            //for (int i = 0; i < UVs.Count; i++) {
            //    Debug.Log($"MeshContainer: UV {i}: {UVs[i]}");
            //}
            //Debug.Log($"----------------");
            Debug.Log($"MeshContainer: Submesh count: {Submesh.Count}");
            for (int i = 0; i < Submesh.Count; i++) {
                Debug.Log($"----------------");
                Debug.Log($"MeshContainer: Submesh {i} indices count: {Submesh[i].Count}");
                Debug.Log($"MeshContainer: Submesh {i} indices: {string.Join(", ", Submesh[i].ToArray())}");
            }
            Debug.Log($"----------------");
            Debug.Log($"MeshContainer: ^^^^^^^^ end ^^^^^^^^");
        }
    }
}
