using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MergeCapturedPoses
{
	public enum BvhChannels
	{
		Xposition,
		Yposition,
		Zposition,
		Xrotation,
		Yrotation,
		Zrotation,
	}

	/// <summary>
	/// Represents BVH Joint.
	/// </summary>
	public class BvhJoint
	{
		public BvhJoint(Vector3 offset = default(Vector3), bool root = false)
		{
			this.Offset = offset;
			this.IsRoot = root;
			this.Channels = new List<BvhChannels>();
			this.Children = new List<BvhJoint>();
		}

		public Vector3 Offset { get; set; }
		public string Name { get; set; }
		public bool IsRoot { get; set; }

		public List<BvhChannels> Channels
		{
			get;
			set;
		}

		public List<BvhJoint> Children
		{
			get;
			set;
		}

		public bool IsEndSite
		{
			get
			{
				return this.Children.Count == 0;
			}
		}
	}

	public class BvhSkeleton
	{
		public int NumOfChannels { get; set; }
		private Stack<Matrix> matrixStack = new Stack<Matrix>();

		public BvhSkeleton()
		{
			this.NumOfChannels = 0;
			this.Poses = new Dictionary<BvhJoint, Matrix>();
			this.JointList = new List<BvhJoint>();
			this.JointNameToId = new Dictionary<string, int>();
		}

		public Dictionary<BvhJoint, Matrix> Poses
		{
			get;
			set;
		}

		public Dictionary<string, int> JointNameToId
		{
			get;
			set;
		}

		public List<BvhJoint> JointList
		{
			get;
			set;
		}

		public void ApplyMotion(IEnumerable<float> motion)
		{
			matrixStack.Push(Matrix.Identity);
			ApplyMotionRecursively(this.Root, motion.GetEnumerator());
			matrixStack.Pop();
			System.Diagnostics.Debug.Assert(matrixStack.Count == 0, "matrixStack operations not balanced");
		}


		/* 
		 * v' = v * R * T
		 * 
		 * ====================================
		 * THE STRUCTURE OF MATRIX STACK:
		 * 
		 * [Top of matrixStack]
		 * Mn = T_n * O_n * R_(n-1) * M_(n-1)
		 * ...
		 * M2 = T2 * O2 * R1 * M1
		 * M1 = T1 * O1 * R0 * M0
		 * M0 = T0 * O0 * I * W
		 * [Buttom of matrixStack]
		 * ====================================
		 * 
		 * subjects to:
		 * 
		 *   Skeleton.Poses[Joint_i] = M_i
		 *   Joint_0 == Skeleton.Root
		 *   
		 *   Position of Joint_i in world space = 0 * M_i == M_i.Translation
		 *   Position of Bone_i (from Joint_(i-1) to Joint_i):
		 *     start = 0 * M_(i-1)
		 *     end = O_i * M_i
		 */

		private void ApplyMotionRecursively(BvhJoint joint, IEnumerator<float> motion)
		{
			// O_i * R_(i-1) * M_(i-1)
			Matrix parentMatrix = matrixStack.Peek();			

			// TODO: Possible optimizations:
			// 1) Use Matrix.CreateFromYawPitchRoll for Z-X-Y rotation convention;
			// 2) Quaternions.
			Vector3 translation = Vector3.Zero;
			Matrix rotation;
			Quaternion rot = Quaternion.Identity;

			foreach (var ch in joint.Channels) {
				motion.MoveNext();
				float v = motion.Current;
				
				switch (ch) {
				case BvhChannels.Xposition:
					translation.X = v;
					break;
				case BvhChannels.Yposition:
					translation.Y = v;
					break;
				case BvhChannels.Zposition:
					translation.Z = v;
					break;
				case BvhChannels.Xrotation:
					// rotation = Matrix.CreateRotationX(MathHelper.ToRadians(v)) * rotation;
					rot = rot * Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(v));
					break;
				case BvhChannels.Yrotation:
					// rotation = Matrix.CreateRotationY(MathHelper.ToRadians(v)) * rotation;
					rot = rot * Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(v));
					break;
				case BvhChannels.Zrotation:
					// rotation = Matrix.CreateRotationZ(MathHelper.ToRadians(v)) * rotation;
					rot = rot * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(v));
					break;
				default:
					throw new Exception("An unknown error occured.");
				}
			}
			Matrix.CreateFromQuaternion(ref rot, out rotation);

			// M_i = T_i * O_i * R_(i-1) * M_(i-1)
			this.Poses[joint] = Matrix.CreateTranslation(translation) * parentMatrix;

			// R_i * M_i
			rotation = rotation * this.Poses[joint];	// todo

			foreach (BvhJoint j in joint.Children) {
				// O_(i+1) * R_i * M_i
				matrixStack.Push(Matrix.CreateTranslation(j.Offset) * rotation);
				ApplyMotionRecursively(j, motion);
				matrixStack.Pop();
			}
			
		}

		public void Reset()
		{
			float[] originalPose = new float[this.NumOfChannels];
			ApplyMotion(originalPose);
		}

		public BvhJoint Root
		{
			get;
			set;
		}
	}
}
