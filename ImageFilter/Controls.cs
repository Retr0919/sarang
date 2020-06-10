using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageFilter
{
    public partial class Controls : Form
    {
        private List<FilterControl> filterControls;
        // главный Класс
        private MainForm mainForm;
        // индекс фильтра
        private int index = -1;

        public List<FilterControl> FilterControls { get => filterControls; set => filterControls = value; }

        // конструктор
        // Controls(List<FilterControl> filterControls - список с конфигурациями
        // MainForm mainForm - объект главного класса
        public Controls(List<FilterControl> filterControls, MainForm mainForm)
        {
            InitializeComponent();
            this.filterControls = filterControls;
            this.mainForm = mainForm;

            // отрысовываем элементы
            DrawControls();
        }

        // Функция для отрисовки элементов
        private void DrawControls()
        {
            // очищаем tableLayout, если в нем что то было
            tableLayoutPanel.RowStyles.Clear();
            tableLayoutPanel.Controls.Clear();

            // если конфигурация null, то скрываем окно и прекращаем выполнение функции
            if (this.FilterControls == null)
            {
                this.Hide();
                return;
            }  

            // номер инерации
            int index = 0;
            
            // проходимся по всем объектам filterControl в списке
            foreach(var filterControl in this.FilterControls)
            {
                // Создаем новые объект типа Label
                Label label = new Label();
                // Меняем его текст на название элемента
                label.Text = filterControl.Name;

                //создаем новую строку в tableLayoutPanel
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
                //добавляем туда элемент управления
                tableLayoutPanel.Controls.Add(label, 0, index);
                tableLayoutPanel.Controls.Add(filterControl.Control, 1, index);

                index++;
            }

            //перезагружаем лэйаут
            tableLayoutPanel.Refresh();
            //показываем окно
            this.Show();
        }

        //обновление конфигурации
        public void Update(int index)
        {
            this.index = index;
            
            DrawControls();
        }

        // обновление конфигурации
        public void Update(List<FilterControl> filterControls, int index)
        {
            this.filterControls = filterControls;
            this.index = index;

            DrawControls();
        }

        private void Controls_Load(object sender, EventArgs e)
        {
            
        }

        // обработка нажатия на применить
        private void submitButton_Click(object sender, EventArgs e)
        {
            // приводим все к типу NumericUpDown и считываем значения всех элементов управления
            List<double> args = (from c in filterControls select (double)(c.Control as NumericUpDown).Value).ToList();

            //вызываем функцию из главного класса для пересчета фильтров с параметрами
            mainForm.ReprocessWithParams(index, args);
        }

        // обработка закрытия окна
        private void Controls_FormClosing(object sender, FormClosingEventArgs e)
        {
            // скрываем окно
            this.Hide();
            // отменяем полное закрытие
            e.Cancel = true;
        }
    }
}
