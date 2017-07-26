using System;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;

namespace EdifToSystemC
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = System.Reflection.Assembly.GetEntryAssembly().Location.
                Remove(System.Reflection.Assembly.GetEntryAssembly().Location.Length - 17);
            openFileDialog1.ShowDialog();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.InitialDirectory = System.Reflection.Assembly.GetEntryAssembly().Location.
                Remove(System.Reflection.Assembly.GetEntryAssembly().Location.Length - 17);
            saveFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try {
                textBoxOpenFile.Text = openFileDialog1.FileName;
                textBoxOpenFile.SelectionStart = textBoxOpenFile.Text.Length - 1;
                textBoxOpenFile.SelectionLength = 0;
                if (textBoxSaveFile.Text == "") { //если имя выходного файла не задано, задать по умлочанию
                    textBoxSaveFile.Text = openFileDialog1.FileName.Remove(openFileDialog1.FileName.Length - 4) + ".h";
                    textBoxSaveFile.SelectionStart = textBoxSaveFile.Text.Length - 1;
                    textBoxSaveFile.SelectionLength = 0;
                }
            }
            catch { }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try {
                textBoxSaveFile.Text = saveFileDialog1.FileName;
                textBoxSaveFile.SelectionStart = textBoxSaveFile.Text.Length - 1;
                textBoxSaveFile.SelectionLength = 0;
            }
            catch { }
        }

        private void buttonExecute_Click(object sender, EventArgs e)
        {
            //начало отсчета времени выполнения преобразования
            var watch = System.Diagnostics.Stopwatch.StartNew();

            String EDIFstr ,   //строковое представление EDIF файла
                   SCstr = "";      //строковое представление SystemC файла

            //чтение из EDIF файла
            try
            {
                using (StreamReader sr = new StreamReader(textBoxOpenFile.Text))
                {
                    EDIFstr = sr.ReadToEnd();
                    if (EDIFstr.Length > 0)
                    {                                   //если файл не пустой
                        if (EDIFstr.IndexOf("edifVersion 2 0 0") > 0)
                        {         //если в файле используется версия EDIF 2 0 0
                            //Анализ EDIF файла
                            Convertation.AnalyseEDIF(EDIFstr);  //выполнение анализа EDIF файла
                            textBoxInfo.Text = Convertation.GetSchemeInfo(); //вывод информации о файле

                            //Создание строки с описанием на SystemC
                            SCstr = Convertation.GetSystemCCode();

                            Convertation.Modules.Clear();   //очистка списка модулей
                        }
                        else MessageBox.Show("EDIF 2 0 0 file required.", "EDIF file version error",
                                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else MessageBox.Show("Empty file.", "EDIF file error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (NullReferenceException expt) { MessageBox.Show(expt.Message, "EDIF analysis error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (ArgumentException expt) { MessageBox.Show(expt.Message, "EDIF analysis error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception expt) {MessageBox.Show(expt.Message, "EDIF reading error", MessageBoxButtons.OK, MessageBoxIcon.Error);}

            try {
                File.WriteAllText(textBoxSaveFile.Text,SCstr);
            }
            catch (Exception expt) { MessageBox.Show(expt.Message, "SystemC writing error", MessageBoxButtons.OK, MessageBoxIcon.Error); }

            //окончание отсчета времени выполнения преобразования
            watch.Stop();
            labelExecutionTime.Text = Convert.ToString(Math.Round(watch.Elapsed.TotalMilliseconds * 1000)) + " "+'\u03bc' +"s";
        }

    }
}
