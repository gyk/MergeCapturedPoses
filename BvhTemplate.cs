using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace MergeCapturedPoses
{
	class BvhTemplate
	{
		#region String representing DefaultModelHierarchy
		public static readonly string DefaultModelHierarchyPlain = @"HIERARCHY
ROOT Hip
{
	OFFSET 0.000000 0.000000 0.000000
	CHANNELS 6 Xposition Yposition Zposition Zrotation Yrotation Xrotation
	JOINT LowerSpine
	{
		OFFSET 0.000 7.342 -4.513
		CHANNELS 3 Zrotation Yrotation Xrotation
		JOINT MiddleSpine
		{
			OFFSET 0.000 9.552 0.000
			CHANNELS 3 Zrotation Yrotation Xrotation
			JOINT Chest
			{
				OFFSET 0.000 13.460 0.000
				CHANNELS 3 Zrotation Yrotation Xrotation
				JOINT Neck
				{
					OFFSET 0.000 22.577 0.527
					CHANNELS 3 Zrotation Yrotation Xrotation
					JOINT Head
					{
						OFFSET 0.000 7.815 0.000
						CHANNELS 3 Zrotation Yrotation Xrotation
						End Site
						{
							OFFSET 0.000 17.367 0.000
						}
					}
				}
				JOINT RClavicle
				{
					OFFSET -2.779 20.840 0.000
					CHANNELS 3 Zrotation Yrotation Xrotation
					JOINT RShoulder
					{
						OFFSET -13.720 -3.473 0.869
						CHANNELS 3 Zrotation Yrotation Xrotation
						JOINT RForearm
						{
							OFFSET -27.472 -1.996 -1.202
							CHANNELS 3 Zrotation Yrotation Xrotation
							JOINT RHand
							{
								OFFSET -23.202 0.696 0.857
								CHANNELS 3 Zrotation Yrotation Xrotation
								End Site
								{
									OFFSET -13.904 -0.452 0.513
								}
							}
						}
					}
				}
				JOINT LClavicle
				{
					OFFSET 2.779 20.840 0.000
					CHANNELS 3 Zrotation Yrotation Xrotation
					JOINT LShoulder
					{
						OFFSET 13.720 -3.473 0.869
						CHANNELS 3 Zrotation Yrotation Xrotation
						JOINT LForearm
						{
							OFFSET 27.472 -1.996 -1.202
							CHANNELS 3 Zrotation Yrotation Xrotation
							JOINT LHand
							{
								OFFSET 23.202 0.696 0.857
								CHANNELS 3 Zrotation Yrotation Xrotation
								End Site
								{
									OFFSET 13.904 -0.452 0.513
								}
							}
						}
					}
				}
			}
		}
	}
	JOINT RThigh
	{
		OFFSET -7.815 0.394 0.000
		CHANNELS 3 Zrotation Yrotation Xrotation
		JOINT RShin
		{
			OFFSET -0.434 -37.947 0.000
			CHANNELS 3 Zrotation Yrotation Xrotation
			JOINT RFoot
			{
				OFFSET 0.000 -39.349 -0.356
				CHANNELS 3 Zrotation Yrotation Xrotation
				JOINT RToe
				{
					OFFSET 0.000 -3.461 10.729
					CHANNELS 3 Zrotation Yrotation Xrotation
					End Site
					{
						OFFSET 0.000 0.000 5.959
					}
				}
			}
		}
	}
	JOINT LThigh
	{
		OFFSET 7.815 0.394 0.000
		CHANNELS 3 Zrotation Yrotation Xrotation
		JOINT LShin
		{
			OFFSET 0.434 -37.947 0.000
			CHANNELS 3 Zrotation Yrotation Xrotation
			JOINT LFoot
			{
				OFFSET 0.000 -39.349 -0.356
				CHANNELS 3 Zrotation Yrotation Xrotation
				JOINT LToe
				{
					OFFSET 0.000 -3.461 10.729
					CHANNELS 3 Zrotation Yrotation Xrotation
					End Site
					{
						OFFSET 0.000 0.000 5.959
					}
				}
			}
		}
	}
}";

		// Similar to DefaultModelHierarchyPlain, but with dummy nodes at branching joints.
		public static readonly string DefaultModelHierarchyDummy = @"HIERARCHY
ROOT Hip
{
	OFFSET 0.000000 0.000000 0.000000
	CHANNELS 6 Xposition Yposition Zposition Zrotation Yrotation Xrotation
	JOINT LowerSpineDummy
	{
		OFFSET 0 0 0
		CHANNELS 3 Zrotation Yrotation Xrotation
		JOINT LowerSpine
		{
			OFFSET 0.000 7.342 -4.513
			CHANNELS 3 Zrotation Yrotation Xrotation
			JOINT MiddleSpine
			{
				OFFSET 0.000 9.552 0.000
				CHANNELS 3 Zrotation Yrotation Xrotation
				JOINT Chest
				{
					OFFSET 0.000 13.460 0.000
					CHANNELS 3 Zrotation Yrotation Xrotation
					JOINT NeckDummy
					{
						OFFSET 0 0 0
						CHANNELS 3 Zrotation Yrotation Xrotation
						JOINT Neck
						{
							OFFSET 0.000 22.577 0.527
							CHANNELS 3 Zrotation Yrotation Xrotation
							JOINT Head
							{
								OFFSET 0.000 7.815 0.000
								CHANNELS 3 Zrotation Yrotation Xrotation
								End Site
								{
									OFFSET 0.000 17.367 0.000
								}
							}
						}
					}

					JOINT RClavicleDummy
					{
						OFFSET 0 0 0
						CHANNELS 3 Zrotation Yrotation Xrotation
						JOINT RClavicle
						{
							OFFSET -2.779 20.840 0.000
							CHANNELS 3 Zrotation Yrotation Xrotation
							JOINT RShoulder
							{
								OFFSET -13.720 -3.473 0.869
								CHANNELS 3 Zrotation Yrotation Xrotation
								JOINT RForearm
								{
									OFFSET -27.472 -1.996 -1.202
									CHANNELS 3 Zrotation Yrotation Xrotation
									JOINT RHand
									{
										OFFSET -23.202 0.696 0.857
										CHANNELS 3 Zrotation Yrotation Xrotation
										End Site
										{
											OFFSET -13.904 -0.452 0.513
										}
									}
								}
							}
						}
					}

					JOINT LClavicleDummy
					{
						OFFSET 0 0 0
						CHANNELS 3 Zrotation Yrotation Xrotation
						JOINT LClavicle
						{
							OFFSET 2.779 20.840 0.000
							CHANNELS 3 Zrotation Yrotation Xrotation
							JOINT LShoulder
							{
								OFFSET 13.720 -3.473 0.869
								CHANNELS 3 Zrotation Yrotation Xrotation
								JOINT LForearm
								{
									OFFSET 27.472 -1.996 -1.202
									CHANNELS 3 Zrotation Yrotation Xrotation
									JOINT LHand
									{
										OFFSET 23.202 0.696 0.857
										CHANNELS 3 Zrotation Yrotation Xrotation
										End Site
										{
											OFFSET 13.904 -0.452 0.513
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	JOINT RThighDummy
	{
		OFFSET 0 0 0
		CHANNELS 3 Zrotation Yrotation Xrotation
		JOINT RThigh
		{
			OFFSET -7.815 0.394 0.000
			CHANNELS 3 Zrotation Yrotation Xrotation
			JOINT RShin
			{
				OFFSET -0.434 -37.947 0.000
				CHANNELS 3 Zrotation Yrotation Xrotation
				JOINT RFoot
				{
					OFFSET 0.000 -39.349 -0.356
					CHANNELS 3 Zrotation Yrotation Xrotation
					JOINT RToe
					{
						OFFSET 0.000 -3.461 10.729
						CHANNELS 3 Zrotation Yrotation Xrotation
						End Site
						{
							OFFSET 0.000 0.000 5.959
						}
					}
				}
			}
		}
	}

	JOINT LThighDummy
	{
		OFFSET 0 0 0
		CHANNELS 3 Zrotation Yrotation Xrotation
		JOINT LThigh
		{
			OFFSET 7.815 0.394 0.000
			CHANNELS 3 Zrotation Yrotation Xrotation
			JOINT LShin
			{
				OFFSET 0.434 -37.947 0.000
				CHANNELS 3 Zrotation Yrotation Xrotation
				JOINT LFoot
				{
					OFFSET 0.000 -39.349 -0.356
					CHANNELS 3 Zrotation Yrotation Xrotation
					JOINT LToe
					{
						OFFSET 0.000 -3.461 10.729
						CHANNELS 3 Zrotation Yrotation Xrotation
						End Site
						{
							OFFSET 0.000 0.000 5.959
						}
					}
				}
			}
		}
	}
}";
		public static readonly string DefaultModelHierarchy = DefaultModelHierarchyPlain;
		#endregion

		// For retargetting in MotionBuilder, the skeleton should be located in correct position.
		public static readonly float[] TranslationTPose = { 0f, 83f, 0f };
		private static string defaultModelMotionHeader = @"
MOTION
Frames: {0:D}
Frame Time: {1:F7}
";
		private static int? nChannels = null;
		private static BvhSkeleton bvhSkeleton = null;

		public static string GetDefaultModelWithoutAngles(float delta, int nFrames)
		{
			string model = BvhTemplate.DefaultModelHierarchy +
				String.Format(BvhTemplate.defaultModelMotionHeader, nFrames, delta);
			return model;
		}

		public static string GetDefaultModel(float delta, int nFrames = 1)
		{
			string model = BvhTemplate.GetDefaultModelWithoutAngles(delta, nFrames);
			if (!BvhTemplate.nChannels.HasValue) {
				MatchCollection matches = Regex.Matches(BvhTemplate.DefaultModelHierarchy,
					@"(?:CHANNELS\s)(\d)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
				int n = 0;
				foreach (Match match in matches) {
					n += int.Parse(match.Groups[1].Value);
				}

				BvhTemplate.nChannels = n;
			}

			float[] dummyAngles = new float[BvhTemplate.nChannels.Value];
			for (int i = 0; i < 3; i++) {
				dummyAngles[i] = BvhTemplate.TranslationTPose[i];
			}

			return model + String.Join(" ", dummyAngles) + "\n";
		}

		public static BvhSkeleton GetDefaultSkeleton()
		{
			if (BvhTemplate.bvhSkeleton != null) {
				return BvhTemplate.bvhSkeleton;
			}

			string bvhString = BvhTemplate.GetDefaultModel(1.0f);
			TextReader bvhReader = new StringReader(bvhString);
			BvhSkeleton skel = new BvhSkeleton();

			using (bvhReader) {
				BvhParser parser = new BvhParser(bvhReader);
				parser.Parse(skel);
			}

			skel.Reset();
			BvhTemplate.bvhSkeleton = skel;
			return skel;
		}
	}
}
