using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByTheBook.Utils
{
    public static class MurderControllerExtensionMethods
    {
        public static MurderController.Murder? GetLatestMurder(this MurderController controller)
        {
            if (controller.activeMurders.Count == 0)
            {
                return null;
            }

            return controller.activeMurders[controller.activeMurders.Count - 1];
        }
    }
}
