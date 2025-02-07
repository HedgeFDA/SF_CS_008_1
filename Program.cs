using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace SF_CS_008_1
{
    internal class Program
    {
        /// <summary>
        /// Функция выполняющее преобразования строки в число.
        /// </summary>
        /// <param name="stringValue"></param>
        /// <param name="integerValue"></param>
        /// <returns></returns>
        static bool IsCorrectIntegerValue(string stringValue, out int integerValue) => int.TryParse(stringValue, out integerValue);

        /// <summary>
        /// Функция реализующая запрос ввода у пользоватеся строкового значения
        /// </summary>
        /// <param name="message">Текст запроса ввода строки к пользователю</param>
        /// <returns></returns>
        static string InputStringValue(string message)
        {
            Console.Write(message);

            string? value = Console.ReadLine();

            return value == null ? "" : value;
        }

        /// <summary>
        /// Функция реализующая запрос ввода у пользоватеся числового значения, выполнеяет контроль что введенное значение является числом,
        /// а также при необходимости контроль за тем, чтоб введенное значение было больше 0
        /// </summary>
        /// <param name="message">Текст запроса ввода числа к пользователю</param>
        /// <param name="needCheckZero">Признак необходимости контролировать чтоб введенное знеачение было больше 0</param>
        /// <returns></returns>
        static int InputIntegerValue(string message, bool needCheckZero = false)
        {
            int value = 0;

            while (true)
            {
                bool isCorrectIntegerValue = IsCorrectIntegerValue(InputStringValue(message), out value);

                if (!isCorrectIntegerValue)
                    Console.WriteLine("   необходимо ввести числовое значение");
                else if (needCheckZero & value <= 0)
                    Console.WriteLine("   введенное значение должно быть больше 0");
                else break;
            }

            return value;
        }

        /// <summary>
        /// Процедура выводящяя информацию об ошибке в консоли.
        /// </summary>
        /// <param name="message">Текст сообщения который будет добавлен к сообщению после заголовка "Ошибка: "</param>
        static void ErrorMessage(string message) => Console.WriteLine("\nОшибка: {0}", message);

        /// <summary>
        /// Метод удаляющий все вложенные файлы старше даты deadline
        /// </summary>
        /// <param name="folderPath">Путь к каталогу в котором следует удалить файлы и рекурсивно обработать вложенные каталоги</param>
        /// <param name="deadline">Граница (дата), файлы с датой и времеменм последнего доступа старше которой будут удалены</param>
        static void CleanFiles(string folderPath, DateTime deadline)
        {
            foreach (string file in Directory.GetFiles(folderPath))
            {
                FileInfo fileInfo = new FileInfo(file);

                // Файл моложе границы удаления, пропускаем
                if (fileInfo.LastAccessTime >= deadline) continue;

                try
                {
                    fileInfo.Delete();
                }
                catch (Exception ex)
                {
                    ErrorMessage($"Не удалось удалить файл \"{file}\" по причине: {ex.Message}");

                    continue;
                }

                Console.WriteLine("Удален файл: {0}", file);
            }
        }

        /// <summary>
        /// Рекурсивный метод удаляющий все вложенные файлы и каталоги старше даты deadline
        /// </summary>
        /// <param name="folderPath">Путь к каталогу в котором следует удалить файлы и рекурсивно обработать вложенные каталоги</param>
        /// <param name="deadline">Граница (дата), файлы с датой и времеменм последнего доступа старше которой будут удалены</param>
        static void CleanFolder(string folderPath, DateTime deadline)
        {
            // Удаляем вложенные файлы в текущем каталоге
            CleanFiles(folderPath, deadline);

            foreach (string subFolderPath in Directory.GetDirectories(folderPath))
            {
                // Продолжение рекурсии очистки
                CleanFolder(subFolderPath, deadline);

                DirectoryInfo dirInfo = new DirectoryInfo(subFolderPath);

                // Остались вложенные файлы или каталоги, не можем удалять, пропускаем
                if (dirInfo.GetFiles().Length > 0 || dirInfo.GetDirectories().Length > 0) continue;

                try
                {
                    // Удаляем вложенный каталог 
                    dirInfo.Delete();
                }
                catch (Exception ex)
                {
                    ErrorMessage($"Не удалось удалить каталог \"{subFolderPath}\" по причине: {ex.Message}");

                    continue;
                }

                Console.WriteLine("Удален каталог: {0}", subFolderPath);
            }
        }

        /// <summary>
        /// Процедура реализующая основной алгоритм работы программы по удалению файлов и каталогов
        /// Включающая получение необходимых вводных значений от пользователя
        /// </summary>
        static void PerformCleaning()
        {
            string folderPath = "";

            // Запросим у пользователя каталог файлы которого нужно удалить
            while (true)
            {
                folderPath = InputStringValue("Укажите путь к каталогу который необходимо очистить (для отмены введите пустое значение): ");

                if (string.IsNullOrEmpty(folderPath))
                    return;
                else if (Directory.Exists(folderPath))
                    break;

                Console.WriteLine("Каталог \"{0}\" не найден", folderPath);
            }

            // Определим границу удаления (признания файлов устаревшими)
            DateTime deadline = DateTime.Now.AddMinutes(-InputIntegerValue("Укажите срок устаревания вложенных файлов и каталогов для очистки (в минутах): ", true));

            // Подтвердим удаление всех вложенных файлов и каталогов
            while (true)
            {
                string answer = InputStringValue($"\nВложенные файлы и каталоги с последней датой изменения старше \"{deadline}\" в каталоге \"{folderPath}\" будут удалены.\nПродолжить?\n    пустое значение - отменить\n    yes - продолжить\nВаше решение: ");

                if (answer == "yes")
                    break;

                if (string.IsNullOrEmpty(answer))
                    return;

                Console.WriteLine("    ответ должен быть yes или пустое значение.");
            }

            Console.WriteLine("\nВыполняем очистку...");

            try
            {
                // Запуск рекурсии очистки
                CleanFolder(folderPath, deadline);

                Console.WriteLine("\nОчистка завершена успешно.");
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
            }
        }

        static void Main(string[] args)
        {
            PerformCleaning();

            Console.WriteLine("\nВыполнение программы завершено.");
        }
    }
}