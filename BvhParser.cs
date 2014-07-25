using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MergeCapturedPoses
{

	/// <summary>
	/// Parses BioVision Hierarchical (BVH) files.
	/// </summary>
	public class BvhParser
	{
		private int lineNo;
		private Queue<string> tokenQueue;
		private Stack<BvhJoint> jointStack;
		private TextReader reader;
		private BvhSkeleton skeleton;

		/// <summary>
		/// Frame time
		/// </summary>
		public float Delta { get; protected set; }

		public int NumOfChannels { get; protected set; }
		public int NumOfFrames { get; protected set; }

		#region Helper functions

		private BvhChannels GetChannel(string chnl)
		{
			switch (chnl) {
			case "Xposition":
				return BvhChannels.Xposition;
			case "Yposition":
				return BvhChannels.Yposition;
			case "Zposition":
				return BvhChannels.Zposition;
			case "Xrotation":
				return BvhChannels.Xrotation;
			case "Yrotation":
				return BvhChannels.Yrotation;
			case "Zrotation":
				return BvhChannels.Zrotation;
			default:
				throw new InvalidContentException(ErrorString("[XYZ](position|rotation)", chnl));
			}
		}

		private string ErrorString(string expectedToken, string actualToken)
		{
			return string.Format("Syntax error in line {0}: '{1}' expected, got '{2}' instead.",
				this.lineNo, expectedToken, actualToken);
		}

		#endregion

		public BvhParser(TextReader reader)
		{
			if (reader == null) {
				throw new ArgumentException("reader");
			}
			this.reader = reader;
			this.tokenQueue = new Queue<string>();
			this.jointStack = new Stack<BvhJoint>();
			this.lineNo = 0;
			this.Delta = 0f;
			this.NumOfChannels = 0;
			this.NumOfFrames = 0;
		}

		public virtual IEnumerable<float>[] Parse(BvhSkeleton skeleton)
		{
			this.skeleton = skeleton;
			ParseHierarchy();
			return ParseMotion();
		}

		protected virtual void ParseHierarchy()
		{
			string tok;
			tok = GetToken();
			if (tok != "HIERARCHY") {
				throw new InvalidContentException(ErrorString("HIERARCHY", tok));
			}

			tok = GetToken();
			if (tok != "ROOT") {
				throw new InvalidContentException(ErrorString("ROOT", tok));
			}

			BvhJoint root = new BvhJoint(default(Vector3), true);
			this.skeleton.Root = root;
			this.skeleton.JointList.Add(root);
			this.jointStack.Push(this.skeleton.Root);
			ParseJoint();
			this.skeleton.NumOfChannels = this.NumOfChannels;

			for (int i = 0; i < this.skeleton.JointList.Count; i++) {
				this.skeleton.JointNameToId[this.skeleton.JointList[i].Name] = i;
			}
		}

		protected virtual void ParseJoint()
		{
			string name = GetToken();
			jointStack.Peek().Name = name;

			string tok;
			tok = GetToken();
			if (tok != "{") {
				throw new InvalidContentException(ErrorString("{", tok));
			}
			while (true) {
				tok = GetToken();
				BvhJoint joint;
				switch (tok) {
				case "OFFSET":
					float x = FloatToken(), y = FloatToken(), z = FloatToken();
					this.jointStack.Peek().Offset = new Vector3(x, y, z);
					break;
				case "CHANNELS":
					int n = IntToken();
					this.NumOfChannels += n;
					for (int i = 0; i < n; i++) {
						tok = GetToken();
						this.jointStack.Peek().Channels.Add(GetChannel(tok));
					}
					break;
				case "JOINT":
					joint = new BvhJoint();
					this.skeleton.JointList.Add(joint);
					this.jointStack.Peek().Children.Add(joint);
					this.jointStack.Push(joint);
					ParseJoint();  // parses joint recursively
					break;
				case "End":
					joint = new BvhJoint();
					this.jointStack.Peek().Children.Add(joint);
					this.jointStack.Push(joint);
					ParseJoint();
					break;
				case "}":
					var top = this.jointStack.Peek();
					if (top.IsEndSite) {
						// if the name of joint here is "End"
						top.Name = "End Site";
					}
					this.jointStack.Pop();
					goto EXIT_LOOP;
				default:
					throw new InvalidContentException(
						string.Format("Syntax error in line {0}: Unknown word '{1}'.", this.lineNo, tok));
				}
			}
		EXIT_LOOP: ;
		}

		protected virtual IEnumerable<float>[] ParseMotion()
		{
			string tok;
			try {
				tok = GetToken();
			} catch (EndOfStreamException) {
				System.Diagnostics.Debug.WriteLine("No motion data.");
				return null;
			}
			if (tok != "MOTION") {
				throw new InvalidContentException(ErrorString("MOTION", tok));
			}

			// Reads the number of frames
			tok = GetToken();
			if (tok != "Frames:") {
				throw new InvalidContentException(ErrorString("Frames:", tok));
			}
			this.NumOfFrames = IntToken();

			// Reads the frame time
			tok = GetToken();
			if (tok != "Frame" || (tok = GetToken()) != "Time:") {
				throw new InvalidContentException(ErrorString("Frame Time:", tok));
			}
			this.Delta = FloatToken();

			// Reads the channel values
			IEnumerable<float>[] motions = new IEnumerable<float>[this.NumOfFrames];
			for (int i = 0; i < this.NumOfFrames; i++) {
				string[] tokens = this.ReadLine().Split(null as char[],  // default whitespaces
					StringSplitOptions.RemoveEmptyEntries);

				if (tokens.Length != this.NumOfChannels) {
					throw new InvalidContentException(
						string.Format("Syntax error in line {0}: {1} float numbers expected, got {2} instead.",
							this.lineNo, this.NumOfChannels, tokens.Length));
				}
				motions[i] = from channel in tokens select float.Parse(channel);
			}
			return motions;
		}

		private string GetToken()
		{
			if (this.tokenQueue.Count == 0) {
				CreateTokens(ReadLine());
				return GetToken();
			} else {
				return this.tokenQueue.Dequeue();
			}
		}

		private int IntToken()
		{
			string tok = GetToken();
			try {
				return int.Parse(tok);
			} catch (Exception) {
				throw new InvalidContentException(ErrorString("Integer", tok));
			}
		}

		private float FloatToken()
		{
			string tok = GetToken();
			try {
				// TODO: Should it be ``float.Parse(GetToken(), NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat);''?
				return float.Parse(tok);
			} catch (Exception) {
				throw new InvalidContentException(ErrorString("Float", tok));
			}
		}

		private void CreateTokens(string s)
		{
			this.tokenQueue = new Queue<string>(
				s.Split(null as char[], StringSplitOptions.RemoveEmptyEntries));
		}

		private string ReadLine()
		{
			this.tokenQueue.Clear();
			string line = this.reader.ReadLine();
			this.lineNo += 1;

			// StreamReader.ReadLine doesn't throw EndOfStreamException on the end of stream. 
			// Instead it returns null.
			if (line == null) {
				throw new EndOfStreamException();
			}
			return line;
		}
	}

	public class InvalidContentException : Exception
	{
		public InvalidContentException() { }
		public InvalidContentException(string message) : base(message) { }

	}
}
