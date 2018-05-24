/* -----------------------------------------------------------------------------
 * Author:              Kay Phan
 * Title:               CBIRS Version 2
 * Date Created:        1/24/2017
 * Date Last Revision:  2/1/2017
 * -----------------------------------------------------------------------------
 * This is the coding for the file handling of the CBIRS. It creates new files
 * for the matrices if they are not found, and reads the files that are found.
 * 
 * This program allows a user to select an image and sort the image by 
 * color code, intensity, or both combined. It also has a feature that allows
 * the user to select relevant images and by submitting them, refine the search.
 * -----------------------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CBIRSVer2
{
    public partial class MainWindow : Window
    {
        /* ----------------------------------------------------------------------
 * readIntensityFile
 * ----------------------------------------------------------------------
 * This method opens the intensity text file containing the intensity  
 * matrix with the histogram bin values. for each image. The contents 
 * of the matrix are processed and stored in a two dimensional array 
 * called intensityMatrix.
 */
        public void readIntensityFile()
        {
            readFile("intensity.txt", intensityMatrix);
        }

        /* ----------------------------------------------------------------------
         * readColorCodeFile
         * ----------------------------------------------------------------------
         * This method opens the color code text file containing the color code 
         * matrix with the histogram bin values for each image. The contents of
         * the matrix are processed and stored in a two dimensional array
         * called colorCodeMatrix.
         */
        private void readColorCodeFile()
        {
            readFile("colorCode.txt", colorCodeMatrix);
        }

        /* ----------------------------------------------------------------------
         * readFile
         * ----------------------------------------------------------------------
         * Private helper for file reading. Reads items in given file into
         * the given matrix. If file with filename is not found, then the flag
         * that suggests that the files need to be created is set.
         * Preconditions:
         *  -filename is valid (does not need to exist yet)
         *  -matrix is valid 
         */
        private void readFile(string filename, double[,] matrix)
        {
            StreamReader read;
            string line;
            int lineNumber = 0;
            try
            {
                read = new StreamReader(filename);
                while ((line = read.ReadLine()) != null)
                {
                    String[] substrings = line.Split(' ');
                    for (int i = 0; i < substrings.Length - 1; i++)
                    {
                        int substringlength = substrings.Length - 1;
                        matrix[lineNumber, i] =
                            (double)Int32.Parse(substrings[i]);
                    }
                    lineNumber++;
                }
                read.Close();
            }
            catch (FileNotFoundException EE)
            {
                Console.WriteLine("The file " + filename + " does not exist");
                hasFiles = false;
            }
        }

        /* ----------------------------------------------------------------------
         * getMatrices
         * ----------------------------------------------------------------------
         * Will only create matrices if files are missing
         */
        private void getMatrices(BitmapImage image)
        {
            Bitmap bmp = BitmapImage2Bitmap(image);
            int imgHeight = bmp.Height;
            int imgWidth = bmp.Width;
            getIntensity(bmp, imgHeight, imgWidth);
            getColorCode(bmp, imgHeight, imgWidth);
            writeIntensityFile();
            writeColorCodeFile();
        }

        /* ----------------------------------------------------------------------
         * getIntensity
         * ----------------------------------------------------------------------
         * Gets the intensity of every pixel in all images and stores it in a 
         * histogram, called intensityMatrix. Intensity of a pixel is determined
         * by the following formula: 0.299 * red + .587 * green + 0.114 * blue
         * Preconditions:
         * -image: the image to store the color code of
         * -height: image height
         * -width: image width
         */
        public void getIntensity(Bitmap image, int height, int width)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    System.Drawing.Color c = image.GetPixel(i, j);
                    int red = c.R;
                    int green = c.G;
                    int blue = c.B;
                    double intensity = (0.299 * red) + (.587 * green) + (0.114 * blue);
                    int bucket = (int)(intensity / 10);
                    if (bucket >= intensityMatrix.GetLength(1)) bucket--;
                    intensityMatrix[this.imageCount, bucket]++;
                }
            }


        }

        /* ----------------------------------------------------------------------
         * getColorCode
         * ----------------------------------------------------------------------
         * Gets the color code of every pixel in all images and stores it in a 
         * histogram, called colorCodeMatrix. Color code is determined by 
         * converting RGB values to 8 bit binary codes, taking the largest two
         * significant digits of each code, and creating a six bit binary code 
         * with the following format:
         * red | red | green | green | blue | blue
         * Preconditions:
         * -image: the image to store the color code of
         * -height: image height
         * -width: image width
         */
        public void getColorCode(Bitmap image, int height, int width)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    System.Drawing.Color c = image.GetPixel(i, j);
                    int red = c.R;
                    int green = c.G;
                    int blue = c.B;

                    blue = blue >> 6;
                    green = (green >> 6) << 2;
                    red = (red >> 6) << 4;
                    double colorcode = red + green + blue;
                    int bucket = (int)(colorcode);
                    colorCodeMatrix[imageCount, bucket]++;
                }
            }
        }

        /* ----------------------------------------------------------------------
         * getCombiMatrix
         * ----------------------------------------------------------------------
         * Gets a feature matrix with both the color codes and the intentsities
         * side by side. Therefore, the length of this matrix will be the same
         * as the lengths of the color code matrix and the intensity matrix 
         * combined. Each row (image) is divided by its size.
         */
        private void getCombiMatrix()
        {
            // for each row
            for (int i = 0; i < combiMatrix.GetLength(0); i++)
            {
                // for each image, copy over features from the intensity
                // and divide by image sie
                for (int j = 0; j < intensityMatrix.GetLength(1); j++)
                    combiMatrix[i, j] = (intensityMatrix[i, j]) / imageSize[i];
                int offset = intensityMatrix.GetLength(1);

                // divide each row by its image size
                for (int j = intensityMatrix.GetLength(1); j <
                    combiMatrix.GetLength(1); j++)
                {
                    combiMatrix[i, j] = (colorCodeMatrix[i, j - offset])
                        / imageSize[i];
                }
            }

            // normalize matrix
            // get averages
            for (int i = 0; i < combiMatrix.GetLength(1); i++)
            {
                double average = 0;
                for (int j = 0; j < combiMatrix.GetLength(0); j++)
                {
                    average += combiMatrix[j, i];
                }
                meanMatrix[i] = average / 100;
            }
            // get standard deviations
            double minsd = 999999;
            for (int i = 0; i < 89; i++)
            {
                double errorSquare = 0;
                for (int j = 0; j < combiMatrix.GetLength(0); j++)
                {
                    errorSquare += (combiMatrix[j, i] - meanMatrix[i]) * (combiMatrix[j, i] - meanMatrix[i]);
                }
                double sd = Math.Sqrt(errorSquare / 99);
                sdMatrix[i] = sd;
                if (sd < minsd && sd != 0) minsd = sd;
            }
            // normalize matrix
            for (int i = 0; i < combiMatrix.GetLength(1); i++)
            {
                for (int j = 0; j < combiMatrix.GetLength(0); j++)
                {
                    if (sdMatrix[i] == 0) combiMatrix[j, i] = (combiMatrix[j, i] - meanMatrix[i]) / (minsd / 2);
                    else combiMatrix[j, i] = (combiMatrix[j, i] - meanMatrix[i]) / sdMatrix[i];
                }
            }
        }

        /* ----------------------------------------------------------------------
         * writeIntensityFile
         * ---------------------------------------------------------------------
         * Outputs the intentsity matrix to a file called "intensity.txt"
         */
        private void writeIntensityFile()
        {
            writeFile("intensity.txt", intensityMatrix);
        }

        /* ----------------------------------------------------------------------
         * writeColorCodeFile
         * ---------------------------------------------------------------------
         * Outputs the color code matrix to a file called "colorCode.txt"
         */
        private void writeColorCodeFile()
        {
            writeFile("colorCode.txt", colorCodeMatrix);
        }

        /* ----------------------------------------------------------------------
         * writeFile
         * ----------------------------------------------------------------------
         * This method writes the contents of the specified 2D array to a file 
         * of the given name.
         * Preconditions:
         *  -name is a string of a valid filename
         *  -matrix is valid
         */
        private void writeFile(string filename, double[,] matrix)
        {
            StreamWriter writer = new StreamWriter(filename);
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    writer.Write((int)matrix[i, j]);
                    writer.Write(" ");
                }
                writer.WriteLine();
            }
            writer.Close();
        }

        /* ----------------------------------------------------------------------
         * BitmapImage2Bitmap
         * ----------------------------------------------------------------------
         * This method takes in a BitmapImage and converts it into a bitmap.
         * Returns a bitmap. Serves as a helper function to get intensity and
         * color code.
         * Preconditions:
         *  -BitmapImage is valid(not null)
         */
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new
                    System.Drawing.Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }
    }



}
