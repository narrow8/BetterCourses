﻿using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.CognitiveServices.Speech;
using System.Threading;
using Emgu.CV;

namespace BetterCourses
{
    public partial class Form1 : Form
    {

        const string subscriptionKey = "bbeacdccdcbc474787bee03d9e8096bf";
        const string uriBase =
         "https://westeurope.api.cognitive.microsoft.com/face/v1.0/detect";

        static string result = "";
        static bool speaking = false;

        static int time = 0;

        string[] photos = new string[200];

        const int interval = 30;
        static bool opened = false;

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        public Form1()
        {
            InitializeComponent();

            timer.Interval = 1000;

            timer.Tick += delegate
            {
                time++;

                if (time % interval == 0)
                {
                    VideoCapture capture = new VideoCapture(); //create a camera capture
                    Bitmap image = capture.QueryFrame().Bitmap; //take a picture
                    image.Save("cc0.bmp");
                    pictureBox1.Image = image;

                    MakeAnalysisRequest("cc0.bmp");
                }

                try
                {
                    if (!speaking)
                        MakeTranscript();
                }
                catch { }
            };
        }

        async void MakeTranscript()
        {
            speaking = true;

            var config = SpeechConfig.FromSubscription("0be2c48d5bf14f51b98a74bcc5e385bf"
            , "westeurope");

            using (var recognizer = new SpeechRecognizer(config))
            {
                // Starts recognizing.
                Console.WriteLine("Say something...");

                var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                if (textBox1.InvokeRequired)
                    textBox1.Invoke(new Action(() => textBox1.Text += result.Text + Environment.NewLine));


                // Checks result.
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"RECOGNIZED: Text={result.Text}");
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
            speaking = false;
        }

        async void MakeAnalysisRequest(string imageFilePath)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", subscriptionKey);

            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false" +
                "&returnFaceAttributes=emotion";

            string uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(imageFilePath);

            try
            {
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    response = await client.PostAsync(uri, content);

                    string contentString = await response.Content.ReadAsStringAsync();

                    Console.WriteLine(JsonPrettyPrint(contentString));

                    result = contentString;

                    Person[] students = JsonConvert.DeserializeObject<Person[]>(result);

                    Class room = new Class(students);

                    if (textBox1.InvokeRequired)
                    {
                        textBox1.Invoke(new Action(() => textBox1.Text += "Attention rate: "
                                                                 + (room.getFocus() * 100).ToString()
                                                                 + Environment.NewLine));
                    }
                    else
                        textBox1.Text += "Attention rate: " + (room.getFocus() * 100).ToString() + Environment.NewLine;
                }

            }
            catch { }
        }

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            try
            {
                using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
                {
                    BinaryReader binaryReader = new BinaryReader(fileStream);
                    return binaryReader.ReadBytes((int)fileStream.Length);
                }
            }
            catch { return null; }
        }

        private static void SaveToFile(string List)
        {
            var saveFileDialog1 = new SaveFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                Filter = string.Format("{0}Text files (*.txt)|*.txt|All files (*.*)|*.*", "ARG0"),
                RestoreDirectory = true,
                ShowHelp = true,
                CheckFileExists = false
            };
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                File.WriteAllText(saveFileDialog1.FileName, List);
        }

        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ////
            //var fbd = new FolderBrowserDialog();
           // DialogResult result = fbd.ShowDialog();

            //if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            //{
            //    DirectoryInfo d = new DirectoryInfo(fbd.SelectedPath);//Assuming Test is your Folder
            //    FileInfo[] Files = d.GetFiles("*.jpg"); //Getting Text files

            //    int k = 1;
            //    foreach (FileInfo file in Files)
            //    {
            //        photos[k++] = file.FullName;
            //    }
            //}


            /////

            /*
            string imageFilePath = "";

            OpenFileDialog fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                imageFilePath = fileDialog.FileName;
            }

            pictureBox1.Image = new Bitmap(imageFilePath);

            MakeAnalysisRequest(imageFilePath);
            */
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "Start")
            {



                //var fbd = new FolderBrowserDialog();
                //DialogResult result = fbd.ShowDialog();

                //if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                //{
                //    //photos = Directory.GetFiles(fbd.SelectedPath);

                //    DirectoryInfo d = new DirectoryInfo(fbd.SelectedPath);//Assuming Test is your Folder
                //    FileInfo[] Files = d.GetFiles("*.jpg"); //Getting Text files

                //    int k = 1;
                //    foreach (FileInfo file in Files)
                //    {
                //        photos[k++] = file.FullName;
                //    }

                //}

                VideoCapture capture = new VideoCapture(); //create a camera capture
                Bitmap image = capture.QueryFrame().Bitmap; //take a picture
                image.Save("cc0.bmp");

                pictureBox1.Image = image;

                opened = true;
                button2.Text = "Stop";
                timer.Enabled = true;

            }

            else
            {
                SaveToFile(textBox1.Text);
                timer.Enabled = false;
                button2.Text = "Start";
                textBox1.Text = "";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
