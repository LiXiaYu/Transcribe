using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Transcribe.Identification;

namespace Transcribe
{
    public class PatentTranscribe
    {
        public virtual dynamic Transcribe(string[] paths)
        {

            Identification.Link();

            ResetPictures reset = new ResetPictures();
            this.LoadPictures(reset,paths);
            this.CutPicture(reset);
            this.ReIdentificate(reset);

            return reset.ddata;
        }
        public virtual dynamic Transcribe(string[] paths,dynamic ddata)
        {

            Identification.Link();

            ResetPictures reset = new ResetPictures();
            reset.ddata = ddata;
            this.LoadPictures(reset, paths);
            this.CutPicture(reset);
            this.ReIdentificate(reset);

            return reset.ddata;
        }

        public virtual void LoadPictures(ResetPictures reset, string[] paths)
        {
            reset.LoadPictures(paths);
        }

        /// <summary>
        /// 厘定要切割的部分，并重新排列。基类默认方法为直接把所有图片拼接起来。
        /// </summary>
        /// <param name="reset">图片集</param>
        public virtual void CutPicture(ResetPictures reset)
        {
            for(int index_originImage=0;index_originImage<reset.originImages.Count;index_originImage++)
            {
                (var temp_w,var temp_h) = Identification.GetBytesSize(reset.originImages[index_originImage]);
                reset.ranges.Add((0,index_originImage, temp_w, temp_h, 0, 0));
            }

            reset.ReArray();
        }
        public virtual void ReIdentificate(ResetPictures reset)
        {
            
            List<JObject> jbs = new List<JObject>();

            foreach(var image in reset.rearrayImages)
            {
                try
                {
                    jbs.Add(Identification.IdentifyImage(image, ApiVersion.General));
                }
                catch (NoApiTimesException e)
                {
                    throw e;
                }

            }

            reset.ddata= new System.Dynamic.ExpandoObject();

        }
    }


    public class PageNameTranscribe : PatentTranscribe
    {
        private int cutImageNumber;

        public override void CutPicture(ResetPictures reset)
        {
            //var originImages_cf = reset.originImages.Select(n => Identification.BytesColorFilter(n, 70)).ToList();
            var image = reset.originImages[0];
            var image_cf = Identification.BytesColorFilter(image, 70);

#if DEBUG
            DEBUG_saveImage(image_cf);
#endif
            var cutimages = Identification.BytesCutColumn(image_cf, 7);

#if DEBUG
            DEBUG_save(cutimages);
#endif
            this.cutImageNumber = cutimages.Count;
            cutimages.RemoveAt(6);
            cutimages.RemoveAt(5);
            cutimages.RemoveAt(4);
            cutimages.RemoveAt(3);
            cutimages.RemoveAt(2);
            //var jb = Identification.IdentifyImage(image_cf, ApiVersion.AccurateBasic);
            //base.CutPicture(reset);
            //var xx = 1;
            reset.rearrayImages.Add(Identification.MegerBytesRow(cutimages,1));

#if DEBUG
            DEBUG_save(reset.rearrayImages);
#endif

        }

        
        public override void ReIdentificate(ResetPictures reset)
        {
            JObject jb = new JObject();

            try
            {
                jb=Identification.IdentifyImage(reset.rearrayImages[0], ApiVersion.GeneralBasic);
            }
            catch (NoApiTimesException e)
            {
                throw e;
            }

            reset.ddata.CaseHistoryId = Convert.ToInt32(Regex.Match(jb["words_result"][0]["words"].ToString(), @"\d*").Value);
            reset.ddata.Name = jb["words_result"][1]["words"].ToString();
            reset.ddata.Sex= jb["words_result"][2]["words"].ToString().Contains("男") ? 1 : 2;//男1女2

            //也许直接用jb["word_result"]的内容数量就可以判定
            if(this.cutImageNumber==10)
            {
                reset.ddata.Years = Convert.ToInt32(jb["words_result"][3]["words"].ToString());
                reset.ddata.MajorDiagnosisCoding = jb["words_result"][4]["words"].ToString();
                reset.ddata.MajorDiagnosis = jb["words_result"][5]["words"].ToString();
            }
            else
            {
                reset.ddata.MajorDiagnosisCoding = jb["words_result"][3]["words"].ToString();
                reset.ddata.MajorDiagnosis = jb["words_result"][4]["words"].ToString();
            }

        }
    }

    public class PageTemperatureChartTranscribe : PatentTranscribe
    {
        public override void CutPicture(ResetPictures reset)
        {
            var image = reset.originImages[0];

            JObject jb;
            try
            {
                jb = Identification.IdentifyImage(image, ApiVersion.General);
            }
            catch (NoApiTimesException e)
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


            reset.rearrayImages.Add(Identification.CutAndPasteBytes(image, width, height, top - 7, left));
            //base.CutPicture(reset);
        }

        public override void ReIdentificate(ResetPictures reset)
        {
            JObject jb = new JObject();

            try
            {
                jb.Add(Identification.IdentifyImage(reset.rearrayImages[0], ApiVersion.GeneralBasic));
            }
            catch (NoApiTimesException e)
            {
                throw e;
            }

            int tag_Kg_cm_none = 3;
            foreach (var jd in jb["words_result"])
            {
                string words = jd["words"].ToString();
                if (new Regex(@"^[0-9]*$").IsMatch(words))
                {
                    if (tag_Kg_cm_none == 3)
                    {
                        reset.ddata.Width = Convert.ToDouble(words);
                        tag_Kg_cm_none = 2;
                    }
                    else if (tag_Kg_cm_none == 2)
                    {
                        reset.ddata.Height = Convert.ToDouble(words);
                        tag_Kg_cm_none = 1;
                    }
                    else if (tag_Kg_cm_none == 1)
                    {
                        break;
                    }
                }
            }

        }
    }

    public class PageCornealEndotheliumRightTranscribe : PatentTranscribe
    {
        public override void ReIdentificate(ResetPictures reset)
        {
            JObject jb;
            try
            {
                jb = Identification.IdentifyImage(reset.rearrayImages[0], ApiVersion.GeneralBasic);
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
                    reset.ddata.Corneal_Endothelium_Right = Convert.ToDouble(Regex.Match(words, @"-?(([1-9]\d*)|0)(\.\d*[1-9])?").Value);
                    if (reset.ddata.Corneal_Endothelium_Right > 1)
                    {
                        reset.ddata.Corneal_Endothelium_Right /= 1000;
                    }
                    break;
                }
            }

        }
    }

    public class PageCornealEndotheliumLeftTranscribe : PageCornealEndotheliumRightTranscribe
    {
        //public override void ReIdentificate(ResetPictures reset)
        //{
        //    JObject jb;
        //    try
        //    {
        //        jb = Identification.IdentifyImage(reset.rearrayImages[0], ApiVersion.GeneralBasic);
        //    }
        //    catch (NoApiTimesException e)
        //    {
        //        throw e;
        //    }

        //    foreach (var jd in jb["words_result"])
        //    {
        //        string words = jd["words"].ToString();
        //        if (words.Contains("角膜厚度") && words.Contains("mm"))
        //        {
        //            reset.ddata.Corneal_Endothelium_Left = Convert.ToDouble(Regex.Match(words, @"-?(([1-9]\d*)|0)(\.\d*[1-9])?").Value);
        //            if (reset.ddata.Corneal_Endothelium_Left > 1)
        //            {
        //                reset.ddata.Corneal_Endothelium_Left /= 1000;
        //            }
        //            break;
        //        }
        //    }

        //}

        public override void ReIdentificate(ResetPictures reset)
        {
            base.ReIdentificate(reset);
            reset.ddata.Corneal_Endothelium_Left = reset.ddata.Corneal_Endothelium_Right;
            (reset.ddata as IDictionary<string, object>).Remove("Corneal_Endothelium_Right");
        }
    }

    public class PageRecord1Transcribe : PatentTranscribe
    {
        public override void CutPicture(ResetPictures reset)
        {
            var image = reset.originImages[0];
            var image_c = Identification.BytesCutRight(image);

            reset.rearrayImages.Add(image_c);
        }

        public override void ReIdentificate(ResetPictures reset)
        {
            
            JObject jb;
            try
            {
                jb = Identification.IdentifyImage(reset.rearrayImages[0], ApiVersion.AccurateBasic);
            }
            catch (NoApiTimesException e)
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

            reset.ddata.HistoryOfPastIllness = Regex.Match(words_all, @"(?:既往史:)([\S|\s]*)(?:个人史:)").Groups[1].Value;

        }

    }

    public class PageRecord2Transcribe : PatentTranscribe
    {
        public override void CutPicture(ResetPictures reset)
        {
            var jb = Identification.IdentifyImage(reset.originImages[0], ApiVersion.General);

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

            reset.rearrayImages.Add(Identification.CutBytes(reset.originImages[0], width, height, top, left));

        }

        public override void ReIdentificate(ResetPictures reset)
        {
            
            //var newImage_g = Identification.BytesFilter(newImage,50);
            var newJb = Identification.IdentifyImage(reset.rearrayImages[0], ApiVersion.AccurateBasic);

            foreach (var jd in newJb["words_result"])
            {
                string words = jd["words"].ToString();

                if (words.Contains("体温") || words.Contains("脉搏") || words.Contains("呼吸") || words.Contains("血压"))
                {
                    string regex = @"-?(([1-9]\d*)|0)(\.\d*[1-9])?";
                    var g = Regex.Matches(words, @"-?(([1-9]\d*)|0)(\.\d*[1-9])?");
                    reset.ddata.Temperature = Convert.ToDouble(g[0].Value);
                    reset.ddata.Pulse = Convert.ToDouble(g[1].Value);
                    reset.ddata.Breath = Convert.ToDouble(g[2].Value);

                    try
                    {
                        if (words.Contains("血压") && g.Count > 4)
                        {
                            reset.ddata.Blood_Pressure_Systolic = Convert.ToDouble(g[3].Value);
                            reset.ddata.Blood_Pressure_Diastolic = Convert.ToDouble(g[4].Value);
                        }
                    }
                    catch (Exception e)
                    {
                        new NoFindContextException("找不到血压");
                    }



                    break;
                }

            }

        }
    }

    public class PageRecord3ExTranscribe : PatentTranscribe
    {
        public override void CutPicture(ResetPictures reset)
        {
            var image_c = Identification.MegerBytesRow(reset.originImages);
            var image_cc = Identification.CutBytes(image_c, Identification.GetBytesSize(image_c).width, 500, 0, 0);

            JObject jb_ad;
            try
            {
                jb_ad = Identification.IdentifyImage(image_cc, ApiVersion.General);
            }
            catch (NoApiTimesException e)
            {
                throw e;
            }

            int top = 0;
            int left = 0;
            int height = 60;
            int width = Identification.GetBytesSize(image_c).width;

            foreach (var jd in jb_ad["words_result"])
            {
                string words = jd["words"].ToString();
                if (words.Contains("眼压") || (words.Contains("眼") && words.Contains("mmHg")))
                {
                    top = Convert.ToInt32(jd["location"]["top"].ToString());
                    left = Convert.ToInt32(jd["location"]["left"].ToString());
                    height = Convert.ToInt32(jd["location"]["height"].ToString());

                    break;
                }
            }
            var newB = Identification.CutBytes(image_cc, width, height, top, left);
            //var var_firstwidth = 125;

            var cbs=Identification.BytesCutColumn(newB,7,0.1);
            var rei = Identification.MegerBytesRow(new List<byte[]> { Identification.MegerBytesColumn(new List<byte[]> { cbs[0], cbs[1] }), cbs[2], cbs[3] });
            reset.rearrayImages.Add(rei);
        }

        public override void ReIdentificate(ResetPictures reset)
        {
            JObject jb;
            try
            {
                jb = Identification.IdentifyImage(reset.rearrayImages[0], ApiVersion.AccurateBasic);
            }
            catch (NoApiTimesException e)
            {
                throw e;
            }

            var regex_num = new Regex(@"\d+(.\d+)?");
            try
            {
                reset.ddata.Right_Intraocular_Pressure = Convert.ToDouble(regex_num.Match(jb["words_result"][1]["words"].ToString()).Value);

            }
            catch (Exception e)
            {

            }
            try
            {
                reset.ddata.Left_Intraocular_Pressure = Convert.ToDouble(regex_num.Match(jb["words_result"][2]["words"].ToString()).Value);

            }
            catch (Exception e)
            {

            }


        }
    }

    public class PageIndexTranscribe : PatentTranscribe
    {
        public override void ReIdentificate(ResetPictures reset)
        {
            JObject jb;
            try
            {
                jb = Identification.IdentifyImage(reset.rearrayImages[0], ApiVersion.AccurateBasic);
            }
            catch (NoApiTimesException e)
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

            if (rg_2 != "")
            {
                reset.ddata.Years = 1;
            }
            else if (rg_1 != "")
            {
                reset.ddata.Years = Convert.ToInt32(rg_1.Substring(1, rg_1.Length - 2));
            }

        }
    }
}
