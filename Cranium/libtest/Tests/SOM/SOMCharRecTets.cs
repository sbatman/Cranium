// // --------------------------------
// // -- File Created 	: 10:12 28/06/2019
// // -- File Part of the Cranium Solution, project LibTest
// // -- Edited By : Steven Batchelor-Manning
// // --------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Cranium.Lib.Data.Preprocessing;
using Cranium.Lib.Structure;
using Cranium.Lib.Structure.ActivationFunction;
using Cranium.Lib.Structure.Layer;
using Cranium.Lib.Structure.Node;
using Cranium.Lib.Test.SupportClasses;

namespace Cranium.Lib.Test.Tests.SOM
{
	internal class SOMCharRecTets
	{
		public const Int32 IMAGE_SIZE = 24;
		public const Int32 OUTPUT_NODE_GRID_WIDTH = 6;
		public const Int32 OUTPUT_COUNT = OUTPUT_NODE_GRID_WIDTH * OUTPUT_NODE_GRID_WIDTH;

		public static Byte[][] NodeData = new Byte[OUTPUT_COUNT][];

		public static void Run()
		{
			Console.WriteLine("This test shows an example of a neural network learning how to categories a series of digits provided along with the test, After a every epochs an image showing the current knowledge of the network is saved in the running directory");
			Console.WriteLine("Press any key to begin");

			List<ImageNormalizer.PreProcessedImage> processedImages = new List<ImageNormalizer.PreProcessedImage>();

			for (Int32 i = 0; i < 10; i++)
			{
				processedImages.AddRange(LoadContent($"Content/Images/{i}/", i.ToString()));
			}

			NetworkConfiguration network = new NetworkConfiguration(IMAGE_SIZE * IMAGE_SIZE, OUTPUT_NODE_GRID_WIDTH);

			for (Int32 i = 0; i < OUTPUT_COUNT; i++) NodeData[i] = new Byte[IMAGE_SIZE * IMAGE_SIZE];

			PresentImagesToNetwork(network, processedImages, 450);

			Console.WriteLine("Complete");
			Console.ReadKey();
		}

		private static void TestNetwork(IEnumerable<ImageNormalizer.PreProcessedImage> processedImages, NetworkConfiguration network, Int32 epoch)
		{
			Dictionary<String, List<Int32>> outcomes = new Dictionary<String, List<Int32>>();

			Int32[,] outputChunk = new Int32[OUTPUT_COUNT, IMAGE_SIZE * IMAGE_SIZE];
			Int32[] outputCount = new Int32[OUTPUT_COUNT];

			foreach (ImageNormalizer.PreProcessedImage image in processedImages)
			{
				if (!outcomes.ContainsKey(image.Tag)) outcomes.Add(image.Tag, new List<Int32>());

				Int32 id = ClassifyImage(network, image);
				outcomes[image.Tag].Add(id);

				outputCount[id]++;

				for (Int32 i = 0; i < IMAGE_SIZE * IMAGE_SIZE; i++) outputChunk[id, i] += image.BwMap[i];
			}

			Byte[] outputImage = new Byte[IMAGE_SIZE * IMAGE_SIZE * OUTPUT_COUNT];

			for (Int32 n = 0; n < OUTPUT_COUNT; n++)
			{
				if (outputCount[n] == 0) continue;
				for (Int32 i = 0; i < IMAGE_SIZE * IMAGE_SIZE; i++) outputChunk[n, i] /= outputCount[n];
			}

			for (Int32 nx = 0; nx < OUTPUT_NODE_GRID_WIDTH; nx++)
			{
				for (Int32 ny = 0; ny < OUTPUT_NODE_GRID_WIDTH; ny++)
				{
					for (Int32 x = 0; x < IMAGE_SIZE; x++)
					{
						for (Int32 y = 0; y < IMAGE_SIZE; y++)
						{
							Int32 targetX = nx * IMAGE_SIZE + x;
							Int32 targetY = ny * IMAGE_SIZE + y;

							Int32 target = targetX + targetY * IMAGE_SIZE * OUTPUT_NODE_GRID_WIDTH;

							outputImage[target] = (Byte) outputChunk[nx + ny * OUTPUT_NODE_GRID_WIDTH, x + y * IMAGE_SIZE];
						}
					}
				}
			}

			ImageLoader.SaveBwImage($"{epoch}.bmp", IMAGE_SIZE * OUTPUT_NODE_GRID_WIDTH, IMAGE_SIZE * OUTPUT_NODE_GRID_WIDTH, outputImage);
		}

		private static IEnumerable<ImageNormalizer.PreProcessedImage> LoadContent(String folder, String addTag = null)
		{
			List<ImageNormalizer.Image> rawImages = new List<ImageNormalizer.Image>(ImageLoader.GetImagesInFolder(folder, IMAGE_SIZE, IMAGE_SIZE));
			List<ImageNormalizer.PreProcessedImage> processedImages = rawImages.Select(img => ImageNormalizer.ProcessImage(img, IMAGE_SIZE, IMAGE_SIZE)).ToList();
			processedImages.ForEach(a => a.Tag = addTag);
			return processedImages;
		}

		private static void PresentImagesToNetwork(NetworkConfiguration network, List<ImageNormalizer.PreProcessedImage> images, Int32 epochs)
		{
			for (Int32 epoch = 0; epoch <= epochs; epoch++)
			{
				Console.Title = $"{epoch}/{epochs}";
				images.Shuffle();

				network.SOMMemoryLayer.CurrentDistanceSupression = 1 - epoch / (Double) epochs;

				foreach (ImageNormalizer.PreProcessedImage image in images)
				{
					//ForwardPass
					Byte[] mapToPresent = image.BwMap;

					BaseNode[] nodes = network.InputLayerNodes.ToArray();
					for (Int32 i = 0; i < mapToPresent.Length; i++)
					{
						nodes[i].SetValue(mapToPresent[i] / 255.0);
					}

					network.NetworkInstance.FowardPass();

					network.NetworkInstance.ReversePass();
				}

				network.NetworkInstance.LearningRate = Math.Max(0.1f, network.NetworkInstance.LearningRate * 0.95);

				if (epoch % 5 == 0) TestNetwork(images, network, epoch);
			}
		}

		private static Int32 ClassifyImage(NetworkConfiguration network, ImageNormalizer.PreProcessedImage image)
		{
			Byte[] mapToPresent = image.BwMap;
			BaseNode[] nodes = network.InputLayerNodes.ToArray();
			for (Int32 i = 0; i < mapToPresent.Length; i++)
			{
				nodes[i].SetValue(mapToPresent[i] / 255.0);
			}

			network.InputLayer.ForwardPass();

			Int32 totalNodes = network.SOMMemoryLayer.GetNodes().Count;
			Int32 widthHeight = (Int32) Math.Sqrt(totalNodes);

			Double lowestDiff = Double.MaxValue;
			Int32 lowestDiffX = 0;
			Int32 lowestDiffY = 0;
			for (Int32 y = 0; y < widthHeight; y++)
			{
				for (Int32 x = 0; x < widthHeight; x++)
				{
					network.SOMMemoryLayer.GetNodeAtLocation(x, y).CalculateError();
					Double diff = network.SOMMemoryLayer.GetNodeAtLocation(x, y).GetError();

					if (!(diff < lowestDiff)) continue;

					lowestDiff = diff;
					lowestDiffX = x;
					lowestDiffY = y;
				}
			}

			return lowestDiffY * widthHeight + lowestDiffX;
		}

		private class NetworkConfiguration
		{
			public readonly Layer InputLayer;
			public readonly List<BaseNode> InputLayerNodes;
			public readonly Network NetworkInstance;
			public readonly SOMLayer SOMMemoryLayer;

			public NetworkConfiguration(Int32 inputNodeCount, Int32 somGridSize)
			{
				NetworkInstance = new Network();

				InputLayer = new Layer();
				InputLayerNodes = new List<BaseNode>();
				for (Int32 i = 0; i < inputNodeCount; i++) InputLayerNodes.Add(new BaseNode(InputLayer, new LinearAF()));
				InputLayer.SetNodes(InputLayerNodes);

				SOMMemoryLayer = new SOMLayer(somGridSize)
				{
					MaxmimumLearningDistance = 6,
					MinimumLearningDistance = 0.2
				};

				InputLayer.ConnectForwardLayer(SOMMemoryLayer);

				NetworkInstance.AddLayer(InputLayer);
				NetworkInstance.AddLayer(SOMMemoryLayer);

				NetworkInstance.Momentum = 0.2f;
				NetworkInstance.LearningRate = 0.1f;

				foreach (Layer layer in NetworkInstance.GetCurrentLayers()) layer.PopulateNodeConnections();

				NetworkInstance.RandomiseWeights(1.0f, true);
			}
		}
	}
}