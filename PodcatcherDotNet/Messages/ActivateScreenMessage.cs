
using Caliburn.Micro;
namespace PodcatcherDotNet.Messages {
   public class ActivateScreenMessage {
       public Screen Screen { get; private set; }

       public ActivateScreenMessage(Screen screen) {
           Screen = screen;
       }
    }
}
