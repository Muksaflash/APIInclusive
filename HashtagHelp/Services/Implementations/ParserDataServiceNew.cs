using System.Text.RegularExpressions;
using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations
{
    public class ParserDataServiceNew : IParserDataService
    {
        readonly char separator = Path.DirectorySeparatorChar;

        readonly string newLine = Environment.NewLine;

        public Dictionary<string, int> RedoFiles(string tagsTaskContent)
        {
            var freqDict = new Dictionary<string, int>();
            char[] separators = new char[] { ' ', ',', '\t'};

            IEnumerable<IEnumerable<string>> hashtagFreq = tagsTaskContent
                .Split('\n')
                .Where(line => line.Split(':').Length < 15)
                .Where(line => line.Split(':')[^1] != 0.ToString())
                .Select(line => line.Split(':')[^1].Split(separators, StringSplitOptions.RemoveEmptyEntries));

            var hashtags = hashtagFreq.SelectMany(line => line)
                .Select(hashtag => hashtag.TrimStart('#'))
                .Where(word => !ContainsWrongSymbols(word));


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
        public List<string> CreateFunnels(FunnelEntity model)
        {

            var floorFreq = model.MinTagMediaCount;
            var topFreq = model.MaxTagMediaCount;
            var freqStep = model.MinMediaCountInterval;
            var hashtagFunnelNumber = model.HashtagsNumber;

            var parsedHashtagFreq = PrepareDict(model.ParsedHashtagEntities, floorFreq, topFreq);
            var areaHashtagFreq = PrepareDict(model.AreaHashtagEntities, floorFreq, topFreq);
            var semiAreaHashtagFreq = PrepareDict(model.SemiAreaHashtagEntities, floorFreq, topFreq);

            var count = parsedHashtagFreq.ToList().Count;
            long nextBound = floorFreq;
            long hashtagFunnelCount;
            int fullPacksCount = 0;
            bool isDictEmpty = false;
            if (count == 0) isDictEmpty = true;

            var hashtagsLines = new List<string>();
            var funnelsCount = 0;
            long partSize = 0;

            while (!isDictEmpty)
            {
                nextBound = floorFreq;
                hashtagFunnelCount = hashtagFunnelNumber;
                partSize = hashtagFunnelNumber / 3;

                funnelsCount++;
                hashtagsLines.Add($"Воронка {funnelsCount}:\n\n");
                for (int pack = 0; pack < 3; pack++)
                {
                    hashtagsLines.Add("\n");
                    for (int part = 0; part < 3; part++)
                    {
                        var currentDictionary = part == 0 ? areaHashtagFreq : part == 1 ? semiAreaHashtagFreq : parsedHashtagFreq;
                        foreach (var item in currentDictionary)
                        {
                            if (item.Key >= nextBound - 200 && item.Value.Count != 0)
                            {
                                //var hashtagString = $"#{item.Value[0]}" + " " + $"{item.Key}" +" "+ $"{part} \n";
                                var hashtagString = $"#{item.Value[0]}";
                                hashtagsLines.Add(hashtagString);
                                item.Value.RemoveAt(0);
                                //nextBound = item.Key + freqStep;
                                nextBound += freqStep;
                                if (--hashtagFunnelCount == 0)
                                {
                                    fullPacksCount++;
                                    hashtagFunnelCount = hashtagFunnelNumber;
                                }
                                if (--partSize == 0)
                                {
                                    partSize = hashtagFunnelNumber / 3;
                                    break;
                                }
                            }
                        }
                    }
                }
                isDictEmpty = !parsedHashtagFreq.Any(item => item.Value.Count != 0) && !areaHashtagFreq.Any(item => item.Value.Count != 0) && !semiAreaHashtagFreq.Any(item => item.Value.Count != 0);
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
