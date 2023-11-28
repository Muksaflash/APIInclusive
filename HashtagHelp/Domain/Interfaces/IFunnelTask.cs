using HashtagHelp.Domain.Enums;

namespace HashtagHelp.Domain.Interfaces;
public interface IFunnelTask
{
    public StatusTaskEnum Status { get; set; }

    public string ErrorInfo { get; set; }

    
}
