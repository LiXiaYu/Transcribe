using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Transcribe;

namespace TranscribeData
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Init();
        }

        private static void Init()
        {

        }

        public void GeneralBasicDemo()
        {

            // 如果有可选参数
            var options = new Dictionary<string, object>{
                {"language_type", "CHN_ENG"},
                {"detect_direction", "true"},
                {"detect_language", "true"},
                {"probability", "true"}
            };

            Transcribe.Transcribe.Link();

            //var name = Transcribe.Transcribe.Record1(@"D:\programming\Transcribe\image_test\3cb644b5-9df0-4b53-a3e6-b760eba4e125\Record1.png");
            //foreach (var property in (IDictionary<String, Object>)name)
            //{
            //    richTextBox1.Text += "" + property.Key + ": " + property.Value + "\n";
            //}
            //var name = Transcribe.Transcribe.Name(@"D:\programming\Transcribe\image_1\20190310192957_325d046d-983c-4d5e-a949-2f594b56dbb5\Name.png");
            //foreach (var property in (IDictionary<String, Object>)name)
            //{
            //    richTextBox1.Text += "" + property.Key + ": " + property.Value + "\n";
            //}
            //var indexpage= Transcribe.Transcribe.IndexPage(@"D:\programming\Transcribe\image_1\20190310192957_325d046d-983c-4d5e-a949-2f594b56dbb5\IndexPage.png");
            //foreach (var property in (IDictionary<String, Object>)indexpage)
            //{
            //    richTextBox1.Text += "" + property.Key + ": " + property.Value + "\n";
            //}
            var r3ex = Transcribe.Transcribe.Record3Ex(new string[] { @"D:\programming\Transcribe\image_1_2\20190310194539_cf9fe050-ea2f-4b56-b4bd-a9444c96e090\Record3.png", @"D:\programming\Transcribe\image_1_2\20190310194539_cf9fe050-ea2f-4b56-b4bd-a9444c96e090\Record4.png" });
            foreach (var property in (IDictionary<String, Object>)r3ex)
            {
                richTextBox1.Text += "" + property.Key + ": " + property.Value + "\n";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GeneralBasicDemo();

            //DemoNew();

        }

        private void DemoNew()
        {
            PageNameTranscribe pageNameTranscriber = new PageNameTranscribe();
            var ddata=pageNameTranscriber.Transcribe(new string[] { @"D:\programming\Transcribe\image_1\20190310192957_325d046d-983c-4d5e-a949-2f594b56dbb5\Name.png" });

            PatentTranscribe pt = new PageRecord3ExTranscribe();
            var r3ex = pt.Transcribe(new string[] { @"D:\programming\Transcribe\image_1_2\20190310194539_cf9fe050-ea2f-4b56-b4bd-a9444c96e090\Record3.png", @"D:\programming\Transcribe\image_1_2\20190310194539_cf9fe050-ea2f-4b56-b4bd-a9444c96e090\Record4.png" });
            foreach (var property in (IDictionary<String, Object>)r3ex)
            {
                richTextBox1.Text += "" + property.Key + ": " + property.Value + "\n";
            }
        }
    }
}
