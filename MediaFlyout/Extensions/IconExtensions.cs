using System.Drawing;
using System.Reflection;

namespace MediaFlyout.Extensions
{
    // https://github.com/File-New-Project/EarTrumpet/blob/4553d39d9e2eaee64ad5edac007e4c6e81b95042/EarTrumpet/Extensions/IconExtensions.cs
    public static class IconExtensions
    {
        public static Icon AsDisposableIcon(this Icon icon)
        {
            // System.Drawing.Icon does not expose a method to declare
            // ownership of its wrapped handle so we have to use reflection
            // here.

            // See also: https://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/Icon.cs,71

            icon.GetType().GetField("ownHandle", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(icon, true);
            return icon;
        }
    }
}
