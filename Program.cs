using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MergeCapturedPoses
{
	class Program
	{
		static void Main(string[] args)
		{
			string root = @"E:\KinectCapture\RawXML";
			string[] dirNames = Directory.GetDirectories(root);
			foreach (var d in dirNames) {
				string[] fileNames = Directory.GetFiles(d, "*.pose.xml");
				var writer = new System.IO.StreamWriter(Path.Combine(d, "output.bvh"));

				writer.Write(BvhTemplate.GetDefaultModel(0.5f, fileNames.Length + 1));
				foreach (var f in fileNames) {
					Dictionary<string, float[]> boneAngleDict = IPIMocapXmlToBvhFrame.GetBoneAngleDict(f);
					var t = IPIMocapXmlToBvhFrame.RemapToBvhChannel(boneAngleDict);
					foreach (var i in t) {
						// prevents scientific notations
						var str = String.Format("{0:F6}", i);
						writer.Write("{0} ", str.TrimEnd("0".ToCharArray()));
					}
					writer.WriteLine();
				}
				writer.Close();
			}
			
			//Console.Read();
		}
	}
}
