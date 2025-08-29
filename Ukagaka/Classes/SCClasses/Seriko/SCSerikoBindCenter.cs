using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
 
namespace Ukagaka
{
    public class SCSerikoBindCenter : IDisposable
    {
        private readonly SCSession _session;
        private readonly SCShell _shell;
        private readonly SCSeriko _seriko;

        private bool _hasBindGroupSubMenu = false;
        private ContextMenu _bindGroupMenu;
        private string _bindGroupMenuTitle;

        private readonly Dictionary<int, BindGroup> _sakuraBindGroups = new Dictionary<int, BindGroup>();
        private readonly Dictionary<int, BindGroup> _keroBindGroups = new Dictionary<int, BindGroup>();
        private readonly List<SCSerikoEnabledBindGroup> _enabledGroups = new List<SCSerikoEnabledBindGroup>();

        public SCSerikoBindCenter(SCSession session, SCShell shell, SCSeriko seriko)
        {
            _session = session;
            _shell = shell;
            _seriko = seriko;

            // Load bind groups from ghost defaults
            var ghostDefaults = SCGhostManager.SharedGhostManager().GetGhostDefaults(session.GetGhostPath());
            var shellDirName = shell.GetDirName();

            var sakuraBindGroupsToEnable = (ghostDefaults?.ContainsKey($"seriko.{shellDirName}.sakura.bindgroup") ?? false)
                ? ghostDefaults[$"seriko.{shellDirName}.sakura.bindgroup"].ToString().Split(',')
                : new string[0] ;

            var keroBindGroupsToEnable = (ghostDefaults?.ContainsKey($"seriko.{shellDirName}.kero.bindgroup") ?? false)
                ? ghostDefaults[$"seriko.{shellDirName}.kero.bindgroup"].ToString().Split(',')
                : new string[0];

            // Create bind group menu if needed
            var descm = shell.GetDescManager();
            if (session.GetMasterSpirit()?.GetName() == null) return;

            var shellName = descm.GetStrValue("name");
            _bindGroupMenuTitle = session.GetMasterSpirit().GetName() + (string.IsNullOrEmpty(shellName) ? "" : $" - {shellName}");

            var sakuraBindGroupKeys = descm.GetKeysStartsWith("sakura.bindgroup");
            var keroBindGroupKeys = descm.GetKeysStartsWith("kero.bindgroup");
            var sakuraMenuItemKeys = descm.GetKeysStartsWith("sakura.menuitem");
            var keroMenuItemKeys = descm.GetKeysStartsWith("kero.menuitem");

            if (sakuraBindGroupKeys.Count > 0 || keroBindGroupKeys.Count > 0)
            {
                _hasBindGroupSubMenu = true;
                _bindGroupMenu = SCMenuController.SharedMenuController().MakeNewBindGroupSubMenu(_bindGroupMenuTitle);

                var hontaiName = descm.GetStrValue("sakura.name") ?? "[SAKURA]";
                var unyuuName = descm.GetStrValue("kero.name") ?? "[KERO]";

                ConstructBindMenu(descm, hontaiName, SCFoundation.SAKURA,
                    sakuraBindGroupsToEnable, sakuraBindGroupKeys, sakuraMenuItemKeys, _sakuraBindGroups);

                ConstructBindMenu(descm, unyuuName, SCFoundation.KERO,
                    keroBindGroupsToEnable, keroBindGroupKeys, keroMenuItemKeys, _keroBindGroups);
            }
        }

        private void ConstructBindMenu(SCDescription descm, string name, int type,
            string[] bindGroupsToEnable, List<string> descBindGroups,
            List<string> descMenuItems, Dictionary<int, BindGroup> bindGroups)
        {
            string sakuraOrKero = (type == SCFoundation.SAKURA) ? "sakura" : "kero";

            if (descBindGroups.Count > 0)
            {
                // Add header item
                var headerItem = new MenuItem
                {
                    Header = name,
                    IsEnabled = false
                };
                _bindGroupMenu.Items.Add(headerItem);
            }

            // Parse bind groups
            foreach (var key in descBindGroups)
            {
                if (!key.EndsWith(".name")) continue;

                var bindGroupData = descm.GetStrValue(key);
                if (string.IsNullOrEmpty(bindGroupData)) continue;

                // Parse bind group data (format: "category,name,thumbnail")
                var parts = bindGroupData.Split(',');
                if (parts.Length < 2) continue;

                // Parse ID from key (e.g. "sakura.bindgroup0.name")
                var idBlock = key.Split('.')[1]; // "bindgroup0"
                if (!int.TryParse(idBlock.Substring("bindgroup".Length), out int id))
                    continue;

                var category = parts[0];
                var bindName = parts[1];
                var thumbnailName = parts.Length > 2 ? parts[2] : "";

                bindGroups[id] = new BindGroup(id, category, bindName, thumbnailName);
            }

            // Add menu items
            if (descMenuItems.Count == 0)
            {
                // Add all bind groups in default order
                foreach (var bindGroup in bindGroups.Values.OrderBy(b => b.Id))
                {
                    var menuItem = new MenuItem
                    {
                        Header = $"{bindGroup.Category}  {bindGroup.BindName}",
                        Tag = type == SCFoundation.SAKURA ? bindGroup.Id + 1 : (bindGroup.Id + 1) * -1,
                        IsChecked = bindGroupsToEnable.Contains(bindGroup.Id.ToString()) ||
                                   descm.GetIntValue($"{sakuraOrKero}.bindgroup{bindGroup.Id}.default") == 1
                    };

                    menuItem.Click += (sender, e) => ToggleBindGroup((MenuItem)sender);
                    _bindGroupMenu.Items.Add(menuItem);
                }
            }
            else
            {
                // Add menu items in specified order
                for (int i = 0; i < descMenuItems.Count; i++)
                {
                    var key = descMenuItems.FirstOrDefault(k => k == $"{sakuraOrKero}.menuitem{i}");
                    if (key == null) continue;

                    var value = descm.GetStrValue(key);
                    if (value == null) continue;

                    if (value.Contains("-"))
                    {
                        // Separator
                        _bindGroupMenu.Items.Add(new Separator());
                    }
                    else if (int.TryParse(value, out int bindId))
                    {
                        if (bindGroups.TryGetValue(bindId, out var bindGroup))
                        {
                            var menuItem = new MenuItem
                            {
                                Header = $"{bindGroup.Category}  {bindGroup.BindName}",
                                Tag = type == SCFoundation.SAKURA ? bindGroup.Id + 1 : (bindGroup.Id + 1) * -1,
                                IsChecked = bindGroupsToEnable.Contains(bindId.ToString()) ||
                                           descm.GetIntValue($"{sakuraOrKero}.bindgroup{bindId}.default") == 1
                            };

                            menuItem.Click += (sender, e) => ToggleBindGroup((MenuItem)sender);
                            _bindGroupMenu.Items.Add(menuItem);
                        }
                    }
                }
            }
        }

        public void EnableCheckedBindGroups()
        {
            if (_bindGroupMenu == null) return;

            foreach (var item in _bindGroupMenu.Items.OfType<MenuItem>())
            {
                if (item.IsChecked && item.Tag is int tag)
                {
                    if (tag > 0) // Sakura
                    {
                        Enable(tag - 1, true);
                    }
                    else // Kero
                    {
                        Enable((-1 * tag) - 1, false);
                    }
                }
            }
        }

        public void ToggleBindGroup(MenuItem sender)
        {
            if (!(sender.Tag is int tag)) return;

            if (!sender.IsChecked)
            {
                // Enable
                sender.IsChecked = true;
                if (tag > 0) // Sakura
                {
                    Enable(tag - 1, true);
                }
                else // Kero
                {
                    Enable((-1 * tag) - 1, false);
                }
            }
            else
            {
                // Disable
                sender.IsChecked = false;
                if (tag > 0) // Sakura
                {
                    Disable(tag - 1, true);
                }
                else // Kero
                {
                    Disable((-1 * tag) - 1, false);
                }
            }
        }

        private void Enable(int seqId, bool isSakura)
        {
            var bindGroups = isSakura ? _sakuraBindGroups : _keroBindGroups;
            if (!bindGroups.TryGetValue(seqId, out var bindGroup)) return;

            // Disable other groups in same category
            foreach (var enabledGroup in _enabledGroups.ToList())
            {
                if (enabledGroup.IsSakura() == isSakura)
                {
                    var groupInfo = bindGroups[enabledGroup.SeqId()];
                    if (groupInfo.Category == bindGroup.Category)
                    {
                        // Find and uncheck menu item
                        var menuItem = _bindGroupMenu.Items
                            .OfType<MenuItem>()
                            .FirstOrDefault(mi =>
                                mi.Tag is int tag &&
                                tag == (isSakura ? enabledGroup.SeqId() + 1 : (enabledGroup.SeqId() + 1) * -1));

                        menuItem.IsChecked = false;
                        enabledGroup.Disable();
                        _enabledGroups.Remove(enabledGroup);
                    }
                }
            }

            _enabledGroups.Add(new SCSerikoEnabledBindGroup(_session, _shell, _seriko, seqId, isSakura));
        }

        private void Disable(int seqId, bool isSakura)
        {
            var group = _enabledGroups.FirstOrDefault(g =>
                g.SeqId() == seqId && g.IsSakura() == isSakura);

            if (group != null)
            {
                group.Disable();
                _enabledGroups.Remove(group);
            }
        }

        public void DisableAll()
        {
            foreach (var group in _enabledGroups)
            {
                group.Disable();
            }
            _enabledGroups.Clear();
        }

        public void Terminate()
        {
            // Save state
            var sakuraBindGroups = string.Join(",",
                _enabledGroups.Where(g => g.IsSakura())
                              .Select(g => g.SeqId().ToString()));

            var keroBindGroups = string.Join(",",
                _enabledGroups.Where(g => !g.IsSakura())
                              .Select(g => g.SeqId().ToString()));

            var ghostDefaults = SCGhostManager.SharedGhostManager().GetGhostDefaults(_session.GetGhostPath());
            var shellDirName = _shell.GetDirName();

            if (!string.IsNullOrEmpty(sakuraBindGroups))
            {
                ghostDefaults[$"seriko.{shellDirName}.sakura.bindgroup"] = sakuraBindGroups;
            }
            else
            {
                ghostDefaults.Remove($"seriko.{shellDirName}.sakura.bindgroup");
            }

            if (!string.IsNullOrEmpty(keroBindGroups))
            {
                ghostDefaults[$"seriko.{shellDirName}.kero.bindgroup"] = keroBindGroups;
            }
            else
            {
                ghostDefaults.Remove($"seriko.{shellDirName}.kero.bindgroup");
            }

            SCGhostManager.SharedGhostManager().SetGhostDefaults(_session.GetGhostPath(), ghostDefaults);

            // Cleanup
            DisableAll();
            if (_hasBindGroupSubMenu)
            {
                SCMenuController.SharedMenuController().MakeNewBindGroupSubMenu(_bindGroupMenuTitle);
            }
        ;
            _bindGroupMenu = null;
            _sakuraBindGroups.Clear();
            _keroBindGroups.Clear();
        }

        public ContextMenu GetBindMenu() => _bindGroupMenu;

        public void Dispose() => Terminate();

        private class BindGroup
        {
            public int Id { get; }
            public string Category { get; }
            public string BindName { get; }
            public string ThumbnailName { get; }

            public BindGroup(int id, string category, string bindName, string thumbnailName)
            {
                Id = id;
                Category = category;
                BindName = bindName;
                ThumbnailName = thumbnailName;
            }
        }
    }
}