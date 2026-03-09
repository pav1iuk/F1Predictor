using System.Windows;
using System.Net;

namespace F1Predictor.UI
{
    public partial class App : Application
    {
        public App()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        }
    }
}