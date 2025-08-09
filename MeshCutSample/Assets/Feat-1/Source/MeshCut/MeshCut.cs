using System.Collections.Generic;
using UnityEngine;

namespace Feat1.MeshCut {
	/// <summary>
	/// �ؒf�������s���N���X
	/// </summary>
	public class MeshCut : MonoBehaviour {

		/// <summary>
		/// �Ώۂ̃I�u�W�F�N�g��ؒf���郁�\�b�h
		/// </summary>
		/// <param name="targetMesh"> �ؒf�Ώۂ̃��b�V����� </param>
		/// <param name="targetTransform"> �ؒf�Ώۂ̕ψʏ�� </param>
		/// <param name="cutter"> �ؒf���镽�� </param>
		/// <param name="hasCutSurfaceMaterial"> <see langword="true"/> �Ȃ�ؒf�ʂɃ}�e���A����ǉ����� </param>
		/// <returns> ���ʂ̕\�Ɨ��ɐؒf���ꂽ��̃��b�V����� </returns>
		public static (
			Mesh frontsideMeshOfPlane,
			Mesh backsideMeshOfPlane
		) Cut(
			Mesh targetMesh,
			Transform targetTransform,
			Plane cutter,
			bool hasCutSurfaceMaterial = false
		) {
			// �ؒf���ʂ����s���Ɛؒf�ł��Ȃ��̂ŁAnull ��Ԃ�
			if (cutter.normal == Vector3.zero) {
				Debug.LogError("���ʂ����s�ł�");

				Mesh empty = new();
				empty.vertices = new Vector3[] { };
				return (null, null);
			}

			// ���[�J�����ʗp
			Vector3 scale = targetTransform.localScale;
			Vector3 pointOnPlane = cutter.normal * cutter.distance;
			Vector3 localPlaneNormal = Vector3.Scale(scale, targetTransform.InverseTransformDirection(cutter.normal)).normalized;
			Vector3 anchor = targetTransform.transform.InverseTransformPoint(pointOnPlane);
			float localPlaneDistance = Vector3.Dot(localPlaneNormal, anchor);
			Plane localPlane = new(localPlaneNormal, localPlaneDistance);

			// �ؒf�O�I�u�W�F�N�g�̃��b�V�����̐���
			MeshContainer originMesh = new(targetMesh);

			// �ؒf��̃��b�V�������i�[
			MeshContainer frontsideMesh = new();
			MeshContainer backsideMesh = new();

			// �ؒf�Ώۂ̒��_�̐ؒf���ʂƂ̈ʒu�֌W�̔���p
			bool[] getsideTruth = new bool[originMesh.Vertices.Count];

			// �ؒf��̒��_�ԍ��z��͓��������邪�A�ؒf�O�̊e���_�����ꂼ��̔z��ŉ��ԖڂɊi�[����邩�����炩���ߗp�ӂ��� (�U�����ƌ����_�C���f�b�N�X���Ή����Ă���)
			int[] trackerArray = new int[originMesh.Vertices.Count];

			// �ؒf�������s����|���S�����ؒf���ꂽ��́A�V�K�|���S�������i�[����
			SubdivideDataBuffer subdivideDataBuffer = new(localPlane);


			// ==== ��������f�o�b�O�p�R�[�h��ǉ� ====
			// �f�o�b�O�p�Ƀ��[�J�����ʂ�`�悷��
			// �f�o�b�O�`�悪�s�v�ɂȂ����炱�̃u���b�N���R�����g�A�E�g�܂��͍폜���Ă��������B
			GameObject debugPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
			// Collider�͕s�v�Ȃ̂ō폜
			Collider debugCollider = debugPlane.GetComponent<Collider>();
			if (debugCollider != null) {
				GameObject.Destroy(debugCollider);
			}

			// �`�悷�镽�ʂ�Transform��targetTransform�̃��[�J����Ԃɍ��킹��
			// �܂��AlocalPlane�̏������[���h��Ԃɖ߂��K�v������
			// ���[�J�����ʂ̖@���Ƌ������烏�[���h��Ԃ̏����č\�z
			// ���[���h��Ԃł̕��ʏ�̓_ (localPlaneNormal * localPlaneDistance) ���AtargetTransform��TransformPoint�Ń��[���h���W�ɖ߂�
			Vector3 worldPointOnLocalPlane = targetTransform.TransformPoint(localPlane.normal * localPlane.distance);
			// ���[���h��Ԃł̖@�� (localPlaneNormal) ���AtargetTransform��TransformDirection�Ń��[���h���W�ɖ߂�
			Vector3 worldNormalOfLocalPlane = targetTransform.TransformDirection(localPlane.normal).normalized;

			debugPlane.transform.position = worldPointOnLocalPlane;
			debugPlane.transform.rotation = Quaternion.LookRotation(worldNormalOfLocalPlane);

			// Quad�̓f�t�H���g��XY���ʂɐ�������邽�߁ALookRotation��Z�����@�������ɂȂ�܂��B
			// Plane�̖@���͒ʏ������iY�j��\�����߁A�K�؂Ȍ����ɂ���ɂ͒������K�v�ȏꍇ������܂��B
			// �����ł́AZ�����@�������ɂȂ�悤��LookRotation���g�p���Ă��܂��B

			// �e�q�֌W��ݒ肵�āA�ؒf�ΏۃI�u�W�F�N�g�ƈꏏ�Ɉړ��E��]����悤�ɂ���
			// debugPlane.transform.SetParent(targetTransform, true); // true: ���[���h���W���ێ����Đe�q�֌W��ݒ�

			// �f�o�b�O�p�}�e���A���̐ݒ� (�C��)
			debugPlane.GetComponent<Renderer>().material = new Material(Shader.Find("Custom/FrontBlueBackRedShader"));

			// �f�o�b�O�I�u�W�F�N�g�̖��O��ݒ�
			debugPlane.name = "DebugCutPlane (Local)";
			// =================================

			// frontside �� backside �̊e���_���z��̃C���f�b�N�X�̔���p
			int IndexCountAssignedFrontside = 0, IndexCountAssignedBackside = 0;


			// trackerArray �̏������ƁA�ؒf�������s��Ȃ��|���S���̊i�[���s��
			for (int i = 0; i < originMesh.Vertices.Count; i++) {
				getsideTruth[i] = localPlane.GetSide(originMesh.Vertices[i]);

				// frontside �̒��_���z��Ɋi�[
				if (getsideTruth[i]) {
					frontsideMesh.AddVertex(
						originMesh.Vertices[i],
						originMesh.Normals[i],
						originMesh.UVs[i],
						out _
					);
					trackerArray[i] = IndexCountAssignedFrontside++;
				}
				// backside �̒��_���z��Ɋi�[
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

			// �I�u�W�F�N�g�ƂȂ肦��ŏ����_�\���̗��͎̂l�ʑ� (���_��: 4) �Ȃ̂ŁA�ؒf��̂ǂ��炩�̒��_��������ȉ��̏ꍇ�͐ؒf�������s��Ȃ�
			if (IndexCountAssignedFrontside < 4 || IndexCountAssignedBackside < 4) {
				Debug.LogError($"�ؒf��̃��b�V���̒��_�������Ȃ����邽��, �ؒf�������s���܂���. : {IndexCountAssignedFrontside}, {IndexCountAssignedBackside}");
				return (targetMesh, null);
			}

			// �T�u���b�V���O���[�v�̐��������[�v
			for (int submeshGroupNumber = 0; submeshGroupNumber < originMesh.SubmeshCount; submeshGroupNumber++) {
				List<int> subVertices = originMesh.Submesh[submeshGroupNumber];
				frontsideMesh.Submesh.Add(new List<int>());
				backsideMesh.Submesh.Add(new List<int>());

				// �e�O�p�`�ɑ΂��ď������s��
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

					// �ؒf���ʂ̖@�����ɂ��ׂĂ̒��_������ꍇ (�ؒf��Ώ�)
					if (triangleSideTruth[0] && triangleSideTruth[1] && triangleSideTruth[2]) {
						// frontsideMesh �ɂ��̂܂܎O�p�`��ǉ�
						frontsideMesh.MakeTriangle(
							submeshGroupNumber,
							trackerArray[triangle[0]],
							trackerArray[triangle[1]],
							trackerArray[triangle[2]]
						);
					}
					// �ؒf���ʂ̔��@�����ɂ��ׂĂ̒��_������ꍇ (�ؒf��Ώ�)
					else if (!triangleSideTruth[0] && !triangleSideTruth[1] && !triangleSideTruth[2]) {
						// backsideMesh �ɂ��̂܂܎O�p�`��ǉ�
						backsideMesh.MakeTriangle(
							submeshGroupNumber,
							trackerArray[triangle[0]],
							trackerArray[triangle[1]],
							trackerArray[triangle[2]]
						);
					}
					// �ؒf���ʂ��|���S�����܂����ꍇ (�ؒf�Ώ�)
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

			// �Z���E�~�ς����ؒf�Ώۃ|���S���������ƂɁAMeshTopology.Triangles �ōĐ������s��
			subdivideDataBuffer.MakeAllPolygon(
				trackerArray,
				originMesh,
				frontsideMesh,
				backsideMesh,
				hasCutSurfaceMaterial
			);
			// �����ŁA�ؒf�ʂɑ΂��鏈�����s��

			Mesh frontMesh = frontsideMesh.ToMesh("FrontsideMesh");
			Mesh backMesh = backsideMesh.ToMesh("BacksideMesh");

			Debug.Log("�ؒf����������I�����܂���");

			return (frontMesh, backMesh);
		}

		/// <summary>
		/// �ؒf�Ώۂ̎O�p�`�̒��_�C���f�b�N�X���A�ؒf���ʂ̖@�����Ɣ��@�����ɕ����ă\�[�g����
		/// </summary>
		/// <param name="triangle"> triangle ���z�� (�ؒf�O���b�V��) </param>
		/// <param name="triangleSideTruth"> triangle �̊e���_�� GetSide() ���ʂ̔z�� </param>
		/// <returns> (SideIndexInfo) �\�[�g���ꂽ�C���f�b�N�X�̃f�[�^�\�� </returns>
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