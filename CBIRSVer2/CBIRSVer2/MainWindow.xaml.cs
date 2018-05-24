/* -----------------------------------------------------------------------------
 * Author:              Kay Phan
 * Title:               CBIRS Version 2
 * Date Created:        1/24/2017
 * Date Last Revision:  2/1/2017
 * -----------------------------------------------------------------------------
 * This is the coding for the main window of the CBIRS. It backs the
 * MainWindow.xaml.
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
        // List of images to display in the UI
        private ObservableCollection<ImageSource> img_list = new
            ObservableCollection<ImageSource>();
        private List<String> img_names = new List<String>();
        private String regex_pattern = @"\\([\w ]+).(?:jpg|png)$";


        // Ordering Variables
        Dictionary<int, BitmapImage> img_map = new Dictionary<int,
            BitmapImage>();
        private int[] img_order = new int[100];

        // Arrays for files
        private int imageCount = 0;
        private double[,] intensityMatrix = new double[100, 25];
        private double[,] colorCodeMatrix = new double[100, 64];
        private int[] imageSize = new int[100];


        // only remake reading files if not true
        private bool hasFiles = true;
        private bool relevantMode = false;
        int picNo = 0;  // Selected image

        // varibles for the feature matrix
        private double[,] combiMatrix = new double[100, 89];
        private double[] sdMatrix = new double[89]; // standard dev.
        private double[] meanMatrix = new double[89]; // mean
        private double[] weightMatrix = new double[89]; // all weights

        // for page control and display
        private HashSet<int> selected = new HashSet<int>();
        private HashSet<int>[] currentlySelected = new HashSet<int>[5];
        private int currentPage = 1;
        private int noOfPages = 5;
        private int pageSize = 20;



       /* ----------------------------------------------------------------------
        * MainWindow
        * ----------------------------------------------------------------------
        * Initializes the main window component.
        */
        public MainWindow()
        {
            InitializeComponent();
            LoadImages();
        }

       /* ----------------------------------------------------------------------
        * LoadImages
        * ----------------------------------------------------------------------
        * Loads the images to from the image folder/database.
        * Constructs the colorcode, intensity, and combined feature matrices.
        */
        public void LoadImages()
        {
            readColorCodeFile();
            readIntensityFile();
            //Image current_image;
            //String img_path = @"C:\users\phank\documents\visual studio 2017\Projects\WPFTutorial\WPFTutorial\Resources\Images\";
            String img_path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            img_path = img_path + "\\Resources\\Images\\";
            List<String> filenames = new List<String>(System.IO.Directory.EnumerateFiles(img_path, "*.jpg"));
            int index = 0;
            BitmapImage image;
            foreach (String filename in filenames)
            {
                // put images in storage
                image = new BitmapImage(new Uri(filename));
                img_map.Add(index, image);
                // img_list.Add(image);
                img_order[index] = index;
                imageSize[index] = image.PixelHeight * image.PixelWidth;
                Match regex_match = Regex.Match(filename.Trim(), regex_pattern);
                String matched_img_name = regex_match.Groups[1].Value;
                this.img_names.Add(matched_img_name);

                // must add to matrix if no files
                if (!hasFiles)
                {
                    getMatrices(image);
                }

                index++;
                imageCount++;
            }
            getCombiMatrix();
            ImgListView.ItemsSource = img_list;
            Preview.Source = img_map[0];
            // initialize current page
            for (int i = 0; i < 5; i++)
                currentlySelected[i] = new HashSet<int>();
            displayPage();
        }
    }
}