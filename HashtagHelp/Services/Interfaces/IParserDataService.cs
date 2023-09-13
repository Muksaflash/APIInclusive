using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IParserDataService
    {
        Dictionary<string, int> RedoFiles(string tagsTaskContent);
        Dictionary<string, int> RareFreqTagsRemove(Dictionary<string, int> freqDict, int minFollowerTagsCount);
        List<string> CreateFunnels(FunnelEntity model, List<HashtagEntity> listHashtags);
    }
}
