using System.Collections.Generic;
using UnityEngine;

namespace Feat1.MeshCut {
	/// <summary>
	/// 切断処理を行うクラス
	/// </summary>
	public class MeshCut : MonoBehaviour {

		/// <summary>
		/// 対象のオブジェクトを切断するメソッド
		/// </summary>
		/// <param name="targetMesh"> 切断対象のメッシュ情報 </param>
		/// <param name="targetTransform"> 切断対象の変位情報 </param>
		/// <param name="cutter"> 切断する平面 </param>
		/// <param name="hasCutSurfaceMaterial"> <see langword="true"/> なら切断面にマテリアルを追加する </param>
		/// <returns> 平面の表と裏に切断された後のメッシュ情報 </returns>
		public static (
			Mesh frontsideMeshOfPlane,
			Mesh backsideMeshOfPlane
		) Cut(
			Mesh targetMesh,
			Transform targetTransform,
			Plane cutter,
			bool hasCutSurfaceMaterial = false
		) {
			// 切断平面が平行だと切断できないので、null を返す
			if (cutter.normal == Vector3.zero) {
				Debug.LogError("平面が平行です");

				Mesh empty = new();
				empty.vertices = new Vector3[] { };
				return (null, null);
			}

			// ローカル平面用
			Vector3 scale = targetTransform.localScale;
			Vector3 pointOnPlane = cutter.normal * cutter.distance;
			Vector3 localPlaneNormal = Vector3.Scale(scale, targetTransform.InverseTransformDirection(cutter.normal)).normalized;
			Vector3 anchor = targetTransform.transform.InverseTransformPoint(pointOnPlane);
			float localPlaneDistance = Vector3.Dot(localPlaneNormal, anchor);
			Plane localPlane = new(localPlaneNormal, localPlaneDistance);

			// 切断前オブジェクトのメッシュ情報の整理
			MeshContainer originMesh = new(targetMesh);

			// 切断後のメッシュ情報を格納
			MeshContainer frontsideMesh = new();
			MeshContainer backsideMesh = new();

			// 切断対象の頂点の切断平面との位置関係の判定用
			bool[] getsideTruth = new bool[originMesh.Vertices.Count];

			// 切断後の頂点番号配列は二つ生成されるが、切断前の各頂点がそれぞれの配列で何番目に格納されるかをあらかじめ用意する (振分情報と元頂点インデックスが対応している)
			int[] trackerArray = new int[originMesh.Vertices.Count];

			// 切断処理が行われるポリゴンが切断された後の、新規ポリゴン情報を格納する
			SubdivideDataBuffer subdivideDataBuffer = new(localPlane);


			// ==== ここからデバッグ用コードを追加 ====
			// デバッグ用にローカル平面を描画する
			// デバッグ描画が不要になったらこのブロックをコメントアウトまたは削除してください。
			GameObject debugPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
			// Colliderは不要なので削除
			Collider debugCollider = debugPlane.GetComponent<Collider>();
			if (debugCollider != null) {
				GameObject.Destroy(debugCollider);
			}

			// 描画する平面のTransformをtargetTransformのローカル空間に合わせる
			// まず、localPlaneの情報をワールド空間に戻す必要がある
			// ローカル平面の法線と距離からワールド空間の情報を再構築
			// ワールド空間での平面上の点 (localPlaneNormal * localPlaneDistance) を、targetTransformのTransformPointでワールド座標に戻す
			Vector3 worldPointOnLocalPlane = targetTransform.TransformPoint(localPlane.normal * localPlane.distance);
			// ワールド空間での法線 (localPlaneNormal) を、targetTransformのTransformDirectionでワールド座標に戻す
			Vector3 worldNormalOfLocalPlane = targetTransform.TransformDirection(localPlane.normal).normalized;

			debugPlane.transform.position = worldPointOnLocalPlane;
			debugPlane.transform.rotation = Quaternion.LookRotation(worldNormalOfLocalPlane);

			// QuadはデフォルトでXY平面に生成されるため、LookRotationでZ軸が法線方向になります。
			// Planeの法線は通常上方向（Y）を表すため、適切な向きにするには調整が必要な場合があります。
			// ここでは、Z軸が法線方向になるようにLookRotationを使用しています。

			// 親子関係を設定して、切断対象オブジェクトと一緒に移動・回転するようにする
			// debugPlane.transform.SetParent(targetTransform, true); // true: ワールド座標を維持して親子関係を設定

			// デバッグ用マテリアルの設定 (任意)
			debugPlane.GetComponent<Renderer>().material = new Material(Shader.Find("Custom/FrontBlueBackRedShader"));

			// デバッグオブジェクトの名前を設定
			debugPlane.name = "DebugCutPlane (Local)";
			// =================================

			// frontside と backside の各頂点情報配列のインデックスの判定用
			int IndexCountAssignedFrontside = 0, IndexCountAssignedBackside = 0;


			// trackerArray の初期化と、切断処理を行わないポリゴンの格納を行う
			for (int i = 0; i < originMesh.Vertices.Count; i++) {
				getsideTruth[i] = localPlane.GetSide(originMesh.Vertices[i]);

				// frontside の頂点情報配列に格納
				if (getsideTruth[i]) {
					frontsideMesh.AddVertex(
						originMesh.Vertices[i],
						originMesh.Normals[i],
						originMesh.UVs[i],
						out _
					);
					trackerArray[i] = IndexCountAssignedFrontside++;
				}
				// backside の頂点情報配列に格納
				else {
					backsideMesh.AddVertex(
						originMesh.Vertices[i],
						originMesh.Normals[i],
						originMesh.UVs[i],
						out _
					);
					trackerArray[i] = IndexCountAssignedBackside++;
				}
			}

			// オブジェクトとなりえる最小頂点構成の立体は四面体 (頂点数: 4) なので、切断後のどちらかの頂点数がそれ以下の場合は切断処理を行わない
			if (IndexCountAssignedFrontside < 4 || IndexCountAssignedBackside < 4) {
				Debug.LogError($"切断後のメッシュの頂点数が少なすぎるため, 切断処理を行いません. : {IndexCountAssignedFrontside}, {IndexCountAssignedBackside}");
				return (targetMesh, null);
			}

			// サブメッシュグループの数だけループ
			for (int submeshGroupNumber = 0; submeshGroupNumber < originMesh.SubmeshCount; submeshGroupNumber++) {
				List<int> subVertices = originMesh.Submesh[submeshGroupNumber];
				frontsideMesh.Submesh.Add(new List<int>());
				backsideMesh.Submesh.Add(new List<int>());

				// 各三角形に対して処理を行う
				for (int i = 0; i < subVertices.Count; i += 3) {
					int[] triangle = new int[] {
					subVertices[i],
					subVertices[i + 1],
					subVertices[i + 2]
				};
					bool[] triangleSideTruth = new bool[] {
					getsideTruth[triangle[0]],
					getsideTruth[triangle[1]],
					getsideTruth[triangle[2]]
				};

					// 切断平面の法線側にすべての頂点がある場合 (切断非対象)
					if (triangleSideTruth[0] && triangleSideTruth[1] && triangleSideTruth[2]) {
						// frontsideMesh にそのまま三角形を追加
						frontsideMesh.MakeTriangle(
							submeshGroupNumber,
							trackerArray[triangle[0]],
							trackerArray[triangle[1]],
							trackerArray[triangle[2]]
						);
					}
					// 切断平面の反法線側にすべての頂点がある場合 (切断非対象)
					else if (!triangleSideTruth[0] && !triangleSideTruth[1] && !triangleSideTruth[2]) {
						// backsideMesh にそのまま三角形を追加
						backsideMesh.MakeTriangle(
							submeshGroupNumber,
							trackerArray[triangle[0]],
							trackerArray[triangle[1]],
							trackerArray[triangle[2]]
						);
					}
					// 切断平面がポリゴンをまたぐ場合 (切断対象)
					else {
						Vector3 polygonNormal = originMesh.GetNormal(triangle);
						SideIndexInfo sideIndexInfo = SortIndex(triangle, triangleSideTruth);
						subdivideDataBuffer.AddData(
							submeshGroupNumber,
							polygonNormal,
							sideIndexInfo
						);
					}
				}
			}

			// 融解・蓄積した切断対象ポリゴン情報をもとに、MeshTopology.Triangles で再生成を行う
			subdivideDataBuffer.MakeAllPolygon(
				trackerArray,
				originMesh,
				frontsideMesh,
				backsideMesh,
				hasCutSurfaceMaterial
			);
			// ここで、切断面に対する処理を行う

			Mesh frontMesh = frontsideMesh.ToMesh("FrontsideMesh");
			Mesh backMesh = backsideMesh.ToMesh("BacksideMesh");

			Debug.Log("切断処理が正常終了しました");

			return (frontMesh, backMesh);
		}

		/// <summary>
		/// 切断対象の三角形の頂点インデックスを、切断平面の法線側と反法線側に分けてソートする
		/// </summary>
		/// <param name="triangle"> triangle 情報配列 (切断前メッシュ) </param>
		/// <param name="triangleSideTruth"> triangle の各頂点の GetSide() 結果の配列 </param>
		/// <returns> (SideIndexInfo) ソートされたインデックスのデータ構造 </returns>
		private static SideIndexInfo SortIndex(
			int[] triangle,
			bool[] triangleSideTruth
		) {
			if (triangleSideTruth[0]) {
				// t|t|f
				if (triangleSideTruth[1]) {
					return new SideIndexInfo(new int[] { triangle[0], triangle[1] }, new int[] { triangle[2] });
				}
				else {
					// t|f|t
					if (triangleSideTruth[2]) {
						return new SideIndexInfo(new int[] { triangle[2], triangle[0] }, new int[] { triangle[1] });
					}
					// t|f|f
					else {
						return new SideIndexInfo(new int[] { triangle[0] }, new int[] { triangle[1], triangle[2] });
					}
				}
			}
			else {
				// f|f|t
				if (!triangleSideTruth[1]) {
					return new SideIndexInfo(new int[] { triangle[2] }, new int[] { triangle[0], triangle[1] });
				}
				else {
					// f|t|t
					if (triangleSideTruth[2]) {
						return new SideIndexInfo(new int[] { triangle[1], triangle[2] }, new int[] { triangle[0] });
					}
					// f|t|f
					else {
						return new SideIndexInfo(new int[] { triangle[1] }, new int[] { triangle[2], triangle[0] });
					}
				}
			}
		}
	}
}