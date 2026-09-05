using System.Windows.Controls;

namespace FldrFltr
{
    /// <summary>
    /// The "Variabele invoegen ▾" dropdown, grouped into submenus so the full §2 variable list
    /// stays manageable (same idea as FldrSrtr's VariableMenuHelper). A null entry renders as a
    /// separator (used in "Bestand" to split general/Created/Modified file properties).
    /// </summary>
    public static class VariableMenuHelper
    {
        private static readonly (string Group, string[] Tokens)[] VariableGroups =
        {
            ("Algemeen", new[] { "{Counter}", "{Counter:100}", "{Counter:100:5}", "{Guid}", "{Random}", "{Random:0000}", "{RandomString}", "{RandomString:12}" }),
            ("Bestand", new[]
            {
                "{FileName}", "{OriginalName}", "{Extension}", "{OriginalExtension}", "{FullPath}", "{Directory}", "{FileSize}",
                null,
                "{CreatedYear}", "{CreatedMonth}", "{CreatedDay}", "{CreatedHour}", "{CreatedMinute}", "{CreatedSecond}", "{CreatedDate}", "{CreatedTime}",
                null,
                "{ModifiedYear}", "{ModifiedMonth}", "{ModifiedDay}", "{ModifiedHour}", "{ModifiedMinute}", "{ModifiedSecond}", "{ModifiedDate}", "{ModifiedTime}"
            }),
            ("Datum (huidige datum/tijd)", new[] { "{Year}", "{Month}", "{Day}", "{Hour}", "{Minute}", "{Second}", "{Date}", "{Time}" })
        };

        public static void ShowVariableMenu(Button anchor, TextBox target)
        {
            var menu = new ContextMenu();
            foreach ((string group, string[] tokens) in VariableGroups)
            {
                var groupItem = new MenuItem { Header = group };
                foreach (string token in tokens)
                {
                    if (token == null)
                    {
                        groupItem.Items.Add(new Separator());
                        continue;
                    }

                    var item = new MenuItem { Header = token };
                    item.Click += (_, __) => InsertAtCaret(target, token);
                    groupItem.Items.Add(item);
                }
                menu.Items.Add(groupItem);
            }

            anchor.ContextMenu = menu;
            menu.PlacementTarget = anchor;
            menu.IsOpen = true;
        }

        private static void InsertAtCaret(TextBox textBox, string token)
        {
            int caret = textBox.CaretIndex;
            textBox.Text = (textBox.Text ?? string.Empty).Insert(caret, token);
            textBox.CaretIndex = caret + token.Length;
            textBox.Focus();
        }
    }
}
