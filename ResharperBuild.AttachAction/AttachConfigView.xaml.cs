using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using JetBrains.UI.Wpf;

namespace ResharperBuild.AttachAction
{
    /// <summary>
    /// Interaction logic for AttachConfigView.xaml
    /// </summary>
    [View(ViewKind.Wpf)]
    public partial class AttachConfigView : StackPanel, IView<AttachConfigAutomation>, IView, IComponentConnector
    {
        public AttachConfigView()
        {
            InitializeComponent();
        }

        private void SelectOnKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!e.KeyboardDevice.IsKeyDown(Key.Tab))
                return;
            ((TextBoxBase)sender).SelectAll();
        }
    }
}
