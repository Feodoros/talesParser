using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace talesParser
{
    class Program
    {
        static void Main(string[] args)
        {
            const string s1 = "https://1001rasskaz.ru/category/detskie-rasskazy";
            const string s2 = "https://1001rasskaz.ru/category/detskie-rasskazy/page/2";
            const string s3 = "https://1001rasskaz.ru/category/rasskazy-pro-lyubov";
            const string s4 = "https://1001rasskaz.ru/category/fmntastika";
            const string s5 = "https://1001rasskaz.ru/category/strashnye-rasskazy";
            const string s6 = "https://1001rasskaz.ru/category/smeshnye-rasskazy";

            ///"Просто вписываем любую категорию с этого сайта и получаем все рассказы.
            string URL = s6;

            List<string> talesURL = MakeListOfTalesURL(URL);

            List<string> tales = MakeListOfTales(talesURL);
            
        }

        public static string CleanText (string s)
        {
            string ans = "";

            ///Заменяем html символы.  
            ans = s.Replace("&nbsp;", " ").Replace("&ndash;", "–").Replace("&mdash;", "——").Replace("&lsquo;", "‘").Replace("&rsquo;", "’").Replace("&sbquo;", "‚").Replace("&ldquo;", "“").Replace("&rdquo;", "”").Replace("&bdquo;", "„").Replace("&laquo;", "«").Replace("&raquo;", "»");

            ///Убираем мусор.
            List<string> list = new List<string>(ans.Split('<', StringSplitOptions.RemoveEmptyEntries)).FindAll(i => i.Contains('<') == false && i.Contains('>') == false);
            foreach (string ss in list)
            {
                ans = ss;
            }

            return ans;
        }

        public static List<string> MakeListOfTalesURL (string URL)
        {
            var web = new HtmlWeb();

            var docType = web.Load(URL);

            ///Парсим доктайп по классам "постов - рассказов".            
            var tempData = docType.DocumentNode.SelectNodes("//*[@class = 'post-list-content']");

            string textHtml = "";

            ///Просто делаем стрингу из InnerHtml.
            foreach (var dataClass in tempData)
            {
                textHtml += dataClass.InnerHtml.ToString();
            }

            ///Сплитим по "", тк ссылки всегда между ковычек. 
            string[] tempParsedArray = textHtml.Split('"', StringSplitOptions.RemoveEmptyEntries);

            ///Сплитим ссылки и забираем только уникальные.
            List<string> talesURL = new List<string>(tempParsedArray).FindAll(i => i.StartsWith("http") && i.EndsWith(".html") == true).Distinct().ToList();

            return talesURL;
        }

        public static List<string> MakeListOfTales (List<string> talesURL)
        {
            var web = new HtmlWeb();

            List<string> tales = new List<string>();

            foreach (string url in talesURL)
            {
                var docType = web.Load(url);
                var tempData = docType.DocumentNode.SelectNodes("//*[@class = 'entry-content']");
                var temDataTitle = docType.DocumentNode.SelectNodes("//*[@class = 'entry-title']");                

                string textHtml = "";

                ///Просто делаем стрингу из InnerHtml.
                foreach (var dataClass in tempData)
                {
                    textHtml += dataClass.InnerHtml.ToString();
                }

                string[] sep = { "<p>", "</p>" };

                ///Сплитим по тэгам абзацы. 
                string[] tempParsedArray1 = textHtml.Split(sep, StringSplitOptions.RemoveEmptyEntries);

                List<string> list = new List<string>(tempParsedArray1).FindAll(i => (i.StartsWith("\n") == false) && (i.StartsWith("\r") == false) && (i.StartsWith("<") == false) && (i.StartsWith("&") == false)).Distinct().ToList();

                ///Начинаем с заголовка.
                string textOfTale = temDataTitle[0].InnerHtml.ToString() + "." + "\n";

                foreach (string s in list)
                {
                    textOfTale += CleanText(s);
                }

                tales.Add(textOfTale);
            }

            return tales;
        }
    }
}
