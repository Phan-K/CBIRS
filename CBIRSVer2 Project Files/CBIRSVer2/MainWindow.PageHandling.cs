/* -----------------------------------------------------------------------------
 * Author:              Kay Phan
 * Title:               CBIRS Version 2
 * Date Created:        1/24/2017
 * Date Last Revision:  2/1/2017
 * Filename:            MainWindow.PageHandling.cs
 * -----------------------------------------------------------------------------
 * This is the coding for the page handling for the main window of the CBIRS.
 * This handles the code for the back and forward pages, as well as loading
 * the page.
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

/* -----------------------------------------------------------------------------
 * This application initializes the main window. It will load all images in
 * the folder called "resources" and display it in the database.
 * -----------------------------------------------------------------------------
 */
namespace CBIRSVer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /* ----------------------------------------------------------------------
        * btnNext Click Definitions
        * ----------------------------------------------------------------------
        * Increases current page by one, displays next 20 items.
        */
        private void btnNavForward_Click(object sender, RoutedEventArgs e)
        {
            btnNav_Click();
            currentPage++;
            displayPage();
        }

        /* ----------------------------------------------------------------------
         * btnPrev Click Definitions
         * ----------------------------------------------------------------------
         * Decreases current page by one, displays previous 20 items.
         */
        private void btnNavBack_Click(object sender, RoutedEventArgs e)
        {
            btnNav_Click();
            currentPage--;
            displayPage();
        }

        /* ----------------------------------------------------------------------
         * btnNav_Click
         * ----------------------------------------------------------------------
         * If in refining relevant mode, then all selected items are added to the
         * selected items list
         */
        private void btnNav_Click()
        {
            // if in relevant mode
            if (relevantMode)
            {
                HashSet<int> currentSelection = new HashSet<int>();
                // add all currently selected items to the list
                List<int> selectedItemIndexes = (from BitmapImage i in
                    ImgListView.SelectedItems
                                                 select
                       ImgListView.Items.IndexOf(i)).ToList();
                foreach (int i in selectedItemIndexes)
                {
                    int convertedIndex = i + ((currentPage - 1) * pageSize);
                    currentSelection.Add(i);
                }
                currentlySelected[currentPage - 1] = currentSelection;
            }
        }

        /* ----------------------------------------------------------------------
         * displayPage
         * ----------------------------------------------------------------------
         * Updates the page display, showing the 20 items of the current page.
         * If on the first page, the previous button
         * is disabled. If on the last page, the next button is disabled.
         * If in between, both are enabled. Reselects the selected images.
         */
        private void displayPage()
        {
            // if first page, deactivate back button
            if (currentPage == 1)
            {
                btnPrev.IsEnabled = false;
            }
            // if last page, deactivate forward button
            else if (currentPage == noOfPages)
            {
                btnNext.IsEnabled = false;
            }
            // if in between, both buttons are enabled
            else
            {
                btnPrev.IsEnabled = true;
                btnNext.IsEnabled = true;
            }
            // refresh list displayed
            Update_List();

            // reselecting images
            if (!relevantMode)
            {
                int indexRange = (currentPage - 1) * pageSize;
                if (picNo >= indexRange && picNo < indexRange + pageSize)
                    ImgListView.SelectedIndex = (picNo - indexRange);
            }

            else
            {
                foreach (int i in currentlySelected[currentPage - 1])
                {
                    ImgListView.SelectedItems.Add(ImgListView.Items[i]);
                }
            }
        }
    }
}
