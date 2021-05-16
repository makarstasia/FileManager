using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;              // Каталог каталога
using System.Reflection; // Перечисление BindingFlags

namespace MyFileManager
{
    //Про инструменты папки "недавние" на компьютере, Win + R, ввод Недавние можно назвать
    class RecentFilesUtil
    {
        // Получить целевой путь (реальный путь) ярлыка в соответствии с именем ярлыка (полный путь)
        public static string GetShortcutTargetFilePath(string shortcutFilename)
        {
            // Получаем тип WScript.Shell
            var type = Type.GetTypeFromProgID("WScript.Shell");

            // Создать экземпляр этого типа
            object instance = Activator.CreateInstance(type);

            var result = type.InvokeMember("CreateShortCut", BindingFlags.InvokeMethod, null, instance, new object[] { shortcutFilename });
            // Получаем целевой путь (реальный путь）
            var targetFilePath = result.GetType().InvokeMember("TargetPath", BindingFlags.GetProperty, null, result, null) as string;

            return targetFilePath;
        }
        // Получаем перечисляемую коллекцию пути к недавно использовавшемуся файлу
        public static IEnumerable<string> GetRecentFiles()
        {// Получить недавний путь
            var recentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Recent);

            // Получаем имя файла в папке "Недавние" на компьютере (полный путь)
            return from file in Directory.EnumerateFiles(recentFolder)

                       //Принимать только файлы с ярлыками
                   where Path.GetExtension(file) == ".lnk"

                   // Удалить соответствующий реальный путь
                   select GetShortcutTargetFilePath(file);
        }
    }
}
