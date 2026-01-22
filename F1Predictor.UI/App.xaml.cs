using System.Windows;
using System.Net; // <--- НЕ ЗАБУДЬ ЦЕ

namespace F1Predictor.UI
{
    public partial class App : Application
    {
        public App()
        {
            // !!! ЦЕЙ РЯДОК МАГІЧНО ВИПРАВЛЯЄ ЗАВАНТАЖЕННЯ КАРТИНОК !!!
            // Він змушує програму використовувати сучасний протокол безпеки
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        }
    }
}