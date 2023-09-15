﻿using HashtagHelp.Domain.Models;
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
            char[] separators = new char[] { ' ', ',' };

            IEnumerable<IEnumerable<string>> hashtagFreq = tagsTaskContent
                .Split('\n')
                .Where(line => line.Split(':').Length < 15)
                .Where(line => line.Split(':')[^1] != 0.ToString())
                .Select(line => line.Split(':')[^1].Split(separators, StringSplitOptions.RemoveEmptyEntries));

            var hashtags = hashtagFreq.SelectMany(line => line)
                .Where(word => !ContainsEmoji(word))
                .Select(hashtag => hashtag.TrimStart('#'));

            foreach (var hashtag in hashtags)
            {
                if (!freqDict.ContainsKey(hashtag))
                    freqDict[hashtag] = 1;
                else
                    freqDict[hashtag] += 1;
            }
            return freqDict;
        }

        public Dictionary<string, int> RareFreqTagsRemove(Dictionary<string, int> freqDict, int minFollowerTagsCount)
        {
            int bottomBorder = freqDict.Count / 1000 + 5;
            if (bottomBorder > 50) bottomBorder = 50;
            var newFreqDict = freqDict.Where(x => x.Value >= bottomBorder).ToDictionary(x => x.Key, x => x.Value);
            while (newFreqDict.Count < minFollowerTagsCount)
            {
                bottomBorder -= 1;
                newFreqDict = freqDict.Where(x => x.Value >= bottomBorder).ToDictionary(x => x.Key, x => x.Value);
                if (bottomBorder == 1) break;
            }
            return newFreqDict;
        }

        public List<string> CreateFunnels(FunnelEntity model, List<HashtagEntity> listHashtags)
        {
            ILookup<long, string> hashtagFreq = listHashtags.ToLookup(hashtag => long.Parse(hashtag.MediaCount), hashtag => hashtag.Name);

            var floorFreq = model.MinTagMediaCount;
            var topFreq = model.MaxTagMediaCount;
            var freqStep = model.MinMediaCountInterval;
            var hashtagFunnelNumber = model.HashtagsNumber;

            var hashtagFreq1 = hashtagFreq.Where(x => x.Key >= floorFreq && x.Key <= topFreq);
            var count = hashtagFreq1.ToList().Count;
            var hashtagFreq2 = hashtagFreq1.ToDictionary(group => group.Key, group => group.ToList())
                .OrderBy(group => group.Key);
            long nextBound = floorFreq;

            long hashtagFunnelCount;
            int fullPacksCount = 0;
            bool isDictEmpty = false;
            if (count == 0) isDictEmpty = true;

            var hashtagsLines = new List<string>();
            var funnelsCount = 0;

            while (!isDictEmpty)
            {
                nextBound = floorFreq;
                hashtagFunnelCount = hashtagFunnelNumber;
                funnelsCount++;
                hashtagsLines.Add($"Воронка {funnelsCount}:\n\n");
                foreach (var item in hashtagFreq2)
                {
                    if (item.Key >= nextBound && item.Value.Count != 0)
                    {
                        var hashtagString = $"#{item.Value[0]}\t{item.Key}\n";
                        hashtagsLines.Add(hashtagString);
                        item.Value.RemoveAt(0);
                        nextBound = item.Key + freqStep;

                        if (--hashtagFunnelCount == 0)
                        {
                            fullPacksCount++;
                            hashtagFunnelCount = hashtagFunnelNumber;
                            hashtagsLines.Add("\n");
                        }
                    }
                }

                isDictEmpty = !hashtagFreq2.Any(item => item.Value.Count != 0);
                hashtagsLines.Add("\n");
            }

            var information = string.Empty;
            if (count == 0)
            {
                information = "В указанной папке отсутствовали записи нужной частотности " +
                    "либо неправильный формат!" +
                    " Работа завершена!";
            }
            else
                information = $"Создано " + funnelsCount + " воронок всего с " + fullPacksCount + " паками по " + hashtagFunnelNumber
                    + " хэштегов и, возможно, есть паки меньше." + '\n' +
                    "Работа успешно завершена!\n\n";
            hashtagsLines.Insert(0, information);

            return hashtagsLines;
        }
        static bool ContainsEmoji(string text)
        {
            foreach (char c in text)
            {
                if (char.IsSurrogate(c))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
