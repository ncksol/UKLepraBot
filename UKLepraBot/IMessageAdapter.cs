using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace UKLepraBot
{
    public interface IMessageAdapter
    {
        Task Process(Activity activity);
    }
}