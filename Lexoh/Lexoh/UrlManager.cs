using System.Threading.Tasks;
using Xamarin.Forms;

namespace Lexoh
{
    public interface IUrlManager
    {
        Task<bool> OpenUrl(string url);
        void ClearCache();
        void InstallLexohRootCertificate();
    }
    public class UrlManager
    {
        public static IUrlManager Manager { get; } = DependencyService.Get<IUrlManager>();
    }
}
