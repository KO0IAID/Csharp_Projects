using Microsoft.VisualBasic;
using Microsoft.Win32;
using SpoilerTracker;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TranslationLibrary.Emotracker.ItemDatabase;
using TranslationLibrary.Emotracker.LocationDatabase;
using TranslationLibrary.SpoilerLog.Controller;
using TranslationLibrary.SpoilerLog.Enumerators;
using TranslationLibrary.SpoilerLog.Models;


namespace SpoilerTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Spoiler Log 
        SpoilerLog spoilerLog = new SpoilerLog();

        
        public MainWindow()
        {
            InitializeComponent();
            AutoLoadSpoilerLog();
        }

        private async void SpoilerBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Title = "Select a file";
            ofd.Filter = "Text files (*.txt)| *.txt|All files (*.*)|*.*";
            var result = ofd.ShowDialog();

            if (result == true && ofd.FileName.Contains("Spoiler"))   
            { 
                await spoilerLog.AddFileContents(ofd.FileName);

                SpoilerLogPrompt.Text = "🗹";
                CurrentHeader.Visibility = Visibility.Visible;
                FileNamePrompt.Text = System.IO.Path.GetFileName(ofd.FileName);
                FileDatePrompt.Text = System.IO.File.GetLastWriteTime(ofd.FileName).ToString("g");

            }
            else if (result == false)
            {
                // Canceled out of OFD

            }
            else if (!ofd.FileName.Contains("Spoiler"))
            {
                SpoilerLogPrompt.Text = "🗙"; 
                FileNamePrompt.Text = "Not a spoiler log";
            }


                BindCollections();
                

        }
        private void TrackerBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Title = "Select a file";
            ofd.Filter = "Json file (*.json)| *.json|All files (*.*)|*.*";

            if      (ofd.ShowDialog() == true && ofd.FileName.Contains("Tracker"))  { TrackerPrompt.Text = "🗹"; }
            else if (!ofd.FileName.Contains("Tracker"))                             { TrackerPrompt.Text = "🗙"; }
        }
        

        #region GameSettings
        private async void GameSettings_Alphabetic_Click(object sender, RoutedEventArgs e) 
        { 
            await spoilerLog.SortCollections(SortBy.GameSettingsAlphabetic);
            GameSettingsDataGrid.ItemsSource = spoilerLog.GameSettings;


        }
        private async void GameSettings_LogOrder_Click(object sender, RoutedEventArgs e)
        {
            await spoilerLog.SortCollections(SortBy.GameSettingsLogOrder);
            GameSettingsDataGrid.ItemsSource = spoilerLog.GameSettings;
        }
        #endregion
        #region Tricks
        private async void Tricks_Alphabetic_Click(object sender, RoutedEventArgs e)
        {
            await spoilerLog.SortCollections(SortBy.TricksAlphabetic);
            TricksDataGrid.ItemsSource = spoilerLog.Tricks;
        }
        private async void Tricks_Difficulty_Click(object sender, RoutedEventArgs e)
        {
            await spoilerLog.SortCollections(SortBy.TricksDifficulty);
            TricksDataGrid.ItemsSource = spoilerLog.Tricks;
        }
        private async void Tricks_LogOrder_Click(object sender, RoutedEventArgs e)
        {
            await spoilerLog.SortCollections(SortBy.TricksLogOrder);
            TricksDataGrid.ItemsSource = spoilerLog.Tricks;
        }
        #endregion
        #region Entrances
        private async void Entrances_LongShort_Click(object sender, RoutedEventArgs e)
        {
            SortBy sortBy;

            switch (spoilerLog.Entrances_SortBy)
            {
                #region Long & Short

                // Long to Short
                case SortBy.EntrancesLong:
                    sortBy = SortBy.EntrancesShort;
                    break;

                // Short to Long
                case SortBy.EntrancesShort:
                    sortBy = SortBy.EntrancesLong;
                    break;

                #endregion

                #region Alphabetic

                // Short Alphabetic to Long Alphabetic
                case SortBy.EntrancesShortAlphabetic:
                    sortBy = SortBy.EntrancesLongAlphabetic;
                    break;

                // Long Alphabetic to Short Alphabetic
                case SortBy.EntrancesLongAlphabetic:
                    sortBy = SortBy.EntrancesShortAlphabetic;
                    break;

                #endregion

                #region Reverse Alphabetic

                // Short Reverse Alphabetic to Long Reverse Alphabetic
                case SortBy.EntrancesShortReverseAlphabetic:
                    sortBy = SortBy.EntrancesLongReverseAlphabetic;
                    break;

                // Long Reverse Alphabetic to Short ReverseAlphabetic
                case SortBy.EntrancesLongReverseAlphabetic:
                    sortBy = SortBy.EntrancesShortReverseAlphabetic;
                    break;

                #endregion

                #region Game

                // Short Game to Long Game
                case SortBy.EntrancesShortGame:
                    sortBy = SortBy.EntrancesLongGame;
                    break;

                // Long Game to Short Game
                case SortBy.EntrancesLongGame:
                    sortBy = SortBy.EntrancesShortGame;
                    break;

                #endregion Game

                #region Reverse Game

                // Short Reverse Game to Long Reverse Game
                case SortBy.EntrancesShortReverseGame:
                    sortBy = SortBy.EntrancesLongReverseGame;
                    break;

                // Long Reverse Game to Short Reverse Game
                case SortBy.EntrancesLongReverseGame:
                    sortBy = SortBy.EntrancesShortReverseGame;
                    break;

                #endregion

                // Fallback - Default
                default:
                    sortBy = SortBy.EntrancesShort;
                    break;
            }

            await spoilerLog.SortCollections(sortBy);
            EntrancesDataGrid.ItemsSource = spoilerLog.Entrances;
            UpdateSortByDisplays();

            if (EntranceColumn.Binding is Binding binding && binding.Path?.Path == "ShortEntrance")
            {
                EntranceColumn.Binding = new Binding("LongEntrance");
                DestinationColumn.Binding = new Binding("LongDestination");
                SwapEntranceStyleBtn.Content = "Long";
            }
            else
            {
                EntranceColumn.Binding = new Binding("ShortEntrance");
                DestinationColumn.Binding = new Binding("ShortDestination");
                SwapEntranceStyleBtn.Content = "Short";
            }
            
        }
        private async void Entrances_Alphabetic_Click(object sender, RoutedEventArgs e)
        {
            SortBy newSort;

            switch (spoilerLog.Entrances_SortBy)
            {
                // Short to ShortAlphabetic
                case SortBy.EntrancesShort:
                    newSort = SortBy.EntrancesShortAlphabetic;
                    break;

                // Long to LongAlphabetic
                case SortBy.EntrancesLong:
                    newSort = SortBy.EntrancesLongAlphabetic;
                    break;

                // Short Alphabetic to Short Reverse Alphabetic
                case SortBy.EntrancesShortAlphabetic:
                    newSort = SortBy.EntrancesShortReverseAlphabetic;
                    break;

                // Short Reverse Alphabetic to Short Alphabetic
                case SortBy.EntrancesShortReverseAlphabetic:
                    newSort = SortBy.EntrancesShortAlphabetic;
                    break;

                // Long Alphabetic to Long Reverse Alphabetic
                case SortBy.EntrancesLongAlphabetic:
                    newSort = SortBy.EntrancesLongReverseAlphabetic;
                    break;

                // Long Reverse Alphabetic to Long Alphabetic
                case SortBy.EntrancesLongReverseAlphabetic:
                    newSort = SortBy.EntrancesLongAlphabetic;
                    break;

                // Fallback to default
                default:
                    newSort = SortBy.EntrancesShortAlphabetic; 
                    break;
            }

            await spoilerLog.SortCollections(newSort);

            EntrancesDataGrid.ItemsSource = spoilerLog.Entrances;

            UpdateSortByDisplays();
        }
        private async void Entrances_Game_Click(object sender, RoutedEventArgs e)
        {
            SortBy sortBy;

            switch (spoilerLog.Entrances_SortBy)
            {
                #region Short/Long to Game

                // Short to Short Game
                case SortBy.EntrancesShort:
                    sortBy = SortBy.EntrancesShortGame;
                    break;

                // Long to Long Game
                case SortBy.EntrancesLong:
                    sortBy = SortBy.EntrancesLongGame;
                    break;
                #endregion

                #region Alphabetic Short/Long to Game

                // Short Alphabetic to Short Game
                case SortBy.EntrancesShortAlphabetic:
                    sortBy = SortBy.EntrancesShortGame;
                    break;

                // Long Alphabetic to Long Game
                case SortBy.EntrancesLongAlphabetic:
                    sortBy = SortBy.EntrancesLongGame;
                    break;

                #endregion

                #region Reverse Alphabetic Short/Long to Game

                // Short Reverse Alphabeticto Short Game
                case SortBy.EntrancesShortReverseAlphabetic:
                    sortBy = SortBy.EntrancesShortReverseGame;
                    break;

                // Long Reverse Alphabeticto Short Game
                case SortBy.EntrancesLongReverseAlphabetic:
                    sortBy = SortBy.EntrancesLongReverseGame;
                    break;

                #endregion

                #region Game to Reverse Game

                // Short Game to Reverse Short Game
                case SortBy.EntrancesShortGame:
                    sortBy = SortBy.EntrancesShortReverseGame;
                    break;

                // Long Game to Reverse Long Game
                case SortBy.EntrancesLongGame:
                    sortBy = SortBy.EntrancesLongReverseGame;
                    break;

                #endregion

                #region Reverse Game to Game

                // Short Game to Reverse Short Game
                case SortBy.EntrancesShortReverseGame:
                    sortBy = SortBy.EntrancesShortGame;
                    break;

                // Long Game to Reverse Long Game
                case SortBy.EntrancesLongReverseGame:
                    sortBy = SortBy.EntrancesLongGame;
                    break;

                #endregion

                default:
                    sortBy = SortBy.EntrancesShortGame;
                    break;
            }

            await spoilerLog.SortCollections(sortBy);

            EntrancesDataGrid.ItemsSource = spoilerLog.Entrances;

            UpdateSortByDisplays();
        }
        private async void Entrances_Reset_Click(object sender, EventArgs e)
        {

                EntranceColumn.Binding = new Binding("ShortEntrance");
                DestinationColumn.Binding = new Binding("ShortDestination");
                SwapEntranceStyleBtn.Content = "Short";
            

            await spoilerLog.SortCollections(SortBy.EntrancesShort);
            EntrancesDataGrid.ItemsSource = spoilerLog.Entrances;

            UpdateSortByDisplays();
        }

        #endregion

        private async void BindCollections()
        {
            SeedInfoDataGrid.ItemsSource = spoilerLog.SeedInfo;
            GameSettingsDataGrid.ItemsSource = spoilerLog.GameSettings;
            SpecialConditionsListbox.ItemsSource = spoilerLog.SpecialConditions;
            TricksDataGrid.ItemsSource = spoilerLog.Tricks;
            JunkLocationsListbox.ItemsSource= spoilerLog.JunkLocations;
            WorldFlagsDataGrid.ItemsSource = spoilerLog.WorldFlags;
            EntrancesDataGrid.ItemsSource = spoilerLog.Entrances;
            WayOfTheHeroDataGrid.ItemsSource = spoilerLog.WayOfTheHeroHints;

            UpdateSortByDisplays();
            UpdateUIColumns();
        }

        


        private async void AutoLoadSpoilerCheckBox_Click(object sender, RoutedEventArgs e)
        {
            // Disabled Logic
            if (AutoLoadSpoilerCheckBox.IsChecked == false)
            {
                // Clear the saved folder path when unchecked
                SettingsManager.ClearFolderPath();
                FolderPathPrompt.Text = "";

                // Reset UI elements
                if (!spoilerLog.HasValue())
                {
                    CurrentHeader.Visibility = Visibility.Hidden;
                    FileNamePrompt.Text = "";
                    FileDatePrompt.Text = string.Empty;
                    SpoilerLogPrompt.Text = string.Empty;
                }

                return;
            }

            // Enabled Logic
            var dialog = new OpenFolderDialog();

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.FolderName))
            {
                // Save it
                SettingsManager.SaveFolderPath(dialog.FolderName);

                // Load most recent file
                string? recent = SettingsManager.GetMostRecentSpoilerLogFullPath(dialog.FolderName);
                if (recent != null)
                {
                    await spoilerLog.AddFileContents(recent);

                    SpoilerLogPrompt.Text = "🗹";
                    FolderPathPrompt.Text = SettingsManager.LoadFolderPath();

                    CurrentHeader.Visibility = Visibility.Visible;
                    FileNamePrompt.Text = System.IO.Path.GetFileName(recent);
                    FileDatePrompt.Text = System.IO.File.GetLastWriteTime(recent).ToString("g");

                    BindCollections();
                }
                else
                {
                    // Handle the case when no matching file is found
                    FolderPathPrompt.Text = SettingsManager.LoadFolderPath();
                    SpoilerLogPrompt.Text = "🗙";

                    CurrentHeader.Visibility = Visibility.Hidden;
                    FileNamePrompt.Text = "No Spoiler logs found";
                    FileDatePrompt.Text = string.Empty;
                }
            }
        }
        private async Task AutoLoadSpoilerLog()
        {
            // Load the saved folder path from the settings
            string? savedFolder = SettingsManager.LoadFolderPath();

            if (!string.IsNullOrEmpty(savedFolder))
            {
                // If a folder path is set, keep the checkbox checked
                AutoLoadSpoilerCheckBox.IsChecked = true;

                // Get the most recent spoiler log file from the folder
                string? recentFile = SettingsManager.GetMostRecentSpoilerLogFullPath(savedFolder);

                if (!string.IsNullOrEmpty(recentFile))
                {
                    // Load the file contents
                    await spoilerLog.AddFileContents(recentFile);

                    // Update UI elements
                    FolderPathPrompt.Text = SettingsManager.LoadFolderPath();
                    SpoilerLogPrompt.Text = "🗹";

                    CurrentHeader.Visibility = Visibility.Visible;
                    FileNamePrompt.Text = System.IO.Path.GetFileName(recentFile);
                    FileDatePrompt.Text = System.IO.File.GetLastWriteTime(recentFile).ToString("g");

                    BindCollections();
                }
                else
                {
                    // No recent file found
                    FileNamePrompt.Text = "No spoiler log in folder.";
                }
            }
            else
            {
                // Folder path not found or invalid
                AutoLoadSpoilerCheckBox.IsChecked = false; // Uncheck the checkbox if no folder path is set
                CurrentHeader.Visibility = Visibility.Hidden;
                FileNamePrompt.Text = "";
            }
        }


        private void UpdateSortByDisplays()
        {
            EntrancesSortByDisplay.Text = spoilerLog.Entrances_SortBy.CustomToString();
        }

        #region Conditional UI Column Hiding
        private void UpdateUIColumns()
        {
            HideNullColumns();
        }
        private void HideNullColumns()
        {
            bool anyEntranceWorlds = spoilerLog.Entrances.Any(e => !string.IsNullOrWhiteSpace(e.World));
            EntranceWorldColumn.Visibility = anyEntranceWorlds ? Visibility.Visible : Visibility.Collapsed;

            bool anyWorldFlagsWorlds = spoilerLog.WorldFlags.Any(e => !string.IsNullOrWhiteSpace(e.World));
            WorldFlagsWorldColumn.Visibility = anyWorldFlagsWorlds ? Visibility.Visible : Visibility.Collapsed;

            bool anyWayOfHeroWorlds = spoilerLog.WayOfTheHeroHints.Any(e => !string.IsNullOrWhiteSpace(e.World));
            WayOfTheHeroWorldColumn.Visibility = anyWayOfHeroWorlds ? Visibility.Visible : Visibility.Collapsed;

        }






        #endregion

        private void EntrancesSortGameBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}