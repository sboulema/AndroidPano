namespace WebVRPano.Services
{
    public interface IXmlService
    {
        void Init();
        void WriteToFile(string dir);
        void AddScene(string cubeUrl);
    }
}
