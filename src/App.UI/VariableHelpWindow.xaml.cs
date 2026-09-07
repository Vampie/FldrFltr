using System.Windows;

namespace FldrFltr
{
    public partial class VariableHelpWindow : Window
    {
        public VariableHelpWindow()
        {
            InitializeComponent();
            GroupsItemsControl.ItemsSource = VariableHelpContent.BuildLocalized();
        }
    }
}
