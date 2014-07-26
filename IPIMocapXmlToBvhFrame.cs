using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MergeCapturedPoses
{
	class IPIMocapXmlToBvhFrame
	{
		public static string ROOT_TRANSLATION = "_ROOT_TRANSLATION";
		
		public static Dictionary<string, float[]> GetBoneAngleDict(string xmlPath)
		{
			var xdoc = XDocument.Load(xmlPath);
			var poseData = xdoc.Element("PoseData");
			//string strArray = (string)poseData.Element("RootTranslation").Element("Translation").Attribute("value");
			//float[] translation = Array.ConvertAll(strArray.Split(' '), float.Parse);
			float[] translation = BvhTemplate.TranslationTPose;

			Dictionary<string, float[]> boneAngleDict = new Dictionary<string, float[]>();
			foreach (var bone in poseData.Elements().Skip(1)) {
				string boneName = (string)bone.Attribute("Name");
				float[] rots = (from rot in bone.Elements()
								select float.Parse((string)rot.Attribute("angle"))).ToArray();
				boneAngleDict[boneName] = rots;
			}

			boneAngleDict[IPIMocapXmlToBvhFrame.ROOT_TRANSLATION] = translation;
			return boneAngleDict;
		}

		//public static Dictionary<string, int> GetBoneNameToIdDict()
		//{
		//    var skeleton = BvhTemplate.GetDefaultSkeleton();
		//    return skeleton.JointNameToId;
		//}

		public static float[] RemapToBvhChannel(Dictionary<string, float[]> boneAngleDict)
		{
			var skeleton = BvhTemplate.GetDefaultSkeleton();
			float[] bvhChannels = new float[skeleton.NumOfChannels];
			foreach (var joint in skeleton.JointList) {
				int id;
				if (skeleton.JointNameToId.TryGetValue(joint.Name, out id)) {
					float[] angle;
					boneAngleDict.TryGetValue(joint.Name, out angle);
					if (angle == null) {
						angle = new float[3] { 0f, 0f, 0f };
					}
					for (int i = 0; i < 3; i++) {
						bvhChannels[3 + id * 3 + i] = angle[2 - i];
					}
				}
			}

			for (int i = 0; i < 3; i++) {
				bvhChannels[i] = boneAngleDict[IPIMocapXmlToBvhFrame.ROOT_TRANSLATION][i];
			}

			return bvhChannels;
		}
	}
}
