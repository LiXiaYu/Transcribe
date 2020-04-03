using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Transcribe
{
    public static class Identification
    {
        #region 不许偷看
        // 设置APPID/AK/SK

        static string APP_ID ;
        static string API_KEY ;
        static string SECRET_KEY ;


        public static void SetAPPID(string id, string key, string s_key)
        {
            Identification.APP_ID = id;
            Identification.API_KEY = key;
            Identification.SECRET_KEY = s_key;
        }
        #endregion
        static Baidu.Aip.Ocr.Ocr client;

        public enum ApiVersion
        {
            GeneralBasic,
            General,
            AccurateBasic,
            Accurate,
            Numbers
        }

        public static void Link()
        {
            Identification.client = new Baidu.Aip.Ocr.Ocr(Identification.API_KEY, Identification.SECRET_KEY);
            Identification.client.Timeout = 60000;// 修改超时时间
        }
        /// <summary>
        /// 从文件中识别图片中的文字
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <returns></returns>
        public static JObject IdentifyImageFile(string path)
        {
            var image = File.ReadAllBytes(path);
            // 调用通用文字识别, 图片参数为本地图片，可能会抛出网络等异常，请使用try/catch捕获
            //var result = client.GeneralBasic(image);//基本版
            var result = client.Accurate(image);//高精度版 位置
            return result;
        }
        /// <summary>
        /// 从文件中识别图片中的文字
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <param name="options">识别的参数，百度api</param>
        /// <returns></returns>
        public static JObject IdentifyImageFile(string path, Dictionary<string, object> options)
        {
            var image = File.ReadAllBytes(path);
            // 如果有可选参数
            //var options = new Dictionary<string, object>{
            //    {"language_type", "CHN_ENG"},
            //    {"detect_direction", "true"},
            //    {"detect_language", "true"},
            //    {"probability", "true"}
            //};
            // 带参数调用通用文字识别, 图片参数为本地图片
            //var result = client.GeneralBasic(image, options);//基本版
            var result = client.Accurate(image, options);//高精度版 位置
            return result;
        }

        public static JObject IdentifyImage(string path, ApiVersion apiVersion)
        {
            var image = File.ReadAllBytes(path);
            JObject result;
            result = JudgeMethod(apiVersion, image);
            if (result["error_code"] != null)
            {
                throw new NoApiTimesException("BaiduYun Error: " + result["error_code"] + " " + result["error_msg"]);
            }
            return result;
        }
        public static JObject IdentifyImage(byte[] image, ApiVersion apiVersion)
        {
            JObject result;
            result = JudgeMethod(apiVersion, image);
            if (result["error_code"] != null)
            {
                throw new NoApiTimesException("BaiduYun Error: " + result["error_code"] + " " + result["error_msg"]);
            }
            return result;
        }
        private static JObject JudgeMethod(ApiVersion apiVersion, byte[] image)
        {
            JObject result;
            switch (apiVersion)
            {
                case ApiVersion.GeneralBasic:
                    result = client.GeneralBasic(image);
                    break;
                case ApiVersion.General:
                    result = client.General(image);
                    break;
                case ApiVersion.AccurateBasic:
                    result = client.AccurateBasic(image);
                    break;
                case ApiVersion.Accurate:
                    result = client.Accurate(image);
                    break;
                case ApiVersion.Numbers:
                    result = client.Numbers(image);
                    break;
                default:
                    result = null;
                    break;
            }
            return result;
        }

        public static JObject IdentifyImageFile_GeneralBasic(string path)
        {
            var image = File.ReadAllBytes(path);
            // 调用通用文字识别, 图片参数为本地图片，可能会抛出网络等异常，请使用try/catch捕获
            var result = client.GeneralBasic(image);//基本版
            //var result = client.Accurate(image);//高精度版 位置
            return result;
        }
        public static JObject IdentifyImage_GeneralBasic(byte[] image)
        {
            var result = client.GeneralBasic(image);
            return result;
        }
        public static JObject IdentifyImage(byte[] image)
        {
            var result = client.Accurate(image);//高精度版 位置
            return result;
        }

        public static byte[] BitmapToBytes(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] buffer = ms.ToArray();
            ms.Close();
            ms.Dispose();

            return buffer;
        }
        public static Bitmap BytesToBitmap(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            Bitmap bmp = new Bitmap(ms);
            ms.Close();

            return bmp;
        }
        public static Bitmap CutBitmap(Bitmap oldBmp, int width, int height, int top, int left)
        {

            //新图像，并设置宽高
            Bitmap newBmp = new Bitmap(width, height);
            Graphics draw = Graphics.FromImage(newBmp);

            draw.DrawImage(oldBmp, 0, 0, width, height);
            draw.DrawImage(oldBmp, 0, 0, new Rectangle(left, top, width, height), GraphicsUnit.Pixel);//两句一起的

            draw.Dispose();

            oldBmp.Dispose();//一定要把源图Dispose调，因为保存的是相同路径，需要把之前的图顶替调，如果不释放的话会报错：（GDI+ 中发生一般性错误。）
            return newBmp;
        }
        public static byte[] CutBytes(byte[] old, int width, int height, int top, int left)
        {
            return BitmapToBytes(CutBitmap(BytesToBitmap(old), width, height, top, left));
        }


        /// <summary>
        /// 图像灰度化
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap ConvertToGray(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel(i, j);
                    //利用公式计算灰度值
                    int gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    Color newColor = Color.FromArgb(gray, gray, gray);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        internal static byte[] BytesLinkRecord3456(byte[][] v)
        {
            Mat[] ims = new Mat[v.Length];

            for (int i = 0; i < ims.Length; i++)
            {
                ims[i] = Mat.FromImageData(v[i]);
            }

            var (r, c) = (new Range(ims[0].Rows * 54 / 1024, ims[0].Rows * 994 / 1024), new Range(0, ims[0].Cols * 1050 / 1280));


            Mat[] im_c = new Mat[ims.Length];

            for (int i = 0; i < im_c.Length; i++)
            {
                im_c[i] = ims[i][r, c];
            }

            Mat im_l = new Mat(new OpenCvSharp.Size(im_c[0].Rows * v.Length, im_c[0].Cols), im_c[0].Type());
            Cv2.VConcat(im_c, im_l);

#if DEBUG
            DEBUG_saveImage(im_l);
#endif
            return im_l.ToBytes();
        }

        /// <summary>
        /// 图像二值化1：取图片的平均灰度作为阈值，低于该值的全都为0，高于该值的全都为255
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap ConvertTo1Bpp1(Bitmap bmp)
        {
            int average = 0;
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color color = bmp.GetPixel(i, j);
                    average += color.B;
                }
            }
            average = (int)average / (bmp.Width * bmp.Height);

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel(i, j);
                    int value = 255 - color.B;
                    Color newColor = value > average ? Color.FromArgb(0, 0, 0) : Color.FromArgb(255, 255, 255);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }
        /// <summary>
        /// 图像二值化2
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Bitmap ConvertTo1Bpp2(Bitmap img)
        {
            int w = img.Width;
            int h = img.Height;
            Bitmap bmp = new Bitmap(w, h, PixelFormat.Format1bppIndexed);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
            for (int y = 0; y < h; y++)
            {
                byte[] scan = new byte[(w + 7) / 8];
                for (int x = 0; x < w; x++)
                {
                    Color c = img.GetPixel(x, y);
                    if (c.GetBrightness() >= 0.5) scan[x / 8] |= (byte)(0x80 >> (x % 8));
                }
                Marshal.Copy(scan, 0, (IntPtr)((int)data.Scan0 + data.Stride * y), scan.Length);
            }
            return bmp;
        }

        /// <summary>
        /// 图像增强处理（二值化）
        /// </summary>
        /// <param name="image">图像bytes</param>
        /// <param name="threshold">阈值</param>
        /// <returns></returns>
        public static byte[] BytesFilter(byte[] image, int threshold = 38)
        {
            string tempname = "temp_BytesFilter_" + DateTime.Now.ToBinary() + ".jpg";
            File.WriteAllBytes(System.AppDomain.CurrentDomain.BaseDirectory + tempname, image);
            Mat im = new Mat(tempname);
            //Mat im_g=im.GaussianBlur(new OpenCvSharp.Size(3,3),3);
            Mat im_g = im.Threshold(threshold, 255, ThresholdTypes.Binary);
            im_g.SaveImage(System.AppDomain.CurrentDomain.BaseDirectory + tempname + ".gaussianBlur.jpg");

            return File.ReadAllBytes(System.AppDomain.CurrentDomain.BaseDirectory + tempname + ".gaussianBlur.jpg");
        }

        /// <summary>
        /// 取出对应颜色的图像区域
        /// </summary>
        /// <param name="image">图像bytes</param>
        /// <param name="threshold">颜色</param>
        /// <returns></returns>
        public static byte[] BytesColorFilter(byte[] image, int threshold = 1)
        {
            Mat im = Mat.FromImageData(image);
            Mat im_blue = im.Split()[0];//BGR 


            Mat.FromStream(new MemoryStream(image), ImreadModes.Color);

            int[] im_blue_r = new int[im_blue.Rows];
            int top = -1;
            int bottom = -1;
            for (int index_i = 0; index_i < im_blue.Rows; index_i++)
            {
                for (int index_j = 0; index_j < im_blue.Cols; index_j++)
                {
                    im_blue_r[index_i] += im_blue.At<byte>(index_i, index_j) > threshold ? 0 : 1;
                }

                //im_blue_r[index_i] /= im_blue.Cols;

                //if (index_i == 0)
                //{
                //    continue;
                //}

                if (im_blue_r[index_i] > 500 && top == -1)
                {
                    top = index_i;
                }
                if (im_blue_r[index_i] < 500 && bottom == -1 && top != -1)
                {
                    bottom = index_i;
                }
            }
            Mat im_blue_cc = im_blue.Threshold(threshold * 2 / 7, 255, ThresholdTypes.Binary);

            OpenCvSharp.Size size = new OpenCvSharp.Size(bottom - top + 1, im_blue.Cols);
            Mat im_c = im_blue_cc[new Range(top, bottom), Range.All];
            //Mat im_g=im.GaussianBlur(new OpenCvSharp.Size(3,3),3);
            //Mat im_g = im.Threshold(threshold, 255, ThresholdTypes.Binary);
#if DEBUG
            DEBUG_saveImage(im_c);
#endif

            return im_c.ToBytes();
        }

        public static byte[] BytesCutRight(byte[] image)
        {
            Mat mi = Mat.FromImageData(image);
            OpenCvSharp.Size size = new OpenCvSharp.Size(mi.Cols * 1050 / 1280, mi.Rows);
            Mat im_c = mi[Range.All, new Range(0, size.Width)];

#if DEBUG
            DEBUG_saveImage(im_c);
#endif

            return im_c.ToBytes();
        }


        public static void DEBUG_saveImage(Mat m, string name = "")
        {
            if (false == System.IO.Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"\temp"))
            {
                //创建pic文件夹
                System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"\temp");
            }

            string tempname = "temp_" + name + DateTime.Now.ToBinary() + ".jpg";
            m.SaveImage(System.AppDomain.CurrentDomain.BaseDirectory + @"\temp\" + tempname + ".cut.jpg");
        }

        public static void DEBUG_saveImage(Bitmap b, string name = "")
        {
            if (false == System.IO.Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"\temp"))
            {
                //创建pic文件夹
                System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"\temp");
            }
            string tempname = "temp_" + name + DateTime.Now.ToBinary() + ".jpg";
            b.Save(System.AppDomain.CurrentDomain.BaseDirectory + @"\temp\" + tempname + ".cut.jpg");
        }
        public static void DEBUG_saveImage(byte[] b, string name = "")
        {
            if (false == System.IO.Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"\temp"))
            {
                //创建pic文件夹
                System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"\temp");
            }
            string tempname = "temp_" + name + DateTime.Now.ToBinary() + ".jpg";
            File.WriteAllBytes(System.AppDomain.CurrentDomain.BaseDirectory + @"\temp\" + tempname + ".cut.jpg", b);
        }

        public static void DEBUG_save(byte[] b, string name = "")
        {
            if (false == System.IO.Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"\temp"))
            {
                //创建pic文件夹
                System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + @"\temp");
            }
            var guid = Guid.NewGuid().ToString();
            string tempname = "temp_" + name + DateTime.Now.ToBinary() + "_" + guid + ".jpg";
            File.WriteAllBytes(System.AppDomain.CurrentDomain.BaseDirectory + @"\temp\" + tempname + ".cut.jpg", b);
        }
        public static void DEBUG_save(List<byte[]> bs, string name = "")
        {
            foreach (var b in bs)
            {
                DEBUG_save(b, name);
            }
        }
        public static byte[] CutAndPasteBytes(byte[] obs, int width, int height, int top, int left)
        {
            Mat om = Mat.FromImageData(obs);
            OpenCvSharp.Size size = new OpenCvSharp.Size(width, height);
            Mat im_c = om[new Range(top, top + height), new Range(left, left + width)];
            OpenCvSharp.Size size_half = new OpenCvSharp.Size(width, height / 2);
            OpenCvSharp.Size size_addblock = new OpenCvSharp.Size(width, height / 2 + height);

            Mat im_block = om[new Range(0, size_addblock.Height - 1), new Range(0, size_addblock.Width - 1)];
            im_block.SetTo(new Scalar(0, 0, 0, 0));
            im_block[new Range(0, size_half.Height - 1), new Range(0, size_half.Width - 1)] = im_c[new Range(0, size_half.Height - 1), new Range(0, size_half.Width - 1)];
            im_block[new Range(size.Height, size.Height + size_half.Height - 1), new Range(0, size_half.Width - 1)] = im_c[new Range(size_half.Height, size_half.Height + size_half.Height - 1), new Range(0, size_half.Width - 1)];

            return im_block.ToBytes();

        }

        /// <summary>
        /// 按矩形分割图片，并组合成大图
        /// TODO
        /// </summary>
        /// <param name="originBytes">原始图像</param>
        /// <param name="blocks">矩形区</param>
        /// <param name="lineWidth">大图的线宽</param>
        /// <returns></returns>
        public static byte[] ReArrange(byte[] originBytes, List<(int width, int height, int top, int left)> blocks, int lineWidth = 50)
        {
            Mat om = Mat.FromImageData(originBytes);
            List<Mat> ms = new List<Mat>();

            int width = om.Width;
            int height = 0;
            foreach (var blockInfo in blocks)
            {
                ms.Add(om[new Range(blockInfo.top, blockInfo.top + blockInfo.height), new Range(blockInfo.left, blockInfo.left + blockInfo.width)]);
                height += blockInfo.height + 50;
            }

            Mat im_c = new Mat(new OpenCvSharp.Size(width, height), om.Type());

            int moving_top = 0;
            foreach (var m in ms)
            {
                im_c[new Range(moving_top, moving_top + m.Height), new Range(0, m.Width)] = m;
                moving_top += m.Height + 50;
            }
            Mat im_block = im_c;
            return im_block.ToBytes();
        }

        public static byte[] MegerBytesRow(List<byte[]> images, int block_height = 5)
        {

            var ims = new List<Mat>();

            int maxwidth = 0;
            int maxheight = 0;
            for (var index_m = 0; index_m < images.Count; index_m++)
            {
                var im = Mat.FromImageData(images[index_m]);
                ims.Add(im);

                if (im.Width > maxwidth)
                {
                    maxwidth = im.Width;
                }

                maxheight += im.Height + block_height;

            }

            Mat m = new Mat(new OpenCvSharp.Size(maxwidth, maxheight), ims[0].Type());
            Cv2.BitwiseNot(m, m);
            int moving_top = 0;
            for (var index_m = 0; index_m < ims.Count; index_m++)
            {
                m[new Range(moving_top, moving_top + ims[index_m].Height), new Range(0, ims[index_m].Width)] = ims[index_m];
                moving_top += ims[index_m].Height + block_height;
            }

            return m.ToBytes();
        }

        public static byte[] MegerBytesColumn(List<byte[]> images, int block_width = 5)
        {

            var ims = new List<Mat>();

            int maxheight = 0;
            int maxwidth = 0;
            for (var index_m = 0; index_m < images.Count; index_m++)
            {
                var im = Mat.FromImageData(images[index_m]);
                ims.Add(im);

                if (im.Height > maxheight)
                {
                    maxheight = im.Height;
                }

                maxwidth += im.Width + block_width;

            }

            Mat m = new Mat(new OpenCvSharp.Size(maxwidth, maxheight), ims[0].Type());
            Cv2.BitwiseNot(m, m);
            int moving_left = 0;
            for (var index_m = 0; index_m < ims.Count; index_m++)
            {
                m[new Range(0, ims[index_m].Height), new Range(moving_left, moving_left + ims[index_m].Width)] = ims[index_m];
                moving_left += ims[index_m].Width + block_width;
            }

            return m.ToBytes();
        }

        public static (int width, int height) GetBytesSize(byte[] image)
        {
            Mat im = Mat.FromImageData(image);
            return (im.Width, im.Height);
        }
        /// <summary>
        /// 把图像按行切开，沿着行中的空白部分切
        /// </summary>
        /// <param name="image">原始图像</param>
        /// <param name="iWidth">最小间隔</param>
        /// <param name="noiseRate">空白部分容许的噪音比例</param>
        /// <returns></returns>
        public static List<byte[]> BytesCutColumn(byte[] image, int iWidth = 7, double noiseRate = 0)
        {
            var result = new List<byte[]>();
            Mat im = Mat.FromImageData(image, ImreadModes.Grayscale);
            //Cv2.CvtColor(im, im, ColorConversionCodes.BGR2GRAY);

            //Mat mb = new Mat(new OpenCvSharp.Size(1, im.Cols), im.Type());

            var reIm = new byte[im.Rows][];
            for (int i = 0; i < im.Rows; i++)
            {
                reIm[i] = new byte[im.Cols];
                for (int j = 0; j < im.Cols; j++)
                {
                    reIm[i][j] = im.At<byte>(i, j);
                }
            }


            Mat mb = im.Reduce(ReduceDimension.Row, ReduceTypes.Sum, MatType.CV_32S);
            //Cv2.Reduce(im, mb, ReduceDimension.Row, ReduceTypes.Sum, im.Type());

            //mb.CvtColor(ColorConversionCodes.BGR2GRAY);

            int lastStart = 0;
            int whiteWidth = 0;
            bool firstBlock = true;
            for (int i = 0; i < mb.Cols; i++)
            {
                if (mb.At<int>(0, i) >= 255 * im.Rows * (1 - noiseRate) - 0.5)
                {
                    whiteWidth++;
                }
                else
                {
                    if (whiteWidth >= iWidth && firstBlock == false)
                    {

                        result.Add(im[Range.All, new Range(lastStart, i - whiteWidth / 2 + iWidth / 2)].ToBytes());
                        lastStart = i - iWidth / 2 >= 0 ? i - iWidth / 2 : 0;
                    }
                    else if (whiteWidth >= iWidth && firstBlock == true)
                    {
                        lastStart = i - iWidth / 2 >= 0 ? i - iWidth / 2 : 0;
                    }
                    whiteWidth = 0;
                    firstBlock = false;
                }
            }
            result.Add(im[Range.All, new Range(lastStart, (mb.Cols - whiteWidth + iWidth / 2 >= mb.Cols) ? (mb.Cols - 1) : (mb.Cols - whiteWidth + iWidth / 2))].ToBytes());


            return result;
        }
    }

}
