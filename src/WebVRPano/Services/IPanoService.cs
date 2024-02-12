using System.Threading.Tasks;

namespace WebVRPano.Services;

public interface IPanoService
{
    Task LoadPano(string tinyId);
}
