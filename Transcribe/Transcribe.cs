using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static Transcribe.Identification;

namespace Transcribe
{
    public static class Transcribe
    {

        public static void Link()
        {
            Identification.Link();
        }

        public static dynamic Name(string[] paths)
        {
            return Name(paths[0]);
        }
        /// <summary>
        /// 带姓名id的一页，绿色选中
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <returns>数据
        /// Id int32
        /// Name string
        /// Sex int32 男1女2
        /// Years int32
        /// MajorDiagnosisCoding string ICD-10疾病编码(旧)
        /// MajorDiagnosis string
        /// </returns>
        public static dynamic Name(string path)
        {
            dynamic ddata = new System.Dynamic.ExpandoObject();

            var image = File.ReadAllBytes(path);
            var image_cf = Identification.BytesColorFilter(image, 70);


            string words_all = "";
            try
            {
                var jb = Identification.IdentifyImage(image_cf, ApiVersion.AccurateBasic);

                StringWriter strWriter = new StringWriter();
                StringBuilder sb = strWriter.GetStringBuilder();

                foreach (var jd in jb["words_result"])
                {
                    string words = jd["words"].ToString();

                    sb.Insert(sb.Length, words);
                }
                words_all = sb.ToString();
            }
            catch(NoApiTimesException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new NoFindContextException("找不到姓名页");
            }

            var rId = new Regex(@"\d{8}");
            var rg_1 = rId.Match(words_all).ToString();
            if (rg_1 == "")
            {
                rId = new Regex(@"\d{7}");
                rg_1 = rId.Match(words_all).ToString();
            }
            if (rg_1 == "")
            {
                rId = new Regex(@"\d{6}");
                rg_1 = rId.Match(words_all).ToString();
            }
            ddata.CaseHistoryId = Convert.ToInt32(rg_1.ToString());
            string words_d__id = words_all.Substring(words_all.IndexOf(rg_1) + rg_1.Length);
            var rg_2 = Regex.Match(words_d__id, @"\D*").ToString();
            ddata.Name = rg_2;
            string words_d__name = (new Regex(@"\D*")).Replace(words_d__id, "", 1);
            var rg_3 = Regex.Match(words_d__name, @"[(男)?(女)?]\d*").ToString();
            ddata.Sex = rg_3.Contains("男") ? 1 : 2;//男1女2
            if(rg_3.Length>=2)
            {
                ddata.Years = Convert.ToInt32(rg_3.Substring(1));
            }
            int i_rg_3 = words_d__name.IndexOf(rg_3);
            var words_d__years = words_d__name.Substring(i_rg_3 + rg_3.Length);
            var rg_4 = Regex.Match(words_d__years, @"[A-Za-z0-9.]*").ToString();
            ddata.MajorDiagnosisCoding = rg_4;//	ICD-10疾病编码(旧)
            var words_d__majorDiagnosisCoding = words_d__years.Replace(rg_4, "");
            string words_MajorDiagnosis_d = Regex.Match(words_d__majorDiagnosisCoding, @"\S*").Value;
            ddata.MajorDiagnosis = words_MajorDiagnosis_d;
            return ddata;
        }
        public static dynamic Record1(string[] paths)
        {
            return Record1(paths[0]);
        }
        public static dynamic Record1(string path)
        {
            dynamic ddata = new System.Dynamic.ExpandoObject();

            var image = File.ReadAllBytes(path);
            var image_c = Identification.BytesCutRight(image);
            JObject jb;
            try
            {
                jb = Identification.IdentifyImage(image_c, ApiVersion.AccurateBasic);
            }
            catch(NoApiTimesException e)
            {
                throw e;
            }

            StringWriter strWriter = new StringWriter();
            StringBuilder sb = strWriter.GetStringBuilder();
            string words_all = "";
            foreach (var jd in jb["words_result"])
            {
                string words = jd["words"].ToString();
                sb.Insert(sb.Length, words);
            }
            words_all = sb.ToString();

            ddata.HistoryOfPastIllness = Regex.Match(words_all, @"(?:既往史:)([\S|\s]*)(?:个人史:)").Groups[1].Value;
            return ddata;
        }
        /// <summary>
        /// 入院记录2号
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <returns>数据</returns>

        public static dynamic Record2(string[] paths)
        {
            return Record2(paths[0]);
        }
        public static dynamic Record2(string path)
        {
            dynamic ddata = new System.Dynamic.ExpandoObject();

            var jb = Identification.IdentifyImage(path, ApiVersion.General);

            int top = 0;
            int left = 0;
            int height = 60;
            int width = 1000;

            foreach (var jd in jb["words_result"])
            {
                string words = jd["words"].ToString();
                if (Convert.ToInt32(words.Contains("体温")) + Convert.ToInt32(words.Contains("脉搏")) + Convert.ToInt32(words.Contains("呼吸")) + Convert.ToInt32(words.Contains("血压")) > 2)
                {
                    top = Convert.ToInt32(jd["location"]["top"].ToString());
                    left = Convert.ToInt32(jd["location"]["left"].ToString());
                    height = Convert.ToInt32(jd["location"]["height"].ToString());
                    width = Convert.ToInt32(jd["location"]["width"].ToString());

                    break;

                }
            }

            Bitmap b = new Bitmap(path);
            var newB = Identification.CutBitmap(b, width, height, top, left);

#if DEBUG
            DEBUG_saveImage(newB);
#endif
            var newImage = BitmapToBytes(newB);
            //var newImage_g = Identification.BytesFilter(newImage,50);
            var newJb = Identification.IdentifyImage(newImage, ApiVersion.AccurateBasic);

            foreach (var jd in newJb["words_result"])
            {
                string words = jd["words"].ToString();

                if (words.Contains("体温") || words.Contains("脉搏") || words.Contains("呼吸") || words.Contains("血压"))
                {
                    string regex = @"-?(([1-9]\d*)|0)(\.\d*[1-9])?";
                    var g = Regex.Matches(words, @"-?(([1-9]\d*)|0)(\.\d*[1-9])?");
                    ddata.Temperature = Convert.ToDouble(g[0].Value);
                    ddata.Pulse = Convert.ToDouble(g[1].Value);
                    ddata.Breath = Convert.ToDouble(g[2].Value);

                    try
                    {
                        if (words.Contains("血压") && g.Count > 4)
                        {
                            ddata.Blood_Pressure_Systolic = Convert.ToDouble(g[3].Value);
                            ddata.Blood_Pressure_Diastolic = Convert.ToDouble(g[4].Value);
                        }
                    }
                    catch (Exception e)
                    {
                        new NoFindContextException("找不到血压");
                    }



                    break;
                }

            }
            return ddata;
        }

        public static dynamic Record3Ex(string[] paths)
        {

            dynamic ddata = new System.Dynamic.ExpandoObject();

            List<byte[]> bss = new List<byte[]>();
            for(int i=0;i<paths.Length;i++)
            {
                try
                {
                    bss.Add(File.ReadAllBytes(paths[i]));
                }
                catch
                {

                }
            }

            var image_c = Identification.BytesLinkRecord3456(bss.ToArray());

            //为眼压剪裁
            var image_cc = Identification.CutBytes(image_c, Identification.BytesToBitmap(image_c).Width, 500, 0, 0);
#if DEBUG
            DEBUG_saveImage(image_cc);
#endif
            JObject jb_ad;
            try
            {
                jb_ad = Identification.IdentifyImage(image_cc, ApiVersion.General);
            }
            catch(NoApiTimesException e)
            {
                throw e;
            }

            int top = 0;
            int left = 0;
            int height = 60;
            int width = Identification.BytesToBitmap(image_c).Width;

            foreach (var jd in jb_ad["words_result"])
            {
                string words = jd["words"].ToString();
                if (words.Contains("眼压")||(words.Contains("眼")&&words.Contains("mmHg")))
                {
                    top = Convert.ToInt32(jd["location"]["top"].ToString());
                    left = Convert.ToInt32(jd["location"]["left"].ToString());
                    height = Convert.ToInt32(jd["location"]["height"].ToString());

                    break;
                }
            }
            var newB = Identification.CutBitmap(Identification.BytesToBitmap(image_c), width, height, top, left);
#if DEBUG
            DEBUG_saveImage(newB);
#endif
            var var_firstwidth = 125;
            var newC = Identification.ReArrange(Identification.BitmapToBytes(newB), new List<(int width, int height, int top, int left)> { (var_firstwidth, height, 0, 0), (+ (width - var_firstwidth * 2) / 2 - var_firstwidth * 1 / 3, height, 0, var_firstwidth), ((width - var_firstwidth) / 2, height, 0, var_firstwidth*2/3 + (width - var_firstwidth*2) / 2) });
#if DEBUG
            DEBUG_saveImage(newC);
#endif
            JObject jb;
            try
            {
                jb = Identification.IdentifyImage(newC, ApiVersion.AccurateBasic);
            }
            catch(NoApiTimesException e)
            {
                throw e;
            }
            

            //StringWriter strWriter = new StringWriter();
            //StringBuilder sb = strWriter.GetStringBuilder();
            //string words_all = "";
            //foreach (var jd in jb["words_result"])
            //{
            //    string words = jd["words"].ToString();
            //    sb.Insert(sb.Length, words);
            //}
            //words_all = sb.ToString();
            var regex_num = new Regex(@"\d+(.\d+)?");
            try
            {
                ddata.Right_Intraocular_Pressure = Convert.ToDouble(regex_num.Match(jb["words_result"][1]["words"].ToString()).Value);

            }
            catch (Exception e)
            {

            }
            try
            {
                ddata.Left_Intraocular_Pressure = Convert.ToDouble(regex_num.Match(jb["words_result"][2]["words"].ToString()).Value);

            }
            catch (Exception e)
            {

            }

            //var regstr = @"(\d+)[^\d]+(\d+)";
            //var a = Regex.Matches(words_all, regstr);
            //ddata.Right_Intraocular_Pressure = Convert.ToDouble(a[0].Groups[0].Value);
            //ddata.Left_Intraocular_Pressure = Convert.ToDouble(a[0].Groups[1].Value);

            //var a = Regex.Matches(words_all, @"(\d+)");



            return ddata;
        }

        public static dynamic TemperatureChart(string[] paths)
        {
            return TemperatureChart(paths[0]);
        }
        /// <summary>
        /// 体温单
        /// </summary>
        /// <param name="path"><图片路径/param>
        /// <returns>数据</returns>
        public static dynamic TemperatureChart(string path)
        {
            dynamic ddata = new System.Dynamic.ExpandoObject();
            var image = File.ReadAllBytes(path);

            JObject jb;
            try
            {
                jb = Identification.IdentifyImage(path, ApiVersion.General);
            }
            catch(NoApiTimesException e)
            {
                throw e;
            }

            


            int top = Int32.MaxValue;
            int left = Int32.MaxValue;
            int height = 70;
            int width = 230;
            var (top_A, left_A, height_A, width_A) = (0, 0, 0, 0);
            var (top_B, left_B, height_B, width_B) = (0, 0, 0, 0);
            var A = 0;
            var B = 0;
            foreach (var jd in jb["words_result"])
            {
                string words = jd["words"].ToString();

                if (words.Contains("体重") || words.Contains("Kg"))
                {
                    A = 1;
                    top_A = Convert.ToInt32(jd["location"]["top"].ToString());
                    left_A = Convert.ToInt32(jd["location"]["left"].ToString());
                    height_A = Convert.ToInt32(jd["location"]["height"].ToString());
                    width_A = Convert.ToInt32(jd["location"]["width"].ToString());
                }
                if (words.Contains("身高") || words.Contains("cm"))
                {
                    B = 1;
                    top_B = Convert.ToInt32(jd["location"]["top"].ToString());
                    left_B = Convert.ToInt32(jd["location"]["left"].ToString());
                    height_B = Convert.ToInt32(jd["location"]["height"].ToString());
                    width_B = Convert.ToInt32(jd["location"]["width"].ToString());
                }
            }

            if (A == 0 && B == 1)
            {
                top = top_B - (int)(height_B * 1.2);
                left = left_B;
                height = height_B + (int)(height_B * 1.2);
                width = (int)(width_B * 3.2);
            }
            else if (A == 1 && B == 0)
            {
                top = top_A;
                left = left_A;
                height = height_A + (int)(height_A * 1.2);
                width = (int)(width_A * 3.2);
            }
            else if (A == 1 && B == 1)
            {
                top = top_A;
                left = Math.Min(left_A, left_B);
                height = height_A + (int)(height_B * 1.2);
                width = (int)(Math.Max(width_A, width_B) * 3.2);
            }
            else
            {
                throw new NoFindContextException("找不到\"体重Kg\"和\"身高cm\"");
            }

            Bitmap b = new Bitmap(path);
            var newB = Identification.CutAndPasteBytes(image, width, height, top-7, left);
#if DEBUG
            DEBUG_saveImage(newB);
#endif
            JObject newJb;
            try
            {
                newJb = Identification.IdentifyImage(newB, ApiVersion.AccurateBasic);
            }
            catch(NoApiTimesException e)
            {
                throw e;
            }
            

            int tag_Kg_cm_none = 3;
            foreach (var jd in newJb["words_result"])
            {
                string words = jd["words"].ToString();
                if(new Regex(@"^[0-9]*$").IsMatch(words))
                {
                    if(tag_Kg_cm_none==3)
                    {
                        ddata.Width = Convert.ToDouble(words);
                        tag_Kg_cm_none = 2;
                    }
                    else if(tag_Kg_cm_none==2)
                    {
                        ddata.Height = Convert.ToDouble(words);
                        tag_Kg_cm_none = 1;
                    }
                    else if(tag_Kg_cm_none==1)
                    {
                        break;
                    }
                }
            }
            return ddata;
        }

        /// <summary>
        /// 住院病案首页
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <returns>数据</returns>
        //public static dynamic IndexPage(string path)
        //{
        //    dynamic ddata = new System.Dynamic.ExpandoObject();

        //    var image = File.ReadAllBytes(path);

        //    var jb = Identification.IdentifyImage(path, ApiVersion.General);

        //    int top=0;
        //    int left=0;
        //    int height=300;
        //    int width=500;

        //    foreach (var jd in jb["words_result"])
        //    {
        //        string words = jd["words"].ToString();
        //        if (words.Contains("年龄") || words.Contains("岁"))
        //        {
        //            top = Convert.ToInt32(jd["location"]["top"].ToString());
        //            left = Convert.ToInt32(jd["location"]["left"].ToString());
        //            height = Convert.ToInt32(jd["location"]["height"].ToString());
        //            width = Convert.ToInt32(jd["location"]["width"].ToString());
        //            break;
        //        }
        //    }


        //    Bitmap b = new Bitmap(path);
        //        //    var newB = Identification.CutBitmap(b, width, height, top, left);
        //#if DEBUG
        //        DEBUG_saveImage(newB);
        //#endif
        //    var newImage = File.ReadAllBytes(path + ".1temp.jpg");
        //    var newImage_g = Identification.BytesFilter(newImage);
        //    var newJb = Identification.IdentifyImage(newImage_g, ApiVersion.AccurateBasic);

        //    foreach (var jd in newJb["words_result"])
        //    {
        //        string words = jd["words"].ToString();

        //        if (words.Contains("年龄") || words.Contains("岁"))
        //        {
        //            ddata.Years = Convert.ToInt32(Regex.Replace(words, @"[^0-9]+", ""));
        //        }
        //    }
        //    return ddata;
        //}

        public static dynamic IndexPage(string[] paths)
        {
            return IndexPage(paths[0]);
        }
        /// <summary>
        /// 住院病案首页(不识别，空，占位用）
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <returns>数据</returns>
        public static dynamic IndexPage(string path)
        {
            dynamic ddata = new System.Dynamic.ExpandoObject();
            var image = File.ReadAllBytes(path);

            JObject jb;
            try
            {
                jb = Identification.IdentifyImage(image, ApiVersion.AccurateBasic);
            }
            catch(NoApiTimesException e)
            {
                throw e;
            }

            

            StringWriter strWriter = new StringWriter();
            StringBuilder sb = strWriter.GetStringBuilder();

            string words_all = "";
            foreach (var jd in jb["words_result"])
            {
                string words = jd["words"].ToString();
                sb.Insert(sb.Length, words);
            }

            words_all = sb.ToString();
            var rg_1 = Regex.Match(words_all, @"龄\d*岁").ToString();
            var rg_2 = Regex.Match(words_all, @"龄\d*月").ToString();

            if(rg_2!="")
            {
                ddata.Years = 1;
            }
            else if(rg_1!="")
            {
                ddata.Years = Convert.ToInt32(rg_1.Substring(1, rg_1.Length - 2));
            }

            return ddata;
        }
        public static dynamic CornealEndotheliumRight(string[] paths)
        {
            return CornealEndotheliumRight(paths[0]);
        }
        /// <summary>
        /// 角膜厚度右
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <returns></returns>
        public static dynamic CornealEndotheliumRight(string path)
        {
            dynamic ddata = new System.Dynamic.ExpandoObject();
            try
            {
                var image = File.ReadAllBytes(path);
            }
            catch
            {
                return null;
            }

            JObject jb;
            try
            {
                jb = Identification.IdentifyImage(path, ApiVersion.GeneralBasic);
            }
            catch(NoApiTimesException e)
            {
                throw e;
            }

            foreach (var jd in jb["words_result"])
            {
                string words = jd["words"].ToString();
                if (words.Contains("角膜厚度") && words.Contains("mm"))
                {
                    ddata.Corneal_Endothelium_Right = Convert.ToDouble(Regex.Match(words, @"-?(([1-9]\d*)|0)(\.\d*[1-9])?").Value);
                    if (ddata.Corneal_Endothelium_Right > 1)
                    {
                        ddata.Corneal_Endothelium_Right /= 1000;
                    }
                    break;
                }
            }

            return ddata;
        }
        public static dynamic CornealEndotheliumLeft(string[] paths)
        {
            return CornealEndotheliumLeft(paths[0]);
        }
        /// <summary>
        /// 角膜厚度左
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <returns></returns>
        public static dynamic CornealEndotheliumLeft(string path)
        {
            dynamic ddata = new System.Dynamic.ExpandoObject();

            try
            {
                var image = File.ReadAllBytes(path);
            }
            catch
            {
                return null;
            }

            JObject jb;
            try
            {
                jb = Identification.IdentifyImage(path, ApiVersion.GeneralBasic);
            }
            catch (NoApiTimesException e)
            {
                throw e;
            }

            foreach (var jd in jb["words_result"])
            {
                string words = jd["words"].ToString();
                if (words.Contains("角膜厚度") && words.Contains("mm"))
                {
                    ddata.Corneal_Endothelium_Left = Convert.ToDouble(Regex.Match(words, @"-?(([1-9]\d*)|0)(\.\d*[1-9])?").Value);
                    if(ddata.Corneal_Endothelium_Left>1)
                    {
                        ddata.Corneal_Endothelium_Left /= 1000;//   f/10**floor(log10(f))
                    }
                    break;
                }
            }

            return ddata;
        }
    }
}
