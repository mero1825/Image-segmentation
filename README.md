# Image-segmentation
This C# library provides tools for image processing and segmentation using graph-based techniques. It includes functionality to load images, apply Gaussian filtering, create image graphs, perform segmentation based on color channels, and visualize the results. The library is designed for use with Windows Forms for displaying processed images.
Features


## Features

Image Loading: Load images into an RGB pixel matrix using OpenImage.



Gaussian Filtering: Apply 1D Gaussian blur to images with configurable filter size and sigma using GaussianFilter1D.



Graph Creation: Construct an image graph with edges representing pixel relationships using CreateGraph.



Edge Sorting: Generate and sort edges based on color channel differences (red, green, or blue) using Get_Sorted_Edges.



Segmentation: Perform graph-based segmentation for each color channel and combine results using ProcessChannel and IntersectLabelMaps.



Connected Components: Ensure proper region connectivity with CheckConnection.



Visualization: Visualize segmented regions with random colors using VisualizeSegmentation.



Statistics Output: Save segment size statistics to a file using WriteSegmentStats.

## Usage

The library is designed to work with RGB images and supports 8-bit, 24-bit, and 32-bit pixel formats. It uses a graph-based segmentation algorithm inspired by the Felzenszwalb-Huttenlocher method, where pixels are grouped into regions based on color similarity and spatial connectivity. The output includes a segmented image and a text file with region size statistics.

## Requirements





.NET Framework with Windows Forms for image display.



Input images in standard formats (e.g., BMP, PNG, JPEG).



Visual Studio or compatible IDE for building and running the code.
