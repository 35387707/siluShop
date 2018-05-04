using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelexBarBLL.Models
{
    class VerifyCode
    {
        #region 验证码长度(默认4个验证码的长度)

        private int length = 4;

        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        #endregion

        #region 验证码字体大小(为了显示扭曲效果，默认40像素，可以自行修改)

        private int fontSize = 24;

        public int FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }

        #endregion

        #region 边框补(默认1像素)

        private int padding = 1;

        public int Padding
        {
            get { return padding; }
            set { padding = value; }
        }

        #endregion

        #region 是否输出燥点(默认不输出)

        private bool chaos = true;

        public bool Chaos
        {
            get { return chaos; }
            set { chaos = value; }
        }

        #endregion

        #region 输出燥点的颜色(默认灰色)

        private Color chaosColor = Color.LightGray;

        public Color ChaosColor
        {
            get { return chaosColor; }
            set { chaosColor = value; }
        }

        #endregion

        #region 自定义背景色(默认白色)

        private Color backgroundColor = Color.White;

        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        #endregion

        #region 自定义随机颜色数组

        private Color[] colors =
        {
      Color.Black, Color.Red, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown,
      Color.DarkCyan, Color.Purple
    };

        public Color[] Colors
        {
            get { return colors; }
            set { colors = value; }
        }

        #endregion

        #region 自定义字体数组

        private string[] fonts = { "Arial", "Georgia" };

        public string[] Fonts
        {
            get { return fonts; }
            set { fonts = value; }
        }

        #endregion

        #region 自定义随机码字符串序列(使用逗号分隔)

        private string codeSerial = "0,1,2,3,4,5,6,7,8,9";
        // string codeSerial = "0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
        // string codeSerial = "殃,央,鸯,秧,杨,扬,佯,疡,羊,洋,阳,氧,仰,痒,养,样,漾,邀,腰,妖,瑶,摇,尧,遥,窑,谣,姚,咬,舀,药,要,耀,椰,噎,耶,爷,野,冶,也,页,掖,业,叶,曳,腋,夜,液,一,壹,医,揖,铱,依,伊,衣,颐,夷,遗,移,仪,胰,疑,沂,宜,姨,彝,椅,蚁,倚,已,乙,矣,以,艺,抑,易,邑,屹,亿,役,臆,逸,肄,疫,亦,裔,意,毅,忆,义,益,溢,诣,议,谊,译,异,翼,翌,绎,茵,荫,因,殷,音,阴,姻,吟,银,淫,寅,饮,尹,引,隐,印,英,樱,婴,鹰,应,缨,莹,萤,营,荧,蝇,迎,赢,盈,影,颖,硬,映,哟,拥,佣,臃,痈,庸,雍,踊,蛹,咏,泳,涌,永,恿,勇,用,幽,优,悠,忧,尤,由,邮,铀,犹,油,游,酉,有,友,右,佑,釉,诱,又,幼,迂,淤,于,盂,榆,虞,愚,舆,余,俞,逾,鱼,愉,渝,渔,隅,予,娱,雨,与,屿,禹,株,蛛,朱,猪,诸,诛,逐,竹,烛,煮,拄,瞩,嘱,主,著,柱,助,蛀,贮,铸,筑,住,注,祝,驻,抓,爪,拽,专,砖,转,撰,赚,篆,桩,庄,装,妆,撞,壮,状,椎,锥,追,赘,坠,缀,谆,准,捉,拙,卓,桌,琢,茁,酌,啄,着,灼,浊,兹,咨,资,姿,滋,淄,孜,紫,仔,籽,滓,子,自,渍,字,鬃,棕,踪,宗,综,总,纵,邹,走,奏,揍,租,足,卒,族,祖,诅,阻,组,钻,纂,嘴,醉,最,罪,尊,遵,昨,左,佐,柞,做,作,坐,座";
        public string CodeSerial
        {
            get { return codeSerial; }
            set { codeSerial = value; }
        }

        #endregion

        //产生波形滤镜效果

        #region 产生波形滤镜效果

        private const double PI = 3.1415926535897932384626433832795;
        private const double PI2 = 6.283185307179586476925286766559;

        /**/

        /// <summary>
        /// 正弦曲线Wave扭曲图片（Edit By 51aspx.com）
        /// </summary>
        /// <param name="srcBmp">图片路径</param>
        /// <param name="bXDir">如果扭曲则选择为True</param>
        /// <param name="nMultValue">波形的幅度倍数，越大扭曲的程度越高，一般为3</param>
        /// <param name="dPhase">波形的起始相位，取值区间[0-2*PI)</param>
        /// <returns></returns>
        public System.Drawing.Bitmap TwistImage(Bitmap srcBmp, bool bXDir, double dMultValue, double dPhase)
        {
            System.Drawing.Bitmap destBmp = new Bitmap(srcBmp.Width, srcBmp.Height);

            // 将位图背景填充为白色
            System.Drawing.Graphics graph = System.Drawing.Graphics.FromImage(destBmp);
            graph.FillRectangle(new SolidBrush(System.Drawing.Color.White), 0, 0, destBmp.Width, destBmp.Height);
            graph.Dispose();

            double dBaseAxisLen = bXDir ? (double)destBmp.Height : (double)destBmp.Width;

            for (int i = 0; i < destBmp.Width; i++)
            {
                for (int j = 0; j < destBmp.Height; j++)
                {
                    double dx = 0;
                    dx = bXDir ? (PI2 * (double)j) / dBaseAxisLen : (PI2 * (double)i) / dBaseAxisLen;
                    dx += dPhase;
                    double dy = Math.Sin(dx);

                    // 取得当前点的颜色
                    int nOldX = 0, nOldY = 0;
                    nOldX = bXDir ? i + (int)(dy * dMultValue) : i;
                    nOldY = bXDir ? j : j + (int)(dy * dMultValue);

                    System.Drawing.Color color = srcBmp.GetPixel(i, j);
                    if (nOldX >= 0 && nOldX < destBmp.Width
                      && nOldY >= 0 && nOldY < destBmp.Height)
                    {
                        destBmp.SetPixel(nOldX, nOldY, color);
                    }
                }
            }

            return destBmp;
        }



        #endregion

        #region 生成校验码图片

        public Bitmap CreateImageCode(string code)
        {
            int fSize = FontSize;
            int fWidth = fSize + Padding;

            int imageWidth = (int)(code.Length * fWidth) + 4 + Padding * 2;
            int imageHeight = fSize * 2 + Padding;

            System.Drawing.Bitmap image = new System.Drawing.Bitmap(imageWidth, imageHeight);

            Graphics g = Graphics.FromImage(image);

            g.Clear(BackgroundColor);

            Random rand = new Random();

            //给背景添加随机生成的燥点
            if (this.Chaos)
            {

                Pen pen = new Pen(ChaosColor, 0);
                int c = Length * 10;

                for (int i = 0; i < c; i++)
                {
                    int x = rand.Next(image.Width);
                    int y = rand.Next(image.Height);

                    g.DrawRectangle(pen, x, y, 1, 1);
                }
            }

            int left = 0, top = 0, top1 = 1, top2 = 1;

            int n1 = (imageHeight - FontSize - Padding * 2);
            int n2 = n1 / 4;
            top1 = n2;
            top2 = n2 * 2;

            Font f;
            Brush b;

            int cindex, findex;

            #region 随机字体和颜色的验证码字符

            for (int i = 0; i < code.Length; i++)
            {
                cindex = rand.Next(Colors.Length - 1);
                findex = rand.Next(Fonts.Length - 1);

                f = new System.Drawing.Font(Fonts[findex], fSize, System.Drawing.FontStyle.Bold);
                b = new System.Drawing.SolidBrush(Colors[cindex]);

                if (i % 2 == 1)
                {
                    top = top2;
                }
                else
                {
                    top = top1;
                }

                left = i * fWidth;

                g.DrawString(code.Substring(i, 1), f, b, left, top);
            }

            //画一个边框 边框颜色为Color.Gainsboro
            g.DrawRectangle(new Pen(Color.Gainsboro, 0), 0, 0, image.Width - 1, image.Height - 1);
            g.Dispose();

            //产生波形
            //image = TwistImage(image, true, 8, 4);

            return image;
        }

        #endregion

    }
}
#endregion