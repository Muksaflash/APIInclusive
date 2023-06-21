namespace HashtagHelp.Services.Implementations
{
    public class ParserDataService
    {
        static readonly char separator = Path.DirectorySeparatorChar;
        static readonly string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        static readonly string parserFolderPath = Path.Combine(desktopPath, $"Parser{separator}");
        static readonly string newLine = Environment.NewLine;

        public static void RedoFiles()
        {
            var freqDict = new Dictionary<string, int>();
            Console.WriteLine($"{newLine}Введите минимальную частотность встречи хэштегов");
            var MinFreq = int.Parse(Console.ReadLine());

            var files = FindFiles(parserFolderPath, ".txt");

            var listHashtags = GetDataParser(files);

            IEnumerable<IEnumerable<string>> hashtagFreq = listHashtags
                .Where(line => line.Split(':').Length == 5)
                .Where(line => line.Split(':')[4] != 0.ToString())
                .Select(line => line.Split(':')[4].Split(','));

            var text1 = hashtagFreq.SelectMany(line => line);
            var text2 = text1.Select(x => "#" + x);

            foreach (var word in text2)
            {
                if (!freqDict.ContainsKey(word))
                    freqDict[word] = 1;
                else
                    freqDict[word] += 1;
            }

            Console.WriteLine();

            var fileName = "Частота.txt";
            fileName = CreateNameNewFile(fileName, desktopPath);

            foreach (var word in freqDict)
            {
                if (word.Value >= MinFreq)
                {
                    Console.WriteLine(word.Key + '\t' + word.Value);
                    File.AppendAllText(Path.Combine(desktopPath, fileName), word.Key + '\t' + word.Value + newLine);
                }

            }
            Console.ForegroundColor = ConsoleColor.Red; // устанавливаем цвет
            Console.WriteLine(newLine + $"Создан файл \"{fileName}\" на рабочем столе!");
            Console.ResetColor(); // сбрасываем в стандартный
        }
        static IEnumerable<string?> GetDataParser(IEnumerable<string> fileNames)
        {
            foreach (var file in fileNames)
            {
                StreamReader sr = new StreamReader(file);
                while (!sr.EndOfStream)
                {
                    yield return sr.ReadLine();
                }
                sr.Close();
            }
        }
        public static string CreateNameNewFile(string fileName, string folder)
        {
            string newFileName = fileName;
            var fileCount = 1;
            while (File.Exists(Path.Combine(folder, newFileName)))
            {
                fileCount++;
                newFileName = fileName.Substring(0, fileName.Length - 4) + " (" + fileCount.ToString() + ")" + ".txt";
            }
            return newFileName;
        }
        static IEnumerable<string> FindFiles(string folderName, string desiredExtension)
        {
            List<string> salesFiles = new List<string>();

            var foundFiles = Directory.EnumerateFiles(folderName, "*", SearchOption.AllDirectories);

            foreach (var file in foundFiles)
            {
                var extension = Path.GetExtension(file);
                if (extension == ".txt")
                {
                    salesFiles.Add(file);
                }
            }

            return salesFiles;
        }
    }
}
