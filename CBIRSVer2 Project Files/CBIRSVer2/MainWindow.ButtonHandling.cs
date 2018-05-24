/* -----------------------------------------------------------------------------
 * Author:              Kay Phan
 * Title:               CBIRS Version 2
 * Date Created:        1/24/2017
 * Date Last Revision:  2/1/2017
 * -----------------------------------------------------------------------------
 * This is the coding for the buttons of the CBIRS, such as the selection and
 * sort buttons.
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
        * Button Click Definitions
        * ----------------------------------------------------------------------
        */
        private void Intensity_Click(object sender, RoutedEventArgs e)
        {
            if (picNo > -1) sort(intensityMatrix);
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            if (picNo > -1) sort(colorCodeMatrix);
        }

        /* ----------------------------------------------------------------------
         * Both Click Definitions
         * ----------------------------------------------------------------------
         * Activated when user clicks on the "Sort by Color + Intensity" button
         * Initializes weights and sorts the images by the combined feature
         * matrix. 
         */
        private void Both_Click(object sender, RoutedEventArgs e)
        {
            relevantMode = true;
            var inten = Intensity.IsEnabled = false;
            var colo = Color.IsEnabled = false;
            var both = Both.IsEnabled = false;
            var stop = Stop.IsEnabled = true;
            var rel = Relevant.IsEnabled = true;
            ImgListView.SelectionMode =
                System.Windows.Controls.SelectionMode.Multiple;
            // initialize weights
            double features = weightMatrix.Length;
            for (int i = 0; i < weightMatrix.Length; i++)
            {
                weightMatrix[i] = 1 / features;
            }
            currentlySelected[0].Add(0);
            selected.Add(picNo);
            sortFeature(combiMatrix);
        }

        /* ----------------------------------------------------------------------
         * Update_List
         * ----------------------------------------------------------------------
         * Updates the order of the list in the UI
         */
        private void Update_List()
        {
            int pic = picNo;
            img_list.Clear();
            int currPageRange = (currentPage - 1) * pageSize;
            for (int i = currPageRange; i < currPageRange + pageSize; i++)
            {
                img_list.Add(img_map[img_order[i]]);
            }
            picNo = pic;
            Preview.Source = img_map[picNo];
        }

        /* ----------------------------------------------------------------------
         * Stop Click Definitions
         * ----------------------------------------------------------------------
         * The user clicks the stop refining button when they do not want to
         * refine the results any longer. This wil enable the sort by color
         * and intensity functions, and disable the refine search and stop
         * buttons, as well as make it so that only one image can be selected
         * at a time.
         */
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            var inten = Intensity.IsEnabled = true;
            var colo = Color.IsEnabled = true;
            var both = Both.IsEnabled = true;
            var stop = Stop.IsEnabled = false;
            var rel = Relevant.IsEnabled = false;
            ImgListView.SelectionMode =
                System.Windows.Controls.SelectionMode.Single;


            this.selected.Clear();
            for (int i = 0; i < 5; i++)
                this.currentlySelected[i] = new HashSet<int>();
            relevantMode = false;
        }

        /* ----------------------------------------------------------------------
         * Relevant Click Definitions
         * ----------------------------------------------------------------------
         * Adds user selected images to their definition of relevant images.
         */
        private void Relevant_Click(object sender, RoutedEventArgs e)
        {
            // store the selected
            btnNav_Click();

            // gather all selected items
            for (int i = 0; i < 5; i++)
            {
                foreach (int j in currentlySelected[i])
                {
                    int toadd = img_order[(currentPage - 1) * 20 + j];

                    selected.Add(img_order[(currentPage - 1) * 20 + j]);
                }
            }
            updateWeight();
            sortFeature(combiMatrix);

        }


        /* ----------------------------------------------------------------------
         * ImgListView_SelectionChanged
         * ----------------------------------------------------------------------
         * When the mode is not in refining mode, then the image selected will
         * be the display image, and the image to compare with.
         */
        private void ImgListView_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            // only when in single selection mode this happens
            if (!relevantMode)
            {
                int index = ImgListView.SelectedIndex + ((currentPage - 1) * pageSize);
                if (index > -1 && index < 100)
                {
                    picNo = img_order[index];
                    Preview.Source = img_map[picNo];
                }
            }
        }
    }
}
