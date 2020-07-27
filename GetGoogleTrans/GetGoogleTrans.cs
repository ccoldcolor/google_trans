using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace GetGoogleTrans
{
    class Trans
    {
        private string from_lang = "zh-CN";

        private string to_lang = "en";

        private string url = "https://translate.google.cn/translate_a/single?";

        public string Keyword { get; set; }

        private void SetLang()
        {
            if (!CheckIsZh(this.Keyword)) {
                this.from_lang = "en";
                this.to_lang = "zh-CN";
            }
        }

        public JArray GetTrans()
        {
            this.SetLang();
            string token = GetGoogleToken(this.Keyword);
            string url = this.url;
            url += "client=t&sl=";
            url += this.from_lang;
            url += "&tl=";
            url += this.to_lang;
            url += "&hl=zh-CN";
            url += "&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw";
            url += "&dt=rm&dt=ss&dt=t&ie=UTF-8&oe=UTF-8&otf=2&ssel=0&tsel=0&kc=1&tk=";
            url += token;
            url += "&q=" + HttpUtility.UrlEncode(this.Keyword);
            HttpWebResponse res = CreateGetHttpResponse(url);
            string bbb = GetResponseString(res);
            JArray jsonRes = JArray.Parse(bbb);
            return jsonRes;
        }

        /// <summary>
        /// 获取接口 token
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string GetGoogleToken(string text)
        {
            int b = 406644;
            long b1 = 3293161072;
            string jd = ".";
            string sb = "+-a^+6";
            string Zb = "+-3^+b+-f";
            List<int> e = TransformQuery(text);
            long a = b;
            for (int f = 0; f < e.Count; f++)
            {
                a += e[f];
                a = RL(a, sb);
            }
            a = RL(a, Zb);
            a ^= b1;
            if (0 > a)
            {
                a = (a & 2147483647L) + 2147483648L;
            }
            a = a % 1000000;
            return a.ToString() + "." + (a ^ b);

        }

        /// <summary>
        /// 获取token的方法
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private static List<int> TransformQuery(string query)
        {
            char[] charArr = query.ToCharArray();
            List<int> e = new List<int>();
            for (int f = 0, g = 0; g < query.Length; g++)
            {
                int m = (int)charArr[g];
                if (m < 128)
                {
                    e.Add(m);
                }
                else if (m < 2048)
                {
                    e.Add(m >> 6 | 192);
                    e.Add(m & 0x3F | 0x80);
                }
                else if (0xD800 == (m & 0xFC00) && g + 1 < query.Length && 0xDC00 == ((int)charArr[g + 1] & 0xFC00))
                {
                    m = (byte)((1 << 16) + ((m & 0x03FF) << 10) + ((int)charArr[++g] & 0x03FF));
                    e.Add(m >> 18 | 0xF0);        //  111100{l[9-8*]}
                    e.Add(m >> 12 & 0x3F | 0x80); //  10{l[7*-2]}
                    e.Add(m & 0x3F | 0x80);     //  10{(l+1)[5-0]}
                }
                else
                {
                    e.Add(m >> 12 | 0xE0);        //  1110{l[15-12]}
                    e.Add(m >> 6 & 0x3F | 0x80);  //  10{l[11-6]}
                    e.Add(m & 0x3F | 0x80);     //  10{l[5-0]}
                }
            }
            return e;
        }

        /// <summary>
        /// 获取token的方法
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static long RL(long a, string b)
        {
            for (int i = 0; i < b.Length - 2; i += 3)
            {

                char c = b.ToCharArray()[i + 2];
                int d = (c >= 'a') ? ((int)c - 87) : Convert.ToInt32(c + "");
                long f = (b.ToCharArray()[i + 1] == '+') ? (_rshift(a, d)) : ((Int32)a << d);
                a = (b.ToCharArray()[i] == '+') ? (a + f & 4294967295L) : (a ^ f);
            }
            return a;
        }

        /// <summary>
        /// 获取token的方法
        /// </summary>
        /// <param name="val"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static long _rshift(long val, int n)
        {
            //long l = 0;
            //int i = (int)val;
            //if (val >= 0) { l = val >> n; }
            //else
            //{
            //    string s = Convert.ToString(i >> n, 2);
            //    Console.WriteLine(s);
            //    s = s.Substring(6).PadLeft(32, '0');
            //    l = Convert.ToInt32(s, 2);
            //}
            long l = (val >= 0) ? (val >> n) : (val + 0x100000000L) >> n;
            return l;
        }

        /// <summary>
        /// 发送http Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static HttpWebResponse CreateGetHttpResponse(string url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";//链接类型
            return request.GetResponse() as HttpWebResponse;
        }

        /// <summary>
        /// 从HttpWebResponse对象中提取响应的数据转换为字符串
        /// </summary>
        /// <param name="webresponse"></param>
        /// <returns></returns>
        private static string GetResponseString(HttpWebResponse webresponse)
        {
            using (Stream s = webresponse.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// 检查字符串中是否包含中文
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static bool CheckIsZh(string input)
        {
            string pattern = "[\u4e00-\u9fbb]";
            return Regex.IsMatch(input, pattern);
        }
    }
}
