using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Transcribe
{
    public class ResetPictures
    {
        public List<byte[]> originImages = new List<byte[]>();

        public List<(int reindex, int index, int width, int height, int top, int left)> ranges = new List<(int reindex, int index, int width, int height, int top, int left)>();

        public List<byte[]> rearrayImages = new List<byte[]>();

        public dynamic ddata = new System.Dynamic.ExpandoObject();

        public ResetPictures()
        {
        }

        public ResetPictures(string[] paths)
        {
            this.LoadPictures(paths);
        }

        public void LoadPictures(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                try
                {
                    this.originImages.Add(File.ReadAllBytes(paths[i]));
                }
                catch
                {

                }
            }
        }

        public void ReArray()
        {

            List<List<byte[]>> re_ranges=new List<List<byte[]>>();
            int temp_lengthRe= ranges.Select(n => n.reindex).ToList().Max()+1;
            for (int index_re=0;index_re< temp_lengthRe; index_re++)
            {
                re_ranges.Add(new List<byte[]>());
            }

            for(int index_ir=0;index_ir<ranges.Count;index_ir++)
            {
                (var ci_ri, var ci_i, var ci_w, var ci_h, var ci_t, var ci_l) = ranges[index_ir];

                re_ranges[ci_ri].Add(Identification.CutBytes(originImages[ci_i], ci_w, ci_h, ci_t, ci_l));

            }

            for(int index_re=0;index_re<re_ranges.Count;index_re++)
            {
                rearrayImages.Add(Identification.MegerBytesRow(re_ranges[index_re]));
            }
            
        }

    }
}
