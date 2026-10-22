using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace phoenix_prototype
{
    public class MenuItemStyleSelector : StyleSelector
    {
        public Style TopLevelStyle { get; set; }
        public Style SubmenuStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (container is MenuItem mi)
            {
                if (mi.Role == MenuItemRole.TopLevelHeader)
                    return TopLevelStyle;

                if (mi.Role == MenuItemRole.SubmenuItem)
                    return SubmenuStyle;
            }

            return base.SelectStyle(item, container);
        }
    }

}
