using System.Xml.Linq;

namespace InstallerCreator.ModInstaller {
    public static class XmlHelpers {
        internal static XAttribute ExplicitOrder() {
            return new XAttribute("order", "Explicit");
        }

        internal static XAttribute Name(string name) {
            return new XAttribute("name", name);
        }

        internal static XAttribute Type(SelectType type) {
            return new XAttribute("type", type.ToString());
        }

        internal static XElement OptionalTypeDescriptor() {
            return new XElement("typeDescriptor", new XElement("type", new XAttribute("name", "Optional")));
        }

        internal static XElement Description(string desc = null) {
            return new XElement("description", string.IsNullOrWhiteSpace(desc) ? string.Empty : desc);
        }

        internal static XElement NonePlugin() {
            return new XElement("plugin", new XAttribute("name", "None"), Description(), OptionalTypeDescriptor());
        }

        internal enum SelectType {
            SelectAny,
            SelectAll,
            SelectExactlyOne
        }
    }
}