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
    }
}
