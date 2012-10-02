using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Gredactor
{
    public interface IEffect
    {
        /// <summary>
        /// Название эффекта
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Описание, используемое при консольном запуске
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Позволяет выполнить действия, необходимые до начала обработки изображения
        /// </summary>
        /// <param name="obj">Если console == true, то строка аргумента (при наличии)</param>
        /// <param name="console">Если true, значит программа запущена в консольном варианте</param>
        /// <returns>true при отсутствии ошибок. Иначе следует прекратить работу с данным эффектом</returns>
        bool Prepare(object obj, bool console = false);

        /// <summary>
        /// Выполняет преобразование над изображением. Сохранность переданного параметра не гарантируется
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        Bitmap Apply(Bitmap original);

        /// <summary>
        /// В какой группе меню разместить пункт меню, либо ""
        /// </summary>
        string MenuGroup { get; }
        ToolStripMenuItem MenuItem { get; }

        /// <summary>
        /// Кнопка, которая будет вынесена на левую панель
        /// </summary>
        Button Button { get; }

        /// <summary>
        /// Короткий ключ для запуска из консоли (без "-")
        /// </summary>
        char ShortConsoleKey { get; }

        /// <summary>
        /// Длинный ключ для запуска из консоли (без "--")
        /// </summary>
        string LongConsoleKey { get; }

        /// <summary>
        /// Строка параметров, требуемых из консоли
        /// </summary>
        string ConsoleParams { get; }
    }
}
