using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace InvisibleManXRay.Handlers
{
    using Models;
    using Services;
    using Values;

    public class NotifyHandler : Handler
    {
        private NotifyIcon notifyIcon;

        private Func<Mode> getMode;
        private Action onOpenClick;
        private Action onUpdateClick;
        private Action onAboutClick;
        private Action onCloseClick;
        private Action onProxyModeClick;
        private Action onTunnelModeClick;

        private Dictionary<Mode, ToolStripMenuItem> modeItems;

        private LocalizationService LocalizationService => ServiceLocator.Get<LocalizationService>();

        public void Setup(
            Func<Mode> getMode,
            Action onOpenClick,
            Action onCloseClick,
            Action onProxyModeClick,
            Action onTunnelModeClick
        )
        {
            this.getMode = getMode;
            this.onOpenClick = onOpenClick;
            this.onCloseClick = onCloseClick;
            this.onProxyModeClick = onProxyModeClick;
            this.onTunnelModeClick = onTunnelModeClick;
        }

        public void CheckModeItem(Mode mode)
        {
            ToolStripMenuItem modeItem = modeItems[mode];
            UncheckAllItems();
            CheckItem(modeItem);
        }

        public void InitializeNotifyIcon()
        {
            if (IsNotifyIconAlreadyExists())
                notifyIcon.Dispose();

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = GetNotifyIcon();
            notifyIcon.Visible = true;

            HandleNotifyIconClick();
            AddMenuStrip();

            bool IsNotifyIconAlreadyExists() => notifyIcon != null;

            Icon GetNotifyIcon()
            {
                return Icon.ExtractAssociatedIcon(
                    System.Environment.GetCommandLineArgs().First()
                );
            }
        }

        private void HandleNotifyIconClick()
        {
            notifyIcon.MouseClick += (sender, e) => {
                if (e.Button == MouseButtons.Left)
                    onOpenClick.Invoke();
            };
        }

        private void AddMenuStrip()
        {
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
            modeItems = new Dictionary<Mode, ToolStripMenuItem>() {
                { Mode.PROXY, CreateItem("Proxy", OnProxyModeClick, true, getMode.Invoke() == Mode.PROXY) },
                { Mode.TUN, CreateItem("TUN", OnTunnelModeClick, true, getMode.Invoke() == Mode.TUN) }
            };
            
            AddMenuItem(LocalizationService.GetTerm(Localization.NOTIFY_OPEN), OnOpenClick);
            AddMenuItem(LocalizationService.GetTerm(Localization.NOTIFY_MODE), delegate { }, modeItems.Values.ToArray());
            AddMenuItem(LocalizationService.GetTerm(Localization.NOTIFY_CLOSE), OnCloseClick);

            notifyIcon.ContextMenuStrip = contextMenuStrip;

            void AddMenuItem(string text, Action onClick, ToolStripMenuItem[] children = default)
            {
                ToolStripMenuItem item = CreateItem(text, onClick);

                if (children != null)
                    foreach(ToolStripMenuItem child in children)
                        item.DropDownItems.Add(child);
                
                contextMenuStrip.Items.Add(item);
            }

            ToolStripMenuItem CreateItem(
                string text, 
                Action onClick, 
                bool isToggle = default,
                bool isChecked = default
            )
            {
                ToolStripMenuItem item = new ToolStripMenuItem() { Text = text, Checked = isChecked };
                item.Click += (sender, e) => { 
                    HandleToggleClick();
                    onClick.Invoke(); 
                };
                
                return item;

                void HandleToggleClick()
                {
                    if (!isToggle)
                        return;

                    UncheckAllItems();
                    CheckItem(item);
                }
            }

            void OnProxyModeClick()
            {
                onProxyModeClick.Invoke();
            }

            void OnTunnelModeClick()
            {
                onTunnelModeClick.Invoke();
            }

            void OnOpenClick()
            {
                onOpenClick.Invoke();
            }

            void OnCloseClick()
            {
                onCloseClick.Invoke();
            }
        }

        private void UncheckAllItems()
        {
            foreach(ToolStripMenuItem itemElement in modeItems.Values)
                itemElement.Checked = false;
        }

        private void CheckItem(ToolStripMenuItem item)
        {
            item.Checked = true;
        }
    }
}