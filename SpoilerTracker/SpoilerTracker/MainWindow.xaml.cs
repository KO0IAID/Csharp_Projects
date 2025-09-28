using Microsoft.VisualBasic;
using Microsoft.Win32;
using SpoilerTracker;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TranslationLibrary.Emotracker.Controller;
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
        Spoiler spoiler = new Spoiler();
        EmoTracker emoTracker = new EmoTracker();

        #region Collections
        ObservableCollection<SeedInfo>? seedInfo;
        ObservableCollection<Setting>? gameSettings;
        ObservableCollection<Conditions>? specialConditions;
        ObservableCollection<Trick>? tricks;
        ObservableCollection<Glitch>? glitches;
        ObservableCollection<string>? junkLocations;
        ObservableCollection<WorldFlag>? worldFlags;
        ObservableCollection<Entrance>? entrances;
        ObservableCollection<WayOfTheHeroHint>? wayOfTheHeroHints;
        ObservableCollection<FoolishHint>? foolishHints;
        ObservableCollection<SpecificHint>? specificHints;
        ObservableCollection<RegionalHint>? regionalHints;
        ObservableCollection<FoolishRegion>? foolishRegions;
        ObservableCollection<WayOfTheHeroPath>? wayOfTheHeroPaths;
        ObservableCollection<Sphere>? spheres;
        ObservableCollection<ItemLocation>? locationList;
        #endregion


        public MainWindow()
        {
            InitializeComponent();
            AutoLoadSpoilerLog();
            ExportToEmotracker();
        }
        private async void SpoilerBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Title = "Select a file";
            ofd.Filter = "Text files (*.txt)| *.txt|All files (*.*)|*.*";
            var result = ofd.ShowDialog();

            if (result == true && ofd.FileName.Contains("Spoiler"))   
            { 
                await spoiler.AddFileContents(ofd.FileName);

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
            ofd.Filter = "EmoTracker file (*.json)| *.json|All files (*.*)|*.*";

            if      (ofd.ShowDialog() == true && ofd.FileName.Contains("EmoTracker"))  { TrackerPrompt.Text = "🗹"; }
            else if (!ofd.FileName.Contains("EmoTracker"))                             { TrackerPrompt.Text = "🗙"; }
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
                if (!spoiler.HasValue())
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
                    await spoiler.AddFileContents(recent,true);

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
        private async void AutoLoadSpoilerLog()
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
                    await spoiler.AddFileContents(recentFile, true);

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
        private void BindCollections()
        {
            // Binds the Lists from SpoilerLog to ObservableCollections for WPF use
            seedInfo            = new ObservableCollection<SeedInfo>            (spoiler.SeedInfo ?? []);
            gameSettings        = new ObservableCollection<Setting>             (spoiler.GameSettings ?? []);
            specialConditions   = new ObservableCollection<Conditions>          (spoiler.SpecialConditions ?? []);
            tricks              = new ObservableCollection<Trick>               (spoiler.Tricks ?? []);
            glitches            = new ObservableCollection<Glitch>              (spoiler.Glitches ?? []);
            junkLocations       = new ObservableCollection<string>              (spoiler.JunkLocations ?? []);
            worldFlags          = new ObservableCollection<WorldFlag>           (spoiler.WorldFlags ?? []);
            entrances           = new ObservableCollection<Entrance>            (spoiler.Entrances ?? []);
            wayOfTheHeroHints   = new ObservableCollection<WayOfTheHeroHint>    (spoiler.WayOfTheHeroHints ?? []);
            foolishHints        = new ObservableCollection<FoolishHint>         (spoiler.FoolishHints ?? []);
            specificHints       = new ObservableCollection<SpecificHint>        (spoiler.SpecificHints ?? []);
            regionalHints       = new ObservableCollection<RegionalHint>        (spoiler.RegionalHints ?? []);
            foolishRegions      = new ObservableCollection<FoolishRegion>       (spoiler.FoolishRegions ?? []);
            wayOfTheHeroPaths   = new ObservableCollection<WayOfTheHeroPath>    (spoiler.WayOfTheHeroPaths ?? []);
            spheres             = new ObservableCollection<Sphere>              (spoiler.Spheres ?? []);
            locationList        = new ObservableCollection<ItemLocation>        (spoiler.LocationList ?? []);


            // Binds to Xaml Elements
            SeedInfoDataGrid.ItemsSource = seedInfo;
            GameSettingsDataGrid.ItemsSource = gameSettings;
            SpecialConditionsListbox.ItemsSource = specialConditions;
            TricksDataGrid.ItemsSource = tricks;
            GlitchesDataGrid.ItemsSource = glitches;
            JunkLocationsListbox.ItemsSource = junkLocations;
            WorldFlagsDataGrid.ItemsSource = worldFlags;
            EntrancesDataGrid.ItemsSource = entrances;
            WayOfTheHeroDataGrid.ItemsSource = wayOfTheHeroHints;
            FoolishHintsDataGrid.ItemsSource = foolishHints;
            SpecificHintsDataGrid.ItemsSource = specificHints;
            RegionalHintsDataGrid.ItemsSource = regionalHints;
            FoolishRegionsDataGrid.ItemsSource = foolishRegions;
            WayOfTheHeroPathsDataGrid.ItemsSource = wayOfTheHeroPaths;
            SpheresDataGrid.ItemsSource= spheres;
            LocationsListDataGrid.ItemsSource = locationList;

            UpdateSortByDisplays();
            UpdateUIColumns();
        }
        private async void ExportToEmotracker()
        {
            await emoTracker.ImportTracker(null,true);
            await emoTracker.UpdateTracker(spoiler);
        }

        #region GameSettings
        private void GameSettings_Alphabetic_Click(object sender, RoutedEventArgs e) 
        {
            spoiler.SortCollections(SortBy.GameSettingsAlphabetic);
            BindCollections();
        }
        private void GameSettings_LogOrder_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.GameSettingsLogOrder);
            BindCollections();
        }
        #endregion
        #region Tricks
        private void Tricks_Alphabetic_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.TricksAlphabetic);
            BindCollections();
        }
        private void Tricks_Difficulty_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.TricksDifficulty);
            BindCollections();
        }
        private void Tricks_LogOrder_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.TricksLogOrder);
            BindCollections();
        }
        #endregion
        #region Glitches
        private void Glitches_Alphabetic_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.GlitchesAlphabetic);
            BindCollections();
        }
        private void Glitches_Difficulty_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.GlitchesDifficulty);
            BindCollections();
        }
        private void Glitches_LogOrder_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.GlitchesLogOrder);
            BindCollections();
        }
        #endregion
        #region Entrances
        private void Entrances_LongShort_Click(object sender, RoutedEventArgs e)
        {
            SortBy sortBy;

            switch (spoiler.Entrances_SortBy)
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

            spoiler.SortCollections(sortBy);
            EntrancesDataGrid.ItemsSource = spoiler.Entrances;
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
        private void Entrances_Alphabetic_Click(object sender, RoutedEventArgs e)
        {
            SortBy newSort;

            switch (spoiler.Entrances_SortBy)
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

            spoiler.SortCollections(newSort);

            BindCollections();

            
        }
        private void Entrances_Game_Click(object sender, RoutedEventArgs e)
        {
            SortBy sortBy;

            switch (spoiler.Entrances_SortBy)
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

            spoiler.SortCollections(sortBy);

            BindCollections();

            
        }
        private void Entrances_Reset_Click(object sender, EventArgs e)
        {

            EntranceColumn.Binding = new Binding("ShortEntrance");
            DestinationColumn.Binding = new Binding("ShortDestination");
            SwapEntranceStyleBtn.Content = "Short";
            

            spoiler.SortCollections(SortBy.EntrancesShort);
            BindCollections();

            
        }

        #endregion
        #region WayOfTheHero Hints
        private void WayOfTheHeroHints_World_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.WayOfTheHeroHintsWorld);

            BindCollections();
        }
        private void WayOfTheHeroHints_Location_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.WayOfTheHeroHintsLocation);

            BindCollections(); ;
        }
        private void WayOfTheHeroHints_Item_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.WayOfTheHeroHintsItems);

            BindCollections();
        }

        #endregion
        #region FoolishHints
        private void FoolishHints_World_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.FoolishHintsWorld);

            BindCollections();
        }
        private void FoolishHints_Gossip_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.FoolishHintsGossip);

            BindCollections();
        }
        private void FoolishHints_Location_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.FoolishHintsLocation);

            BindCollections();
        }
        #endregion
        #region SpecificHints
        private void SpecificHints_World_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.SpecificHintsWorld);

            BindCollections();
        }
        private void SpecificHints_Gossip_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.SpecificHintsGossip);

            BindCollections();
        }
        private void SpecificHints_Location_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.SpecificHintsLocation);

            BindCollections();
        }
        private void SpecificHints_Item_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.SpecificHintsItem);

            BindCollections();
        }
        #endregion
        #region RegionalHints
        private void RegionalHints_World_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.RegionalHintsWorld);

            BindCollections();
        }
        private void RegionalHints_Gossip_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.RegionalHintsGossip);

            BindCollections();
        }
        private void RegionalHints_Region_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.RegionalHintsRegion);

            BindCollections();
        }
        private void RegionalHints_Item_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.RegionalHintsItem);

            BindCollections();
        }
        #endregion
        #region FoolishRegions
        private void FoolishRegions_World_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.FoolishRegionsWorld);

            BindCollections();
        }
        private void FoolishRegions_Region_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.FoolishRegionsRegion);

            BindCollections();
        }
        private void FoolishRegions_Count_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.FoolishRegionsCount);

            BindCollections();
        }
        #endregion
        #region WayOfTheHero Paths
        private void WayOfTheHeroPaths_World_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.WayOfTheHeroPathsWorld);

            BindCollections();
        }
        private void WayOfTheHeroPaths_Description_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.WayOfTheHeroPathsDescription);

            BindCollections();
        }
        private void WayOfTheHeroPaths_Player_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.WayOfTheHeroPathsPlayer);

            BindCollections();
        }
        private void WayOfTheHeroPaths_Item_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.WayOfTheHeroPathsItem);

            BindCollections();
        }
        #endregion
        #region Spheres
        private void Spheres_World_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.SpheresWorld);

            BindCollections();
        }
        private void Spheres_Number_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.SpheresNumber);

            BindCollections();
        }
        private void Spheres_Type_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.SpheresType);

            BindCollections();
        }
        private void Spheres_Location_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.SpheresLocation);

            BindCollections();
        }
        private void Spheres_Player_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.SpheresPlayer);

            BindCollections();
        }
        private void Spheres_Item_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.SpheresItem);

            BindCollections();
        }
        #endregion
        #region LocationsList
        private void LocationsList_World_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.LocationsListWorld);

            BindCollections();
        }
        private void LocationsList_Game_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.LocationsListGame);

            BindCollections();
        }
        private void LocationsList_Region_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.LocationsListRegion);

            BindCollections();
        }
        private void LocationsList_Number_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.LocationsListNumber);

            BindCollections();
        }
        private void LocationsList_Count_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.LocationsListCount);

            BindCollections();
        }
        private void LocationsList_Description_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.LocationsListDescription);

            BindCollections();
        }
        private void LocationsList_Player_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.LocationsListPlayer);

            BindCollections();
        }
        private void LocationsList_Item_Click(object sender, RoutedEventArgs e)
        {
            spoiler.SortCollections(SortBy.LocationsListItem);

            BindCollections();
        }
        #endregion


        private void UpdateSortByDisplays()
        {
            EntrancesSortByDisplay.Text = spoiler.Entrances_SortBy.CustomToString();
            WayOfTheHeroSortByDisplay.Text = spoiler.WayOfTheHeroHints_SortBy.CustomToString();
            FoolishHintsSortByDisplay.Text = spoiler.FoolishHints_SortBy.CustomToString();
            SpecificHintsSortByDisplay.Text = spoiler.SpecificHints_SortBy.CustomToString();
            RegionalHintsSortByDisplay.Text = spoiler.RegionalHints_SortBy.CustomToString();
            FoolishRegionsSortByDisplay.Text = spoiler.FoolishRegions_SortBy.CustomToString();
            WayOfTheHeroPathsSortByDisplay.Text = spoiler.WayOfTheHeroPaths_SortBy.CustomToString();
            SpheresSortByDisplay.Text = spoiler.WayOfTheHeroPaths_SortBy.CustomToString();
            LocationsListSortByDisplay.Text = spoiler.WayOfTheHeroPaths_SortBy.CustomToString() ;
        }

        #region Conditional UI Column Hiding
        private void UpdateUIColumns()
        {
            HideNullColumns();
        }
        private void HideNullColumns()
        {
            // Entrances
            bool anyEntranceWorlds = spoiler.Entrances != null && spoiler.Entrances.Any(e => !string.IsNullOrWhiteSpace(e.World));
            EntranceWorldColumn.Visibility = anyEntranceWorlds ? Visibility.Visible : Visibility.Collapsed;

            // World Flags
            bool anyWorldFlagsWorlds = spoiler.WorldFlags != null && spoiler.WorldFlags.Any(e => !string.IsNullOrWhiteSpace(e.World));
            WorldFlagsWorldColumn.Visibility = anyWorldFlagsWorlds ? Visibility.Visible : Visibility.Collapsed;

            // WayOfTheHero Hints
            bool anyWayOfHeroWorlds = spoiler.WayOfTheHeroHints != null && spoiler.WayOfTheHeroHints.Any(e => !string.IsNullOrWhiteSpace(e.World));
            WayOfTheHeroWorldColumn.Visibility = anyWayOfHeroWorlds ? Visibility.Visible : Visibility.Collapsed;

            // Foolish Hints
            bool anyFoolishWorlds = spoiler.FoolishHints != null && spoiler.FoolishHints.Any(e => !string.IsNullOrWhiteSpace(e.World));
            FoolishHintsWorldColumn.Visibility = anyFoolishWorlds ? Visibility.Visible : Visibility.Collapsed;

            // Specific Hints
            bool anySpecificHintsWorlds = spoiler.SpecificHints != null && spoiler.SpecificHints.Any(e => !string.IsNullOrWhiteSpace(e.World));
            SpecificHintsWorldColumn.Visibility = anySpecificHintsWorlds ? Visibility.Visible : Visibility.Collapsed;

            // Regional Hints
            bool anyRegionalHintsWorlds = spoiler.RegionalHints != null && spoiler.RegionalHints.Any(e => !string.IsNullOrWhiteSpace(e.World));
            RegionalHintsWorldColumn.Visibility = anyRegionalHintsWorlds ? Visibility.Visible : Visibility.Collapsed;

            // Foolish Regions
            bool anyFoolishRegionsWorlds = spoiler.FoolishHints != null && spoiler.FoolishHints.Any(e => !string.IsNullOrWhiteSpace(e.World));
            FoolishRegionsWorldColumn.Visibility = anyFoolishRegionsWorlds ? Visibility.Visible : Visibility.Collapsed;

            // WayOfTheHero Paths
            bool anyWayOfTheHeroPathsWorlds = spoiler.WayOfTheHeroPaths != null && spoiler.WayOfTheHeroPaths.Any(e => !string.IsNullOrWhiteSpace(e.World));
            WayOfTheHeroPathsWorldColumn.Visibility = anyWayOfTheHeroPathsWorlds ? Visibility.Visible : Visibility.Collapsed;

            bool anyWayOfTheHeroPathsPlayers = spoiler.WayOfTheHeroPaths != null && spoiler.WayOfTheHeroPaths.Any(e => !string.IsNullOrWhiteSpace(e.Player));
            WayOfTheHeroPathsPlayerColumn.Visibility = anyWayOfTheHeroPathsPlayers ? Visibility.Visible : Visibility.Collapsed;

            // Spheres
            bool anySpheresWorlds = spoiler.Spheres != null && spoiler.Spheres.Any(e => !string.IsNullOrWhiteSpace(e.World));
            SpheresWorldColumn.Visibility = anyWayOfTheHeroPathsPlayers ? Visibility.Visible : Visibility.Collapsed;

            bool anySpheresPlayers= spoiler.Spheres != null && spoiler.Spheres.Any(e => !string.IsNullOrWhiteSpace(e.Player));
            SpheresPlayerColumn.Visibility = anySpheresPlayers ? Visibility.Visible : Visibility.Collapsed;

            // Locations List
            bool anyLocationsListWorlds = spoiler.LocationList != null && spoiler.LocationList.Any(e => !string.IsNullOrWhiteSpace(e.World));
            LocationsListWorldColumn.Visibility = anyWayOfTheHeroPathsPlayers ? Visibility.Visible : Visibility.Collapsed;

            bool anyLocationsListPlayers = spoiler.LocationList != null && spoiler.LocationList.Any(e => !string.IsNullOrWhiteSpace(e.Player));
            LocationsListPlayerColumn.Visibility = anySpheresPlayers ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion
    }
}