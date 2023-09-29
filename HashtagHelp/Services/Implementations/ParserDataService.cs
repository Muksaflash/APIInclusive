using System.Text.RegularExpressions;
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
            char[] separators = new char[] { ' ', ',' };

            IEnumerable<IEnumerable<string>> hashtagFreq = tagsTaskContent
                .Split('\n')
                .Where(line => line.Split(':').Length < 15)
                .Where(line => line.Split(':')[^1] != 0.ToString())
                .Select(line => line.Split(':')[^1].Split(separators, StringSplitOptions.RemoveEmptyEntries));

            var hashtags = hashtagFreq.SelectMany(line => line)
                .Where(word => !ContainsWrongSymbols(word))
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

        public Dictionary<string, int> RareFreqTagsRemove(Dictionary<string, int> freqDict, int minFollowerTagsCount, int maxFollowerTagNumber)
        {
            int bottomBorder = freqDict.Count / 1000 + 5;
            if (bottomBorder > maxFollowerTagNumber) bottomBorder = maxFollowerTagNumber;
            var newFreqDict = freqDict.Where(x => x.Value >= bottomBorder).ToDictionary(x => x.Key, x => x.Value);
            while (newFreqDict.Count < minFollowerTagsCount)
            {
                bottomBorder -= 1;
                newFreqDict = freqDict.Where(x => x.Value >= bottomBorder).ToDictionary(x => x.Key, x => x.Value);
                if (bottomBorder == 1) break;
            }
            return newFreqDict;
        }
        public IOrderedEnumerable<KeyValuePair<long, List<string>>>? PrepareDict(List<HashtagEntity> listHashtags, long floorFreq, long topFreq)
        {
            ILookup<long, string> hashtagFreq = listHashtags.ToLookup(hashtag => long.Parse(hashtag.MediaCount), hashtag => hashtag.Name);
            var hashtagFreq1 = hashtagFreq.Where(x => x.Key >= floorFreq && x.Key <= topFreq);
            return hashtagFreq1.ToDictionary(group => group.Key, group => group.ToList())
                .OrderBy(group => group.Key);
        }
        public List<string> CreateFunnels(FunnelEntity model, List<HashtagEntity> parsedHashtagEntities, List<HashtagEntity> areaHashtagEntities)
        {

            var floorFreq = model.MinTagMediaCount;
            var topFreq = model.MaxTagMediaCount;
            var freqStep = model.MinMediaCountInterval;
            var hashtagFunnelNumber = model.HashtagsNumber;

            var parsedHashtagFreq = PrepareDict(parsedHashtagEntities, floorFreq, topFreq);
            var areaHashtagFreq = PrepareDict(areaHashtagEntities, floorFreq, topFreq);

            var count = parsedHashtagFreq.ToList().Count;
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
                foreach (var item in parsedHashtagFreq)
                {
                    if (item.Key >= nextBound - 200 && item.Value.Count != 0)
                    {
                        var hashtagString = $"#{item.Value[0]}\n";
                        hashtagsLines.Add(hashtagString);
                        item.Value.RemoveAt(0);
                        nextBound = item.Key + freqStep;

                        if (--hashtagFunnelCount == 0)
                        {
                            fullPacksCount++;
                            hashtagFunnelCount = hashtagFunnelNumber;
                            hashtagsLines.Add("\n");
                        }
                        foreach (var item1 in areaHashtagFreq)
                        {
                            if (item1.Key >= nextBound - 200 && item1.Value.Count != 0)
                            {
                                var hashtagString1 = $"#{item1.Value[0]}\n";
                                hashtagsLines.Add(hashtagString1);
                                item1.Value.RemoveAt(0);
                                nextBound = item1.Key + freqStep;

                                if (--hashtagFunnelCount == 0)
                                {
                                    fullPacksCount++;
                                    hashtagFunnelCount = hashtagFunnelNumber;
                                    hashtagsLines.Add("\n");
                                }
                                break;
                            }
                        }
                    }
                }
                isDictEmpty = !parsedHashtagFreq.Any(item => item.Value.Count != 0) || !areaHashtagFreq.Any(item => item.Value.Count != 0);
                hashtagsLines.Add("\n");
            }

            var information = string.Empty;
            if (count == 0)
            {
                information = "There were no records of the required frequency in the specified folder " +
                "or an incorrect format!" +
                " The operation is completed!";
            }
            else
                information = $"Created " + funnelsCount + " funnels in total with " + fullPacksCount + " packs of " + hashtagFunnelNumber
                    + " hashtags, and there may be packs with fewer hashtags." + '\n' +
                    "The operation has been successfully completed!\n\n";

            hashtagsLines.Insert(0, information);

            return hashtagsLines;
        }
        static bool ContainsWrongSymbols(string text)
        {
            var value = Regex.IsMatch(text, @"[^a-zA-Zа-яА-Я0-9_]");
            return value;
        }
    }
}
