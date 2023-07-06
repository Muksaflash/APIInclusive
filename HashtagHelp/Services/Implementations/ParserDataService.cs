using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations
{
    public class ParserDataService : IParserDataService
    {
        readonly char separator = Path.DirectorySeparatorChar;

        readonly string newLine = Environment.NewLine;

        public Dictionary<string, int> RedoFiles(string tagsTaskContent)
        {
            var freqDict = new Dictionary<string, int>();
            IEnumerable<IEnumerable<string>> hashtagFreq = tagsTaskContent
                .Split('\n')
                .Where(line => line.Split(':').Length == 2)
                .Where(line => line.Split(':')[1] != 0.ToString())
                .Select(line => line.Split(':')[1].Split(' '));

            var hashtags = hashtagFreq.SelectMany(line => line)
                .Select(hashtag => hashtag.TrimStart('#'))
                .Where(hashtag => hashtag != "");

            foreach (var hashtag in hashtags)
            {
                if (!freqDict.ContainsKey(hashtag))
                    freqDict[hashtag] = 1;
                else
                    freqDict[hashtag] += 1;
            }
            return freqDict;
        }

        public void RareFreqTagsRemove(Dictionary<string, int> freqDict)
        {
            int bottomBorder = freqDict.Count / 1000 + 5;
            if (bottomBorder > 50) bottomBorder = 50;
            var newFreqDict = freqDict.Where(x => x.Value >= bottomBorder).ToDictionary(x => x.Key, x => x.Value);
            while(newFreqDict.Count < 100)
            {
                bottomBorder -= 1;
                newFreqDict = freqDict.Where(x => x.Value >= bottomBorder).ToDictionary(x => x.Key, x => x.Value);
            }
            freqDict = newFreqDict;
        }

        public (List<string>, string) CreateFunnels(FunnelEntity model, List<HashtagEntity> listHashtags)
        {
           /*  ILookup<int, string> hashtagFreq = listHashtags
                .Where(line => !string.IsNullOrEmpty(line))
                .ToLookup
                (line =>
                {
                    int key;
                    try
                    {
                        key = int.Parse(string.Join("", line.Split('\t')[1].Where(symbol => char.IsDigit(symbol))));
                    }
                    catch
                    {
                        return -1;
                    }
                    return key;
                },
                line =>
                {
                    string value;
                    value = line.Split('\t')[0];
                    return value;
                }); */

            ILookup<long, string> hashtagFreq = listHashtags.ToLookup(hashtag => long.Parse(hashtag.MediaCount), hashtag => hashtag.Name);

            var floorFreq = model.MinTagMediaCount;
            var topFreq = model.MaxTagMediaCount;
            var freqStep = model.MinMediaCountInterval;
            var hashtagFunnelNumber = model.HashtagsNumber;

            var hashtagFreq1 = hashtagFreq.Where(x => x.Key >= floorFreq && x.Key <= topFreq);
            var count = hashtagFreq1.ToList().Count;
            var hashtagFreq2 = hashtagFreq1.ToDictionary(group => group.Key, group => group.ToList())
                .OrderBy(group => group.Key);
            long nextBound;

            long hashtagFunnelCount;
            int fullFunnelCount = 0;
            int funnelCount = 0;
            bool isDictEmpty = false;
            if (count == 0) isDictEmpty = true;

            var fileName = "Воронка.txt";
            var stringsList = new List<string>();

            while (!isDictEmpty)
            {
                hashtagFunnelCount = hashtagFunnelNumber;
                nextBound = floorFreq;
                stringsList.Add(Environment.NewLine);
                foreach (var item in hashtagFreq2)
                {
                    if (item.Key >= nextBound)
                    {
                        if (item.Value.Count != 0)
                        {
                            var hashtagString = item.Value[0] + '\t' + item.Key + '\n';
                            stringsList.Add(hashtagString);
                            item.Value.RemoveAt(0);
                            nextBound = item.Key + freqStep;
                            if (--hashtagFunnelCount == 0)
                            {
                                fullFunnelCount++;
                                break;
                            }
                        }
                    }
                }
                foreach (var item in hashtagFreq2)
                {
                    if (item.Value.Count != 0)
                    {
                        isDictEmpty = false;
                        break;
                    }
                    isDictEmpty = true;
                }
                funnelCount++;
            }
            
            var information = string.Empty;
            if (count == 0)
            {
                information = "В указанной папке отсутствовали записи нужной частотности " +
                    "либо неправильный формат!" +
                    " Работа завершена!";
            }
            else
                information = $"Создано " + fullFunnelCount + " воронок по " + hashtagFunnelNumber
                    + " хэштегов и ещё " +
                    +funnelCount + " поменьше" + '\n' + '\n' +
                    "Хештеги записаны в файл \"" + fileName + "\"" +
                    "Работа успешно завершена!";
            return (stringsList, information) ;
        }
    }
}
