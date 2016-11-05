using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace RecolorImage
{
    public partial class Form1 : Form
    {
        Bitmap bmp; //= new Bitmap("source.png");
        
        public Form1()
        {
            InitializeComponent();
            bmp = RecolorImage.Properties.Resources.source;
            pictureBox1.Image = bmp;
        }

        private Color Colorizer(Color from, float hue, float saturation, float brightness)
        {
            float originalHue, originalSatur, originalBright;
            ColorToHSV(from, out originalHue, out originalSatur, out originalBright);

            return ColorFromHSV(originalHue + (1f - originalHue / 360f) * hue, originalSatur + (1f - originalSatur) * saturation, originalBright + (1f - originalBright) * brightness);
        }


        private unsafe Bitmap ColorizeBitmap(Bitmap source, float hue, float saturation, float brightness, Func<Color, float, float, float, Color> func)
        {
            var result = new Bitmap(source);
            BitmapData data = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.ReadOnly, result.PixelFormat);
            int PixelSize = 4;

            for (int y = 0; y < data.Height; y++)
            {
                byte* row = (byte*)data.Scan0 + (y * data.Stride);
                for (int x = 0; x < data.Width; x++)
                {
                    byte* r = &row[x * PixelSize + 2];
                    byte* g = &row[x * PixelSize + 1];
                    byte* b = &row[x * PixelSize];

                    Color newColor = func(Color.FromArgb(*r, *g, *b), hue, saturation, brightness);

                    *b = newColor.B;
                    *g = newColor.G;
                    *r = newColor.R;
                }
            }
            result.UnlockBits(data);
            return result;
        }

        public static void ColorToHSV(Color color, out float hue, out float saturation, out float value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1f - (1f * min / max);
            value = max / 255f;
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            switch (hi)
            {
                case 0:
                    return Color.FromArgb(255, v, t, p);
                case 1:
                    return Color.FromArgb(255, q, v, p);
                case 2:
                    return Color.FromArgb(255, p, v, t);
                case 3:
                    return Color.FromArgb(255, p, q, v);
                case 4:
                    return Color.FromArgb(255, t, p, v);
                default:
                    return Color.FromArgb(255, v, p, q);
            }
        }

        private void hueBar_MouseUp(object sender, MouseEventArgs e)
        {
            this.Text = "Please wait...";
            float hue = hueBar.Value;

            try
            {
                var result = ColorizeBitmap(bmp, hue, 0, 0, Colorizer);
                pictureBox1.Image = result;
                pictureBox1.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to change a hue\n\nInfo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.Text = "Windows Hero Color Changer";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = bmp;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.FileName = "Wallpaper";
            s.Filter = "Pictures|*.png";
            if (s.ShowDialog() == DialogResult.OK)
            {
                try { pictureBox1.Image.Save(s.FileName); }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save file\n\nInfo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }
    }
}
