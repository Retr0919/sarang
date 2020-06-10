using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageFilter
{

    // Класс конфигурации элемента управления
    public class FilterControl
    {
        // Название поля
        private string name;
        // Элемент управления
        private Control control;

        //Конструктор
        public FilterControl(string name, Control control)
        {
            this.name = name;
            this.control = control;
        }

        // геттеры и сеттеры
        public string Name { get => name; set => name = value; }
        public Control Control { get => control; set => control = value; }
    }
}
