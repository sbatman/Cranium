// //////////////////////
//  
// Cranium - A neural network framework for C#
// https://github.com/sbatman/Cranium.git
// 
// This work is covered under the Creative Commons Attribution-ShareAlike 3.0 Unported (CC BY-SA 3.0) licence.
// More information can be found about the liecence here http://creativecommons.org/licenses/by-sa/3.0/
// If you wish to discuss the licencing terms please contact Steven Batchelor-Manning
// 
// //////////////////////

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
    class SOMCharRecTets
    {
        class NetworkConfiguration
        {
            public readonly Layer InputLayer;
            public readonly List<BaseNode> InputLayerNodes;
            public readonly Network NetworkInstance;
            public readonly SOMLayer SOMMemoryLayer;
            public List<BaseNode> OuputLayerNodes;

            public NetworkConfiguration(Int32 inputNodeCount, Int32 somGridSize)
            {
                NetworkInstance = new Network();

                InputLayer = new Layer();
                InputLayerNodes = new List<BaseNode>();
                for (Int32 i = 0; i < inputNodeCount; i++) InputLayerNodes.Add(new BaseNode(InputLayer, new LinearAF()));
                InputLayer.SetNodes(InputLayerNodes);

                SOMMemoryLayer = new SOMLayer(somGridSize);
                SOMMemoryLayer.MaxmimumLearningDistance = 6;
                SOMMemoryLayer.MinimumLearningDistance = 0.2;

                InputLayer.ConnectFowardLayer(SOMMemoryLayer);

                NetworkInstance.AddLayer(InputLayer);
                NetworkInstance.AddLayer(SOMMemoryLayer);

                NetworkInstance.SetMomentum(0.0f);
                NetworkInstance.SetLearningRate(0.9f);

                foreach (Layer layer in NetworkInstance.GetCurrentLayers()) layer.PopulateNodeConnections();

                NetworkInstance.RandomiseWeights(1.0f, true);
            }
        }

        public const Int32 IMAGESIZE = 16;
        public const Int32 OUTPUTNODE_GRID_WIDTH = 6;
        public const Int32 OUTPUTCOUNT = OUTPUTNODE_GRID_WIDTH * OUTPUTNODE_GRID_WIDTH;

        public static Byte[][] NodeData = new Byte[OUTPUTCOUNT][];

        public static void Run()
        {
            List<ImageNormalizer.PreProcessedImage> processedImages = new List<ImageNormalizer.PreProcessedImage>();

            for (Int32 i = 0; i < 10; i++)
            {
                processedImages.AddRange(LoadContent($"Content/Images/{i}/", i.ToString()));
            }

            NetworkConfiguration network = new NetworkConfiguration(IMAGESIZE * IMAGESIZE, OUTPUTNODE_GRID_WIDTH);

            for (Int32 i = 0; i < OUTPUTCOUNT; i++) NodeData[i] = new Byte[IMAGESIZE * IMAGESIZE];

            PresentImagesToNetwork(network, processedImages, 450);
        }

        private static void TestNetwork(IEnumerable<ImageNormalizer.PreProcessedImage> processedImages, NetworkConfiguration network, Int32 epoch)
        {
            Dictionary<String, List<Int32>> outcomes = new Dictionary<String, List<Int32>>();

            Int32[,] outputChunk = new Int32[OUTPUTCOUNT, IMAGESIZE * IMAGESIZE];
            Int32[] outputCount = new Int32[OUTPUTCOUNT];

            foreach (ImageNormalizer.PreProcessedImage image in processedImages)
            {
                if (!outcomes.ContainsKey(image.Tag)) outcomes.Add(image.Tag, new List<Int32>());

                Int32 id = ClassifyImage(network, image);
                outcomes[image.Tag].Add(id);

                outputCount[id]++;

                for (Int32 i = 0; i < IMAGESIZE * IMAGESIZE; i++) outputChunk[id, i] += image.BWMap[i];
            }

            Byte[] outputImage = new Byte[IMAGESIZE * IMAGESIZE * OUTPUTCOUNT];

            for (Int32 n = 0; n < OUTPUTCOUNT; n++)
            {
                if (outputCount[n] == 0) continue;
                for (Int32 i = 0; i < IMAGESIZE * IMAGESIZE; i++) outputChunk[n, i] /= outputCount[n];
            }

            for (Int32 nx = 0; nx < OUTPUTNODE_GRID_WIDTH; nx++)
            {
                for (Int32 ny = 0; ny < OUTPUTNODE_GRID_WIDTH; ny++)
                {
                    for (Int32 x = 0; x < IMAGESIZE; x++)
                    {
                        for (Int32 y = 0; y < IMAGESIZE; y++)
                        {
                            Int32 targetX = nx * IMAGESIZE + x;
                            Int32 targetY = ny * IMAGESIZE + y;

                            Int32 target = targetX + targetY * IMAGESIZE * OUTPUTNODE_GRID_WIDTH;

                            outputImage[target] = (Byte) outputChunk[nx + ny * OUTPUTNODE_GRID_WIDTH, x + y * IMAGESIZE];
                        }
                    }
                }
            }
            ImageLoader.SaveBWImage($"{epoch}.bmp", IMAGESIZE * OUTPUTNODE_GRID_WIDTH, IMAGESIZE * OUTPUTNODE_GRID_WIDTH, outputImage);
        }

        private static IEnumerable<ImageNormalizer.PreProcessedImage> LoadContent(String folder, String addTag = null)
        {
            List<ImageNormalizer.Image> rawimages = new List<ImageNormalizer.Image>(ImageLoader.GetImagesInFolder(folder, IMAGESIZE, IMAGESIZE));
            List<ImageNormalizer.PreProcessedImage> processedImages = rawimages.Select(img => ImageNormalizer.ProcessImage(img, IMAGESIZE, IMAGESIZE)).ToList();
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
                    //FowardPass
                    Byte[] mapToPresent = image.BWMap;

                    BaseNode[] nodes = network.InputLayerNodes.ToArray();
                    for (Int32 i = 0; i < mapToPresent.Length; i++)
                    {
                        nodes[i].SetValue(mapToPresent[i] / 255.0);
                    }

                    network.NetworkInstance.FowardPass();

                    network.NetworkInstance.ReversePass();
                }

                network.NetworkInstance.SetLearningRate(Math.Max(0.1f, network.NetworkInstance.GetLearningRate() * 0.95));

                if (epoch % 10 == 0) TestNetwork(images, network, epoch);
            }
        }

        private static Int32 ClassifyImage(NetworkConfiguration network, ImageNormalizer.PreProcessedImage image)
        {
            Byte[] mapToPresent = image.BWMap;
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
    }
}