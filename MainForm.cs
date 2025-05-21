using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace ImageTemplate
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        private ImageGraph imageGraph;

        int imgwidth = 0;
        int imgheight = 0;
        Edge[] myarrr;
        long gaustime = 0;
        long ovearlltime=0;
        long segtime =0 ;
        long txt_time = 0;



        private void btnOpen_Click(object sender, EventArgs e)
        {
            Stopwatch t = Stopwatch.StartNew();
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image Files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                   
                    ImageMatrix = ImageOperations.OpenImage(openFileDialog.FileName);
                    if (ImageMatrix == null)
                    {
                        MessageBox.Show("Failed to load image matrix.");
                        return;
                    }
                    int imgwidth = ImageOperations.GetWidth(ImageMatrix);
                    int imghight= ImageOperations.GetHeight(ImageMatrix);
                    myarrr = new Edge[imgwidth * imgheight];
                    //unsafe
                    //{
                    //    widthptr = &imgwidth;
                    //    heightptr = &imghight;

                    //}
                    txtWidth.Text = imgwidth.ToString();
                    txtHeight.Text = imghight.ToString();
                    textBox3.Text=(imgwidth* imghight).ToString();


                    ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                  //  MessageBox.Show("Image loaded and graph created successfully."); // Debug: Confirm success
                }
                else
                {
                    MessageBox.Show("No file selected.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}");
            }
            long y = t.ElapsedMilliseconds;
            open_time.Text = y.ToString();
            ;
        }
       

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            int height = ImageOperations.GetHeight(ImageMatrix);
            int width = ImageOperations.GetWidth(ImageMatrix);
     
            Stopwatch t = Stopwatch.StartNew();
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value;

            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);

            imageGraph = ImageOperations.CreateGraph(ImageMatrix);
            textBox2.Text = imageGraph.Edge1.Count().ToString();


            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
            t.Stop();
            gaustime = t.ElapsedMilliseconds;
            gus_time.Text = gaustime.ToString();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {  

        }

       private async void button1_Click(object sender, EventArgs e)
{
            if (ImageMatrix == null)
            {
                MessageBox.Show("Please load an image first.");
                return;
            }

            if (imageGraph == null)
            {
                imageGraph = ImageOperations.CreateGraph(ImageMatrix);
                textBox2.Text = imageGraph.Edge1.Length.ToString();
            }

            if (!double.TryParse(textBox1.Text, out double k))
            {
                MessageBox.Show("Please enter a valid number for k.");
                return;
            }

            int height = ImageOperations.GetHeight(ImageMatrix);
            int width = ImageOperations.GetWidth(ImageMatrix);

            Stopwatch ttt = Stopwatch.StartNew();

            SortedEdge[] redsorted = ImageOperations.Get_Sorted_Edges(ImageMatrix, "red", imageGraph.Edge1);
            ImageOperations.ProcessChannel(height, width, k, out int[,] redLabels, redsorted);
            GC.Collect();

            SortedEdge[] greensorted = ImageOperations.Get_Sorted_Edges(ImageMatrix, "green", imageGraph.Edge1);
            ImageOperations.ProcessChannel(height, width, k, out int[,] greenLabels, greensorted);
            GC.Collect();

            SortedEdge[] bluesorted = ImageOperations.Get_Sorted_Edges(ImageMatrix, "blue", imageGraph.Edge1);
            ImageOperations.ProcessChannel(height, width, k, out int[,] blueLabels, bluesorted);
            GC.Collect();

            int[,] finalLabels = ImageOperations.IntersectLabelMaps(redLabels, greenLabels, blueLabels, height, width);
            RGBPixel[,] segmentedImage = ImageOperations.VisualizeSegmentation(finalLabels, ImageMatrix, height, width);
            ImageOperations.DisplayImage(segmentedImage, pictureBox3);

            ttt.Stop();
            segtime = ttt.ElapsedMilliseconds;
            seg_time.Text= segtime.ToString();
           
            string path = "C:/Users/meroa/OneDrive/Desktop/mero.txt";
            Stopwatch writetime = Stopwatch.StartNew();
            ImageOperations.WriteSegmentStats(finalLabels, height, width, path);
            writetime.Stop();
            txt_time = writetime.ElapsedMilliseconds;
            textBox4.Text = txt_time.ToString();    
            overall_time.Text = (segtime + gaustime+ txt_time).ToString();
            /* if (imageGraph == null || ImageMatrix == null)
             {
                 // double sigma = double.Parse(txtGaussSigma.Text);
                 // int maskSize = (int)nudMaskSize.Value;
                 // ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);

                  imageGraph = ImageOperations.CreateGraph(ImageMatrix,myarrr);
                  textBox2.Text = imageGraph.Edge1.Count().ToString();
                 // MessageBox.Show("Please load an image first.");
                 // return;
              }

              if (!double.TryParse(textBox1.Text, out double k))
              {
                  MessageBox.Show("Please enter a valid number for k.");
                  return;
              }

              int height = ImageOperations.GetHeight(ImageMatrix);
              int width = ImageOperations.GetWidth(ImageMatrix);

              Stopwatch ttt = Stopwatch.StartNew();

              // CHANGED: Process channels sequentially with memory cleanup
              SortedEdge[] redsorted, greenaorted, bluesorted;
              int[,] redLabels, greenLabels, blueLabels;

              // Process Red Channel
              redsorted = ImageOperations.Get_Sorted_Edges(ImageMatrix, "red", myarrr);
              ImageOperations.ProcessChannel(height, width,  k, out redLabels, redsorted);

              GC.Collect();

              // Process Green Channel
              greenaorted = ImageOperations.Get_Sorted_Edges( ImageMatrix, "green", myarrr);
              ImageOperations.ProcessChannel(height, width, k, out greenLabels, greenaorted);

              GC.Collect();

              // Process Blue Channel
              bluesorted = ImageOperations.Get_Sorted_Edges( ImageMatrix, "blue", myarrr);
              ImageOperations.ProcessChannel(height, width, k, out blueLabels, bluesorted);

              GC.Collect();

              // Intersect labels and visualize
              int[,] finalLabels = ImageOperations.IntersectLabelMaps(redLabels, greenLabels, blueLabels, height, width);
              RGBPixel[,] segmentedImage = ImageOperations.VisualizeSegmentation(finalLabels, ImageMatrix, height, width);
              ImageOperations.DisplayImage(segmentedImage, pictureBox3);

              ttt.Stop();
              seg_time.Text = ttt.ElapsedMilliseconds.ToString();

              // Write stats
              string path = "C:/Users/meroa/OneDrive/Desktop/final2/pictures to compare/ myout.txt";

              ImageOperations.WriteSegmentStats(finalLabels, height, width, path);*/
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Image != null)
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Title = "Save Image";
                    saveDialog.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*"; ;
                    saveDialog.RestoreDirectory = true;
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Determine format based on selected file type
                        System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                        pictureBox3.Image.Save(saveDialog.FileName, ImageFormat.Bmp);
                        }
                    }
                }
            }
        }
            

    }
}