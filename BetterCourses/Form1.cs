using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace BetterCourses
{
    public partial class Form1 : Form
    {

        const string subscriptionKey = "bbeacdccdcbc474787bee03d9e8096bf";
        const string uriBase =
         "https://westeurope.api.cognitive.microsoft.com/face/v1.0/detect";
        static string result = "";

        static string emotion1 = "";
        static string emotion2 = "";

        Timer timer = new Timer();

        public Form1()
        {
            InitializeComponent();

            timer.Enabled = true;
            timer.Tick += delegate
            {
                try
                {
                    textBox6.Text = emotion1;
                }
                catch { }
            };

            string imageFilePath = "";

            OpenFileDialog fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                imageFilePath = fileDialog.FileName;
            }

            pictureBox1.Image = new Bitmap(imageFilePath);

            MakeAnalysisRequest(imageFilePath);
        }

        static async void MakeAnalysisRequest(string imageFilePath)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", subscriptionKey);

            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false" +
                "&returnFaceAttributes=emotion";

            string uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                response = await client.PostAsync(uri, content);

                string contentString = await response.Content.ReadAsStringAsync();

                Console.WriteLine(JsonPrettyPrint(contentString));

                result = contentString;



                Person[] faces = JsonConvert.DeserializeObject<Person[]>(result);

                //Console.WriteLine(faces[0].faceId);

                string[] emotions = result.Split(new String[] { "\"emotion\":" }, StringSplitOptions.None)[1].Split('}')[0].Split(':');
                double max1 = 0.0;
                double max2 = 0.0;

                for (int i = 0; i < emotions.Length - 1; i++)
                {
                    Console.WriteLine(emotions[i]);
                    if (Convert.ToDouble(emotions[i + 1].Split(',')[0]) >= max1)
                    {
                        max2 = max1;
                        max1 = Convert.ToDouble(emotions[i + 1].Split(',')[0]);

                        emotion2 = emotion1;
                        emotion1 = emotions[i].Split('\"')[1];
                    }
                    else
                    {
                        if (Convert.ToDouble(emotions[i + 1].Split(',')[0]) >= max2)
                        {
                            max2 = Convert.ToDouble(emotions[i + 1].Split(',')[0]);

                            emotion2 = emotions[i].Split('\"')[1];
                        }
                    }
                }
            }
        }

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
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

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
