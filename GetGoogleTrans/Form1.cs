using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GetGoogleTrans
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 当前搜索关键词
        /// </summary>
        static string CurrentKeyword = null;
        public Form1()
        {
            InitializeComponent();
            listView.Columns.Add("翻译", 120, HorizontalAlignment.Left);
            listView.Columns.Add("注释", 290, HorizontalAlignment.Left);
        }

        /// <summary>
        /// 点击按钮翻译
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            //获取用户输入
            string keyword = Input.Text.Trim();
            this.GetTrans(keyword);
        }

        /// <summary>
        /// 翻译并输出
        /// </summary>
        /// <param name="keyword"></param>
        private void GetTrans(string keyword)
        {
            if (keyword != "" && keyword != null && CurrentKeyword != keyword)
            {
                //MessageBox.Show(keyword + " " + CurrentKeyword);
                CurrentKeyword = keyword;
                Trans trans = new Trans();
                trans.Keyword = keyword;
                JArray res = trans.GetTrans();
                listView.Items.Clear();
                //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度
                listView.BeginUpdate();
                ListViewItem Title = new ListViewItem();
                Title.Text = res[0][0][0].ToString();
                listView.Items.Add(Title);
                try
                {
                    //添加接口返回数据
                    foreach (var item in res[1][0][2])
                    {
                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = item[0].ToString();
                        string explanation = "";
                        foreach (var itemExp in item[1])
                        {
                            explanation += " " + itemExp.ToString();
                        }
                        lvi.SubItems.Add(explanation);
                        listView.Items.Add(lvi);
                    }
                }
                catch (Exception ex) { }
                //结束数据处理，UI界面一次性绘制。
                listView.EndUpdate();
            }
        }

        /// <summary>
        /// 单击翻译复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_MouseClick(object sender, MouseEventArgs e)
        {
            ListView listview = (ListView)sender;
            ListViewItem lstrow = listview.GetItemAt(e.X, e.Y);
            ListViewItem.ListViewSubItem lstcol = lstrow.GetSubItemAt(e.X, e.Y);
            string strText = lstcol.Text;
            try
            {
                Clipboard.SetDataObject(strText.Trim());
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(1500, "提示", "内容【" + strText.Trim() + "】复制成功！", ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 监听键盘回车键事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //获取用户输入
                string keyword = Input.Text.Trim();
                this.GetTrans(keyword);
            }
        }
    }
}
