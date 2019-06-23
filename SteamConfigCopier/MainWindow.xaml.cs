using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
using System.Windows.Forms;
using Newtonsoft.Json;
using MessageBox = System.Windows.MessageBox;

namespace SteamConfigCopier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static string configLocation = "config.json";
        private static string gameID = "570";
        private Dictionary<string, string> directories = new Dictionary<string, string>();

        private static string path = "";
        private static string sourceID = "";
        private static string destID = "";

        public MainWindow()
        {
            InitializeComponent();
        }


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ReadConfig(configLocation);
        }

        private void PopulateLists()
        {
            string[] directoryList;
            directoryList = Directory.GetDirectories(path);

            directories.Clear();
            SourceList.Items.Clear();
            DestinationList.Items.Clear();
            foreach (string directory in directoryList){

                directories.Add(System.IO.Path.GetFileNameWithoutExtension(directory), directory);
                SourceList.Items.Add(System.IO.Path.GetFileNameWithoutExtension(directory));
                DestinationList.Items.Add(System.IO.Path.GetFileNameWithoutExtension(directory));
            }
        }

        private void UserDataSelect(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                
                path = folderDialog.SelectedPath;
                UserDataBox.Text = path;
                PopulateLists();
            }
        }

        private void Transfer(object sender, RoutedEventArgs e)
        {
            if(SourceList.SelectedItem != null && DestinationList.SelectedItem != null)
            {
                string sourcePath = "";
                string destinationPath = "";
                string sourceGamePath = "";
                string destinationGamePath = "";

                sourceID = SourceList.SelectedItem.ToString();
                destID = DestinationList.SelectedItem.ToString();

                directories.TryGetValue(sourceID, out sourcePath);
                directories.TryGetValue(destID, out destinationPath);

                destinationGamePath = System.IO.Path.Combine(destinationPath, gameID);
                sourceGamePath = System.IO.Path.Combine(sourcePath, gameID);

                if (Directory.Exists(destinationGamePath))
                {
                    Directory.Delete(destinationGamePath, true);
                }
                Copy(sourceGamePath, destinationGamePath);
                MessageBox.Show("Successfully copied files.");
            }
            else
            {
                MessageBox.Show("You must select a source and destination userID");
            }
            
        }

        /// <summary>
        /// copy directory
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="targetDirectory"></param>
        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        /// <summary>
        /// copy all subdirectories
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // create a directory
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(System.IO.Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        /// <summary>
        /// retrieve configuration file
        /// </summary>
        /// <param name="filelocation"></param>
        private void RetrieveConfig(string filelocation)
        {
            // retrieve the config file and load the original source and destination ids
            string jsonString;
            using (StreamReader streamReader = new StreamReader(filelocation, Encoding.UTF8))
            {
                jsonString = streamReader.ReadToEnd();
            }
            Config configFromJson = JsonConvert.DeserializeObject<Config>(jsonString);

            path = configFromJson.Path;
            sourceID = configFromJson.sourceID;
            destID = configFromJson.destID;
        }

        /// <summary>
        /// Read config file
        /// </summary>
        /// <param name="filelocation"></param>
        public void ReadConfig(string filelocation)
        {
            // read the config file
            if (File.Exists(configLocation))
            {
                RetrieveConfig(filelocation);
                UserDataBox.Text = path;
                PopulateLists();
                if (directories.ContainsKey(sourceID))
                {
                    SourceList.SelectedValue = sourceID.ToString();
                }
                if (directories.ContainsKey(destID))
                {
                    DestinationList.SelectedValue = destID.ToString();
                }
                
            }
        }

        /// <summary>
        /// save configuration file
        /// </summary>
        /// <param name="filelocation"></param>
        public void SaveConfig(string filelocation)
        {
            // save the config to file
            path = UserDataBox.Text;
            if (SourceList.SelectedItem != null)
            {
                sourceID = SourceList.SelectedItem.ToString();
            }

            if (DestinationList.SelectedItem != null)
            {
                destID = DestinationList.SelectedItem.ToString();
            }
            

            Config config = new Config()
            {
                Path = path,
                sourceID = sourceID,
                destID = destID
            };
            var jsonToWrite = JsonConvert.SerializeObject(config, Formatting.Indented);

            using (var writer = new StreamWriter(filelocation))
            {
                writer.Write(jsonToWrite);
            }
        }

        private void ConfigButton_OnClick(object sender, RoutedEventArgs e)
        {
            SaveConfig(configLocation);
        }

    }

    public class Config
    {
        public string Path { get; set; }
        public string sourceID { get; set; }
        public string destID { get; set; }
    }
}
