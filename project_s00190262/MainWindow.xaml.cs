using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace project_s00190262
{
    public partial class MainWindow : Window
    {
        private List<string> selectedFiles;
        private ConcurrentQueue<WordInfo> wordQueue = new ConcurrentQueue<WordInfo>();
        private volatile bool continueSpellChecking = true;
        private ManualResetEvent allWordsProcessed = new ManualResetEvent(false);
        public MainWindow()
        {
            InitializeComponent();
            selectedFiles = new List<string>();

            // Check user color preference from isolated storage
            string colorPreference = ReadColorPreferenceFromIsolatedStorage();

            // Apply color preference to the UI
            SetTheme(colorPreference);
        }

        private string ReadColorPreferenceFromIsolatedStorage()
        {
            string colorPreference = "light"; // Default color preference

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                if (storage.FileExists("colorPreference.txt"))
                {
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("colorPreference.txt", FileMode.Open, storage))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            colorPreference = reader.ReadToEnd();
                        }
                    }
                }
            }

            return colorPreference;
        }

        private void SaveColorPreferenceToIsolatedStorage(string colorPreference)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream("colorPreference.txt", FileMode.Create, storage))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(colorPreference);
                    }
                }
            }
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                string theme = radioButton.Content.ToString().ToLower();
                SetTheme(theme);
                SaveColorPreferenceToIsolatedStorage(theme);
            }
        }
        private void SetTheme(string theme)
        {
            if (theme == "dark")
            {
                // Set the main window background and button colors for dark theme
                var darkBrush = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                var lightBrush = new SolidColorBrush(Colors.White);
                var accentBrush = new SolidColorBrush(Colors.Red);

                this.Background = darkBrush;
                buttonSelectFiles.Background = lightBrush;
                buttonSelectFiles.Foreground = darkBrush;
                buttonStartCheck.Background = lightBrush;
                buttonStartCheck.Foreground = darkBrush;
                buttonReplaceWords.Background = lightBrush;
                buttonReplaceWords.Foreground = darkBrush;
                listBoxMisspelledWords.Background = lightBrush;
                listBoxMisspelledWords.Foreground = darkBrush;
                listBoxCorrectSpelling.Background = lightBrush;
                listBoxCorrectSpelling.Foreground = darkBrush;
                radioButtonDark.Foreground = lightBrush;
                radioButtonLight.Foreground = lightBrush;

            }
            else if (theme == "light")
            {
                // Set the main window background and button colors for light theme
                var lightBrush = new SolidColorBrush(Colors.White);
                var darkBrush = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                var accentBrush = new SolidColorBrush(Colors.Blue);

                this.Background = lightBrush;
                buttonSelectFiles.Background = darkBrush;
                buttonSelectFiles.Foreground = lightBrush;
                buttonStartCheck.Background = darkBrush;
                buttonStartCheck.Foreground = lightBrush;
                buttonReplaceWords.Background = darkBrush;
                buttonReplaceWords.Foreground = lightBrush;
                listBoxMisspelledWords.Background = darkBrush;
                listBoxMisspelledWords.Foreground = lightBrush;
                listBoxCorrectSpelling.Background = darkBrush;
                listBoxCorrectSpelling.Foreground = lightBrush;
                radioButtonDark.Foreground = darkBrush;
                radioButtonLight.Foreground = darkBrush;
            }
        }

        private void buttonSelectFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFiles.AddRange(openFileDialog.FileNames);
            }
        }

        private void buttonStartCheck_Click(object sender, RoutedEventArgs e)
        {
            allWordsProcessed.Reset();

            Dictionary<string, string> fileContents = new Dictionary<string, string>();

            // 1. Read words from files and add them to queue
            foreach (var filePath in selectedFiles)
            {
                string fileContent = File.ReadAllText(filePath);
                fileContents.Add(filePath, fileContent);

                string[] words = fileContent.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    wordQueue.Enqueue(new WordInfo(filePath, word));
                }
            }

            // No more words will be added to the queue
            continueSpellChecking = false;

            // 2. Start the spell-checking threads
            int numThreads = 3; 
            for (int i = 0; i < numThreads; i++)
            {
                StartSpellCheckingThread(i, fileContents);
            }
        }

        private void StartSpellCheckingThread(int threadNumber, Dictionary<string, string> fileContents)
        {
            new Thread(async () =>
            {
                Thread.CurrentThread.Name = "Spell Check Thread " + (threadNumber + 1);
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
                Thread.CurrentThread.IsBackground = true;

                while (continueSpellChecking || !wordQueue.IsEmpty)
                {
                    if (wordQueue.TryDequeue(out WordInfo wordInfo))
                    {
                        string filePath = wordInfo.FilePath;
                        string word = wordInfo.Word;

                        string correctSpelling = await GetCorrectSpelling(word);
                        if (correctSpelling != word)
                        {
                            // Handle misspelled word
                            this.Dispatcher.Invoke(() =>
                            {
                                listBoxMisspelledWords.Items.Add(word);
                                listBoxCorrectSpelling.Items.Add(correctSpelling);
                            });
                        }
                        else
                        {
                            // Handle correctly spelled word
                            this.Dispatcher.Invoke(() =>
                            {
                                listBoxCorrectSpelling.Items.Add(word);
                            });

                            // Update the file with the correct spelling
                            if (fileContents.TryGetValue(filePath, out string fileContent))
                            {
                                fileContent = fileContent.Replace(word, correctSpelling);
                                fileContents[filePath] = fileContent;
                                File.WriteAllText(filePath, fileContent);
                            }
                        }

                        await Task.Delay(1000); // Add a delay between each word
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }

                // If all threads are done, set the ManualResetEvent
                if (wordQueue.IsEmpty)
                {
                    allWordsProcessed.Set();
                }

            }).Start();
        }

        private async Task<string> GetCorrectSpelling(string word)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "adc13a7b49bb431c98af083085f71031");
                var values = new Dictionary<string, string>
                {
                    { "text", word }
                };
                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync("https://api.bing.microsoft.com/v7.0/SpellCheck/?mode=proof&mkt=en-US", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<BingSpellCheckResponse>(await response.Content.ReadAsStringAsync());
                    return result.FlaggedTokens.FirstOrDefault()?.Suggestions
                        .FirstOrDefault()?.suggestion ?? word;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception("Spell check API request failed with status code: " + response.StatusCode + ", Error: " + errorContent);
                }
            }
        }
        private void buttonReplaceWords_Click(object sender, RoutedEventArgs e)
        {
            List<WordInfo> replacedWords = new List<WordInfo>();

            foreach (var selectedItem in listBoxMisspelledWords.SelectedItems)
            {
                string incorrectWord = selectedItem as string;
                int selectedIndex = listBoxMisspelledWords.Items.IndexOf(selectedItem);

                if (selectedIndex >= 0 && selectedIndex < listBoxCorrectSpelling.Items.Count)
                {
                    string correctWord = listBoxCorrectSpelling.Items[selectedIndex] as string;
                    replacedWords.Add(new WordInfo(null, incorrectWord)
                    {
                        Replacement = correctWord
                    });
                }
            }

            ReplaceWordsInFiles(replacedWords);
        }



        private void ReplaceWordsInFiles(List<WordInfo> replacedWords)
        {
            foreach (var filePath in selectedFiles)
            {
                string fileContent = File.ReadAllText(filePath);
                foreach (var wordInfo in replacedWords)
                {
                    fileContent = fileContent.Replace(wordInfo.Word, wordInfo.Replacement);
                }
                File.WriteAllText(filePath, fileContent);
            }
        }
    }

    public class WordInfo
    {
        public string FilePath { get; }
        public string Word { get; }
        public string Replacement { get; set; }

        public WordInfo(string filePath, string word)
        {
            FilePath = filePath;
            Word = word;
        }
    }

    public class BingSpellCheckResponse
    {
        public List<FlaggedToken> FlaggedTokens { get; set; }
    }

    public class FlaggedToken
    {
        public string Token { get; set; }
        public List<Suggestion> Suggestions { get; set; }
    }

    public class Suggestion
    {
        public string suggestion { get; set; }
    }



}
