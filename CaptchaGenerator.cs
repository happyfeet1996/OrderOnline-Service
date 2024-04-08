using System.Drawing;
using System.Drawing.Imaging;

namespace OrderOnline
{
    public class CaptchaGenerator
    {
        public string GenerateCaptcha(int width, int height, string captchaText)
        {
            var image = new Bitmap(width, height);
            var graphics = Graphics.FromImage(image);

            var font = FindBestFitFont(graphics, captchaText, width, height);
            var brush = new SolidBrush(Color.Black);
            var rect = new Rectangle(0, 0, width, height);

            graphics.FillRectangle(Brushes.White, rect);
            graphics.DrawString(captchaText, font, brush, rect);

            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, ImageFormat.Png);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public string GenerateRandomText(int count)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, count).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private Font FindBestFitFont(Graphics graphics, string text, int width, int height)
        {
            // 初始字体大小
            float fontSize = 10;
            Font font;

            // 循环逐步增大字体大小，直到文字占满图片的宽度为止
            do
            {
                font = new Font(FontFamily.GenericSerif, fontSize);
                SizeF size = graphics.MeasureString(text, font);

                if (size.Width > width || size.Height > height)
                {
                    font.Dispose();
                    fontSize--;
                }
                else
                {
                    break;
                }
            } while (true);

            return font;
        }
    }
}
