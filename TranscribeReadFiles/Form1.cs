using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Transcribe;

namespace TranscribeReadFiles
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 记录映射表，规定函数和文件名
        /// </summary>
        List<RecordFunctionMap> rfms = new List<RecordFunctionMap> {
            new RecordFunctionMap() { Name = "Name", FunctionName = "Name", ImageFiles = new string[] { "Name.png" } },
            new RecordFunctionMap() { Name = "IndexPage", FunctionName = "IndexPage", ImageFiles = new string[] { "IndexPage.png" } },
            new RecordFunctionMap() { Name = "Record1", FunctionName = "Record1", ImageFiles = new string[] { "Record1.png" } },
            new RecordFunctionMap() { Name = "Record2", FunctionName = "Record2", ImageFiles = new string[] { "Record2.png" } },
            new RecordFunctionMap() { Name = "Record3Ex", FunctionName = "Record3Ex", ImageFiles = new string[] { "Record3.png","Record4.png","Record5.png","Record6.png" } },
            new RecordFunctionMap() { Name = "TemperatureChart", FunctionName = "TemperatureChart", ImageFiles = new string[] { "TemperatureChart.png" } },
            new RecordFunctionMap() { Name = "CornealEndotheliumRight", FunctionName = "CornealEndotheliumRight", ImageFiles = new string[] { "CornealEndotheliumRight.png" } },
            new RecordFunctionMap() { Name = "CornealEndotheliumLeft", FunctionName = "CornealEndotheliumLeft", ImageFiles = new string[] { "CornealEndotheliumLeft.png" } }
        };

        public Form1()
        {
            InitializeComponent();

            //this.richTextBox1.DataBindings.Add("Text", dsb, "TextResult");
        }

        DataSetBinding dsb = new DataSetBinding();

        string folderPath;
        Thread trfThread;
        Thread watchThread;
        private void buttonFoldersRead_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {

                folderPath = folderBrowserDialog1.SelectedPath;
            }




        }

        private string ReadDataFromFloder(string path)
        {
            string sdata = "";

            //string[] methodName={"Name","IndexPage","Record1","Record2", "Record3","Record4","Record5", "CornealEndotheliumRight", "CornealEndotheliumLeft", "TemperatureChart" };

            var context = new OriginDataContext();

            OriginData od = new OriginData();

            Transcribe.Transcribe.Link();

            DirectoryInfo theFolder = new DirectoryInfo(path);
            FileInfo[] fileInfo = theFolder.GetFiles();


            var years_in_name = 0;
            od.FilesId = theFolder.Name;

            //Record3Ex所需图片
            var recordexs = new List<string>();

            dynamic ddatan;

            foreach (var NextFile in fileInfo)
            {
                var smn = Path.GetFileNameWithoutExtension(NextFile.FullName);

                //提出所有的Record3Ex所需图片
                if (smn == "Record3" || smn == "Record4" || smn == "Record5" || smn == "Record6")
                {
                    recordexs.Add(NextFile.FullName);
                    continue;
                }

                var dndm = typeof(Transcribe.Transcribe).GetMethod(smn);
                try
                {

                    if (dndm != null)
                    {
                        try
                        {
                            ddatan = (dynamic)dndm.Invoke(null, new object[] { NextFile.FullName });
                        }
                        catch (TargetInvocationException tie)
                        {
                            if (tie.InnerException is NoApiTimesException)
                            {
                                Identification.SetAPPID("15495009", "GiErMFTsh5KGLLCjLfD6mmTm", "jQopZT1XCdTnAGubnxHW1s4eFWpVbtZs");
                                //对下面起不到作用，因为两个组件用的不是一个dll，改了这个还有那个，，，
                                ddatan = (dynamic)dndm.Invoke(null, new object[] { NextFile.FullName });
                            }
                            else
                            {
                                throw tie.InnerException;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }

                    //if (smn == "Name")
                    //{
                    //    if (((IDictionary<String, Object>)ddatan).ContainsKey("Years"))
                    //    {
                    //        years_in_name = ddatan.Years;
                    //    }
                    //}

                    //if (smn == "IndexPage")
                    //{
                    //    if (years_in_name != 0)
                    //    {
                    //        continue;
                    //    }
                    //}

                    foreach (var property in (IDictionary<String, Object>)ddatan)
                    {

                        var propertyInfo = od.GetType().GetProperty(property.Key);

                        //haven't debug
                        if (od.Years == 0 && smn == "IndexPage" && property.Key == "Years")
                        {
                            propertyInfo.SetValue(od, property.Value, null);
                        }
                        else if (smn != "IndexPage")
                        {
                            propertyInfo.SetValue(od, property.Value, null);
                        }

                    }

                }
                catch (TargetInvocationException e)
                {
                    this.SetResultText(e.InnerException.Message);
                }
                catch (Exception e)
                {
                    this.SetResultText(e.Message);
                }
            }


            //Record3Ex 特殊处理
            try
            {
                if (recordexs.Count > 0)
                {
                    ddatan = Transcribe.Transcribe.Record3Ex(recordexs.ToArray());
                    foreach (var property in (IDictionary<String, Object>)ddatan)
                    {
                        var propertyInfo = od.GetType().GetProperty(property.Key);
                        propertyInfo.SetValue(od, property.Value, null);
                    }
                }

            }
            catch (TargetInvocationException e)
            {
                this.SetResultText(e.InnerException.Message);
            }

            if (od.CaseHistoryId != 0)
            {
                try
                {
                    DbEntityEntry<OriginData> entityEntry = context.Entry<OriginData>(od);
                    entityEntry.State = EntityState.Added;
                    context.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    this.SetResultText(od.FilesId + ":" + od.Name + "  faild: " + e.EntityValidationErrors.ToArray()[0].ValidationErrors.ToArray()[0].ErrorMessage);
                }
                catch (DbUpdateException e)
                {
                    this.SetResultText(od.FilesId + ":" + od.Name + "  faild: " + e.Message);
                }

                this.SetResultText(od.FilesId + ":" + od.Name + "   done");
            }
            else
            {
                this.SetResultText(od.FilesId + ":" + od.Name + "   faild");
            }

            return sdata;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //初始化数据库
            Database.SetInitializer(new CreateDatabaseIfNotExists<OriginDataContext>());
            //实体化数据库对你

            trfThread = new Thread(ReadDataFromFloders);
            trfThread.Start();
        }

        private void ReadDataFromFloders()
        {
            DirectoryInfo theFolder = new DirectoryInfo(folderPath);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();

            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                ReadDataFromFloder(NextFolder.FullName);
            }

            MessageBox.Show("finished!");
        }

        public void SetResultText(String text)
        {
            if (this.richTextBox1.InvokeRequired)
            {
                this.richTextBox1.Invoke(new Action(() => this.richTextBox1.Text += text));
            }
            else
            {
                this.richTextBox1.Text += text;
            }
        }

        public void UpdateDataFromFloder(string path)
        {
            DirectoryInfo theFolder = new DirectoryInfo(path);
            FileInfo[] fileInfo = theFolder.GetFiles();

            var context = new OriginDataContext();
            OriginData qod = new OriginData();

            Transcribe.Transcribe.Link();

            qod.FilesId = theFolder.Name;


            dynamic dod = new ExpandoObject();
            foreach (RecordFunctionMap rfm in this.rfms)
            {
                //图像文件目录
                var args = rfm.ImageFiles.Select(x => Path.Combine(theFolder.FullName, x)).ToArray();

                try
                {

                    //识别
                    var ddatan = (dynamic)rfm.Run(args);

                    //赋值
                    foreach (var property in (IDictionary<String, Object>)ddatan)
                    {
                        try
                        {
                            //年龄特殊处理
                            if (dod.Years == 0 && rfm.Name == "IndexPage" && property.Key == "Years")
                            {
                                ((IDictionary<String, Object>)dod)[property.Key] = property.Value;
                            }
                            else if (rfm.Name != "IndexPage")
                            {
                                ((IDictionary<String, Object>)dod)[property.Key] = property.Value;
                            }
                        }
                        catch(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
                        {
                            ((IDictionary<String, Object>)dod)[property.Key] = property.Value;
                        }


                    }

                }
                catch (TargetInvocationException tie)
                {
                    if (tie.InnerException is NoApiTimesException)
                    {
                        MessageBox.Show("API不足");
                    }
                    else
                    {
                        throw tie.InnerException;
                    }
                }
                catch
                {

                }

            }


            try
            {
                try
                {
                    qod = context.OriginDatas.First(x => x.FilesId == qod.FilesId);
                }
                catch (Exception ex)
                {

                    DbEntityEntry<OriginData> entityEntry = context.Entry<OriginData>(qod);
                    entityEntry.State = EntityState.Added;

                }
                //更新记录 Update
                foreach (var property in (IDictionary<String, Object>)dod)
                {
                    var propertyInfo = qod.GetType().GetProperty(property.Key);
                    propertyInfo.SetValue(qod, property.Value, null);
                }

                context.SaveChanges();

            }
            catch (DbEntityValidationException e)
            {
                this.SetResultText(qod.FilesId + ":" + qod.Name + "  faild: " + e.EntityValidationErrors.ToArray()[0].ValidationErrors.ToArray()[0].ErrorMessage);
            }
            catch (DbUpdateException e)
            {
                this.SetResultText(qod.FilesId + ":" + qod.Name + "  faild: " + e.Message);
            }

            this.SetResultText(qod.FilesId + ":" + qod.Name + "   done");

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            //初始化数据库
            Database.SetInitializer(new CreateDatabaseIfNotExists<OriginDataContext>());
            //实体化数据库对你

            trfThread = new Thread(()=> {
                DirectoryInfo theFolder = new DirectoryInfo(folderPath);
                DirectoryInfo[] dirInfo = theFolder.GetDirectories();

                foreach (DirectoryInfo NextFolder in dirInfo)
                {
                    UpdateDataFromFloder(NextFolder.FullName);
                }

                MessageBox.Show("finished!");
            });
            trfThread.Start();
        }
    }
}
