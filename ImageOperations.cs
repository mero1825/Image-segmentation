using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace ImageTemplate
{
    public struct RGBPixel
    {
        public byte red, green, blue;
    }

    public struct RGBPixelD
    {
        public double red, green, blue;
    }

    public struct Edge
    {
        public int SourceId;
        public int DestId;
       
    }

    public struct SortedEdge
    {
        public int weight;
        public int src;
        public int dest;
    }

    public class ImageGraph
    {
        //public List<Edge> Edge1;
        public Edge[] Edge1;

        
    }

    public class ImageOperations
    {
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }
                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }

        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];

            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];
            double Sum1 = 0;
            int HalfSize = filterSize / 2;

            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    RGBPixelD Sum = new RGBPixelD { red = 0, green = 0, blue = 0 };
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        int ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            RGBPixel Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    RGBPixelD Sum = new RGBPixelD { red = 0, green = 0, blue = 0 };
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        int jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            RGBPixelD Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }

        public static void SortArray(SortedEdge[] arr)
        {
            Array.Sort(arr, (a, b) => a.weight.CompareTo(b.weight)); 
        }
        public static long ComputeNumEdges(int width, int height)
        {
            long w = width;
            long h = height;
           // return h * (w - 1) + w * (h - 1) + 2 * (h - 1) * (w - 1);
            return w * 4 * h;
        }
        public static ImageGraph CreateGraph(RGBPixel[,] imageData)
        {
            int height = GetHeight(imageData);
            int width = GetWidth(imageData);
            long numEdgesLong = ComputeNumEdges(width, height);

            // Check if the number of edges fits in an array
            if (numEdgesLong > int.MaxValue)
            {
                throw new Exception("Number of edges exceeds array size limit.");
            }

            long numEdges = numEdgesLong;
            Edge[] edges = new Edge[numEdges];
            int edgeIndex = 0;

            int[] rowOffsets = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] colOffsets = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    int srcId = r * width + c;
                    for (int i = 0; i < 8; i++)
                    {
                        int nr = r + rowOffsets[i];
                        int nc = c + colOffsets[i];
                        if (nr >= 0 && nr < height && nc >= 0 && nc < width)
                        {
                            int destId = nr * width + nc;
                            if (srcId < destId) 
                            {
                                edges[edgeIndex++] = new Edge { 
                                    SourceId = srcId,
                                    DestId = destId };
                            }
                        }
                    }
                }
            }

            ImageGraph graph = new ImageGraph { Edge1 = edges };
            return graph;
        }

        public static SortedEdge[] Get_Sorted_Edges( RGBPixel[,] imageMatrix, string channel, Edge[] edgeArray)
        {
            int width = GetWidth(imageMatrix);
            SortedEdge[] sortedarr = new SortedEdge[edgeArray.Length];
            int index = 0;

            foreach (var x in edgeArray)
            {
                int srcRow = x.SourceId / width;
                int srcCol = x.SourceId % width;
                int destRow = x.DestId / width;
                int destCol = x.DestId % width;

                int weight = 0;
                switch (channel.ToLower())
                {
                    case "red":
                        weight = Math.Abs(imageMatrix[srcRow, srcCol].red - imageMatrix[destRow, destCol].red);
                        break;
                    case "green":
                        weight = Math.Abs(imageMatrix[srcRow, srcCol].green - imageMatrix[destRow, destCol].green);
                        break;
                    case "blue":
                        weight = Math.Abs(imageMatrix[srcRow, srcCol].blue - imageMatrix[destRow, destCol].blue);
                        break;
                }

                sortedarr[index++] = new SortedEdge
                { weight = weight,
                    src = x.SourceId,
                    dest = x.DestId
                };
            }

            SortArray(sortedarr); 
            return sortedarr;
        }

        public static void ProcessChannel(int height,int width, double k,out int[,] labels,SortedEdge[] sortarr)
        {
            labels = new int[height, width];

            int[] parent = new int[height * width];
            int[] size = new int[height * width];
            float[] internalDiff = new float[height * width];

            for (int i = 0; i < height * width; i++)
            {
                parent[i] = i;
                size[i] = 1;
                internalDiff[i] = 0;
            }

            int Find(int x)
            {
                if (parent[x] != x)
                    parent[x] = Find(parent[x]);
                return parent[x];
            }
            foreach(var x in sortarr)
            {
                int srcRoot = Find(x.src);
                int destRoot = Find(x.dest);
                if (srcRoot == destRoot) continue;

                double weight = x.weight;

                double threshold = Math.Min(
                    internalDiff[srcRoot] + k / size[srcRoot],
                    internalDiff[destRoot] + k / size[destRoot]
                );

                if (weight <= threshold)
                {
                    if (size[srcRoot] < size[destRoot])
                    {
                        parent[srcRoot] = destRoot;
                        size[destRoot] += size[srcRoot];
                        internalDiff[destRoot] = Math.Max(internalDiff[destRoot], Math.Max(internalDiff[srcRoot], (float)weight));
                    }
                    else
                    {
                        parent[destRoot] = srcRoot;
                        size[srcRoot] += size[destRoot];
                        internalDiff[srcRoot] = Math.Max(internalDiff[srcRoot], Math.Max(internalDiff[destRoot], (float)weight));
                    }
                }

            }

            for (int i = 0; i < height * width; i++)
            {
                int root = Find(i);
                int row = i / width;
                int col = i % width;
                labels[row, col] = root;
            }
        }

        public static int[,] IntersectLabelMaps(int[,] red, int[,] green, int[,] blue, int height, int width)
        {
            int[,] finalLabels = new int[height, width];
            Dictionary<(int, int, int), int> regionMap = new Dictionary<(int, int, int), int>();
            int currentId = 0;

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    var key = (red[r, c], green[r, c], blue[r, c]);
                    if (!regionMap.TryGetValue(key, out int id))
                    {
                        id = currentId++;
                        regionMap[key] = id;
                    }
                    finalLabels[r, c] = id;
                }
            }
            return finalLabels;
        }

        public static int[,] CheckConnection(int[,] regions, int rows, int cols)
        {
            int totalPixels = rows * cols;
            int[] parent = new int[totalPixels];
            int[] rank = new int[totalPixels];

            // Initialize disjoint set
            for (int i = 0; i < totalPixels; i++)
            {
                parent[i] = i;
                rank[i] = 0;
            }

            int GetRoot(int node)
            {
                while (parent[node] != node)
                {
                    parent[node] = parent[parent[node]]; // Path compression
                    node = parent[node];
                }
                return node;
            }

            void Merge(int node1, int node2)
            {
                int root1 = GetRoot(node1);
                int root2 = GetRoot(node2);

                if (root1 == root2) return;

                if (rank[root1] < rank[root2])
                {
                    parent[root1] = root2;
                }
                else if (rank[root1] > rank[root2])
                {
                    parent[root2] = root1;
                }
                else
                {
                    parent[root2] = root1;
                    rank[root1]++;
                }
            }

            int[] rowNeighbours = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] colNeighbours = { -1, 0, 1, -1, 1, -1, 0, 1 };

            // Connect adjacent pixels with the same label
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int currentPixel = i * cols + j;
                    int currentLabel = regions[i, j];

                    for (int k = 0; k < 8; k++)
                    {
                        int newRow = i + rowNeighbours[k];
                        int newCol = j + colNeighbours[k];

                        if (newRow >= 0 && newRow < rows && newCol >= 0 && newCol < cols)
                        {
                            int neighborPixel = newRow * cols + newCol;
                            if (regions[newRow, newCol] == currentLabel)
                            {
                                Merge(currentPixel, neighborPixel);
                            }
                        }
                    }
                }
            }

            int[,] updatedRegions = new int[rows, cols];
            Dictionary<int, int> groupToId = new Dictionary<int, int>();
            int newId = 0;

            // Assign new labels based on connected components
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int pixelIndex = i * cols + j;
                    int root = GetRoot(pixelIndex);

                    if (!groupToId.ContainsKey(root))
                    {
                        groupToId[root] = newId++;
                    }

                    updatedRegions[i, j] = groupToId[root];
                }
            }

            return updatedRegions;
        }

        private static readonly Random rand = new Random();

        public static RGBPixel[,] VisualizeSegmentation(int[,] final_labels, RGBPixel[,] original_image, int height, int width)
        {
            final_labels = CheckConnection(final_labels, height, width);

            int regionIDcounter = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    regionIDcounter = Math.Max(regionIDcounter, final_labels[i, j]);
            RGBPixel[,] finalRegions = new RGBPixel[height, width];
            RGBPixel[] regions = new RGBPixel[regionIDcounter + 1];
            // can use dictionary but array is faster
            // Dictionary<int, RGBPixel> regions = new Dictionary<int, RGBPixel>();
            bool[] CheckInit = new bool[regionIDcounter + 1];
            Random r = new Random();
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {

                    int regionNum = final_labels[i, j];
                    if (CheckInit[regionNum] == false)
                    {
                        regions[regionNum] = new RGBPixel
                        {
                            red = (byte)r.Next(0, 256),
                            green = (byte)r.Next(0, 256),
                            blue = (byte)r.Next(0, 256)

                        };
                        CheckInit[regionNum] = true;
                    }
                    finalRegions[i, j].red = (byte)(regions[regionNum].red);
                    finalRegions[i, j].green = (byte)(regions[regionNum].green);
                    finalRegions[i, j].blue = (byte)(regions[regionNum].blue);


                }



            return finalRegions;
        }
        public static void WriteSegmentStats(int[,] final_labels, int height, int width, string outputFilePath)
        {
            final_labels = CheckConnection(final_labels, height, width);

            int regionIDcounter = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    regionIDcounter = Math.Max(regionIDcounter, final_labels[i, j]);

            int[] regionSizes = new int[regionIDcounter + 1];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    int regionNum = final_labels[i, j];
                    regionSizes[regionNum]++;
                }



            Array.Sort(regionSizes);
            Array.Reverse(regionSizes);
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(outputFilePath, false, System.Text.Encoding.UTF8))
            {
                writer.WriteLine(regionSizes.Length);
                foreach (var singleRegionSize in regionSizes)
                {
                    //if (singleRegionSize == 0)
                    //    break;
                    //else
                        writer.WriteLine(singleRegionSize);
                }
            }
        }
        /*public static void WriteSegmentStats(int[,] final_labels, int height, int width, string outputFilePath)
        {
            final_labels = SplitDisconnectedRegions(final_labels, height, width);

            int regionIDcounter = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    regionIDcounter = Math.Max(regionIDcounter, final_labels[i, j]);

            int[] regionSizes = new int[regionIDcounter + 1];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    int regionNum = final_labels[i, j];
                    regionSizes[regionNum]++;
                }


            List<(int regionID, int size)> sortedRegions = new List<(int, int)>();
            for (int i = 0; i <= regionIDcounter; i++)
            {
                if (regionSizes[i] > 0)
                    sortedRegions.Add((i, regionSizes[i]));
            }

            sortedRegions.Sort((a, b) => b.size.CompareTo(a.size));

            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(outputFilePath, false, System.Text.Encoding.UTF8))
            {
                writer.WriteLine(sortedRegions.Count);
                foreach (var region in sortedRegions)
                {
                    writer.WriteLine(region.size);
                }
            }
        }*/
    }
}