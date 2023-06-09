using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IParserDataService
    {
        Dictionary<string, int> RedoFiles(string tagsTaskContent);
        void RareFreqTagsRemove(Dictionary<string, int> freqDict);
        (List<string>, string) CreateFunnels(FunnelEntity model, List<HashtagEntity> listHashtags);
    }
}
