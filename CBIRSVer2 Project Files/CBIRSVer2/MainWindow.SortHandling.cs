/* -----------------------------------------------------------------------------
 * Author:              Kay Phan
 * Title:               CBIRS Version 2
 * Date Created:        1/24/2017
 * Date Last Revision:  2/1/2017
 * Filename:            MainWindow.PageHandling.cs
 * -----------------------------------------------------------------------------
 * This is the coding for the sort handling for the main window of the CBIRS.
 * This handles the code sorting images by similarity, whether by their 
 * intensity, color code, or both.
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
        * sort
        * ----------------------------------------------------------------------
        * This is the private helper function for both colorCodeHandler and 
        * intensityHandler. 
        * Preconditions:
        * -Double[][]: either specify as intensity matrix or color code matrix
        * -mbins: number of bins in the matrix
        */
        private void sort(Double[,] m)
        {
            SortedDictionary<double, int> map = new SortedDictionary<double,
                int>();
            double picSize = imageSize[picNo];
            double manhattanDistance = 0;
            int mlength = m.GetLength(1);
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < mlength; j++)
                {
                    double orig = m[picNo, j] / picSize;
                    double comp = m[i, j] / imageSize[i];
                    double pixelDiff = (orig - comp);
                    manhattanDistance += Math.Abs(pixelDiff);
                }
                map.Add(manhattanDistance, i);
                manhattanDistance = 0;
            }
            int index = 0;
            foreach (double key in map.Keys)
            {
                img_order[index] = map[key];
                index++;
            }
            imageCount = 0;
            currentPage = 1;
            displayPage();
        }

        /* ----------------------------------------------------------------------
         * sortFeature
         * ----------------------------------------------------------------------
         * This is the private helper function for both colorCodeHandler and 
         * intensityHandler. 
         * Preconditions:
         * -Double[,]: either specify as intensity matrix or color code matrix
         * -mbins: number of bins in the matrix
         */
        private void sortFeature(Double[,] m)
        {
            SortedDictionary<double, int> map = new SortedDictionary<double,
                int>();
            double picSize = imageSize[picNo];
            double manhattanDistance = 0;
            int mlength = m.GetLength(1);
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < mlength; j++)
                {
                    double pixelDiff = m[picNo, j] - m[i, j];
                    manhattanDistance += weightMatrix[j] * Math.Abs(pixelDiff);
                }
                map.Add(manhattanDistance, i);
                manhattanDistance = 0;

            }
            int index = 0;
            foreach (double key in map.Keys)
            {
                img_order[index] = map[key];
                index++;
            }

            // reselect currently selected
            for (int i = 0; i < 5; i++)
                currentlySelected[i].Clear();
            for (int i = 0; i < 100; i++)
            {
                int has = img_order[i];
                int page = i / pageSize;
                int ind = i % pageSize;
                if (selected.Contains(has))
                {
                    currentlySelected[page].Add(ind);
                }


            }
            imageCount = 0;
            currentPage = 1;
            displayPage();
        }

        /* ----------------------------------------------------------------------
         * updateWeight
         * ----------------------------------------------------------------------
         * Updates the weights based on the relevant items. This method works by
         * finding the standard deviations the features of the selected items,
         * then uses this to update the weights, setting W = 1 / SD.
         */
        private void updateWeight()
        {

            // get averages
            for (int i = 0; i < combiMatrix.GetLength(1); i++)
            {
                double average = 0;
                foreach (int j in selected)
                {
                    average += combiMatrix[j, i];
                }
                meanMatrix[i] = average / selected.Count; // get mean
            }
            // get standard deviations
            double minsd = 999999;

            for (int i = 0; i < 89; i++)
            {
                double errorSquare = 0;
                foreach (int j in selected)
                {
                    // sum square error
                    errorSquare += (combiMatrix[j, i] - meanMatrix[i]) * (combiMatrix[j, i] - meanMatrix[i]);
                }
                double sd = Math.Sqrt(errorSquare / 99); // sd = (SSE/(n-1))^1/2
                sdMatrix[i] = sd;
                if (sd < minsd && sd != 0) minsd = sd;
            }

            double totalsd = 0;
            // revise sd's
            for (int i = 0; i < 89; i++)
            {
                if (sdMatrix[i] == 0)
                {
                    // if mean at i is not zero, set sdi to .5 * minsd
                    if (meanMatrix[i] != 0) sdMatrix[i] = minsd * 0.5;
                }
                totalsd += sdMatrix[i];
            }

            // normalize weights
            for (int i = 0; i < combiMatrix.GetLength(1); i++)
            {
                // weight is zero if sd from mean is zero
                if (sdMatrix[i] == 0) weightMatrix[i] = 0;
                else weightMatrix[i] = 1 / sdMatrix[i];
            }
        }
    }
}
