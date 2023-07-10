using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace c_chat_manager
{
    public partial class Form1 : Form
    {
        char[,] table = { { '0', '1', '2', '3', '4', '5', '6', '7' },
                { '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' },
                { 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N' },
                { 'O' ,'P', 'Q' ,'R', 'S', 'T', 'U', 'V' },
                { 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd' },
                { 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l' },
                { 'm', 'n', 'o', 'p', 'q', 'r', 's', 't' },
                { 'u', 'v', 'w', 'x', 'y', 'z', '-', ' ' },
            };
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String targetUrl = textBox1.Text;
            HttpWebRequest request = HttpWebRequest.Create(targetUrl) as HttpWebRequest;
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 30000;
            string result = "";
            // 取得回應資料
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }
            result = RemoveHtmlTag(result);
            // 違規人
            string userId = GetUserId(result);
            // 違規項目(不輸入的話嘗試自動取)
            string type = GetBadSection(result);
            // 違規文章
            string badEssayCode = GetBadEssay(result);
            // 檢舉文章
            string reportEssayCode = "#" + getEssayCode(targetUrl) + " (C_ChatBM)";
            // 違規事證
            string evidence = GetEvidence(result);
            textBox2.Text += String.Format("───────────────────\r\n違規人  ：{0}" +
                "\r\n違規項目：{1} \r\n" +
                "違規文章：{2}\r\n" +
                "檢舉文章：{3}\r\n" +
                "罰則    ：水桶  日\r\n" +
                "違規事證：\r\n{4}\r\n───────────────────\r\n",
                userId, type, badEssayCode, reportEssayCode, evidence);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        // 網址解析
        private String getEssayCode(String url)
        {
            Regex reg = new Regex(@"ChatBM/M.{0,}.");
            string v1 = reg.Match(url).Groups[0].Value;
            string[] v2 = v1.Split('.');
            int n = Convert.ToInt32(v2[1]), m = Convert.ToInt32(v2[3], 16);
            return TransformByTable(Convert.ToString(n, 8)) + TransformByTable(Convert.ToString(m, 8));
        }
        // 獲取文章代碼 
        private String TransformByTable(string oct)
        {
            if (oct.Length % 2 != 0)
                oct = "0" + oct;
            string res = "";
            for (int i = 0; i < oct.Length; i += 2)
            {
                int x = oct[i] - '0';
                int y = oct[i + 1] - '0';
                res += table[x, y];
            }
            return res;
        }
        // 去除html標籤
        private string RemoveHtmlTag(string str)
        {
            str = Regex.Replace(str, "<!DOCTYPE html>[\\s\\S]{0,}(<a class=\"board\" href=\"/bbs/C_ChatBM/index.html\">返回看板</a>)", "");
            return Regex.Replace(str, "<\\/?.+?>", "");
        }

        // 獲得違規人
        private string GetUserId(string body)
        {
            Regex reg = new Regex(@"(?<=二、被檢舉人ID)[\s\S]{0,}(?=三、違規內容節錄)");
            return reg.Match(body).Groups[0].Value.Replace("請標註被檢舉人ID，否則恕不受理","").Trim();
        }
        // 獲得違規文章
        private string GetBadEssay(string body)
        {
            Regex reg = new Regex(@"(?<=一、違規文章代碼)[\s\S]{0,}(?=二、被檢舉人ID)");
            return reg.Match(body).Groups[0].Value.Replace("請貼上違規文章包含看板資訊的文章代碼，以便板主查找違規文章", "").Replace("推文違規請貼上推文所在之文章代碼", "").Trim();
        }
        // 獲得違規事證
        private string GetEvidence(string body)
        {
            Regex reg = new Regex(@"(?<=三、違規內容節錄)[\s\S]{0,}(?=四、違反板規條目)");
            string v1 = reg.Match(body).Groups[0].Value;
            return Regex.Replace(v1, "請節錄違規內容並貼於此處", " ").Trim();
        }
        // 獲得違規項目
        private string GetBadSection(string body)
        {
            Regex reg = new Regex(@"(?<=四、違反板規條目)[\s\S]{0,}(?=五、違規說明或佐證)");
            string v1 = reg.Match(body).Groups[0].Value;
            return Regex.Replace(v1, "請標註違反之板規條目於此處", " ").Trim();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, textBox2.Text);
        }

    }
}
